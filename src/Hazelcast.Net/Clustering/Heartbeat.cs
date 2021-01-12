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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Protocol.Codecs;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    internal class Heartbeat : IAsyncDisposable
    {
        private readonly TimeSpan _period;
        private readonly TimeSpan _timeout;

        private readonly ClusterState _clusterState;
        private readonly ClusterMembers _clusterMembers;
        private readonly ClusterMessaging _clusterMessaging;
        private readonly ILogger _logger;

        private readonly CancellationTokenSource _cancellation;
        private readonly Task _heartbeating;

        private int _active;

        public Heartbeat(ClusterState clusterState, ClusterMembers clusterMembers, ClusterMessaging clusterMessaging, HeartbeatOptions options, ILoggerFactory loggerFactory)
        {
            _clusterState = clusterState ?? throw new ArgumentNullException(nameof(clusterState));
            _clusterMembers = clusterMembers ?? throw new ArgumentNullException(nameof(clusterMembers));
            _clusterMessaging = clusterMessaging ?? throw new ArgumentNullException(nameof(clusterMessaging));
            if (options == null) throw new ArgumentNullException(nameof(options));
            _logger = loggerFactory?.CreateLogger<Heartbeat>() ?? throw new ArgumentNullException(nameof(loggerFactory));

            _period = TimeSpan.FromMilliseconds(options.PeriodMilliseconds);
            _timeout = TimeSpan.FromMilliseconds(options.TimeoutMilliseconds);

            // sanity checks
            if (_timeout <= _period)
            {
                var timeout = TimeSpan.FromMilliseconds(2 * _period.TotalMilliseconds);
                _logger.LogWarning("Heartbeat timeout {Timeout}ms is <= period {Period}ms, falling back to a {Value}ms timeout.",
                    _timeout, _period.TotalMilliseconds, timeout);
                _timeout = timeout;
            }

            _cancellation = new CancellationTokenSource();
            _heartbeating ??= LoopAsync(_cancellation.Token);
        }

        public async ValueTask DisposeAsync()
        {
            // note: DisposeAsync should not throw (CA1065)

            if (Interlocked.CompareExchange(ref _active, 0, 1) == 0)
                return;

            _cancellation.Cancel();

            try
            {
                await _heartbeating.ObserveCanceled().CfAwait();
            }
            catch (Exception e)
            {
                // unexpected
                _logger.LogWarning(e, "Caught an exception while disposing Heartbeat.");
            }
        }

        private async Task LoopAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                await Task.Delay(_period, cancellationToken).CfAwait();
                try
                {
                    await RunAsync(cancellationToken).ObserveCanceled().CfAwait();
                }
                catch (Exception e)
                {
                    // unexpected
                    _logger.LogWarning(e, "Caught exception in Heartbeat.");
                }
            }
        }

        // runs once on the whole cluster
        private async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Run heartbeat");

            var now = DateTime.Now; // now, or utcNow, but *must* be same as what is used in socket connection base!
            var connections = _clusterMembers.SnapshotConnections(true);

            // start one task per member
            // TODO: throttle?
            var tasks = connections
                .Select(x => RunAsync(x, now, cancellationToken))
                .ToList();

            await Task.WhenAll(tasks).CfAwait(); // may throw in case of cancellation
        }

        // runs once on a connection to a member
        private async Task RunAsync(MemberConnection connection, DateTime now, CancellationToken cancellationToken)
        {
            // must ensure that timeout > interval ?!

            var readElapsed = now - connection.LastReadTime;
            var writeElapsed = now - connection.LastWriteTime;

            HConsole.WriteLine(this, $"Heartbeat on {connection.Id.ToShortString()}, written {(long)(now - connection.LastWriteTime).TotalMilliseconds}ms ago, read {(long)(now - connection.LastReadTime).TotalMilliseconds}ms ago");

            // make sure we read from the client at least every 'timeout',
            // which is greater than the interval, so we *should* have
            // read from the last ping, if nothing else, so no read means
            // that the client not responsive - terminate it

            if (readElapsed > _timeout && writeElapsed < _period)
            {
                _logger.LogWarning("Heartbeat timeout for connection {ConnectionId}.", connection.Id);
                if (connection.Active) await connection.TerminateAsync().CfAwait(); // does not throw;
                return;
            }

            // make sure we write to the client at least every 'interval',
            // this should trigger a read when we receive the response
            if (writeElapsed > _period)
            {
                _logger.LogDebug("Ping client {ClientId}", connection.Id);

                var requestMessage = ClientPingCodec.EncodeRequest();

                try
                {
                    // ping should complete within the default invocation timeout
                    var responseMessage = await _clusterMessaging
                        .SendToMemberAsync(requestMessage, connection, cancellationToken)
                        .CfAwait();

                    // just to be sure everything is ok
                    _ = ClientPingCodec.DecodeResponse(responseMessage);
                }
                catch (TaskTimeoutException)
                {
                    _logger.LogWarning("Heartbeat ping timeout for connection {ConnectionId}.", connection.Id);
                    if (connection.Active) await connection.TerminateAsync().CfAwait(); // does not throw;
                }
                catch (Exception e)
                {
                    // unexpected
                    _logger.LogWarning(e, "Heartbeat has thrown an exception.");
                }
            }
        }
    }
}
