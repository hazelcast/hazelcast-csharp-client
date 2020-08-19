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
        private readonly HeartbeatOptions _options;
        private readonly TimeSpan _period;
        private readonly TimeSpan _timeout;

        private readonly Cluster _cluster;
        private readonly ILogger _logger;

        private int _active;
        private Task _heartbeating;
        private CancellationTokenSource _cancellation;
        private CancellationTokenSource _linkedCancellation;

        public Heartbeat(Cluster cluster, HeartbeatOptions options, ILoggerFactory loggerFactory)
        {
            _cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = loggerFactory?.CreateLogger<Heartbeat>() ?? throw new ArgumentNullException(nameof(loggerFactory));

            _period = TimeSpan.FromMilliseconds(options.PeriodMilliseconds);
            _timeout = TimeSpan.FromMilliseconds(options.TimeoutMilliseconds);
        }

        public void Start(CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref _active, 1, 0) == 1)
                throw new InvalidOperationException("Already active.");

            _cancellation = new CancellationTokenSource();
            _linkedCancellation = _cancellation.LinkedWith(cancellationToken);
            _heartbeating ??= LoopAsync(_linkedCancellation.Token);
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _active, 0, 1) == 0)
                return;

            _cancellation.Cancel();

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
            finally
            {
                _cancellation.Dispose();
                _cancellation = null;
                _linkedCancellation.Dispose();
                _linkedCancellation = null;
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
                catch (Exception e)
                {
                    // RunAsync should *not* throw
                    _logger.LogWarning(e, "Heartbeat has thrown an exception.");
                }
            }
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var connections = _cluster.Members.SnapshotConnections(true);
            var now = DateTime.UtcNow;

            _logger.LogDebug("Run heartbeat");

            var tasks = connections
                .Select(x => RunAsync(x, now, cancellationToken))
                .ToList();

            await Task.WhenAll(tasks).CAF();
        }

        private async Task RunAsync(MemberConnection connection, DateTime now, CancellationToken cancellationToken)
        {
            // must ensure that timeout > interval ?!

            // make sure we read from the client at least every timeout
            // which is greater than the interval, so we *should* have
            // read from the last ping, if nothing else, so no read means
            // that the client is kinda dead - kill it for real
            if (now - connection.LastReadTime > _timeout)
            {
                await TerminateConnection(connection).CAF();
                return;
            }

            // make sure we write to the client at least every interval
            // this should trigger a read when we receive the response
            if (now - connection.LastWriteTime > _period)
            {
                _logger.LogDebug("Ping client {ClientId}", connection.Id);

                var requestMessage = ClientPingCodec.EncodeRequest();
                var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                try
                {
                    // cannot wait forever on a ping
                    var responseMessage = await _cluster.Messaging
                        .SendToMemberAsync(requestMessage, connection, cancellation.Token)
                        .TimeoutAfter(_options.PingTimeoutMilliseconds, cancellation, true)
                        .CAF();

                    // just to be sure everything is ok
                    _ = ClientPingCodec.DecodeResponse(responseMessage);
                }
                catch (TaskTimeoutException)
                {
                    await TerminateConnection(connection).CAF();
                }
                catch (Exception e)
                {
                    // unexpected
                    _logger.LogWarning(e, "Heartbeat has thrown an exception.");
                }
                finally
                {
                    // if .SendToClientAsync() throws before awaiting, .TimeoutAfter() is never invoked
                    // and therefore cannot dispose the cancellation = better take care of it
                    cancellation.Dispose();
                }
            }
        }

        private async Task TerminateConnection(MemberConnection connection)
        {
            if (!connection.Active) return;

            _logger.LogWarning("Heartbeat timeout for connection {ConnectionId}, terminating.", connection.Id);
            await connection.TerminateAsync().CAF(); // does not throw

            // TODO: original code has reasons for closing connections
            //connection.Close(reason, new TargetDisconnectedException($"Heartbeat timed out to connection {connection}"));
        }
    }
}
