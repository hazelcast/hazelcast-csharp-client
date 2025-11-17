// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Polyfills;
using Hazelcast.Protocol.Codecs;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Metrics
{
    // the metric publishing service
    internal class MetricsPublisher : IAsyncDisposable
    {
        private readonly Cluster _cluster;
        private readonly MetricsOptions _options;
        private readonly ILogger _logger;
        private readonly List<IMetricSource> _metricSources = new List<IMetricSource>();
        private readonly List<IMetricAsyncSource> _metricAsyncSources = new List<IMetricAsyncSource>();

        private readonly CancellationTokenSource _cancel;
        private readonly Task _publishing;

        public MetricsPublisher(Cluster cluster, MetricsOptions options, ILoggerFactory loggerFactory)
        {
            _cluster = cluster;
            _options = options;
            _logger = loggerFactory.CreateLogger<MetricsPublisher>();

            // start the task
            _cancel = new CancellationTokenSource();
            _publishing = PublishAsync(_cancel.Token);

            _logger.IfDebug()?.LogDebug("Publishing metrics every {PeriodSeconds}s", _options.PeriodSeconds);
        }

        public void AddSource(IMetricSource source)
            => _metricSources.Add(source);

        public void AddSource(IMetricAsyncSource source)
            => _metricAsyncSources.Add(source);

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
                _logger.IfDebug()?.LogDebug("Cannot send metrics, client is not connected.");
                return;
            }

            var timestamp = Clock.Milliseconds;
            var metrics = new List<Metric> { ClientMetricSource.MetricDescriptors.LastStatisticsCollectionTime.WithValue(timestamp) };

            foreach (var metricSource in _metricSources)
                metrics.AddRange(metricSource.PublishMetrics());

            foreach (var metricSource in _metricAsyncSources)
                await foreach (var metric in metricSource.PublishMetrics().ConfigureAwait(false))
                    metrics.Add(metric);

            // TODO add NearCache manager as a metric source! - except, then, the source can be async!
            //await foreach (var nearCache in _nearCaches)
            //    metrics.AddRange(nearCache.Statistics.PublishMetrics());

            if (cancellationToken.IsCancellationRequested) return; // last chance to cancel

            try
            {
                // create the binary representation of metrics
                byte[] bytes;
                using (var compressor = new MetricsCompressor())
                {
                    foreach (var metric in metrics) compressor.Append(metric);
                    bytes = compressor.GetBytesAndReset(); // TODO: consider re-using the compressor
                }

                if (cancellationToken.IsCancellationRequested) return;

                // create the text representation of metrics
                var text = metrics.Serialize();

                if (cancellationToken.IsCancellationRequested) return;

                // non-cancelable
                _logger.IfDebug()?.LogDebug("Send stats:\n        " + text.Replace(",", ",\n        ", StringComparison.OrdinalIgnoreCase));

                await SendMetricsAsync(timestamp, text, bytes).CfAwait();
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
                _logger.IfDebug()?.LogDebug("Cannot send metrics, client is not connected.");
                return;
            }

            _logger.IfDebug()?.LogDebug($"Send metrics ({metrics.Length} items).");

            var requestMessage = ClientStatisticsCodec.EncodeRequest(timestamp, attributes, metrics);
            var responseMessage = await _cluster.Messaging.SendAsync(requestMessage).CfAwait();
            var _ = ClientStatisticsCodec.DecodeResponse(responseMessage);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _cancel.TryCancelAsync().CfAwait();
            await _publishing.CfAwaitCanceled();
            _cancel.Dispose();
        }
    }
}
