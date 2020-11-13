// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Events;
using Hazelcast.Exceptions;
using Hazelcast.Networking;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    internal class ClusterConnections : IAsyncDisposable
    {
        private readonly AddressLocker _addressLocker = new AddressLocker();
        private readonly SemaphoreSlim _onClosedMutex = new SemaphoreSlim(1);

        private readonly ClusterState _clusterState;
        private readonly ClusterMembers _clusterMembers;
        private readonly Authenticator _authenticator;
        private readonly AddressProvider _addressProvider;
        private readonly IRetryStrategy _connectRetryStrategy;
        private readonly ILogger _logger;

        // address -> connection, used for fast querying of connections by network address
        private readonly ConcurrentDictionary<NetworkAddress, MemberConnection> _addressConnections = new ConcurrentDictionary<NetworkAddress, MemberConnection>();

        private readonly bool _smartRouting;
        private readonly ConnectAddresses _connectAddresses;

        private Action<MemberConnection> _connectionCreated;
        private Func<MemberConnection, bool, bool, ValueTask> _connectionOpened;
        private Func<MemberConnection, bool, ValueTask> _connectionClosed;
        private BackgroundTask _reconnect;
        private Guid _clusterId; // the server-side identifier of the cluster

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterConnections"/> class.
        /// </summary>
        public ClusterConnections(ClusterState clusterState, ClusterMembers clusterMembers, ISerializationService serializationService)
        {
            _clusterState = clusterState;
            _clusterMembers = clusterMembers;

            _logger = _clusterState.LoggerFactory.CreateLogger<ClusterConnections>();
            _authenticator = new Authenticator(_clusterState.Options.Authentication, serializationService);
            _addressProvider = new AddressProvider(_clusterState.Options.Networking, _clusterState.LoggerFactory);
            _connectRetryStrategy = new RetryStrategy("connect to cluster", _clusterState.Options.Networking.ConnectionRetry, _clusterState.LoggerFactory);

            if (_clusterState.IsSmartRouting)
            {
                _smartRouting = true;
                _connectAddresses = new ConnectAddresses(GetOrConnectAsync, _clusterState.LoggerFactory);
            }
        }

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
        public Func<MemberConnection, bool, bool, ValueTask> ConnectionOpened
        {
            get => _connectionOpened;
            set
            {
                _clusterState.ThrowIfPropertiesAreReadOnly();
                _connectionOpened = value;
            }
        }

        private async ValueTask RaiseConnectionOpened(MemberConnection connection, bool isFirst, bool isNewCluster)
        {
            if (_connectionOpened == null) return;

            try
            {
                await _connectionOpened.AwaitEach(connection, isFirst, isNewCluster).CAF();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception while raising ConnectionOpened.");
            }
        }

        /// <summary>
        /// Gets or sets an action that will be executed when a connection is closed.
        /// </summary>
        public Func<MemberConnection, bool, ValueTask> ConnectionClosed
        {
            get => _connectionClosed;
            set
            {
                _clusterState.ThrowIfPropertiesAreReadOnly();
                _connectionClosed = value;
            }
        }

        private async ValueTask RaiseConnectionClosed(MemberConnection connection, bool wasLast)
        {
            if (_connectionClosed == null) return;

            try
            {
                await _connectionClosed.AwaitEach(connection, wasLast).CAF();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Caught exception while raising ConnectionClosed.");
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles members being updated.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        public ValueTask OnMembersUpdated(MembersUpdatedEventArgs args)
        {
            // add new members to the connect queue
            foreach (var addedMember in args.AddedMembers)
                _connectAddresses.Add(addedMember.Address);

            return default;
        }

        /// <summary>
        /// Handles a <see cref="MemberConnection"/> going down.
        /// </summary>
        /// <param name="connection">The connection.</param>
        private async ValueTask OnConnectionClosed(MemberConnection connection)
        {
            // handle them one at a time
            await _onClosedMutex.WaitAsync().CAF();
            try
            {
                // not a connection anymore
                // if it was not a connection yet, we can return
                lock (_addressConnections)
                    if (!_addressConnections.TryRemove(connection.Address, out _))
                        return;

                // pause connecting members, so we can figure out whether that one was
                // the last remaining connection, in a reliable way
                if (_smartRouting)
                    await _connectAddresses.PauseAsync().CAF();

                // --- thread safety ---
                // connectAddresses is paused, and we have _onClosedMutex
                // which means that no other connection can be added/removed at that point

                // notify members
                var wasLast = _clusterMembers.NotifyConnectionClosed(connection);

                // report that the connection has been closed
                await RaiseConnectionClosed(connection, wasLast).CAF(); // does not throw

                if (!wasLast) // then we must be smart routing
                {
                    // queue the member so it is reconnected, if it is still a member
                    var member = _clusterMembers.GetMember(connection.MemberId);
                    if (member != null) _connectAddresses.Add(member.Address);
                }

                // resume, drain queue if that was the last connection
                if (_smartRouting)
                    await _connectAddresses.ResumeAsync(wasLast).CAF();

                // if there are remaining connections, we can still talk to the cluster
                if (wasLast) return;
            }
            finally
            {
                _onClosedMutex.Release();
            }

            // otherwise, we have lost the last connection

            // --- thread safety ---
            // although we have released _onClosedMutex, connectAddresses' queue is
            // empty and there are no connections left, which means that no other
            // connection can be added/removed at that point

            // the cluster is now disconnected
            _logger.LogInformation("Disconnected (reconnect mode: {ReconnectMode}, {ReconnectAction})",
                _clusterState.Options.Networking.ReconnectMode,
                _clusterState.Options.Networking.ReconnectMode switch
                {
                    ReconnectMode.DoNotReconnect => "remain disconnected",
                    ReconnectMode.ReconnectSync => "not supported -> trying to reconnect asynchronously",
                    ReconnectMode.ReconnectAsync => "trying to reconnect asynchronously",
                    _ => "Meh?"
                });

            // FIXME what-if we've disposed and are not connected anymore or should not reconnect?

            // what we do next depends on options
            switch (_clusterState.Options.Networking.ReconnectMode)
            {
                case ReconnectMode.DoNotReconnect:
                    // DoNotReconnect = the cluster remains unconnected
                    await _clusterState.TransitionAsync(ConnectionState.NotConnected).CAF();
                    break;

                case ReconnectMode.ReconnectSync: // TODO: implement ReconnectSync?
                    // ReconnectSync = the cluster reconnects
                    // operations are blocked (?) until the cluster is reconnected
                    // was never supported by the CSharp client
                    //_clusterState.OperationsShouldWaitIfReconnecting = true;

                case ReconnectMode.ReconnectAsync:
                    // ReconnectAsync = the cluster reconnects via a background task
                    // operations can still be performed but will fail
                    await _clusterState.TransitionAsync(ConnectionState.Reconnecting).CAF();
                    _reconnect = BackgroundTask.Run(ReconnectAsync);
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        #endregion

        /// <summary>
        /// Connects to the cluster.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when connected.</returns>
        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            // FIXME cancellations?
            using var cancellation = _clusterState.GetLinkedCancellation(cancellationToken, false);
            cancellationToken = cancellation.Token;

            // properties cannot be changed once connected
            _clusterState.SetPropertiesReadOnly();

            await _clusterState.TransitionAsync(ConnectionState.Connecting, ConnectionState.NotConnected).CAF();

            try
            {
                // establishes the first connection, throws if it fails
                await ConnectFirstAsync(cancellationToken).CAF();

                // wait for the members table
                await _clusterMembers.WaitForMembersAsync(cancellationToken).CAF();

                // and now we have been connected (rejoice)
                // though nothing guarantees we still are, or will remain for long
                // but OnConnectionClosed will deal with it
            }
            catch
            {
                // we *have* retried and failed
                await _clusterState.TransitionAsync(ConnectionState.NotConnected).CAF();
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
            await _clusterState.TransitionAsync(ConnectionState.Reconnecting, ConnectionState.Connected).CAF();

            try
            {
                // establishes the first connection, throws if it fails
                await ConnectFirstAsync(cancellationToken).CAF();

                // FIXME reconnect, shall we wait for the first member view event?
                // else we are just hoping that we haven't switched to a new
                // cluster and that this connection matches a member?

                // and now we have been re-connected (rejoice)
                // though nothing guarantees we still are, or will remain for long
                // but OnConnectionClosed will deal with it
            }
            catch (Exception e)
            {
                // we *have* retried and failed
                await _clusterState.TransitionAsync(ConnectionState.NotConnected).CAF();

                // we are a background task and cannot throw!
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
            var addresses = _clusterMembers.GetAddresses();
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
                // try each address (unique by the IPEndPoint)
                foreach (var address in GetClusterAddresses())
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    tried.Add(address);

                    var attempt = await ConnectFirstAsync(address, cancellationToken).CAF(); // does not throw
                    if (attempt)
                        return; // successful exit, a first connection has been opened

                    if (attempt.HasException) // else gather exceptions
                    {
                        exceptions ??= new List<Exception>();
                        exceptions.Add(attempt.Exception);
                    }
                }

                try
                {
                    // try to retry, maybe with a delay - handles cancellation
                    canRetry = await _connectRetryStrategy.WaitAsync(cancellationToken).CAF();
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
            throw new InvalidOperationException($"Unable to connect to the cluster \"{_clusterState.ClusterName}\". " +
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
            // TODO: pointless for a first connection, nothing else is connecting?
            using var locked = _addressLocker.LockAsync(address);

            try
            {
                // this may throw
                return await ConnectAsync(address, cancellationToken).CAF();
            }
            catch (Exception e)
            {
                // don't throw, just fail
                return Attempt.Fail<MemberConnection>(e);
            }
        }

        /// <summary>
        /// Gets or opens a connection to an address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The connection, if successful.</returns>
        /// <remarks>
        /// <para>This method does not throw.</para>
        /// </remarks>
        private async Task<Attempt<MemberConnection>> GetOrConnectAsync(NetworkAddress address, CancellationToken cancellationToken)
        {
            // if we already have a client for that address, return the client
            // if it is active, or fail if it is not - cannot open yet another
            // client to that same address.
            if (_addressConnections.TryGetValue(address, out var clientConnection))
                return Attempt.If(clientConnection.Active, clientConnection);

            // lock the address - can only connect once at a time per address
            using var locked = _addressLocker.LockAsync(address);

            // exit now if canceled
            if (cancellationToken.IsCancellationRequested)
                return Attempt.Fail<MemberConnection>();

            // test again - maybe the address has been reconnected while we waited for the lock
            if (_addressConnections.TryGetValue(address, out clientConnection))
                return Attempt.If(clientConnection.Active, clientConnection);

            // else actually connect
            try
            {
                // this may throw
#pragma warning disable CA2000 // Dispose objects before losing scope - returned
                return await ConnectAsync(address, cancellationToken).CAF();
#pragma warning restore CA2000
            }
            catch (Exception e)
            {
                // don't throw, just fail
                return Attempt.Fail<MemberConnection>(e);
            }
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
            var connection = new MemberConnection(address, _authenticator, _clusterState.Options.Messaging, _clusterState.Options.Networking.Socket, _clusterState.Options.Networking.Ssl, _clusterState.ConnectionIdSequence, _clusterState.CorrelationIdSequence, _clusterState.LoggerFactory)
            {
                Closed = OnConnectionClosed
            };

            RaiseConnectionCreated(connection);

            // connect to the server (may throw and that is ok here)
            // note: soon as this returns, the connection can close anytime
            var result = await connection.ConnectAsync(_clusterState, cancellationToken).CAF();

            // fail fast
            if (cancellationToken.IsCancellationRequested)
            {
                await connection.DisposeAsync().CAF(); // does not throw
                cancellationToken.ThrowIfCancellationRequested(); // throws
            }

            // notify partitioner
            try
            {
                _clusterState.Partitioner.NotifyPartitionsCount(result.PartitionCount);
            }
            catch (Exception e)
            {
                await connection.DisposeAsync().CAF(); // does not throw
                throw new ConnectionException("Failed to open a connection because " +
                                              "the partitions count announced by the member in invalid.", e);
            }

            // report
            _logger.LogInformation("Authenticated client \"{ClientName}\" connection {ClientId}" +
                                   " with cluster \"{ClusterName}\" member {MemberId}" +
                                   " running version {HazelcastServerVersion}" +
                                   " at {RemoteAddress} via {LocalAddress}.",
                _clusterState.ClientName, _clusterState.ClientId.ToString("N").Substring(0, 7),
                _clusterState.ClusterName, result.MemberId.ToString("N").Substring(0, 7),
                result.ServerVersion, result.MemberAddress, connection.LocalEndPoint);

            // register the connection
            lock (_addressConnections)
            {
                _addressConnections[address] = connection;
                if (!connection.Active)
                {
                    _addressConnections.TryRemove(address, out _);
                    throw new ConnectionException("Connection closed immediately.");
                }
            }

            // from now on, if the connection closes, it will be taken care of by
            // OnConnectionClosed - and since that one pauses and waits for connections,
            // the code below will all execute *before* OnConnectionClosed handles
            // this connection

            var isFirst = _clusterMembers.NotifyConnectionOpened(connection);

            var isNewCluster = false;
            if (_clusterId == default)
            {
                _clusterId = connection.ClusterId; // first cluster
            }
            else if (_clusterId != connection.ClusterId)
            {
                // see TcpClientConnectionManager java class handleSuccessfulAuth method
                // does not even consider the cluster identifier when !isFirst
                if (isFirst)
                {
                    _logger.LogWarning("Switching from current cluster {CurrentClusterId} to new cluster {NewClusterId}.", _clusterId, connection.ClusterId);
                    _clusterId = connection.ClusterId; // new cluster
                    isNewCluster = true;
                }
            }

            // now connected
            if (isFirst)
                await _clusterState.TransitionAsync(ConnectionState.Connected).CAF();

            // *after* being connected, else handles might do things too soon?
            // FIXME or, should they listened to something else?
            //  would like to first raise 'connection opened', then raise 'cluster connected'
            //  but 'connection opened' is going to trigger some operations on the cluster,
            //  that need the cluster to be up and running already = ?
            await RaiseConnectionOpened(connection, isFirst, isNewCluster).CAF();

            return connection;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            // stop and dispose the background task that connects members
            if (_clusterState.IsSmartRouting)
                await _connectAddresses.DisposeAsync().CAF(); // does not throw

            await _reconnect.CompletedOrCancelAsync(true).CAF();

            _onClosedMutex.Dispose();
        }
    }
}
