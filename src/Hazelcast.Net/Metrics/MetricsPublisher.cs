// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.NearCaching;
using Hazelcast.Protocol.Codecs;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Metrics
{
    internal class MetricsPublisher : IAsyncDisposable
    {
        private readonly Cluster _cluster;
        private readonly NearCacheManager _nearCaches;
        private readonly MetricsOptions _options;
        private readonly ILogger _logger;

        private readonly CancellationTokenSource _cancel;
        private readonly Task _publishing;

        public MetricsPublisher(Cluster cluster, NearCacheManager nearCaches, MetricsOptions options, ILoggerFactory loggerFactory)
        {
            _cluster = cluster;
            _nearCaches = nearCaches;
            _options = options;
            _logger = loggerFactory.CreateLogger<MetricsPublisher>();

            // start the task
            _cancel = new CancellationTokenSource();
            _publishing = PublishAsync(_cancel.Token);

            _logger.LogDebug($"Publishing metrics every {_options.PeriodSeconds}s");
        }

        private async Task PublishAsync(CancellationToken cancellationToken)
        {
            var delay = TimeSpan.FromSeconds(_options.PeriodSeconds);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(delay, cancellationToken).CfAwait();

                try
                {
                    await SendMetricsAsync(cancellationToken).CfAwait();
                }
                catch (Exception e)
                {
                    // should never happen as SendMetricsAsync should not throw - but better be safe
                    _logger.LogError(e, "Caught exception in MetricsPublished.");
                }
            }
        }

        private async Task SendMetricsAsync(CancellationToken cancellationToken)
        {
            // the Java client gets these from a random connection
            // we try to be more consistent and always pick the oldest active connection
            var connection = _cluster.Members.GetOldestConnection();
            if (connection == null)
            {
                _logger.LogDebug("Cannot send metrics, client is not connected.");
                return;
            }

            var timestamp = Clock.Milliseconds;
            var stats = new List<IStat>();

            // these are the stats currently sent by the Java v4 client

            // FIXME how are "gauge" working in Java?!

            stats.AddStat("lastStatisticsCollectionTime", timestamp);
            stats.AddStat("enterprise", false);
            stats.AddStat("clientType", "CSHARP");
            stats.AddStat("clientVersion", ClientVersion.Version);
            stats.AddStat("clientName", _cluster.ClientName);

            stats.AddStat("clusterConnectionTimestamp", Clock.ToEpoch(connection.ConnectTime.UtcDateTime)); // TODO: ToEpoch supports DateTimeOffset
            stats.AddStat("clientAddress", connection.LocalEndPoint.Address);
            stats.AddStat("credentials.principal", connection.Principal);

            // these are the os values copied from v3, TODO: can we do better?
            stats.AddStat("os.committedVirtualMemorySize", () => Process.GetCurrentProcess().VirtualMemorySize64);
            stats.AddEmptyStat("os.freePhysicalMemorySize");
            stats.AddEmptyStat("os.freeSwapSpaceSize");
            stats.AddEmptyStat("os.maxFileDescriptorCount");
            stats.AddStat("os.openFileDescriptorCount", () => Process.GetCurrentProcess().HandleCount);
            stats.AddStat("os.processCpuTime", () => (long) Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds * 1000000);
            stats.AddEmptyStat("os.systemLoadAverage"); // double value, everything else is long
            stats.AddEmptyStat("os.totalPhysicalMemorySize");
            stats.AddEmptyStat("os.totalSwapSpaceSize");

            // these are the runtime values copied from v3, TODO: can we do better?
            stats.AddStat("runtime.availableProcessors", () => Environment.ProcessorCount);
            stats.AddEmptyStat("runtime.freeMemory");
            stats.AddStat("runtime.maxMemory", () => Process.GetCurrentProcess().MaxWorkingSet.ToInt64());
            stats.AddStat("runtime.totalMemory", () => Process.GetCurrentProcess().WorkingSet64);
            stats.AddStat("runtime.uptime", () => (long) (DateTime.Now - Process.GetCurrentProcess().StartTime).TotalMilliseconds);
            stats.AddStat("runtime.usedMemory", () => Process.GetCurrentProcess().WorkingSet64);

            // this was in v3, not in Java v4
            //stats.AddEmptyStat("executionService.userExecutorQueueSize");

            await foreach (var nearCache in _nearCaches)
                stats.AddStats(nearCache);

            //metricsRegistry.Collect(metricsCollector);
            //var metrics = metricsCollector.GetBytes();

            var metrics = Array.Empty<byte>();

            if (cancellationToken.IsCancellationRequested) return; // last chance to cancel

            try
            {
                // non-cancelable
                var sstats = stats.Serialize();
                _logger.LogDebug("Send stats: " + sstats);
                await SendMetricsAsync(timestamp, sstats, metrics).CfAwait();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to send metrics.");
            }
        }

        private async Task SendMetricsAsync(long timestamp, string attributes, byte[] metrics)
        {
            if (!_cluster.IsConnected) // last chance to avoid an exception
            {
                _logger.LogDebug("Cannot send metrics, client is not connected.");
                return;
            }

            _logger.LogDebug("Send metrics.");

            var requestMessage = ClientStatisticsCodec.EncodeRequest(timestamp, attributes, metrics);
            var responseMessage = await _cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var _ = ClientStatisticsCodec.DecodeResponse(responseMessage);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            _cancel.Cancel();
            await _publishing.CfAwaitCanceled();
            _cancel.Dispose();
        }
    }
}