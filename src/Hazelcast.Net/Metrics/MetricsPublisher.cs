﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.NearCaching;
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

            _logger.LogDebug($"Publishing metrics every {_options.PeriodSeconds}s");
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
                _logger.LogDebug("Cannot send metrics, client is not connected.");
                return;
            }

            var timestamp = Clock.Milliseconds;
            var metrics = new List<Metric> { ClientMetricSource.MetricDescriptors.LastStatisticsCollectionTime.WithValue(timestamp) };

            foreach (var metricSource in _metricSources)
                metrics.AddRange(metricSource.PublishMetrics());

            foreach (var metricSource in _metricAsyncSources)
                await foreach (var metric in metricSource.PublishMetrics())
                    metrics.Add(metric);

            // TODO add NearCache manager as a metric source! - except, then, the souce can be async!
            //await foreach (var nearCache in _nearCaches)
            //    metrics.AddRange(nearCache.Statistics.PublishMetrics());

            if (cancellationToken.IsCancellationRequested) return; // last chance to cancel

            try
            {
                byte[] bytes;
                using (var compressor = new MetricsCompressor())
                {
                    foreach (var metric in metrics)
                        compressor.Append(metric);
                    bytes = compressor.GetBytesAndReset(); // FIXME should we, like, recycle it?
                }

                if (cancellationToken.IsCancellationRequested) return;

                var text = metrics.Serialize();

                if (cancellationToken.IsCancellationRequested) return;

                // non-cancelable
                _logger.LogDebug("Send stats:\n    " + text.Replace(",", ",\n    "));
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