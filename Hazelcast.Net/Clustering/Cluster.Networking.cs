using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Exceptions;
using Hazelcast.Networking;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    public partial class Cluster // Networking
    {
        /// <summary>
        /// Connects to the server-side cluster.
        /// </summary>
        /// <param name="timeout">A timeout.</param>
        /// <returns>A task that will complete when connected.</returns>
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task ConnectAsync(TimeSpan timeout = default)
        {
            var timeoutMilliseconds = timeout.TimeoutMilliseconds(Constants.Cluster.DefaultConnectTimeoutMilliseconds);

            Task task;
            if (timeoutMilliseconds < 0) // infinite
            {
                task = ConnectAsyncInternal(CancellationToken.None);
            }
            else
            {
                var cancellation = _clusterCancellation.WithTimeout(timeoutMilliseconds);
                task = ConnectAsyncInternal(cancellation.Token).OrTimeout(cancellation);
            }

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.ConfigureAwait(false);
#endif
        }


        /// <summary>
        /// Connects to the server-side cluster.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when connected.</returns>
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task ConnectAsync(CancellationToken cancellationToken)
        {
            var cancellation = _clusterCancellation.LinkedWith(cancellationToken);
            var task = ConnectAsyncInternal(cancellation.Token).ThenDispose(cancellation);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.ConfigureAwait(false);
#endif
        }

        /// <summary>
        /// Connects to the server-side cluster.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when connected.</returns>
        private async Task ConnectAsyncInternal(CancellationToken cancellationToken)
        {
            _readonlyProperties = true;

            lock (_clusterStateLock)
            {
                if (_clusterState != ClusterState.NotConnected)
                    throw new InvalidOperationException("Cluster has already been connected.");
                _clusterState = ClusterState.Connecting;
            }

            try
            {
                // connects the first client, throws if it fails
                await ConnectFirstClientAsync(cancellationToken).ConfigureAwait(false);

                // wait for the member table
                await _firstMembersView.WaitAsync(cancellationToken);
                _firstMembersView = null;

                // execute subscribers
                foreach (var subscriber in _clusterEventSubscribers)
                    await subscriber.SubscribeAsync(this, cancellationToken).ConfigureAwait(false);

                // once subscribers have run, they have created subscriptions
                // and we don't need them anymore - only their subscriptions
                _clusterEventSubscribers = null;
            }
            catch
            {
                // FIXME: that is not enough, we need a "die" method
                lock (_clusterStateLock)
                {
                    _clusterState = ClusterState.Disconnected;
                }

                throw;
            }
        }

        private void StartConnectClusterWithLock() // FIXME: wtf?
        {
            //_connectClusterTask ??= ConnectClusterAsync();
        }

        /// <summary>
        /// Connects a first client to the server-side cluster.
        /// </summary>
        /// <returns>A task that will complete when connected.</returns>
        private async Task ConnectFirstClientAsync(CancellationToken cancellationToken)
        {
            var tried = new HashSet<NetworkAddress>();
            var exceptions = new List<Exception>();
            var retryStrategy = new RetryStrategy("connect to cluster", _retryConfiguration, _loggerFactory);

            do
            {
                // gets unique addresses (by the IPEndPoint)
                var addresses = GetCandidateAddresses();
                foreach (var address in addresses)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    tried.Add(address);
                    var attempt = await TryConnectAsync(address, cancellationToken).ConfigureAwait(false);
                    if (attempt) return;

                    if (attempt.HasException) exceptions.Add(attempt.Exception);
                }

            } while (await retryStrategy.WaitAsync(cancellationToken).ConfigureAwait(false));

            if (_connectTaskCancel) return; // FIXME duh?

            var aggregate = new AggregateException(exceptions);
            throw new InvalidOperationException($"Unable to connect to the cluster \"{Name}\". " +
                $"The following addresses where tried: {string.Join(", ", tried)}.", aggregate);
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
        private async Task<Attempt<Client>> TryConnectAsync(NetworkAddress address, CancellationToken cancellationToken)
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

            using var acquired = await LockAcquisition.TryLockAsync(address.Lock).ConfigureAwait(false);
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
                client = await ConnectWithLockAsync(address, cancellationToken).ConfigureAwait(false);
                return client;
            }
            catch (Exception e)
            {
                return Attempt.Fail<Client>(e);
            }
        }

        /// <summary>
        /// Opens a connection to an address, while being locked.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the connection has been established, and represents the associated client.</returns>
        private async Task<Client> ConnectWithLockAsync(NetworkAddress address, CancellationToken cancellationToken)
        {
            // map private address to public address
            address = _addressProvider.Map(address);

            // create the client
            var client = new Client(address, _correlationIdSequence, _clientOptions, _loggerFactory)
            {
                OnReceiveEventMessage = OnEventMessage,
                OnShutdown = HandleClientShutdown
            };

            // connect to the server (may throw)
            await client.ConnectAsync(cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            // authenticate (may throw)
            var info = await _authenticator
                .AuthenticateAsync(client, Name, ClientId, ClientName, _labels, _serializationService, cancellationToken)
                .ConfigureAwait(false);
            if (info == null) throw new HazelcastException("Failed to authenticate");
            client.NotifyAuthenticated(info);

            _logger.LogInformation("Authenticated with server member {MemberId} running version {HazelcastServerVersion}" +
                                   " at {RemoteAddress} via {LocalAddress}.",
                info.MemberId, info.ServerVersion, info.MemberAddress, client.LocalEndPoint);

            // notify partitioner
            Partitioner.NotifyInitialCount(info.PartitionCount);

            // register & prepare the client
            lock (_clusterStateLock)
            {
                var firstClient = _clients.Count == 0;

                // if client is not active anymore, we can't continue - there is no
                // race condition here because the client shutdown handler also lock
                // on _clusterStateLock
                if (!client.Active) throw new HazelcastException();

                _clients[info.MemberId] = client;
                _addressClients[address] = client;

                _clusterState = ClusterState.Connected;

                var otherCluster = _clusterServerSideId != default && _clusterServerSideId != info.ClusterId;
                var newCluster = firstClient && otherCluster;
                _clusterServerSideId = info.ClusterId;

                // FIXME what about cluster change thing?
                if (newCluster)
                {
                    // we did connect to a cluster once, and then we lost all clients,
                    // and now we have a new client, which is connected to a different
                    // cluster identifier

                    _logger.LogWarning("Switching from current cluster {CurrentClusterId} to new cluster {NewClusterId}.", _clusterServerSideId, info.ClusterId);

                    // clear members list
                    _memberTable = new MemberTable(0, Array.Empty<MemberInfo>());

                    // get distributed object factory to re-create objects, etc
                    // FIXME:!!
                    //await _onConnectionToNewCluster();
                }

                lock (_clusterEventsLock)
                {
                    // if we don't have a cluster client yet, start a
                    // single, cluster-wide task ensuring there is a cluster events client
                    if (_clusterEventsClient == null)
                        StartSetClusterEventsClientWithLock(client);
                }

                // per-client task subscribing the client to events
                // this is entirely fire-and-forget, it anything goes wrong it will shut the client down
                // TODO: means we cannot await on it? the client should know about it + timeout or?
                var subscriptions = _subscriptions.Values.Where(x => x.Active).ToList();
                _ = InstallSubscriptionsOnNewClient(client, subscriptions);

                OnConnectionAdded(client); // does not throw

                if (firstClient)
                    OnClientLifecycleEvent(ClientLifecycleState.Connected); // does not throw
            }

            return client;
        }

        // TODO: wtf
        private void EnsureActive() { } // supposed to validate that the client is still active?

        private void HandleClientShutdown(Client client)
        {
            // this runs when a client signals that it is shutting down

            lock (_clusterStateLock)
            {
                _addressClients.TryRemove(client.Address, out _);
                _clients.TryRemove(client.MemberId, out _);

                var lastClient = _clients.Count == 0;

                if (lastClient)
                    OnClientLifecycleEvent(ClientLifecycleState.Disconnected); // does not throw

                OnConnectionRemoved(client); // does not throw

                // just clear subscriptions, cannot unsubscribes from the server since
                // the client is not connected anymore
                var subscriptions = _subscriptions.Values.ToList();
                ClearClientSubscriptions(subscriptions, client);

                // replace 'cluster client' if needed
                lock (_clusterEventsLock) // TODO: could this be the same state lock?
                {
                    // if the client was the cluster client, and we have more client, start a
                    // single, cluster-wide task ensuring there is a cluster events client
                    if (ClearClusterEventsClientWithLock(client) && !lastClient)
                        StartSetClusterEventsClientWithLock(client);
                }

                if (lastClient)
                {
                    _logger.LogInformation("Disconnected (reconnect mode:{ReconnectMode})", _reconnectMode);

                    switch (_reconnectMode)
                    {
                        case ReconnectMode.DoNotReconnect:
                            _clusterState = ClusterState.Disconnected;
                            // FIXME: need to die somehow
                            break;

                        case ReconnectMode.ReconnectSync:
                            _clusterState = ClusterState.Connecting;
                            // FIXME: need to trigger a reconnect + block invocations?
                            break;

                        case ReconnectMode.ReconnectAsync:
                            _clusterState = ClusterState.Connecting;
                            // FIXME: need to trigger a reconnect
                            break;

                        default:
                            throw new NotSupportedException();
                    }
                }
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

            // take all configured addresses
            foreach (var address in _addressProvider.GetAddresses())
            {
                addresses.Add(address);
            }

            // add (de-duplicated thanks to HashSet) all known members
            var members = _memberTable?.Members;
            if (members != null)
            {
                foreach (var address in _memberTable.Members.Values.Select(x => x.Address))
                {
                    addresses.Add(address);
                }
            }

            // shuffle
            return addresses.Shuffle();
        }
    }
}
