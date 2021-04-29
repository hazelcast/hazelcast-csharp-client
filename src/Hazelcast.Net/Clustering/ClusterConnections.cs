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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Networking;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;
using MemberInfo = Hazelcast.Models.MemberInfo;

namespace Hazelcast.Clustering
{
    internal class ClusterConnections : IAsyncDisposable
    {
        private static string _clientVersion;

        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly object _mutex = new object();

        private readonly ClusterState _clusterState;
        private readonly ClusterMembers _clusterMembers;
        private readonly Authenticator _authenticator;
        private readonly AddressProvider _addressProvider;
        private readonly IRetryStrategy _connectRetryStrategy;
        private readonly ILogger _logger;

        // address -> connection
        // TODO: consider we are duplicating this with members?
        private readonly ConcurrentDictionary<Guid, MemberConnection> _connections = new ConcurrentDictionary<Guid, MemberConnection>();

        // connection -> completion
        private readonly ConcurrentDictionary<MemberConnection, TaskCompletionSource<object>> _completions = new ConcurrentDictionary<MemberConnection, TaskCompletionSource<object>>();

        private Action<MemberConnection> _connectionCreated;
        private Func<MemberConnection, bool, bool, bool, ValueTask> _connectionOpened;
        private Func<MemberConnection, ValueTask> _connectionClosed;
        private BackgroundTask _reconnect;
        private Guid _clusterId;

        private readonly Task _connectMembers;

        private volatile int _disposed; // disposed flag

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterConnections"/> class.
        /// </summary>
        public ClusterConnections(ClusterState clusterState, ClusterMembers clusterMembers, MemberConnectionQueue memberConnectionQueue, SerializationService serializationService)
        {
            _clusterState = clusterState;
            _clusterMembers = clusterMembers;

            _logger = _clusterState.LoggerFactory.CreateLogger<ClusterConnections>();
            _authenticator = new Authenticator(_clusterState.Options.Authentication, serializationService);
            _addressProvider = new AddressProvider(_clusterState.Options.Networking, _clusterState.LoggerFactory);
            _connectRetryStrategy = new RetryStrategy("connect to cluster", _clusterState.Options.Networking.ConnectionRetry, _clusterState.LoggerFactory);

            if (_clusterState.IsSmartRouting)
                _connectMembers = ConnectMembers(memberConnectionQueue, _cancel.Token);

            _clusterState.StateChanged += OnStateChanged;
            
            HConsole.Configure(options => options.Set(this, x => x.SetPrefix("CCNX")));
        }

        #region Connect Members

        // background task that connect members
        private async Task ConnectMembers(MemberConnectionQueue memberConnectionQueue, CancellationToken cancellationToken)
        {
            await foreach(var (member, token) in memberConnectionQueue.WithCancellation(cancellationToken))
            {
                var attempt = Attempt<MemberConnection>.Failed;
                bool canceled;
                Exception exception = null;

                HConsole.WriteLine(this, $"Ensure a connection for member {member.Id.ToShortString()} (at {member.Address})");
                using (var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, token))
                {
                    try
                    {
                        attempt = await EnsureConnectionAsync(member, source.Token).CfAwait();
                    }
                    catch (OperationCanceledException)
                    { }
                    catch (Exception e)
                    {
                        exception = e;
                    }

                    canceled = source.IsCancellationRequested;
                }

                if (_disposed > 0)
                {
                    _logger.LogWarning($"Could not connect to member at {member.Address}: shutting down.");
                }
                else
                {
                    var details = canceled ? "canceled" : "failed";
                    _logger.LogWarning(exception, $"Could not connect to member at {member.Address}: {details}.");

                    memberConnectionQueue.Add(member);
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Gets or sets an action that will be executed when a connection is created.
        /// </summary>
        public Action<MemberConnection> ConnectionCreated
        {
            get => _connectionCreated;
            set
            {
                _clusterState.ThrowIfPropertiesAreReadOnly();
                _connectionCreated = value;
            }
        }

        private void RaiseConnectionCreated(MemberConnection connection)
        {
            _connectionCreated?.Invoke(connection);
        }

        /// <summary>
        /// Gets or sets an action that will be executed when a connection is opened.
        /// </summary>
        public Func<MemberConnection, bool, bool, bool, ValueTask> ConnectionOpened
        {
            get => _connectionOpened;
            set
            {
                _clusterState.ThrowIfPropertiesAreReadOnly();
                _connectionOpened = value;
            }
        }

        private async ValueTask RaiseConnectionOpened(MemberConnection connection, bool isFirstEver, bool isFirst, bool isNewCluster)
        {
            if (_connectionOpened == null) return;

            try
            {
                await _connectionOpened.AwaitEach(connection, isFirstEver, isFirst, isNewCluster).CfAwait();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception while raising ConnectionOpened.");
            }
        }

        /// <summary>
        /// Gets or sets an action that will be executed when a connection is closed.
        /// </summary>
        public Func<MemberConnection, ValueTask> ConnectionClosed
        {
            get => _connectionClosed;
            set
            {
                _clusterState.ThrowIfPropertiesAreReadOnly();
                _connectionClosed = value;
            }
        }

        private async ValueTask RaiseConnectionClosed(MemberConnection connection)
        {
            if (_connectionClosed == null) return;

            try
            {
                await _connectionClosed.AwaitEach(connection).CfAwait();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception while raising ConnectionClosed.");
            }
        }

        #endregion

        #region Event Handlers

        private ValueTask OnStateChanged(ClientState state)
        {
            _logger.LogDebug($"State changed: {state}");

            // only if disconnected
            if (state != ClientState.Disconnected) return default;

            // and still disconnected - if the cluster is down or shutting down, give up
            if (_clusterState.ClientState != ClientState.Disconnected) 
            {
                _logger.LogInformation("Disconnected (shutting down)");
                return default;
            }

            // the cluster is disconnected, but not down
            _logger.LogInformation("Disconnected (reconnect mode == {ReconnectMode} => {ReconnectAction})",
                _clusterState.Options.Networking.ReconnectMode,
                _clusterState.Options.Networking.ReconnectMode switch
                {
                    ReconnectMode.DoNotReconnect => "shut down",
                    ReconnectMode.ReconnectSync => "reconnect synchronously",
                    ReconnectMode.ReconnectAsync => "reconnect asynchronously",
                    _ => "meh?"
                });

            // what we do next depends on options
            switch (_clusterState.Options.Networking.ReconnectMode)
            {
                case ReconnectMode.DoNotReconnect:
                    // DoNotReconnect = the cluster shuts down
                    _clusterState.RequestShutdown();
                    break;

                case ReconnectMode.ReconnectSync:
                case ReconnectMode.ReconnectAsync:
                    // Reconnect Sync or Async = the cluster reconnects via a background task
                    // operations will either block or fail
                    _reconnect = BackgroundTask.Run(ReconnectAsync);
                    break;

                default:
                    throw new NotSupportedException();
            }

            return default;
        }

        /// <summary>
        /// Handles a <see cref="MemberConnection"/> going down.
        /// </summary>
        /// <param name="connection">The connection.</param>
        private async ValueTask OnConnectionClosed(MemberConnection connection)
        {
            TaskCompletionSource<object> connectCompletion;
            lock (_mutex)
            {
                // if the connection was not added yet, ignore
                if (!_connections.TryRemove(connection.MemberId, out _))
                    return;

                // else get its 'connect' completion source
                _completions.TryGetValue(connection, out connectCompletion);
            }

            // if still connecting... wait until done, because we cannot
            // eg trigger the 'closed' event before or while the 'opened'
            // triggers
            if (connectCompletion != null)
            {
                await connectCompletion.Task.CfAwait();
                _completions.TryRemove(connection, out _);
            }

            // proceed: raise 'closed'
            await RaiseConnectionClosed(connection).CfAwait(); // does not throw
        }

        #endregion

        /// <summary>
        /// Connects to the cluster.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when connected.</returns>
        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            // FIXME: cancellations?
            using var cancellation = _clusterState.GetLinkedCancellation(cancellationToken, false);
            cancellationToken = cancellation.Token;

            // properties cannot be changed once connected
            _clusterState.SetPropertiesReadOnly();

            // we have started, and are now trying to connect
            if (!await _clusterState.ChangeStateAndWait(ClientState.Started, ClientState.Starting).CfAwait())
                throw new ConnectionException("Failed to connect (aborted).");

            try
            {
                // establishes the first connection, throws if it fails
                await ConnectFirstAsync(cancellationToken).CfAwait();

                // once the first connection is established, we should use it to subscribe
                // to the cluster views event, and then we should receive a members view,
                // which in turn should change the state to Connected - unless something
                // goes wrong
                var connected = await _clusterState.WaitForConnectedAsync(cancellationToken).CfAwait();

                if (!connected)
                    throw new ConnectionException("Failed to connect.");

                // we have been connected (rejoice) - of course, nothing guarantees that it
                // will last, but then OnConnectionClosed will deal with it
            }
            catch
            {
                // we *have* retried and failed, shutdown & throw
                _clusterState.RequestShutdown();
                throw;
            }
        }

        /// <summary>
        /// Reconnects to the cluster.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when reconnected.</returns>
        private async Task ReconnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                // establishes the first connection, throws if it fails
                await ConnectFirstAsync(cancellationToken).CfAwait();

                // once the first connection is established, we should use it to subscribe
                // to the cluster views event, and then we should receive a members view,
                // which in turn should change the state to Connected - unless something
                // goes wrong
                var connected = await _clusterState.WaitForConnectedAsync(cancellationToken).CfAwait();

                if (!connected)
                {
                    // we are a background task and cannot throw!
                    _logger.LogError("Failed to reconnect.");
                }
                else
                {
                    _logger.LogDebug("Reconnected");
                }

                // we have been reconnected (rejoice) - of course, nothing guarantees that it
                // will last, but then OnConnectionClosed will deal with it
            }
            catch (Exception e)
            {
                // we *have* retried and failed, shutdown, and log (we are a background task!)
                _clusterState.RequestShutdown();
                _logger.LogError(e, "Failed to reconnect.");
            }

            // in any case, remove ourselves
            _reconnect = null;
        }
        
        /// <summary>
        /// Gets the cluster addresses.
        /// </summary>
        /// <returns>All cluster addresses.</returns>
        /// <remarks>
        /// <para>This methods first list the known members' addresses, and then the
        /// configured addresses. Each group can be shuffled, depending on options.
        /// The returned addresses are distinct across both groups, i.e. each address
        /// is returned only once.</para>
        /// </remarks>
        private IEnumerable<NetworkAddress> GetClusterAddresses()
        {
            var shuffle = _clusterState.Options.Networking.ShuffleAddresses;
            var distinct = new HashSet<NetworkAddress>();

            static IEnumerable<NetworkAddress> Distinct(IEnumerable<NetworkAddress> aa, ISet<NetworkAddress> d, bool s)
            {
                if (s) aa = aa.Shuffle();

                foreach (var a in aa)
                {
                    if (d.Add(a)) yield return a;
                }
            }

            // get known members' addresses
            var addresses = _clusterMembers.GetMembers().Select(x => x.Address);
            foreach (var address in Distinct(addresses, distinct, shuffle))
                yield return address;

            // get configured addresses that haven't been tried already
            addresses = _addressProvider.GetAddresses();
            foreach (var address in Distinct(addresses, distinct, shuffle))
                yield return address;
        }

        /// <summary>
        /// Opens a first connection to the cluster (no connection yet).
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when connected.</returns>
        /// <remarks>
        /// <para>Tries all the candidate addresses until one works; tries again
        /// according to the configured retry strategy, and if nothing works,
        /// end up throwing an exception.</para>
        /// </remarks>
        private async Task ConnectFirstAsync(CancellationToken cancellationToken)
        {
            var tried = new HashSet<NetworkAddress>();
            List<Exception> exceptions = null;
            bool canRetry;

            _connectRetryStrategy.Restart();

            do
            {
                try
                {
                    // try each address (unique by the IPEndPoint)
                    foreach (var address in GetClusterAddresses())
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        tried.Add(address);

                        HConsole.WriteLine(this, $"Try to connect to address {address}");
                        var attempt = await ConnectFirstAsync(address, cancellationToken).CfAwait(); // does not throw
                        if (attempt)
                        {
                            HConsole.WriteLine(this, $"Connected to address {address}");
                            return; // successful exit, a first connection has been opened
                        }

                        HConsole.WriteLine(this, $"Failed to connect to address {address}");

                        if (attempt.HasException) // else gather exceptions
                        {
                            exceptions ??= new List<Exception>();
                            exceptions.Add(attempt.Exception);
                        }
                    }
                }
                catch (Exception e)
                {
                    // the GetClusterAddresses() enumerator itself can throw, if a configured
                    // address is invalid or cannot be resolved via DNS... a DNS problem may
                    // be transient: better retry

                    exceptions ??= new List<Exception>();
                    exceptions.Add(e);

                    // TODO: it's the actual DNS that should retry!
                }

                try
                {
                    // try to retry, maybe with a delay - handles cancellation
                    canRetry = await _connectRetryStrategy.WaitAsync(cancellationToken).CfAwait();
                }
                catch (OperationCanceledException) // don't gather the cancel exception
                {
                    canRetry = false; // retry strategy was canceled
                }
                catch (Exception e) // gather exceptions
                {
                    exceptions ??= new List<Exception>();
                    exceptions.Add(e);
                    canRetry = false; // retry strategy threw
                }

            } while (canRetry);

            var aggregate = new AggregateException(exceptions);

            // canceled exception?
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException($"The cluster connection operation to \"{_clusterState.ClusterName}\" has been canceled. " +
                    $"The following addresses where tried: {string.Join(", ", tried)}.", aggregate);

            // other exception
            throw new ConnectionException($"Unable to connect to the cluster \"{_clusterState.ClusterName}\". " +
                $"The following addresses where tried: {string.Join(", ", tried)}.", aggregate);
        }

        /// <summary>
        /// Opens a first connection to an address (no other connections).
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The opened connection, if successful.</returns>
        /// <remarks>
        /// <para>This method does not throw.</para>
        /// </remarks>
        private async Task<Attempt<MemberConnection>> ConnectFirstAsync(NetworkAddress address, CancellationToken cancellationToken)
        {
            // lock the address - can only connect once at a time per address
            // but! this is the first connection so nothing else can connect
            //using var locked = _addressLocker.LockAsync(address);

            try
            {
                // this may throw
                return await ConnectAsync(address, cancellationToken).CfAwait();
            }
            catch (Exception e)
            {
                // don't throw, just fail
                HConsole.WriteLine(this, "Exceptions while connecting " + e);
                return Attempt.Fail<MemberConnection>(e);
            }
        }

        /// <summary>
        /// Ensures that a connection exists to a member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// <para>This method does not throw.</para>
        /// </remarks>
        private async Task<Attempt<MemberConnection>> EnsureConnectionAsync(MemberInfo member, CancellationToken cancellationToken)
        {
            // if we already have a client for that address, return the client
            // if it is active, or fail if it is not - cannot open yet another
            // client to that same address, we'll have to wait for the inactive
            // connection to be removed.
            if (_connections.TryGetValue(member.Id, out var connection))
            {
                var active = connection.Active;
                HConsole.WriteLine(this, $"Found {(active ? "" : "non-")}active connection for member {member.Id} (at {connection.Address})");
                return Attempt.If(active, connection);
            }

            // ConnectMembers invokes EnsureConnectionAsync sequentially, and is suspended
            // whenever we need to connect the very first address, therefore each address
            // can only be connected once at a time = no need for locks here

            // exit now if canceled
            if (cancellationToken.IsCancellationRequested)
                return Attempt.Fail<MemberConnection>();

            // else actually connect
            try
            {
                // this may throw
                HConsole.WriteLine(this, $"Open connection to {member.Address}");
                return await ConnectAsync(member.Address, cancellationToken).CfAwait();
            }
            catch (Exception e)
            {
                // don't throw, just fail
                return Attempt.Fail<MemberConnection>(e);
            }
        }

        private static string ClientVersion
        {
            get
            {
                if (_clientVersion != null) return _clientVersion;

                var assembly = Assembly.GetExecutingAssembly();
                var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if (attribute != null)
                {
                    var version = attribute.InformationalVersion;
                    var pos = version.IndexOf('+');
                    if (pos > 0 && version.Length > pos + 7)
                        version = version.Substring(0, pos + 7);
                    _clientVersion = version;
                }
                else
                {
                    var v = assembly.GetCustomAttribute<AssemblyVersionAttribute>();
                    _clientVersion = v != null ? v.Version : "?";
                }

                return _clientVersion;
            }
        }

        private static async ValueTask ThrowDisconnected(MemberConnection connection)
        {
            // disposing the connection *will* run OnConnectionClosed which will
            // remove the connection from all the places it needs to be removed from
            await connection.DisposeAsync().CfAwait();
            throw new TargetDisconnectedException();
        }

        private static async ValueTask ThrowRejected(MemberConnection connection)
        {
            // disposing the connection *will* run OnConnectionClosed which will
            // remove the connection from all the places it needs to be removed from
            await connection.DisposeAsync().CfAwait();
            throw new ConnectionException("Connection was not accepted.");
        }

        private static async ValueTask ThrowCanceled(MemberConnection connection)
        {
            // disposing the connection *will* run OnConnectionClosed which will
            // remove the connection from all the places it needs to be removed from
            await connection.DisposeAsync().CfAwait();
            throw new OperationCanceledException();
        }

        /// <summary>
        /// Opens a connection to an address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the connection has been established, and represents the associated client.</returns>
        private async Task<MemberConnection> ConnectAsync(NetworkAddress address, CancellationToken cancellationToken)
        {
            // map private address to public address
            address = _addressProvider.Map(address);

            // create the connection to the member
            var connection = new MemberConnection(address, _authenticator, _clusterState.Options.Messaging, _clusterState.Options.Networking, _clusterState.Options.Networking.Ssl, _clusterState.ConnectionIdSequence, _clusterState.CorrelationIdSequence, _clusterState.LoggerFactory)
            {
                Closed = OnConnectionClosed
            };

            RaiseConnectionCreated(connection);

            if (cancellationToken.IsCancellationRequested) await ThrowCanceled(connection).CfAwait();

            // note: soon as ConnectAsync returns, the connection can close anytime - this is handled by
            // adding the connection to _connections within _connectionsMutex + managing a connection
            // completions that ensures that either neither Opened nor Closed trigger, or both trigger
            // and in the right order, Closed after Opened has completed

            // connect to the server (may throw and that is ok here)
            var result = await connection.ConnectAsync(_clusterState, cancellationToken).CfAwait();

            // report
            _logger.LogInformation("Authenticated client '{ClientName}' ({ClientId}) running version {ClientVersion}"+
                                   " on connection {LocalAddress} -> {RemoteAddress} ({ConnectionId})" +
                                   " to member {MemberId}" + 
                                   " of cluster '{ClusterName}' ({ClusterId}) running version {HazelcastServerVersion}.",
                _clusterState.ClientName, _clusterState.ClientId.ToShortString(), ClientVersion,
                connection.LocalEndPoint, result.MemberAddress, connection.Id.ToShortString(),
                result.MemberId.ToShortString(), 
                _clusterState.ClusterName, result.ClusterId.ToShortString(), result.ServerVersion);

            // notify partitioner
            try
            {
                _clusterState.Partitioner.SetOrVerifyPartitionCount(result.PartitionCount);
            }
            catch (Exception e)
            {
                await connection.DisposeAsync().CfAwait(); // does not throw
                throw new ConnectionException("Failed to open a connection because " +
                                              "the partitions count announced by the member is invalid.", e);
            }

            if (cancellationToken.IsCancellationRequested) await ThrowCanceled(connection).CfAwait();
            if (!connection.Active) await ThrowDisconnected(connection).CfAwait();

            // isFirst: this is the first connection (but maybe after we've been disconnected)
            // isFirstEver: this is the first connection, ever
            // isNewCluster: when isFirst, this is also a new cluster (either because isFirstEver, or because of a cluster id change)
            var isFirst = false;
            var isFirstEver = false;
            var isNewCluster = false;
            var accepted = false;

            // register the connection
            lock (_mutex)
            {
                if (_disposed == 0)
                {
                    isFirst = _connections.Count == 0;
                    isFirstEver = isFirst && _clusterId == default;
                    accepted = true;

                    // ok to connect to a different cluster only if this is the very first connection
                    isNewCluster = _clusterId != connection.ClusterId;
                    if (isNewCluster)
                    {
                        if (_connections.Count > 0)
                        {
                            _logger.LogWarning($"Cannot accept a connection to cluster {connection.ClusterId} which is not the current cluster ({_clusterId}).");
                            accepted = false;
                        }
                        else
                        {
                            _clusterId = connection.ClusterId;
                        }
                    }
                }

                // finally, add the connection
                if (accepted)
                {
                    _connections[connection.MemberId] = connection;
                    _completions[connection] = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                }
            }

            // these Throw methods dispose the connection, which will then be removed from _connections
            // we are safe
            if (cancellationToken.IsCancellationRequested) await ThrowCanceled(connection).CfAwait();
            if (!connection.Active) await ThrowDisconnected(connection).CfAwait();
            if (!accepted) await ThrowRejected(connection).CfAwait();
            
            // NOTE: connections are opened either by 'connect first' or by 'connect members' and
            // both ensure that one connection is opened after another - not concurrently - thus
            // making sure that there is no race condition here and the ConnectionOpened for the
            // isFirst connection will indeed trigger before any other connection is created - think
            // about it if adding support for parallel connections!

            // connection is opened
            await RaiseConnectionOpened(connection, isFirstEver, isFirst, isNewCluster).CfAwait();
            
            lock (_mutex)
            {
                // there is always a completion, but we have to TryRemove from concurrent dictionaries
                if (_completions.TryRemove(connection, out var completion)) completion.SetResult(null);
            }

            return connection;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            // be sure to properly terminate _connectMembers, even though, because the
            // MemberConnectionQueue has been disposed already, the task should have
            // ended by now
            _cancel.Cancel();
            if (_connectMembers != null)
                await _connectMembers.CfAwaitCanceled();
            _cancel.Dispose();

            // stop and dispose the reconnect task if it's running
            var reconnect = _reconnect;
            if (reconnect != null)
                await reconnect.CompletedOrCancelAsync(true).CfAwait();

            // trash all remaining connections
            ICollection<MemberConnection> connections;
            lock (_mutex) connections = _connections.Values;
            foreach (var connection in connections)
                await connection.DisposeAsync().CfAwait();
        }
    }
}
