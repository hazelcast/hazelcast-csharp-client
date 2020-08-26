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
        private readonly ClusterState _clusterState;
        private readonly ClusterEvents _clusterEvents;
        private readonly ClusterClusterEvents _clusterClusterEvents;
        private readonly ClusterMembers _clusterMembers;
        private readonly IAuthenticator _authenticator;
        private readonly ISerializationService _serializationService;

        private readonly Func<ValueTask> _terminateAsync;

        private readonly ILogger _logger;

        // address -> client
        // used for fast querying of _memberClients by network address
        private readonly ConcurrentDictionary<NetworkAddress, MemberConnection> _addressConnections = new ConcurrentDictionary<NetworkAddress, MemberConnection>();

        private Func<CancellationToken, ValueTask> _onFirstConnection;
        private Guid _clusterServerSideId; // the server-side identifier of the cluster

        private Task _clusterConnectTask; // the task that connects the first client of the cluster
        private Task _clusterMembersTask; // the task that connects clients for all members of the cluster

        private Func<CancellationToken, ValueTask> _onConnectionToNewCluster;

        public ClusterConnections(ClusterState clusterState, ClusterClusterEvents clusterClusterEvents, ClusterEvents clusterEvents, ClusterMembers clusterMembers, ISerializationService serializationService, Func<ValueTask> terminateAsync)
        {
            _clusterState = clusterState;
            _clusterClusterEvents = clusterClusterEvents;
            _clusterEvents = clusterEvents;
            _clusterMembers = clusterMembers;
            _serializationService = serializationService;
            _terminateAsync = terminateAsync;

            _logger = _clusterState.LoggerFactory.CreateLogger<ClusterConnections>();

            _authenticator = _clusterState.Options.Authentication.Authenticator.Service ??
                             new Authenticator(_clusterState.Options.Authentication);
        }

        /// <summary>
        /// Gets or sets an action that will be executed when the first connection is established.
        /// </summary>
        public Func<CancellationToken, ValueTask> OnFirstConnection
        {
            get => _onFirstConnection;
            set
            {
                _clusterState.ThrowIfReadOnlyProperties();
                _onFirstConnection = value;
            }
        }

        /// <summary>
        /// Gets or sets an action that will be executed when connecting to a new cluster.
        /// </summary>
        public Func<CancellationToken, ValueTask> OnConnectingToNewCluster
        {
            get => _onConnectionToNewCluster;
            set
            {
                _clusterState.ThrowIfReadOnlyProperties();
                _onConnectionToNewCluster = value;
            }
        }

        /// <summary>
        /// Connects to the server-side cluster.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when connected.</returns>
        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            using var cancellation = _clusterState.GetLinkedCancellation(cancellationToken, false);
            cancellationToken = cancellation.Token;

            _clusterState.MarkPropertiesReadOnly();

            using (await _clusterState.ClusterLock.AcquireAsync(CancellationToken.None).CAF())
            {
                if (_clusterState.ConnectionState != ClusterConnectionState.NotConnected)
                    throw new InvalidOperationException("Cluster has already been connected.");
                _clusterState.ConnectionState = ClusterConnectionState.Connecting;
            }

            try
            {
                // establishes the first connection, throws if it fails
                await ConnectFirstAsync(cancellationToken).CAF();

                // wait for the member table
                await _clusterMembers.WaitForFirstMemberViewEventAsync(cancellationToken).CAF();

                // start connecting members
                // this is a background task that cancels with the cluster
                _clusterMembersTask = EnsureMemberConnectionsAsync(_clusterState.CancellationToken);

                // execute subscribers
                await OnFirstConnection(cancellationToken).CAF();
            }
            catch
            {
                await _terminateAsync().CAF();
                throw;
            }
        }

        /// <summary>
        /// Starts <see cref="ConnectFirstAsync"/> in a background task.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// <para>This method is used to reconnect a cluster that has been disconnected.</para>
        /// </remarks>
        private void StartReconnectWithLock(CancellationToken cancellationToken)
        {
            _clusterConnectTask ??= ConnectFirstAsync(cancellationToken).ContinueWith(async x =>
            {
                if (x.IsFaulted)
                {
                    await _terminateAsync().CAF();
                }
            }, cancellationToken, TaskContinuationOptions.None, TaskScheduler.Current);
        }

        /// <summary>
        /// Establishes a first connection to the server-side cluster.
        /// </summary>
        /// <returns>A task that will complete when connected.</returns>
        private async Task ConnectFirstAsync(CancellationToken cancellationToken)
        {
            var tried = new HashSet<NetworkAddress>();
            var retryStrategy = new RetryStrategy("connect to cluster", _clusterState.Options.Networking.ConnectionRetry, _clusterState.LoggerFactory);
            List<Exception> exceptions = null;
            bool canRetry;

            do
            {
                // gets unique addresses (by the IPEndPoint)
                var addresses = _clusterMembers.GetCandidateAddresses();
                foreach (var address in addresses)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    tried.Add(address);
                    var attempt = await TryConnectAsync(address, cancellationToken).CAF();
                    if (attempt)
                    {
                        // avoid race conditions, this task is going to end, and if the
                        // cluster disconnects we want to be sure we restart the task
                        _clusterConnectTask = null;

                        return; // successful exit
                    }

                    if (attempt.HasException)
                    {
                        exceptions ??= new List<Exception>();
                        exceptions.Add(attempt.Exception);
                    }
                }

                try
                {
                    // try to retry, maybe with a delay - handles cancellation
                    canRetry = await retryStrategy.WaitAsync(cancellationToken).CAF();
                }
                catch (Exception e)
                {
                    exceptions ??= new List<Exception>();
                    exceptions.Add(e);
                    canRetry = false;
                }

            } while (canRetry);

            var aggregate = new AggregateException(exceptions);

            // throw the right exception
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException($"The cluster connection operation to \"{_clusterState.ClusterName}\" has been canceled. " +
                    $"The following addresses where tried: {string.Join(", ", tried)}.", aggregate);

            throw new InvalidOperationException($"Unable to connect to the cluster \"{_clusterState.ClusterName}\". " +
                $"The following addresses where tried: {string.Join(", ", tried)}.", aggregate);
        }

        /// <summary>
        /// Ensures that a connection is established to all members.
        /// </summary>
        /// <remarks>
        /// <para>Runs as a background task until the cluster is terminated.</para>
        /// </remarks>
        private async Task EnsureMemberConnectionsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken).CAF();

                foreach (var member in _clusterMembers.SnapshotMembers())
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    await TryConnectAsync(member.Address, cancellationToken).CAF(); // does not throw

                    // ignore result
                    // TryConnectAsync does add the connection to ...
                }
            }
        }

        /// <summary>
        /// Tries to get, or open, a connection to an address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the connection has been retrieved or established,
        /// and represents an attempt representing the client.</returns>
        /// <remarks>
        /// <para>This method does not throw but returns a failed attempt.</para>
        /// </remarks>
        private async Task<Attempt<MemberConnection>> TryConnectAsync(NetworkAddress address, CancellationToken cancellationToken)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            if (_addressConnections.TryGetValue(address, out var clientConnection))
            {
                // if we already have a client for that address, return the client
                // if it is active, or fail if it is not - cannot open yet another
                // client to that same address
                if (clientConnection.Active) return clientConnection;
                return Attempt.Failed;
            }

            // ensure we only connect to an endpoint once at a time
            // addresses are unique by their IPEndPoint as a NetworkAddress hash
            // is in fact its IPEndPoint hash - so we can lock the address
            // itself to achieve what we want (no two addresses can point to the
            // same endpoint).
            //
            // we do not *wait* for the lock, either we can have it or not - this
            // is the only place where an address is locked, so if it's locked
            // some other code is already trying to connect to it and there is no
            // point waiting to try too - faster to just fail immediately

            using var acquired = await address.Lock.TryAcquireAsync().CAF();
            if (!acquired) return Attempt.Failed;

            try
            {
                // ReSharper disable once InconsistentlySynchronizedField
                if (_addressConnections.TryGetValue(address, out clientConnection))
                {
                    // if we already have a client for that address, return the client
                    // if it is active, or fail if it is not - cannot open yet another
                    // client to that same address
                    if (clientConnection.Active) return clientConnection;
                    return Attempt.Failed;
                }

                // else actually connect - this may throw
#pragma warning disable CA2000 // Dispose objects before losing scope - will be disposed, eventually
                clientConnection = await ConnectWithLockAsync(address, cancellationToken).CAF();
#pragma warning restore CA2000
                return clientConnection;
            }
            catch (Exception e)
            {
                return Attempt.Fail<MemberConnection>(e);
            }
        }

        /// <summary>
        /// Opens a connection to an address, while being locked.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the connection has been established, and represents the associated client.</returns>
        private async Task<MemberConnection> ConnectWithLockAsync(NetworkAddress address, CancellationToken cancellationToken)
        {
            // map private address to public address
            address = _clusterMembers.MapAddress(address);

            // create the connection to the member
            var connection = new MemberConnection(address, _clusterState.Options.Messaging, _clusterState.Options.Networking.Socket, _clusterState.CorrelationIdSequence, _clusterState.LoggerFactory)
            {
                OnReceiveEventMessage = _clusterEvents.OnEventMessage,
                OnShutdown = HandleConnectionTermination
            };

            // connect to the server (may throw)
            await connection.ConnectAsync(cancellationToken).CAF();
            cancellationToken.ThrowIfCancellationRequested();

            // authenticate (may throw)
            var info = await _authenticator
                .AuthenticateAsync(connection, _clusterState.ClusterName, _clusterState.ClientId, _clusterState.ClientName, _clusterState.Options.Labels, _serializationService, cancellationToken)
                .CAF();
            if (info == null) throw new HazelcastException("Failed to authenticate");
            connection.NotifyAuthenticated(info);

            _logger.LogInformation("Authenticated client \"{ClientName}\" ({ClientId})" +
                                   " with cluster \"{ClusterName}\" member {MemberId}" +
                                   " running version {HazelcastServerVersion}" +
                                   " at {RemoteAddress} via {LocalAddress}.",
                _clusterState.ClientName, _clusterState.ClientId.ToString("N").Substring(0, 7),
                _clusterState.ClusterName, info.MemberId.ToString("N").Substring(0, 7),
                info.ServerVersion, info.MemberAddress, connection.LocalEndPoint);

            // notify partitioner (may throw)
            _clusterState.Partitioner.NotifyPartitionsCount(info.PartitionCount);

            // register & prepare the client
            using (await _clusterState.ClusterLock.AcquireAsync(CancellationToken.None).CAF())
            {
                // if client is not active anymore, we can't continue - there is no
                // race condition here because the client shutdown handler also lock
                // on _clusterStateLock
                if (!connection.Active)
                    throw new HazelcastException("Client is not active.");

                var isFirst = _clusterMembers.NotifyNewConnection(info.MemberId, connection);

                _addressConnections[address] = connection;

                // cluster becomes connected and can send messages
                _clusterState.ConnectionState = ClusterConnectionState.Connected;

                var otherCluster = _clusterServerSideId != default && _clusterServerSideId != info.ClusterId;
                var newCluster = isFirst && otherCluster;
                _clusterServerSideId = info.ClusterId;

                // even the Java code does not deal with this situation - seems to assume that a 'new
                // cluster' can only happen when connecting to the first client - what happens during
                // splits?
                if (otherCluster && !newCluster)
                    _logger.LogWarning("Connected to another cluster, ignoring.");

                if (newCluster)
                {
                    // we did connect to a cluster once, and then we lost all clients,
                    // and now we have a new client, which is connected to a different
                    // cluster identifier

                    _logger.LogWarning("Switching from current cluster {CurrentClusterId} to new cluster {NewClusterId}.", _clusterServerSideId, info.ClusterId);

                    // clear members list
                    _clusterMembers.NotifyNewCluster();

                    // get distributed object factory to re-create objects, etc
                    await _onConnectionToNewCluster(_clusterState.CancellationToken).CAF();
                }

                _clusterEvents.NotifyConnectionEstablished(connection);

                await _clusterClusterEvents.OnConnectionAdded(/*client*/).CAF(); // does not throw

                if (isFirst)
                    await _clusterClusterEvents.OnClientLifecycleEvent(ClientLifecycleState.Connected).CAF(); // does not throw
            }

            return connection;
        }

        private async ValueTask HandleConnectionTermination(MemberConnection connection)
        {
            // this runs when a client signals that it is shutting down

            var terminate = false;

            using (await _clusterState.ClusterLock.AcquireAsync(CancellationToken.None).CAF())
            {
                _addressConnections.TryRemove(connection.Address, out _);

                var wasLast = _clusterMembers.NotifyTerminatedConnection(connection);

                if (wasLast)
                    await _clusterClusterEvents.OnClientLifecycleEvent(ClientLifecycleState.Disconnected).CAF(); // does not throw

                await _clusterClusterEvents.OnConnectionRemoved(/*client*/).CAF(); // does not throw

                _clusterEvents.NotifyConnectionTerminated(connection, wasLast);

                if (!wasLast)
                    return;

                _logger.LogInformation("Disconnected (reconnect mode: {ReconnectMode}, {ReconnectAction})",
                    _clusterState.Options.Networking.ReconnectMode,
                    _clusterState.Options.Networking.ReconnectMode switch
                    {
                        ReconnectMode.DoNotReconnect => "remain disconnected",
                        ReconnectMode.ReconnectSync => "not supported -> trying to reconnect asynchronously",
                        ReconnectMode.ReconnectAsync => "trying to reconnect asynchronously",
                        _ => "Meh?"
                    });

                switch (_clusterState.Options.Networking.ReconnectMode)
                {
                    case ReconnectMode.DoNotReconnect:
                        _clusterState.ConnectionState = ClusterConnectionState.Disconnected;
                        terminate = true;
                        break;

                    // was never supported by the CSharp client, really - fallback to async
                    case ReconnectMode.ReconnectSync: // TODO: implement ReconnectSync

                    case ReconnectMode.ReconnectAsync:
                        _clusterState.ConnectionState = ClusterConnectionState.Connecting;
                        StartReconnectWithLock(_clusterState.CancellationToken);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            if (terminate)
            {
                await _terminateAsync().CAF();
            }
        }

        public async ValueTask DisposeAsync()
        {
            // FIXME: should we capture the tasks with a lock?
            await TaskEx.AwaitCanceled(_clusterMembersTask).CAF();
            await TaskEx.AwaitCanceled(_clusterConnectTask).CAF();
        }
    }
}
