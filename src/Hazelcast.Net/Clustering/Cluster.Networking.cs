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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Events;
using Hazelcast.Exceptions;
using Hazelcast.Networking;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    internal partial class Cluster // Networking
    {
        private Func<CancellationToken, ValueTask> _onFirstClientConnected;

        public Func<CancellationToken, ValueTask> OnFirstClientConnected
        {
            get => _onFirstClientConnected;
            set
            {
                if (_readonlyProperties)
                    throw new InvalidOperationException(ExceptionMessages.PropertyIsNowReadOnly);
                _onFirstClientConnected = value;
            }
        }

        /// <summary>
        /// Connects to the server-side cluster.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when connected.</returns>
        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            using var cancellation = _clusterCancellation.LinkedWith(cancellationToken);
            await ConnectAsyncInternal(cancellation.Token).CAF();
        }

        /// <summary>
        /// Connects to the server-side cluster.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when connected.</returns>
        private async Task ConnectAsyncInternal(CancellationToken cancellationToken)
        {
            _readonlyProperties = true;

            using (await _clusterLock.AcquireAsync(CancellationToken.None).CAF())
            {
                if (_clusterState != ClusterState.NotConnected)
                    throw new InvalidOperationException("Cluster has already been connected.");
                _clusterState = ClusterState.Connecting;
            }

            try
            {
                // connects the first client, throws if it fails
                await ConnectFirstClientAsync(cancellationToken).CAF();

                // wait for the member table
                await _firstMembersView.WaitAsync(cancellationToken).CAF();
                _firstMembersView = null;

                // start connecting members
                // this is a background task that cancels with the cluster
                _clusterMembersTask = ConnectAllClientsAsync(_clusterCancellation.Token);

                // execute subscribers
                await OnFirstClientConnected(cancellationToken).CAF();
            }
            catch
            {
                await TerminateAsync().CAF();
                throw;
            }
        }

        private void StartConnectClusterWithLock(CancellationToken cancellationToken)
        {
            _clusterConnectTask ??= ConnectFirstClientAsync(cancellationToken).ContinueWith(async x =>
            {
                if (x.IsFaulted)
                {
                    await TerminateAsync().CAF();
                }
            }, cancellationToken, TaskContinuationOptions.None, TaskScheduler.Current);
        }

        /// <summary>
        /// Connects a first client to the server-side cluster.
        /// </summary>
        /// <returns>A task that will complete when connected.</returns>
        private async Task ConnectFirstClientAsync(CancellationToken cancellationToken)
        {
            var tried = new HashSet<NetworkAddress>();
            var retryStrategy = new RetryStrategy("connect to cluster", _options.Networking.ConnectionRetry, _loggerFactory);
            List<Exception> exceptions = null;
            bool canRetry;

            do
            {
                // gets unique addresses (by the IPEndPoint)
                var addresses = GetCandidateAddresses();
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
                throw new OperationCanceledException($"The cluster connection operation to \"{Name}\" has been canceled. " +
                    $"The following addresses where tried: {string.Join(", ", tried)}.", aggregate);

            throw new InvalidOperationException($"Unable to connect to the cluster \"{Name}\". " +
                $"The following addresses where tried: {string.Join(", ", tried)}.", aggregate);
        }

        /// <summary>
        /// Connects clients for each members.
        /// </summary>
        private async Task ConnectAllClientsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken).CAF();

                var members = _memberTable?.Members.Values; // capture
                if (members == null || members.Count == 0) return;

                foreach (var member in members)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    await TryConnectAsync(member.Address, cancellationToken).CAF(); // does not throw

                    // ignore result
                    // TryConnectAsync does add the client to _clients etc
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
        private async Task<Attempt<ClientConnection>> TryConnectAsync(NetworkAddress address, CancellationToken cancellationToken)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            if (_addressClients.TryGetValue(address, out var client))
            {
                // if we already have a client for that address, return the client
                // if it is active, or fail if it is not - cannot open yet another
                // client to that same address
                if (client.Active) return client;
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
                if (_addressClients.TryGetValue(address, out client))
                {
                    // if we already have a client for that address, return the client
                    // if it is active, or fail if it is not - cannot open yet another
                    // client to that same address
                    if (client.Active) return client;
                    return Attempt.Failed;
                }

                // else actually connect - this may throw
#pragma warning disable CA2000 // Dispose objects before losing scope - will be disposed, eventually
                client = await ConnectWithLockAsync(address, cancellationToken).CAF();
#pragma warning restore CA2000
                return client;
            }
            catch (Exception e)
            {
                return Attempt.Fail<ClientConnection>(e);
            }
        }

        /// <summary>
        /// Opens a connection to an address, while being locked.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the connection has been established, and represents the associated client.</returns>
        private async Task<ClientConnection> ConnectWithLockAsync(NetworkAddress address, CancellationToken cancellationToken)
        {
            // map private address to public address
            address = _addressProvider.Map(address);

            // create the client
            var client = new ClientConnection(address, _options.Messaging, _options.Networking.Socket, _correlationIdSequence, _loggerFactory)
            {
                OnReceiveEventMessage = OnEventMessage,
                OnShutdown = HandleClientShutdown
            };

            // connect to the server (may throw)
            await client.ConnectAsync(cancellationToken).CAF();
            cancellationToken.ThrowIfCancellationRequested();

            // authenticate (may throw)
            var info = await _authenticator
                .AuthenticateAsync(client, Name, ClientId, ClientName, _options.Labels, _serializationService, cancellationToken)
                .CAF();
            if (info == null) throw new HazelcastException("Failed to authenticate");
            client.NotifyAuthenticated(info);

            _logger.LogInformation("Authenticated client \"{ClientName}\" ({ClientId})" +
                                   " with cluster \"{ClusterName}\" member {MemberId}" +
                                   " running version {HazelcastServerVersion}" +
                                   " at {RemoteAddress} via {LocalAddress}.",
                ClientName, ClientId.ToString("N").Substring(0, 7),
                Name, info.MemberId.ToString("N").Substring(0, 7),
                info.ServerVersion, info.MemberAddress, client.LocalEndPoint);

            // notify partitioner (may throw)
            Partitioner.NotifyPartitionsCount(info.PartitionCount);

            // register & prepare the client
            using (await _clusterLock.AcquireAsync(CancellationToken.None).CAF())
            {
                var firstClient = _clients.Count == 0;

                // if client is not active anymore, we can't continue - there is no
                // race condition here because the client shutdown handler also lock
                // on _clusterStateLock
                if (!client.Active)
                    throw new HazelcastException("Client is not active.");

                if (_clients.ContainsKey(info.MemberId))
                    throw new HazelcastException("Duplicate client.");

                _clients[info.MemberId] = client;
                _addressClients[address] = client;

                _clusterState = ClusterState.Connected;

                var otherCluster = _clusterServerSideId != default && _clusterServerSideId != info.ClusterId;
                var newCluster = firstClient && otherCluster;
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
                    _memberTable = new MemberTable(0, Array.Empty<MemberInfo>());

                    // get distributed object factory to re-create objects, etc
                    await _onConnectionToNewCluster(_clusterCancellation.Token).CAF();
                }

                // if we don't have a cluster client yet, start a
                // single, cluster-wide task ensuring there is a cluster events client
                if (_clusterEventsClient == null)
                    StartSetClusterEventsClientWithLock(client, _clusterCancellation.Token);

                // per-client task subscribing the client to events
                // this is entirely fire-and-forget, it anything goes wrong it will shut the client down
                var subscriptions = _subscriptions.Values.Where(x => x.Active).ToList();
                client.StartBackgroundTask(token => InstallSubscriptionsOnNewClient(client, subscriptions, token), _clusterCancellation.Token);

                await OnConnectionAdded(/*client*/).CAF(); // does not throw

                if (firstClient)
                    await OnClientLifecycleEvent(ClientLifecycleState.Connected).CAF(); // does not throw
            }

            return client;
        }

        private async ValueTask HandleClientShutdown(ClientConnection client)
        {
            // this runs when a client signals that it is shutting down

            var die = false;

            using (await _clusterLock.AcquireAsync(CancellationToken.None).CAF())
            {
                _addressClients.TryRemove(client.Address, out _);
                _clients.TryRemove(client.MemberId, out _);

                var lastClient = _clients.Count == 0;

                if (lastClient)
                    await OnClientLifecycleEvent(ClientLifecycleState.Disconnected).CAF(); // does not throw

                await OnConnectionRemoved(/*client*/).CAF(); // does not throw

                // just clear subscriptions, cannot unsubscribes from the server since
                // the client is not connected anymore
                var subscriptions = _subscriptions.Values.ToList();
                ClearClientSubscriptions(subscriptions, client);

                // if the client was the cluster client, and we have more client, start a
                // single, cluster-wide task ensuring there is a cluster events client
                if (ClearClusterEventsClientWithLock(client) && !lastClient)
                    StartSetClusterEventsClientWithLock(client, _clusterCancellation.Token);

                if (!lastClient)
                    return;

                _logger.LogInformation("Disconnected (reconnect mode: {ReconnectMode}, {ReconnectAction})",
                    _options.Networking.ReconnectMode,
                    _options.Networking.ReconnectMode switch
                    {
                        ReconnectMode.DoNotReconnect => "remain disconnected",
                        ReconnectMode.ReconnectSync => "not supported",
                        ReconnectMode.ReconnectAsync => "trying to reconnect",
                        _ => "Meh?"
                    });

                switch (_options.Networking.ReconnectMode)
                {
                    case ReconnectMode.DoNotReconnect:
                        _clusterState = ClusterState.Disconnected;
                        die = true;
                        break;

                    case ReconnectMode.ReconnectSync:
                        _clusterState = ClusterState.Connecting;
                        // TODO: implement ReconnectSync
                        // in original code this does ReconnectAsync
                        throw new NotSupportedException("Reconnect mode 'ReconnectSync' is not supported.");

                    case ReconnectMode.ReconnectAsync:
                        _clusterState = ClusterState.Connecting;
                        StartConnectClusterWithLock(_clusterCancellation.Token);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            if (die)
            {
                await TerminateAsync().CAF();
            }
        }

        /// <summary>
        /// Gets all candidate network addresses for connecting to the cluster.
        /// </summary>
        /// <returns>All candidate network addresses.</returns>
        /// <remarks>
        /// <para>Returns unique addresses (unique by their IPEndPoint).</para>
        /// </remarks>
        private IEnumerable<NetworkAddress> GetCandidateAddresses()
        {
            var addresses = new HashSet<NetworkAddress>();

            // do it in two batches,
            // each batch may be shuffled, but know members always come first

            // first, add all known members
            var members = _memberTable?.Members;
            if (members != null)
            {
                var memberAddresses = _memberTable.Members.Values.Select(x => x.Address);
                if (_options.Networking.ShuffleAddresses)
                    memberAddresses = memberAddresses.Shuffle();

                foreach (var address in memberAddresses)
                    addresses.Add(address);
            }

            // second, add all known addresses (de-duplicated thanks to HashSet)
            var configuredAddresses = _addressProvider.GetAddresses();
            if (_options.Networking.ShuffleAddresses)
                configuredAddresses = configuredAddresses.Shuffle();

            foreach (var address in configuredAddresses)
                addresses.Add(address);

            return addresses;
        }
    }
}
