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
using System.Globalization;
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
        private TimeSpan _period;
        private TimeSpan _timeout;

        private readonly TerminateConnections _terminateConnections;
        private readonly ClusterState _clusterState;
        private readonly ClusterMessaging _clusterMessaging;
        private readonly ILogger _logger;

        private CancellationTokenSource _cancel;
        private Task _heartbeating;

        private readonly HashSet<MemberConnection> _connections = new HashSet<MemberConnection>();
        private readonly object _mutex = new object();

        private int _active;

        public Heartbeat(ClusterState clusterState, ClusterMessaging clusterMessaging, HeartbeatOptions options, TerminateConnections terminateConnections)
        {
            _clusterState = clusterState ?? throw new ArgumentNullException(nameof(clusterState));
            _clusterMessaging = clusterMessaging ?? throw new ArgumentNullException(nameof(clusterMessaging));
            _terminateConnections = terminateConnections;
            if (options == null) throw new ArgumentNullException(nameof(options));

            _logger = clusterState.LoggerFactory.CreateLogger<Heartbeat>();

            _clusterState.ClusterOptionsChanged += (ClusterOptions options) =>
            {
                if (options.Heartbeat == null) return;

                InitializeDuration(options.Heartbeat);
            };

            if (options.PeriodMilliseconds < 0)
            {
                _logger.LogInformation("Heartbeat is disabled (period < 0)");
                return;
            }

            HConsole.Configure(x => x.Configure<Heartbeat>().SetPrefix("HEARTBEAT"));

            InitializeDuration(options);
        }

        private void InitializeDuration(HeartbeatOptions options)
        {
            HConsole.WriteLine(this, "Initialize durations...");

            // stop heartbeat during exchanging. 
            if (_active == 1 && _heartbeating != null)
            {
                // heartbeating won't throw.
                _cancel.Cancel();
                _cancel.Dispose();
            }

            if (options.PeriodMilliseconds < 0)
            {
                _logger.LogInformation("Heartbeat is disabled (period < 0)");
                _active = 0;
                return;
            }

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
            _active = 1;
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

        public async ValueTask DisposeAsync()
        {
            // note: DisposeAsync should not throw (CA1065)

            if (Interlocked.CompareExchange(ref _active, 0, 1) == 0)
                return;

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
                //Exception observed
            }
        }

        // runs once on the whole cluster
        private async Task RunAsync(CancellationToken cancellationToken)
        {
            _logger.IfDebug()?.LogDebug("Run heartbeat");

            var now = DateTime.Now; // now, or utcNow, but *must* be same as what is used in socket connection base!
            const int maxTasks = 4; // max 4 at a time TODO: consider making it an option?

            // run for each member
            var tasks = new List<Task>();

            // capture and enumerate connections
            List<MemberConnection> connections;
            lock (_mutex) connections = new List<MemberConnection>(_connections);
            using var connectionsEnumerator = connections.Where(x => x.Active).GetEnumerator();

            void StartCurrent()
            {
                var connection = connectionsEnumerator.Current;
                if (connection != null && connection.Active)
                    tasks.Add(RunAsync(connection, now, cancellationToken));
            }

            // start maxTasks tasks
            while (tasks.Count < maxTasks && connectionsEnumerator.MoveNext() && !cancellationToken.IsCancellationRequested) StartCurrent();

            // each time a task completes, replace it with another task
            while (tasks.Count > 0)
            {
                var task = await Task.WhenAny(tasks).CfAwait();
                tasks.Remove(task);

                if (connectionsEnumerator.MoveNext() && !cancellationToken.IsCancellationRequested) StartCurrent();
            }
        }

        // runs once on a connection to a member
        private async Task RunAsync(MemberConnection connection, DateTime now, CancellationToken cancellationToken)
        {
            var readElapsed = now - connection.LastReadTime;
            var writeElapsed = now - connection.LastWriteTime;

            HConsole.WriteLine(this, $"Heartbeat {_clusterState.ClientName} on {connection.Id.ToShortString()} to {connection.MemberId.ToShortString()} at {connection.Address}, " +
                                     $"written {(int)writeElapsed.TotalSeconds}s ago, " +
                                     $"read {(int)readElapsed.TotalSeconds}s ago");

            // make sure we read from the client at least every 'timeout',
            // which is greater than the interval, so we *should* have
            // read from the last ping, if nothing else, so no read means
            // that the client not responsive - terminate it

            if (readElapsed > _timeout && writeElapsed < _period)
            {
                _logger.IfWarning()?.LogWarning("Heartbeat timeout for connection {ConnectionId}, terminating.", connection.Id.ToShortString());
                if (connection.Active) _terminateConnections.Add(connection);
                return;
            }

            // make sure we write to the client at least every 'period',
            // this should trigger a read when we receive the response
            if (writeElapsed > _period)
            {
                _logger.IfDebug()?.LogDebug("Ping connection {ConnectionId} to {MemberId} at {MemberAddress}.", connection.Id.ToShortString(), connection.MemberId.ToShortString(), connection.Address);

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
                catch (ClientOfflineException)
                {
                    // down
                }
                catch (TaskTimeoutException)
                {
                    _logger.IfWarning()?.LogWarning("Heartbeat ping timeout for connection {ConnectionId}, terminating.", connection.Id.ToShortString());
                    if (connection.Active) _terminateConnections.Add(connection);
                }
                catch (Exception e)
                {
                    // unexpected
                    _logger.IfWarning()?.LogWarning(e, "Heartbeat has thrown an exception, but will continue.");
                }
            }
        }
    }
}
