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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Models;
using Hazelcast.Networking;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Models;
using Hazelcast.Serialization;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    internal class ClusterConnections : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private readonly object _mutex = new object();

        private readonly ClusterState _clusterState;
        private readonly ClusterMembers _clusterMembers;
        private readonly IRetryStrategy _connectRetryStrategy;
        private readonly ILogger _logger;


        // member id -> connection
        // TODO: consider we are duplicating this with members?
        private readonly ConcurrentDictionary<Guid, MemberConnection> _connections = new ConcurrentDictionary<Guid, MemberConnection>();

        // connection -> completion
        private readonly ConcurrentDictionary<MemberConnection, TaskCompletionSource<object>> _completions = new ConcurrentDictionary<MemberConnection, TaskCompletionSource<object>>();

        private Authenticator _authenticator;
        private Action<MemberConnection> _connectionCreated;
        private Func<MemberConnection, bool, bool, bool, ClusterVersion, ValueTask> _connectionOpened;
        private Func<MemberConnection, ValueTask> _connectionClosed;
        private BackgroundTask _reconnect;
        private Guid _clusterId;

        private readonly Task _connectMembers;

        private volatile int _disposed; // disposed flag

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterConnections"/> class.
        /// </summary>
        public ClusterConnections(ClusterState clusterState, ClusterMembers clusterMembers, SerializationService serializationService)
        {
            _clusterState = clusterState;
            _clusterMembers = clusterMembers;

            _logger = _clusterState.LoggerFactory.CreateLogger<ClusterConnections>();
            _authenticator = new Authenticator(_clusterState.Options.Authentication, serializationService, _clusterState.LoggerFactory);
            _connectRetryStrategy = new RetryStrategy("connect to cluster", _clusterState.Options.Networking.ConnectionRetry, _clusterState.LoggerFactory);

            if (_clusterState.IsSmartRouting)
                _connectMembers = ConnectMembers(_cancel.Token);

            _clusterState.StateChanged += OnStateChanged;

            //Cluster changed, renew options if necessary.
            _clusterState.Failover.ClusterChanged += options =>
            {
                _authenticator = new Authenticator(options.Authentication, serializationService, _clusterState.LoggerFactory);
            };

            HConsole.Configure(x => x.Configure<ClusterConnections>().SetPrefix("CCNX"));
        }

        #region Connect Members

        private async Task<(bool, bool, Exception)> EnsureConnectionInternalAsync(MemberInfo member, CancellationToken cancellationToken)
        {
            Exception exception = null;
            var wasCanceled = false;

            try
            {
                var attempt = await EnsureConnectionAsync(member, cancellationToken).CfAwait();
                if (attempt) return (true, false, null);
                exception = attempt.Exception;
            }
            catch (OperationCanceledException)
            {
                wasCanceled = true;
            }
            catch (Exception e)
            {
                exception = e;
            }

            return (false, wasCanceled, exception);
        }

        // background task that connect members
        private async Task ConnectMembers(CancellationToken cancellationToken)
        {
            await foreach (var connectionRequest in _clusterMembers.MemberConnectionRequests.WithCancellation(cancellationToken))
            {
                var member = connectionRequest.Member;

                _logger.IfDebug()?.LogDebug("Ensure client {ClientName} is connected to member {MemberId} at {ConnectAddress}.", _clusterState.ClientName, member.Id.ToShortString(), member.ConnectAddress);

                var (success, wasCanceled, exception) = await EnsureConnectionInternalAsync(member, cancellationToken).CfAwait();
                if (success)
                {
                    connectionRequest.Complete(success: true);
                    continue;
                }

                if (_disposed > 0)
                {
                    _logger.IfWarning()?.LogWarning("Could not connect to member {MemberId} at {ConnectAddress}: shutting down.", member.Id.ToShortString(), member.ConnectAddress);
                }
                else
                {
                    var details = wasCanceled ? "canceled" : "failed";
                    if (exception is RemoteException { Error: RemoteError.HazelcastInstanceNotActive })
                    {
                        exception = null;
                        details = "failed (member is not active)";
                    }
                    else if (exception is TimeoutException)
                    {
                        exception = null;
                        details = "failed (socket timeout)";
                    }
                    //ClientNotAllowedInClusterException is reported here.
                    else if (exception != null)
                        details = $"failed ({exception.GetType()}: {exception.Message})";
                    _logger.IfWarning()?.LogWarning(exception, "Could not connect to member {MemberId} at {ConnectAddress}: {Details}.", member.Id.ToShortString(), member.ConnectAddress, details);
                }

                connectionRequest.Complete(success: false);
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
        public Func<MemberConnection, bool, bool, bool, ClusterVersion, ValueTask> ConnectionOpened
        {
            get => _connectionOpened;
            set
            {
                _clusterState.ThrowIfPropertiesAreReadOnly();
                _connectionOpened = value;
            }
        }

        private async ValueTask RaiseConnectionOpened(MemberConnection connection, bool isFirstEver, bool isFirst, bool isNewCluster, ClusterVersion clusterVersion)
        {
            if (_connectionOpened == null) return;

            try
            {
                _logger.IfDebug()?.LogDebug("Raise ConnectionOpened");
                await _connectionOpened.AwaitEach(connection, isFirstEver, isFirst, isNewCluster, clusterVersion).CfAwait();
                _logger.IfDebug()?.LogDebug("Raised ConnectionOpened");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception while raising ConnectionOpened.");
                throw;
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
                _logger.IfDebug()?.LogDebug("Raise ConnectionClosed");
                await _connectionClosed.AwaitEach(connection).CfAwait();
                _logger.IfDebug()?.LogDebug("Raised ConnectionClosed");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception while raising ConnectionClosed.");

                // the connection is going down, there is little we can do here
            }
        }

        #endregion

        #region Event Handlers

        private ValueTask OnStateChanged(ClientState state)
        {
            _logger.IfDebug()?.LogDebug("State changed: {State}", state);

            // only if disconnected
            if (state != ClientState.Disconnected) return default;

            // and still disconnected - if the cluster is down or shutting down, give up
            if (_clusterState.ClientState != ClientState.Disconnected)
            {
                _logger.LogInformation("Disconnected (shutting down)");
                return default;
            }

            // the cluster is disconnected, but not down
            var reconnect = _clusterState.Options.Networking.ReconnectMode == ReconnectMode.ReconnectAsync ||
                            _clusterState.Options.Networking.ReconnectMode == ReconnectMode.ReconnectSync;
            _logger.LogInformation("Disconnected (reconnect mode == {ReconnectMode} => {ReconnectAction})",
                _clusterState.Options.Networking.ReconnectMode,
                _clusterState.Options.Networking.ReconnectMode switch
                {
                    ReconnectMode.DoNotReconnect => "shut down",
                    ReconnectMode.ReconnectSync => "reconnect, synchronously",
                    ReconnectMode.ReconnectAsync => "reconnect, asynchronously",
                    _ => "meh?"
                });

            if (reconnect || _clusterState.Failover.Enabled)
            {
                // reconnect via a background task
                // operations will either retry until timeout, failover(if enabled) or fail
                _reconnect = BackgroundTask.Run(ReconnectAsync);
            }
            else
            {
                _clusterState.RequestShutdown();
            }

            return default;
        }

        /// <summary>
        /// Handles a <see cref="MemberConnection"/> going down.
        /// </summary>
        /// <param name="connection">The connection.</param>
        private async ValueTask OnConnectionClosed(MemberConnection connection)
        {
            _logger.If(LogLevel.Information)?.LogInformation("Connection {ConnectionId} to member {MemberId} at {Address} closed.", connection.Id.ToShortString(), connection.MemberId.ToShortString(), connection.Address);

            TaskCompletionSource<object> connectCompletion;
            lock (_mutex)
            {
                // if the connection was not added yet, ignore
                if (!_connections.TryGetValue(connection.MemberId, out var existing))
                {
                    _logger.IfDebug()?.LogDebug("Found no connection to member {MemberId}, ignore.", connection.MemberId.ToShortString());
                    return;
                }

                // must be matching, might have been replaced
                if (existing.Id == connection.Id)
                {
                    // else remove (safe, mutex)
                    _connections.TryRemove(connection.MemberId, out _);
                    _logger.IfDebug()?.LogDebug("Removed connection {ConnectionId} to member {MemberId} at {Address}.", connection.Id.ToShortString(), connection.MemberId.ToShortString(), connection.Address);
                }
                else
                {
                    _logger.IfDebug()?.LogDebug("Connection {ConnectionId} to member {MemberId} already replaced by {ExistingId)}.", connection.Id.ToShortString(), connection.MemberId.ToShortString(), existing.Id.ToShortString());
                }

                // and get its 'connect' completion source
                _completions.TryGetValue(connection, out connectCompletion);
            }

            // if still connecting... wait until done, because we cannot
            // eg trigger the 'closed' event before or while the 'opened'
            // triggers
            if (connectCompletion != null)
            {
                HConsole.WriteLine(this, "Must wait for connect completion...");
                await connectCompletion.Task.CfAwait();
                _completions.TryRemove(connection, out _);
            }

            // proceed: raise 'closed'
            HConsole.WriteLine(this, "Now raise connection 'closed'");
            await RaiseConnectionClosed(connection).CfAwait(); // does not throw
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the number of open connections to members.
        /// </summary>
        public int Count => _connections.Count;

        #endregion

        /// <summary>
        /// Connects to the cluster.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when connected.</returns>
        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            using var cancellation = _cancel.LinkedWith(cancellationToken);
            cancellationToken = cancellation.Token;

            // properties cannot be changed once connected
            _clusterState.SetPropertiesReadOnly();

            // we have started, and are now trying to connect
            if (!await _clusterState.ChangeStateAndWait(ClientState.Started, ClientState.Starting).CfAwait())
                throw new ConnectionException("Failed to connect (aborted).");

            bool tryNextCluster;
            do
            {
                tryNextCluster = false;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    HConsole.WriteLine(this, $"{_clusterState.ClientName} connecting");

                    var connected = await ConnectFirstAndWaitForConnectedAsync(cancellationToken).CfAwait();

                    HConsole.WriteLine(this, $"{_clusterState.ClientName} {(connected?"connected":"failed to connect")}");

                    if (!connected)
                        throw new ConnectionException("Failed to connect.");

                    // we have been connected (rejoice) - of course, nothing guarantees that it
                    // will last, but then OnConnectionClosed will deal with it
                }
                catch (Exception e) // could be ClientNotAllowedInClusterException
                {
                    // we *have* retried and failed
                    if (_clusterState.Failover.Enabled)
                    {
                        // try to failover to next cluster
                        if (_clusterState.Failover.TryNextCluster())
                        {
                            // ok to try the next cluster!
                            tryNextCluster = true;
                            _logger.LogWarning(e, "Failed to connect to cluster, trying next cluster.");
                        }
                        else
                        {
                            // this is hopeless, shutdown and throw (but log some details)
                            _clusterState.RequestShutdown();
                            _logger.LogWarning("Failed to connect to cluster, and exhausted failover options.");
                            throw;
                        }
                    }
                    else
                    {
                        // this is hopeless, shutdown and throw
                        _clusterState.RequestShutdown();
                        throw;
                    }
                }
            } while (tryNextCluster);
        }

        private async Task<bool> ConnectFirstAndWaitForConnectedAsync(CancellationToken cancellationToken)
        {
            // establishes the first connection, throws if it fails
            // this restarts the retry strategy ie re-initializes the timeout
            var connection = await ConnectFirstAsync(cancellationToken).CfAwait();

            // TODO: consider *not* waiting for this and running directly on the member we're connected to?

            // once the first connection is established, we should use it to subscribe
            // to the cluster views event, and then we should receive a members view,
            // which in turn should change the state to Connected - unless something
            // goes wrong

            // combine with retry strategy timeout cancellation token, we cannot wait forever
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _connectRetryStrategy.CancellationToken);
            var connected = await _clusterState.WaitForConnectedAsync(connection, cancellation.Token).CfAwait();

            if (!connected)
            {
                // make sure we clean things up
                await connection.DisposeAsync();
            }

            return connected;
        }

        /// <summary>
        /// Reconnects to the cluster.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when reconnected.</returns>
        private async Task ReconnectAsync(CancellationToken cancellationToken)
        {
            bool tryNextCluster;

            void HandleConnectionFailure(Exception e = null)
            {
                // we *have* retried and failed
                if (_clusterState.Failover.Enabled)
                {
                    // try to failover to next cluster
                    if (_clusterState.Failover.TryNextCluster())
                    {
                        // ok to try the next cluster!
                        tryNextCluster = true;
                        _logger.LogWarning(e, "Failed to connect to cluster, failover to next cluster.");
                    }
                    else
                    {
                        // this is hopeless, shutdown, and log (we are a background task!)
                        _clusterState.RequestShutdown();
                        _logger.LogError(e, "Failed to connect to cluster, and exhausted failover options.");
                    }
                }
                else
                {
                    // this is hopeless, shutdown, and log (we are a background task!)
                    _clusterState.RequestShutdown();
                    _logger.LogError(e, "Failed to reconnect.");
                }
            }

            do
            {
                tryNextCluster = false;
                try
                {
                    var connected = await ConnectFirstAndWaitForConnectedAsync(cancellationToken).CfAwait();

                    if (!connected)
                    {
                        // we are a background task and cannot throw!
                        HandleConnectionFailure();
                    }
                    else
                    {
                        _logger.IfDebug()?.LogDebug("Reconnected");

                        // we have been reconnected (rejoice) - of course, nothing guarantees that it
                        // will last, but then OnConnectionClosed will deal with it
                    }
                }
                catch (Exception e) // could be ClientNotAllowedInClusterException...
                {
                    HandleConnectionFailure(e);
                }
            } while (tryNextCluster);

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
            var addresses = _clusterMembers.GetMembersForConnection().Select(x => x.ConnectAddress);
            foreach (var address in Distinct(addresses, distinct, shuffle))
                yield return address;

            // get configured addresses that haven't been tried already
            // shuffle them if needed - but keep trying primary addresses before secondary addresses
            var (primary, secondary) = _clusterState.AddressProvider.GetAddresses();
            foreach (var address in Distinct(primary, distinct, shuffle))
                yield return address;
            foreach (var address in Distinct(secondary, distinct, shuffle))
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
        private async Task<MemberConnection> ConnectFirstAsync(CancellationToken cancellationToken)
        {
            var tried = new HashSet<NetworkAddress>();
            bool canRetry, isExceptionThrown = false;

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

                        HConsole.WriteLine(this, $"Try to connect {_clusterState.ClientName} to member at {address}");

                        _logger.IfDebug()?.LogDebug("Try to connect {ClientName} to cluster {ClusterName} member at {MemberAddress}", _clusterState.ClientName, _clusterState.ClusterName, address);

                        var attempt = await ConnectFirstAsync(address, cancellationToken).CfAwait(); // does not throw

                        if (attempt)
                        {
                            var connection = attempt.Value;
                            HConsole.WriteLine(this, $"Connected {_clusterState.ClientName} via {connection.Id.ToShortString()} to {connection.MemberId.ToShortString()} at {address}");
                            return connection; // successful exit, a first connection has been opened
                        }

                        HConsole.WriteLine(this, $"Failed to connect to address {address}");

                        if (attempt.HasException)
                        {
                            if (attempt.Exception is RemoteException { Error: RemoteError.HazelcastInstanceNotActive })
                            {

                                _logger.LogWarning($"Failed to connect to address {address} (member is not active).");
                            }
                            else if (attempt.Exception is TimeoutException)
                            {
                                _logger.LogWarning($"Failed to connect to address {address} (socket timeout).");
                            }
                            else if (attempt.Exception is ClientNotAllowedInClusterException)
                            {
                                isExceptionThrown = true;
                                _logger.IfWarning()?.LogWarning("Failed to connect to cluster since client is not allowed. " +
                                                                "Exception:{Exception}, Message: {Message}", nameof(ClientNotAllowedInClusterException), attempt.Exception.Message);
                                throw attempt.Exception; //no chance, give up
                            }
                            else if (attempt.Exception is InvalidPartitionGroupException)
                            {
                                isExceptionThrown = true;
                                _logger.IfWarning()?.LogWarning("Failed to connect to cluster in multi member routing without partition group. " +
                                                                "Exception:{Exception}, Message: {Message}", nameof(InvalidPartitionGroupException), attempt.Exception.Message);
                                throw attempt.Exception; //no chance, give up
                            }
                            else
                            {
                                isExceptionThrown = true;

                                _logger.LogError(attempt.Exception, message: "Failed to connect to address {Address}.", address.ToString());

                            }
                        }
                        else
                        {
                            _logger.IfWarning()?.LogWarning("Failed to connect to address {Address}.", address);
                        }
                    }
                }
                catch (InvalidPartitionGroupException)
                {
                    // Cannot connect to cluster in multi member routing without partition group.
                    // If failover is possible, it will try next cluster.
                    break;
                }
                catch (ClientNotAllowedInClusterException)
                {
                    // Cluster doesn't allow us, give up. If failover is possible it will take care of situation.
                    break;
                }
                catch (Exception e)
                {
                    // the GetClusterAddresses() enumerator itself can throw, if a configured
                    // address is invalid or cannot be resolved via DNS... a DNS problem may
                    // be transient: better retry

                    isExceptionThrown = true;
                    _logger.LogError(e, "Connection attempt failed due to possible DNS error.");

                    // TODO: it's the actual DNS that should retry!
                }

                // TODO: some errors should not be retried!
                // for instance, an invalid SSL cert will not become magically valid

                try
                {
                    // try to retry, maybe with a delay - handles cancellation
                    canRetry = await _connectRetryStrategy.WaitAsync(cancellationToken).CfAwait();
                }
                catch (OperationCanceledException) // don't gather the cancel exception
                {
                    canRetry = false; // retry strategy was canceled
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Connection attempt has thrown.");
                    canRetry = false; // retry strategy threw
                    isExceptionThrown = true;
                }

            } while (canRetry);

            string msgSomeThingWentWrong = "";

            if (isExceptionThrown)
                msgSomeThingWentWrong = "Some exceptions were thrown and have been written to the log. Please refer to the log for details.";

            // canceled exception?
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException($"The cluster connection operation to \"{_clusterState.ClusterName}\" has been canceled. " +
                    $"The following addresses where tried: {string.Join(", ", tried)}." +
                    $" {msgSomeThingWentWrong}");

            // other exception
            throw new ConnectionException($"Unable to connect to the cluster \"{_clusterState.ClusterName}\". " +
                $"The following addresses where tried: {string.Join(", ", tried)}." +
                $" {msgSomeThingWentWrong}");
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
#pragma warning disable CA2000 // Dispose objects before losing scope
                // "The allocating method does not have dispose ownership; that is, the responsibility
                // to dispose the object is transferred to another object or wrapper that's created
                // in the method and returned to the caller." - here: the Attempt<>.
                return await ConnectAsync(address, address, cancellationToken).CfAwait();
#pragma warning restore CA2000
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
            // if we already have a connection for that member, to the right address, return the connection if
            // it is active, or fail it if is not: cannot open yet another connection to that same address, we'll
            // have to wait for the inactive connection to be removed. OTOH if we have a connection for that
            // member to a wrong address, keep proceeding and try to open a connection to the right address.
            if (_connections.TryGetValue(member.Id, out var connection))
            {
                var active = connection.Active;

                if (_clusterMembers.IsMemberAddress(member, connection.Address))
                {
                    _logger.IfDebug()?.LogDebug("Found {PrefixActive}active connection {ConnectionId} from client {ClientName} to member {MemberId} at {Address}.", (active ? "" : "non-"), connection.Id.ToShortString(), _clusterState.ClientName, member.Id.ToShortString(), connection.Address);
                    return Attempt.If(active, connection);
                }

                _logger.IfDebug()?.LogDebug("Found {PrefixActive}active connection {ConnectionId} from client {ClientName} to member {MemberId} at {Address}, but member address is {ConnectAddress}.", (active ? "" : "non-"), connection.Id.ToShortString(), _clusterState.ClientName, member.Id.ToShortString(), connection.Address, member.ConnectAddress);
            }

            // ConnectMembers invokes EnsureConnectionAsync sequentially, and is suspended
            // whenever we need to connect the very first address, therefore each address
            // can only be connected once at a time = no need for locks here

            // exit now if canceled
            if (cancellationToken.IsCancellationRequested)
                return Attempt.Fail<MemberConnection>();

            try
            {
                // else actually connect
                // this may throw
                _logger.IfDebug()?.LogDebug("Client {ClientName} is not connected to member {MemberId} at {ConnectAddress}, connecting.", _clusterState.ClientName, member.Id.ToShortString(), member.ConnectAddress);

#pragma warning disable CA2000 // Dispose objects before losing scope - CA2000 does not understand CfAwait :(
                var memberConnection = await ConnectAsync(member.ConnectAddress, member.Address, cancellationToken).CfAwait();
#pragma warning restore CA2000
                if (memberConnection.MemberId != member.Id)
                {
                    _logger.IfWarning()?.LogWarning("Client {ClientName} connected address {ConnectAddress} expecting member {MemberId} but found member {MemberId}, dropping the connection.", _clusterState.ClientName, member.ConnectAddress, member.Id.ToShortString(), memberConnection.MemberId);
                    _clusterMembers.TerminateConnection(memberConnection);
                    return Attempt.Fail<MemberConnection>();
                }
                return memberConnection;
            }
            catch (Exception e)
            {
                // don't throw, just fail
                return Attempt.Fail<MemberConnection>(e);
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
        /// <param name="address">The address to connect to.</param>
        /// <param name="privateAddress">The member private address.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The connected member connection.</returns>
        /// <remarks>
        /// <para>The <paramref name="address"/> is the one we connect to, it can be the <paramref name="privateAddress"/>
        /// in case that one is directly reachable, or a public address in case some address mapping takes place.</para>
        /// </remarks>
        private async Task<MemberConnection> ConnectAsync(NetworkAddress address, NetworkAddress privateAddress, CancellationToken cancellationToken)
        {
            // directly connect to the specified address (internal/public determination happened beforehand)
            // we still need to pass the address provider in order to map the TPC ports
            var addressProvider = _clusterState.AddressProvider;

            // create the connection to the member
            var connection = new MemberConnection(address, privateAddress, _authenticator, _clusterState.Options.Messaging, _clusterState.CurrentClusterOptions.Networking, _clusterState.CurrentClusterOptions.Networking.Ssl, _clusterState.CorrelationIdSequence, _clusterState.LoggerFactory, _clusterState.ClientId, addressProvider)
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

            // we now that fail over is an ee feature.
            _clusterState.IsEnterprise = result.FailoverSupported;
            
            // if we are running a failover client but the cluster we just connected to does not support failover
            // then the client is not allowed in that cluster - in this case, terminate the connection and throw
            if (_clusterState.Options.FailoverOptions.Enabled && !result.FailoverSupported)
            {
                await connection.DisposeAsync().CfAwait();
                throw new ClientNotAllowedInClusterException("Client is not allowed in cluster " +
                    "(client is configured with failover but cluster does not support failover. " +
                    "Failover is an Hazelcast Enterprise feature.).");
            }
            
            // if we are running a multi member routing client but the cluster we just connected to does not support multi member routing
            if (result.MemberGroups.SelectedGroup.Count == 0 && _clusterState.Options.Networking.RoutingMode.Mode == RoutingModes.MultiMember)
                await ThrowInvalidPartitionGroup(connection).CfAwait();

            // report
            _logger.LogInformation("Authenticated client '{ClientName}' ({ClientId}) running version {ClientVersion}" +
                                   " on connection {ConnectionId} from {LocalAddress}" +
                                   " to member {MemberId} at {Address}" +
                                   " of cluster '{ClusterName}' ({ClusterId}) running version {HazelcastServerVersion}.",
                _clusterState.ClientName, _clusterState.ClientId.ToShortString(), ClientVersion.Version,
                connection.Id.ToShortString(), connection.LocalEndPoint,
                result.MemberId.ToShortString(), address,
                _clusterState.ClusterName, result.ClusterId.ToShortString(), result.ServerVersion);

            // notify partitioner
            if (!_clusterState.Partitioner.SetOrVerifyPartitionCount(result.PartitionCount))
            {
                await connection.DisposeAsync().CfAwait(); // does not throw
                throw new ClientNotAllowedInClusterException($"Received partition count value {result.PartitionCount} but expected {_clusterState.Partitioner.Count}.");
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
                    isFirst = _connections.IsEmpty;
                    isFirstEver = isFirst && _clusterId == default;
                    accepted = true;

                    // ok to connect to a different cluster only if this is the very first connection
                    isNewCluster = _clusterId != connection.ClusterId;
                    if (isNewCluster)
                    {
                        if (!_connections.IsEmpty)
                        {
                            _logger.IfWarning()?.LogWarning("Cannot accept a connection to cluster {ClusterId} which is not the current cluster ({CurrentClusterId}).", connection.ClusterId, _clusterId);
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
                    _logger.IfDebug()?.LogDebug("Added connection {ConnectionId} to member {MemberId} at {Address}.", connection.Id.ToShortString(), connection.MemberId.ToShortString(), connection.Address);
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

            void RemoveCompletion()
            {
                if (_completions.TryRemove(connection, out var completion)) completion.TrySetResult(null);
            }

            // Override all cp leader information if CP direct to leader is enabled
            // The design assumes each authentication could be a reconnection, so we need to update CP group information
            // Although client state changes can deal with that. 
            if(_clusterState.Options.Networking.CPDirectToLeaderEnabled)
                _clusterMembers.ClusterCPGroups.SetCPGroupIds(result.CPGroupLeaders);
            
            _clusterMembers.SubsetClusterMembers.SetSubsetMembers(result.MemberGroups);
            
            // connection is opened
            try
            {
                await RaiseConnectionOpened(connection, isFirstEver, isFirst, isNewCluster, result.ClusterVersion).CfAwait();
            }
            catch
            {
                // we cannot keep using this connection which has not been properly opened, tear it down
                // and rethrow - but before we dispose the connection, complete/remove its completion,
                // else dispose leads to OnConnectionClosed which would wait on the completion and thus
                // would hang - the completion is here to prevent the closed event from triggering before
                // the opened even, but only if we manage to open properly.
                RemoveCompletion();
                await connection.DisposeAsync().CfAwait();
                throw;
            }

            RemoveCompletion();
            return connection;
        }
        private async Task ThrowInvalidPartitionGroup(MemberConnection connection)
        {
            await connection.DisposeAsync().CfAwait();
            throw new InvalidPartitionGroupException("No member group is received from server as partition group." +
                                                     " This should not happen in multi-member routing mode. Make sure that server supports multi member routing.");
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            HConsole.WriteLine(this, "Terminate ConnectMembers");
            // be sure to properly terminate _connectMembers, even though, because the
            // MemberConnectionQueue has been disposed already, the task should have
            // ended by now
            _cancel.Cancel();
            if (_connectMembers != null)
            {
                try
                {
                    await _connectMembers.CfAwait(120_000); // give it 2 mins
                }
                catch (OperationCanceledException)
                { }
                catch (Exception e)
                {
                    _logger.IfWarning()?.LogWarning(e, "Caught exception when waiting for ConnectMembers to terminate.");
                }
            }
            _cancel.Dispose();

            // stop and dispose the reconnect task if it's running
            HConsole.WriteLine(this, "Terminate Reconnect");
            var reconnect = _reconnect;
            if (reconnect != null)
                await reconnect.CompletedOrCancelAsync(true).CfAwait();

            // trash all remaining connections
            HConsole.WriteLine(this, "Tear down Connections");
            ICollection<MemberConnection> connections;
            lock (_mutex) connections = _connections.Values;
            foreach (var connection in connections)
                await connection.DisposeAsync().CfAwait();

            _connectRetryStrategy.Dispose();
        }
    }
}
