// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Models;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    internal class Heartbeat : IAsyncDisposable
    {
        private readonly TimeSpan _period;
        private readonly TimeSpan _timeout;
        private readonly int _parallelCount;

        private readonly TerminateConnections _terminateConnections;
        private readonly ILogger _logger;

        private readonly CancellationTokenSource _cancel;
        private readonly Task _heartbeating;

        private readonly HashSet<MemberConnection> _connections = new();
        private readonly object _mutex = new();

        private int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Heartbeat"/> class.
        /// </summary>
        public Heartbeat(ClusterState clusterState, HeartbeatOptions options, TerminateConnections terminateConnections)
        {
            HConsole.Configure(x => x.Configure<Heartbeat>().SetPrefix("HEARTBEAT"));

            _terminateConnections = terminateConnections;
            if (options == null) throw new ArgumentNullException(nameof(options));

            _logger = clusterState.LoggerFactory.CreateLogger<Heartbeat>();
            
            if (options.PeriodMilliseconds < 0)
            {
                _logger.LogInformation("Heartbeat is disabled (period < 0)");
                return;
            }

            _parallelCount = 4; // TODO: come from options?

            _period = TimeSpan.FromMilliseconds(options.PeriodMilliseconds);
            _timeout = TimeSpan.FromMilliseconds(options.TimeoutMilliseconds);

            if (_timeout <= _period)
            {
                var timeout = TimeSpan.FromMilliseconds(2 * _period.TotalMilliseconds);
                _logger.IfWarning()?.LogWarning("Heartbeat timeout {Timeout}ms is <= period {Period}ms, falling back to a {Value}ms timeout.",
                    (int)_timeout.TotalMilliseconds, (int)_period.TotalMilliseconds, (int)timeout.TotalMilliseconds);
                _timeout = timeout;
            }

            _logger.LogInformation("Heartbeat with {Period}ms period and {Timeout}ms timeout", (int)_period.TotalMilliseconds, (int)_timeout.TotalMilliseconds);

            _cancel = new CancellationTokenSource();
            _heartbeating = BeatAsync(_cancel.Token);
        }

        /// <summary>
        /// Adds a connection.
        /// </summary>
        /// <param name="connection">The connection to add.</param>
        public void AddConnection(MemberConnection connection)
        {
            lock (_mutex) { if (connection.Active) _connections.Add(connection); }
        }

        /// <summary>
        /// Removes a connection.
        /// </summary>
        /// <param name="connection">The connection to remove.</param>
        public void RemoveConnection(MemberConnection connection)
        {
            lock (_mutex) _connections.Remove(connection);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            // note: DisposeAsync should not throw (CA1065)

            if (!_disposed.InterlockedZeroToOne()) return;

            if (_cancel == null) return; // did not run at all

            HConsole.WriteLine(this, "Stopping...");

            _cancel.Cancel();

            try
            {
                await _heartbeating.CfAwaitCanceled();
            }
            catch (Exception e)
            {
                // unexpected
                _logger.IfWarning()?.LogWarning(e, "Caught an exception while disposing Heartbeat.");
            }

            _cancel.Dispose();

            HConsole.WriteLine(this, "Stopped.");
        }

        /// <summary>
        /// Heartbeats periodically. Doesn't throw.
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to be able to cancel.</param>
        /// <returns>awaitable <see cref="Task"/></returns>
        private async Task BeatAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(_period, cancellationToken).CfAwait();
                    if (cancellationToken.IsCancellationRequested) break;
                    HConsole.WriteLine(this, $"Run with period={_period.ToString("hh\\:mm\\:ss", CultureInfo.InvariantCulture)}, timeout={_timeout.ToString("hh\\:mm\\:ss", CultureInfo.InvariantCulture)}");
                    await RunAsync(cancellationToken).CfAwait();
                }
            }
            catch (Exception)
            {
                // exception observed
            }
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.IfDebug()?.LogDebug("Run heartbeat");

            var now = DateTime.Now; // now, or utcNow, but *must* be same as what is used in socket connection base!

            // capture connections
            List<MemberConnection> memberConnections;
            lock (_mutex) memberConnections = new List<MemberConnection>(_connections);

            var cold = GetColdConnections(memberConnections, now);
            if (cold.Count == 0) return;

            var tasks = cold
                .TakeWhile(_ => !cancellationToken.IsCancellationRequested)
                .Select(x => PingAsync(x.MemberConnection, x.MessageConnection, cancellationToken));

            await ParallelRunner.Run(tasks, new ParallelRunner.Options { Count = _parallelCount });
        }

        private List<(MemberConnection MemberConnection, ClientMessageConnection MessageConnection)> GetColdConnections(IEnumerable<MemberConnection> memberConnections, DateTime now)
        {
            var cold = new List<(MemberConnection MemberConnection, ClientMessageConnection MessageConnection)>();
            var temp = new List<(MemberConnection MemberConnection, ClientMessageConnection MessageConnection)>();

            foreach (var memberConnection in memberConnections)
            {
                temp.Clear();
                var state = ConnectionLifeState.Warm;
                foreach (var messageConnection in memberConnection.GetMessageConnections())
                {
                    var s = messageConnection.GetLifeState(now, _period, _timeout);
                    state = state.Worse(s);
                    if (s == ConnectionLifeState.Dead) break;
                    if (s == ConnectionLifeState.Cold) temp.Add((memberConnection, messageConnection));
                }

                if (state == ConnectionLifeState.Dead) _terminateConnections.Add(memberConnection);
                else if (state == ConnectionLifeState.Cold) cold.AddRange(temp);
            }

            return cold;
        }

        private async Task PingAsync(MemberConnection memberConnection, ClientMessageConnection messageConnection, CancellationToken cancellationToken)
        {
            // TODO: better logging everywhere
            _logger.IfDebug()?.LogDebug("Ping connection {ConnectionId} to {MemberId} at {MemberAddress}.", 
                memberConnection.Id.ToShortString(), memberConnection.MemberId.ToShortString(), memberConnection.Address);

            try
            {
                // ping should complete within the default invocation timeout
                var requestMessage = ClientPingCodec.EncodeRequest();
                requestMessage.InvocationFlags |= InvocationFlags.InvokeWhenNotConnected; // run even if client not 'connected'
                var responseMessage = await memberConnection.SendAsync(requestMessage, messageConnection, cancellationToken).CfAwait();
                _ = ClientPingCodec.DecodeResponse(responseMessage); // just to be sure everything is ok
            }
            catch (ClientOfflineException)
            {
                // connection already down
            }
            catch (TaskTimeoutException)
            {
                // timeout, kill the connection
                _logger.IfWarning()?.LogWarning("Heartbeat ping timeout for connection {ConnectionId}, terminating.", memberConnection.Id.ToShortString());
                if (memberConnection.Active) _terminateConnections.Add(memberConnection);
            }
            catch (Exception e)
            {
                // unexpected, don't terminate the connection, we don't know what is going on
                _logger.IfWarning()?.LogWarning(e, "Heartbeat has thrown an exception, but will continue.");
            }
        }
    }
}
