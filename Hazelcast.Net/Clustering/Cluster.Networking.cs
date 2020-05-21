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
    // partial: networking
    public partial class Cluster
    {
        /// <summary>
        /// Gets a random client.
        /// </summary>
        /// <returns>A random client.</returns>
        private Client GetRandomClient()
        {
            // In "smart mode" the clients connect to each member of the cluster. Since each
            // data partition uses the well known and consistent hashing algorithm, each client
            // can send an operation to the relevant cluster member, which increases the
            // overall throughput and efficiency. Smart mode is the default mode.
            //
            // In "uni-socket mode" the clients is required to connect to a single member, which
            // then behaves as a gateway for the other members. Firewalls, security, or some
            // custom networking issues can be the reason for these cases.

            var maxTries = _loadBalancer.Count;

            if (IsSmartRouting)
            {
                for (var i = 0; i < maxTries; i++)
                {
                    var memberId = _loadBalancer.Select();
                    if (_clients.TryGetValue(memberId, out var lbclient))
                        return lbclient;
                }

                var client = _clients.Values.FirstOrDefault();
                if (client == null)
                    throw new HazelcastException("Could not get a client.");

                return client;
            }

            // there should be only one
            var singleClient = _clients.Values.FirstOrDefault();
            if (singleClient == null)
                throw new HazelcastException("Could not get a client.");
            return singleClient;
        }

        /// <summary>
        /// Connects to the server-side cluster.
        /// </summary>
        /// <returns>A task that will complete when connected.</returns>
        public async ValueTask ConnectAsync()
        {
            _readonlyProperties = true;

            // connects the first client
            await ConnectFirstClientAsync();

            // wait for the member table
            await _firstMembersView.WaitAsync();
            _firstMembersView = null;

            // execute subscribers
            foreach (var subscriber in _clusterEventSubscribers)
                await subscriber.SubscribeAsync(this);

            // once subscribers have run, they have created subscriptions
            // and we don't need them anymore - only the subscriptions
            _clusterEventSubscribers = null;
        }

        /// <summary>
        /// Connects a first client to the server-side cluster.
        /// </summary>
        /// <returns>A task that will complete when connected.</returns>
        private async ValueTask ConnectFirstClientAsync()
        {
            var tried = new HashSet<NetworkAddress>();
            var exceptions = new List<Exception>();
            var retryStrategy = new RetryStrategy("connect to cluster", _retryConfiguration, _loggerFactory);

            do
            {
                EnsureActive();

                // gets unique addresses (by the IPEndPoint)
                var addresses = GetCandidateAddresses();
                foreach (var address in addresses)
                {
                    EnsureActive();
                    tried.Add(address);
                    var attempt = await TryConnectAsync(address);
                    if (attempt) return;
                    if (attempt.HasException) exceptions.Add(attempt.Exception);
                }

            } while (await retryStrategy.WaitAsync());

            var clusterName = Name;
            var aggregate = new AggregateException(exceptions);
            throw new InvalidOperationException($"Unable to connect to the cluster \"{clusterName}\". " +
                $"The following addresses where tried: {string.Join(", ", tried)}.", aggregate);
        }

        /// <summary>
        /// Tries to get, or open, a connection to an address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>A task that will complete when the connection has been retrieved or established,
        /// and represents an attempt representing the client.</returns>
        /// <remarks>
        /// <para>This method does not throw but returns a failed attempt.</para>
        /// </remarks>
        private async ValueTask<Attempt<Client>> TryConnectAsync(NetworkAddress address)
        {
            if (_disposed == 1) return Attempt.Failed;

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

            using (var acquired = await address.TryLockAsync())
            {
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
                    client = await ConnectWithLockAsync(address);
                    return client;
                }
                catch (Exception e)
                {
                    return Attempt.Fail<Client>(e);
                }
            }
        }

        /// <summary>
        /// Opens a connection to an address, while being locked.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>A task that will complete when the connection has been established, and represents the associated client.</returns>
        private async ValueTask<Client> ConnectWithLockAsync(NetworkAddress address)
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
            await client.ConnectAsync();

            // authenticate (may throw)
            var info = await _authenticator.AuthenticateAsync(client, Name, ClientId, ClientName, _labels, _serializationService);
            if (info == null) throw new HazelcastException("Failed to authenticate");

            // notify partitioner
            Partitioner.NotifyInitialCount(info.PartitionCount);

            // notify client
            client.NotifyAuthenticated(info); // client.MemberId = info.MemberId;

            // register the client, and capture the subscriptions
            List<ClusterSubscription> subscriptions;
            bool firstClient;
            lock (_clientsLock)
            {
                firstClient = _clients.Count == 0;

                _clients[info.MemberId] = client;
                _addressClients[address] = client;
                _connected = 1;

                subscriptions = _subscriptions.Values.Where(x => x.Active).ToList();
            }

            var newCluster = firstClient && _clusterServerSideId != default && _clusterServerSideId != info.ClusterId;

            if (firstClient)
                _clusterServerSideId = info.ClusterId;

            if (newCluster)
            {
                // we did connect to a cluster once, and then we lost all clients,
                // and now we have a new client, which is connected to a different
                // cluster identifier

                _logger.LogWarning("Switching from current cluster {CurrentClusterId} to new cluster {NewClusterId}.", _clusterServerSideId, info.ClusterId);

                // clear members list
                _memberTable = new MemberTable(0, Array.Empty<MemberInfo>());

                // get distributed object factory to re-create objects, etc
                await _onConnectionToNewCluster();

                OnClientLifecycleEvent(ClientLifecycleState.Connected);
            }
            else if (firstClient)
            {
                OnClientLifecycleEvent(ClientLifecycleState.Connected);
            }

            // FIXME: we've fired 'connected' events before running the rest?

            _logger.LogInformation("Authenticated with server member {MemberId} running version {HazelcastServerVersion}" +
                " at {RemoteAddress} via {LocalAddress}.",
                info.MemberId, info.ServerVersion, info.MemberAddress, client.LocalEndPoint);

            try
            {
                // trigger connection event
                OnConnectionAdded(client);

                // ensure there is always one 'cluster client' dealing with cluster events
                // once this is done, the client will receive the 'members view' and 'partitions
                // view' events, and this will initialize the partitioner & load balancer
                // beware: it is not possible to talk to the cluster before that has happen
                // TODO: do it in the background, see ListenerService?
                await ProposeClusterEventsClient(client);

                // subscribe the new client to all events the cluster has subscriptions for
                // TODO: do it in the background, see ListenerService?
                await InstallSubscriptionsOnNewClient(client, subscriptions);
            }
            catch
            {
                bool disconnected;
                lock (_clientsLock)
                {
                    disconnected = RemoveClientWithLock(client);
                }
                await client.ShutdownAsync();

                // FIXME: this is bad
                if (disconnected)
                    OnClientLifecycleEvent(ClientLifecycleState.Disconnected);

                throw;
            }

            return client;
        }

        private bool RemoveClientWithLock(Client client)
        {
            _addressClients.TryRemove(client.Address, out _);
            _clients.TryRemove(client.MemberId, out _);

            if (_clients.Count > 0 || _connected == 0)
                return false;

            _connected = 0;
            return true;
        }

        // TODO: implement (and for client etc)
        private void EnsureActive() { } // supposed to validate that the client is still active?

        private void HandleClientShutdown(Client client)
        {
            // this runs when a client signals that it is shutting down

            // forget about the client
            List<ClusterSubscription> subscriptions;
            bool disconnected;
            lock (_clientsLock)
            {
                disconnected = RemoveClientWithLock(client);
                subscriptions = _subscriptions.Values.ToList();
            }

            if (disconnected)
                OnClientLifecycleEvent(ClientLifecycleState.Disconnected);

            // clears its subscriptions - does not unsubscribes from the
            // server since the client is not connected anymore
            RemoveClient(subscriptions, client);

            // we need to have one client permanently handling the cluster events.
            // if the client which just shut down was handling cluster events,
            // we need to make sure another client takes over.
            if (ClearClusterEventsClient(client))
                ProposeClusterEventsClient().Wait(); // FIXME: async oops!
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
