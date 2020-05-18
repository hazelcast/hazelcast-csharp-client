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
            // FIXME: why would we have a client already if we try to connect?!
            //  and then we wouldn't need to subscribe to cluster events etc
            if (_addressClients.TryGetValue(address, out var client)) // can the client be inactive?
                return client;

            SemaphoreSlim s = null;

            try
            {
                // ensure we only connect to an endpoint once at a time
                // TODO: why?
                // TODO: leaking semaphores
                s = await LockEndPointAsync(address.IPEndPoint);

                if (_addressClients.TryGetValue(address, out client)) // can the client be inactive?
                    return client;

                // TODO: change this
                // ConnectAsync does register the client in _memberClients & _addressClients
                // but it should be performed here
                client = await ConnectWithLockAsync(address);

                // TODO: tracking client's death? other?
                return client;
            }
            catch (Exception e)
            {
                return Attempt.Fail<Client>(e);
            }
            finally
            {
                s?.Release();
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
            var client = new Client(address, _correlationIdSequence, _loggerFactory)
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

            // gather infos
            client.NotifyAuthenticated(info); // client.MemberId = info.MemberId;
            var serverVersion = info.ServerVersion;
            var remoteAddress = info.MemberAddress;
            var clusterId = info.ClusterId;
            var firstClient = _clients.Count == 0; // FIXME race-cond here?
            var newCluster = firstClient && _clusterServerSideId != default && _clusterServerSideId != clusterId;

            if (newCluster)
            {
                // warn: switching
                // onClusterRestart
                //  dispose disposable stuff (see hz.client internals)
                //  clear member list
                _memberTable = new MemberTable(0, Array.Empty<MemberInfo>());
            }

            // register the client
            lock (_clientsLock)
            {
                _clients[info.MemberId] = client;
                _addressClients[address] = client;
            }

            //
            if (firstClient)
                _clusterServerSideId = clusterId;

            if (newCluster)
            {
                _state = ClusterState.Connecting;
                // initialise (background)
                //   send state to cluster = distributedObjectFactory.CreateAllAsync
                //   and only *then* trigger connected and stuff
                //   but we do this async in the background, and why?
                // execute handler - allow DistributedObjectFactory to re-create objects
                await _onConnectionToNewCluster();
                _state = ClusterState.Connected;
                OnClientLifecycleEvent(ClientLifecycleState.Connected);
            }
            else
            {
                _state = ClusterState.Connected;
                OnClientLifecycleEvent(ClientLifecycleState.Connected);
            }

            // trigger connection event
            OnConnectionAdded(client);

            // ensure there is always one 'cluster client' dealing with cluster events
            // once this is done, the client will receive the 'members view' and 'partitions
            // view' events, and this will initialize the partitioner & load balancer
            // beware: it is not possible to talk to the cluster before that has happen
            // TODO: do it in the background, see ListenerService?
            await AssignClusterEventsClient(client);

            // subscribe the new client to all events the cluster has subscriptions for
            // TODO: do it in the background, see ListenerService?
            await InstallSubscriptionsOnNewClient(client);

            return client;
        }

        // TODO: implement (and for client etc)
        private void EnsureActive() { } // supposed to validate that the client is still active?

        // TODO: leaking semaphores, locking must be refactored
        private readonly ConcurrentDictionary<IPEndPoint, SemaphoreSlim> _semaphores
            = new ConcurrentDictionary<IPEndPoint, SemaphoreSlim>();

        private async ValueTask<SemaphoreSlim> LockEndPointAsync(IPEndPoint endPoint)
        {
            var s = _semaphores.GetOrAdd(endPoint, _ => new SemaphoreSlim(1));
            await s.WaitAsync();
            return s;
        }

        private void HandleClientShutdown(Client client)
        {
            // this runs when a client signals that it is shutting down

            // forget about the client
            lock (_clientsLock)
            {
                _addressClients.TryRemove(client.Address, out _);
                _clients.TryRemove(client.MemberId, out _);
            }

            // clears its subscriptions - does not unsubscribes from the
            // server since the client is not connected anymore
            ClearLostClientSubscriptions(client);

            // we need to have one client permanently handling the cluster events.
            // if the client which just shut down was handling cluster events,
            // we need to make sure another client takes over.
            if (ClearClusterEventsClient(client))
                AssignClusterEventsClient().Wait(); // FIXME: async oops!
        }

        /// <summary>
        /// Gets all candidate network addresses for connecting to the cluster.
        /// </summary>
        /// <returns></returns>
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
