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
        private readonly Authenticator _authenticator;
        private readonly SerializationService _serializationService;

        private readonly Func<ValueTask> _terminateAsync;

        private readonly ILogger _logger;

        private readonly AddressLocker _addressLocker = new AddressLocker();

        // address -> client
        // used for fast querying of _memberClients by network address
        private readonly ConcurrentDictionary<NetworkAddress, MemberConnection> _addressConnections = new ConcurrentDictionary<NetworkAddress, MemberConnection>();

        private Func<CancellationToken, ValueTask> _onFirstConnection;
        private Func<CancellationToken, ValueTask> _onConnectionToNewCluster;

        private Guid _clusterServerSideId; // the server-side identifier of the cluster

        private Task _clusterConnectTask; // the task that connects the first client of the cluster
        private Task _clusterMembersTask; // the task that connects clients for all members of the cluster
        private CancellationTokenSource _clusterMembersCancel; // cancellation for _clusterMemberTask

        public ClusterConnections(ClusterState clusterState, ClusterClusterEvents clusterClusterEvents, ClusterEvents clusterEvents, ClusterMembers clusterMembers, SerializationService serializationService, Func<ValueTask> terminateAsync)
        {
            _clusterState = clusterState;
            _clusterClusterEvents = clusterClusterEvents;
            _clusterEvents = clusterEvents;
            _clusterMembers = clusterMembers;
            _serializationService = serializationService;
            _terminateAsync = terminateAsync;

            _logger = _clusterState.LoggerFactory.CreateLogger<ClusterConnections>();

            _authenticator = new Authenticator(_clusterState.Options.Authentication);
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
        /// Connects to the cluster.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when connected.</returns>
        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            using var cancellation = _clusterState.GetLinkedCancellation(cancellationToken, false);
            cancellationToken = cancellation.Token;

            // properties cannot be changed once connected
            _clusterState.MarkPropertiesReadOnly();

            // FIXME understand the lock
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

                // if we are smart-routing, start the background tasks that ensures that
                // we have one connection per member (else, there is only one connection)
                if (_clusterState.Options.Networking.SmartRouting)
                    _clusterMembersTask = EnsureMemberConnectionsAsync(_clusterState.CancellationToken);

                // run first connection callback (deals with subscribers...)
                await _onFirstConnection(cancellationToken).CAF();
            }
            catch
            {
                // we *have* retried and failed, now terminate
                await _terminateAsync().CAF();
                _clusterState.ConnectionState = ClusterConnectionState.NotConnected;
                throw;
            }
        }

        /// <summary>
        /// Establishes a first connection to the server-side cluster.
        /// </summary>
        /// <returns>A task that will complete when connected.</returns>
        /// <remarks>
        /// <para>Tries all the candidate addresses until one works; tries again
        /// according to the configured retry strategy, and if nothing works,
        /// end up throwing an exception.</para>
        /// </remarks>
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
                    var attempt = await GetOrConnectAsync(address, cancellationToken).CAF();
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
                // TODO: 1s delay should be configurable / not hard-coded
                await Task.Delay(1000, cancellationToken).CAF();

                // TODO: consider connecting to members in parallel

                foreach (var member in _clusterMembers.SnapshotMembers())
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    await GetOrConnectAsync(member.Address, cancellationToken).CAF(); // does not throw

                    // ignore result
                    // TryConnectAsync does add the connection to ...
                }
            }
        }

        /// <summary>
        /// Gets, or opens, a connection to an address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the connection has been retrieved or established,
        /// and represents an attempt representing the client.</returns>
        /// <remarks>
        /// <para>This method does not throw but may return a failed attempt.</para>
        /// </remarks>
        private async Task<Attempt<MemberConnection>> GetOrConnectAsync(NetworkAddress address, CancellationToken cancellationToken)
        {
            // if we already have a client for that address, return the client
            // if it is active, or fail if it is not - cannot open yet another
            // client to that same address
            // ReSharper disable once InconsistentlySynchronizedField
            if (_addressConnections.TryGetValue(address, out var clientConnection))
                return Attempt.If(clientConnection.Active, clientConnection);

            // lock the address - can only connect once at a time per address
            using var locked = _addressLocker.LockAsync(address);

            // exit now if canceled
            if (cancellationToken.IsCancellationRequested)
                return Attempt.Fail<MemberConnection>();

            // test again - maybe the address has been reconnected
            // ReSharper disable once InconsistentlySynchronizedField
            if (_addressConnections.TryGetValue(address, out clientConnection))
                return Attempt.If(clientConnection.Active, clientConnection);

            // else actually connect
            try
            {
                // this may throw
#pragma warning disable CA2000 // Dispose objects before losing scope - will be disposed, eventually
                return await ConnectAsync(address, cancellationToken).CAF();
#pragma warning restore CA2000
            }
            catch (Exception e)
            {
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
            address = _clusterMembers.MapAddress(address);

            // create the connection to the member
            var connection = new MemberConnection(address, _clusterState.Options.Messaging, _clusterState.Options.Networking.Socket, _clusterState.Options.Networking.Ssl, _clusterState.ConnectionIdSequence, _clusterState.CorrelationIdSequence, _clusterState.LoggerFactory)
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

                await _clusterClusterEvents.OnConnectionAdded().CAF(); // does not throw

                if (isFirst)
                    await _clusterClusterEvents.OnClientLifecycleEvent(ClientLifecycleState.Connected).CAF(); // does not throw
            }

            return connection;
        }

        /// <summary>
        /// Deals with a <see cref="MemberConnection"/> going down.
        /// </summary>
        /// <param name="connection">The terminated connection.</param>
        private async ValueTask HandleConnectionTermination(MemberConnection connection)
        {
            var terminate = false;

            // FIXME explain lock
            using (await _clusterState.ClusterLock.AcquireAsync(CancellationToken.None).CAF())
            {
                // not a connection anymore
                _addressConnections.TryRemove(connection.Address, out _);

                // notify members + figure out whether that was the last remaining connection
                var wasLast = _clusterMembers.NotifyTerminatedConnection(connection);

                // FIXME need to reorder things here

                if (wasLast)
                    await _clusterClusterEvents.OnClientLifecycleEvent(ClientLifecycleState.Disconnected).CAF(); // does not throw

                await _clusterClusterEvents.OnConnectionRemoved(/*client*/).CAF(); // does not throw

                _clusterEvents.NotifyConnectionTerminated(connection, wasLast);

                // if there are remaining connections, the background members task will
                // deal with the situation and reconnect whatever needs to be reconnected,
                // but we can still talk to the cluster - otherwise, we are disconnected
                if (!wasLast)
                    return;

                // shutdown the background members task
                _clusterMembersCancel.Cancel();
                await _clusterMembersTask.ObserveCanceled().CAF();

                // look for race conditions - can the background member task restore ?!
                // FIXME what happens when the above task is canceled? DANGER!
                // connecting cannot be aborted randomly, that's dangerous!
                // FIXME can the above task have created another connection in the meantime?

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

                // decide what to do next
                switch (_clusterState.Options.Networking.ReconnectMode)
                {
                    case ReconnectMode.DoNotReconnect:
                        // DoNotReconnect = the cluster becomes & remains Disconnected
                        _clusterState.ConnectionState = ClusterConnectionState.Disconnected;
                        terminate = true;
                        break;

                    // idea here was to suspend (queue?) all calls until reconnected, it
                    // was never supported by the CSharp client, really - fallback to async
                    case ReconnectMode.ReconnectSync: // TODO: implement ReconnectSync?

                    case ReconnectMode.ReconnectAsync:
                        // ReconnectAsync = the cluster becomes Connecting, and tries to reconnect
                        _clusterState.ConnectionState = ClusterConnectionState.Connecting;
                        // FIXME client lifecycle events?
                        _clusterConnectTask ??= ConnectFirstAsync(_clusterState.CancellationToken).ContinueWith(async x =>
                        {
                            await _terminateAsync().CAF();

                        }, default, TaskContinuationOptions.NotOnRanToCompletion, TaskScheduler.Current);

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

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _clusterMembersTask.ObserveCanceled().CAF();
            await _clusterConnectTask.ObserveCanceled().CAF();
        }
    }
}
