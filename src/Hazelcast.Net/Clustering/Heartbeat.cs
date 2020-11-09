﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

        private int _active;
        private Task _heartbeating;

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
        }

        public void Start()
        {
            if (Interlocked.CompareExchange(ref _active, 1, 0) == 1)
                throw new InvalidOperationException("Already active.");

            _heartbeating ??= LoopAsync(_clusterState.CancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _active, 0, 1) == 0)
                return;

            try
            {
                await _heartbeating.CAF();
                _heartbeating = null;
            }
            catch (OperationCanceledException)
            {
                // expected
            }
            catch (Exception e)
            {
                // unexpected
                _logger.LogWarning(e, "Heartbeat has thrown an exception.");
            }
        }

        private async Task LoopAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                await Task.Delay(_period, cancellationToken).CAF();
                try
                {
                    await RunAsync(cancellationToken).CAF();
                }
                catch (OperationCanceledException)
                {
                    // expected
                }
                catch (Exception e)
                {
                    // RunAsync should *not* throw
                    _logger.LogWarning(e, "Heartbeat has thrown an exception.");
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

            await Task.WhenAll(tasks).CAF(); // may throw in case of cancellation
        }

        // runs once on a connection to a member
        private async Task RunAsync(MemberConnection connection, DateTime now, CancellationToken cancellationToken)
        {
            // must ensure that timeout > interval ?!

            var readElapsed = now - connection.LastReadTime;
            var writeElapsed = now - connection.LastWriteTime;

            HConsole.WriteLine(this, $"Heartbeat on {connection.Id.ToString("N").Substring(0, 5)}, written {(long)(now - connection.LastWriteTime).TotalMilliseconds}ms ago, read {(long)(now - connection.LastReadTime).TotalMilliseconds}ms ago");

            // make sure we read from the client at least every 'timeout',
            // which is greater than the interval, so we *should* have
            // read from the last ping, if nothing else, so no read means
            // that the client not responsive - terminate it

            if (readElapsed > _timeout && writeElapsed < _period)
            {
                _logger.LogWarning("Heartbeat timeout for connection {ConnectionId}.", connection.Id);
                await TerminateConnection(connection).CAF();
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
                        .CAF();

                    // just to be sure everything is ok
                    _ = ClientPingCodec.DecodeResponse(responseMessage);
                }
                catch (TaskTimeoutException)
                {
                    _logger.LogWarning("Heartbeat ping timeout for connection {ConnectionId}.", connection.Id);
                    await TerminateConnection(connection).CAF();
                }
                catch (Exception e)
                {
                    // unexpected
                    _logger.LogWarning(e, "Heartbeat has thrown an exception.");
                }
            }
        }

        private static async Task TerminateConnection(MemberConnection connection)
        {
            if (!connection.Active) return;

            await connection.TerminateAsync().CAF(); // does not throw

            // TODO: original code has reasons for closing connections
            //connection.Close(reason, new TargetDisconnectedException($"Heartbeat timed out to connection {connection}"));
        }
    }
}