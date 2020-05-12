using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Exceptions;
using Hazelcast.Networking;

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
                    if (_memberClients.TryGetValue(memberId, out var lbclient))
                        return lbclient;
                }

                var client = _memberClients.Values.FirstOrDefault();
                if (client == null)
                    throw new HazelcastException("Could not get a client.");

                return client;
            }

            // there should be only one
            var singleClient = _memberClients.Values.FirstOrDefault();
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
            await ConnectFirstClientAsync();

            // FIXME need timeout etc here!
            // wait for the first 'members view' & 'partitions view' events
            //await Task.WhenAll(_firstMembersView.WaitAsync(), _firstPartitionsView.WaitAsync());
            //_firstMembersView = _firstPartitionsView = null; // no need to keep them around
            // FIXME partition comes later but then wtf?
            await _firstMembersView.WaitAsync();
            _firstMembersView = null;

            // FIXME and also for each new client etc?!
            foreach (var subscriber in _clusterEventSubscribers)
                await subscriber.SubscribeAsync(this);
        }

        /// <summary>
        /// Connects a first client to the server-side cluster.
        /// </summary>
        /// <returns>A task that will complete when connected.</returns>
        public async ValueTask ConnectFirstClientAsync()
        {
            var tried = new HashSet<NetworkAddress>();
            var exceptions = new List<Exception>();

            // TODO: arguments should come from the configuration
            var retryStrategy = new RetryStrategy(100, 100, 1, 4000, 0);

            do
            {
                EnsureActive();

                var addresses = GetPossibleMemberAddresses();
                foreach (var address in addresses)
                {
                    EnsureActive();
                    tried.Add(address);
                    var attempt = await TryConnectAsync(address);
                    if (attempt) return;
                    if (attempt.HasException) exceptions.Add(attempt.Exception);
                }

            } while (await retryStrategy.WaitAsync());

            var clusterName = ""; // FIXME: cluster name?
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

            // TODO: should lock something?

            try
            {
                // ConnectAsync does register the client in _memberClients & _addressClients
                // (in addition to everything else)
                client = await ConnectAsync(address);
                // TODO: tracking client's death? other?
                return client;
            }
            catch (Exception e)
            {
                return Attempt.Fail<Client>(e);
            }
        }

        /// <summary>
        /// Opens a connection to an address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>A task that will complete when the connection has been established, and represents the associated client.</returns>
        private async ValueTask<Client> ConnectAsync(NetworkAddress address)
        {
            // connect for real

            SemaphoreSlim s = null;
            try
            {
                // FIXME: don't understand why we have 1 per address?!
                // assume it's to *not* try to connect to the same address multiple times
                // but that method is executed only when ... when?
                s = await LockAsync(address.IPEndPoint);

                // translate address
                // + de-duplicate them somehow
                // FIXME: should IPEndPoint be lazily dns-resolved each time?

                // create the client
                var client = new Client(address, _correlationIdSequence)
                {
                    OnReceiveEventMessage = OnEventMessage,
                    OnShutdown = HandleClientShutdown
                };

                // connect to the server (may throw)
                await client.ConnectAsync();

                // authenticate (may throw)
                var info = await _authenticator.AuthenticateAsync(client);
                if (info == null) throw new HazelcastException("Failed to authenticate");

                // notify partitioner
                Partitioner.NotifyInitialCount(info.PartitionCount);

                // gather infos
                client.Update(info); // client.MemberId = info.MemberId;
                var serverVersion = info.ServerVersion;
                var remoteAddress = info.MemberAddress;
                var clusterId = info.ClusterId;
                var firstClient = _memberClients.Count == 0; // FIXME race-cond here?
                var newCluster = firstClient && _clusterId != default && _clusterId != clusterId;

                if (newCluster)
                {
                    // warn: switching
                    // onClusterRestart
                    //  dispose disposable stuff (see hz.client internals)
                    //  clear member list
                    _memberTable = new MemberTable(0, Array.Empty<MemberInfo>());
                }

                // register the client
                _memberClients[info.MemberId] = client;
                _addressClients[address] = client;

                //
                if (firstClient)
                    _clusterId = clusterId;

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
                await InstallSubscriptionsOnNewClient(client);

                // FIXME: we also need to handle clients going down!

                return client;
            }
            finally
            {
                s?.Release();
            }
        }


        // TODO: implement (and for client etc)
        private void EnsureActive() { } // supposed to validate that the client is still active?

        // are we going to leak semaphores? should we manage addresses?
        // should work on the address's hash?
        private readonly ConcurrentDictionary<IPEndPoint, SemaphoreSlim> _semaphores
            = new ConcurrentDictionary<IPEndPoint, SemaphoreSlim>();

        // TODO: lock issues?
        // the original code lock(endpoint) but what does that mean?
        // no guarantee that endpoints are unique + concurrent connection?
        // shouldn't we lock the entire connection thing instead?

        private SemaphoreSlim GetSemaphoreAsync(IPEndPoint endPoint)
        {
            return _semaphores.GetOrAdd(endPoint, _ => new SemaphoreSlim(1));
        }

        private async ValueTask<SemaphoreSlim> LockAsync(IPEndPoint endPoint)
        {
            var s = _semaphores.GetOrAdd(endPoint, _ => new SemaphoreSlim(1));
            await s.WaitAsync();
            return s;
        }

        private void HandleClientShutdown(Client client)
        {
            _addressClients.TryRemove(client.Address, out _);
            _memberClients.TryRemove(client.MemberId, out _);

            // we need to have one client permanently handling the cluster events.
            // if the client which just shut down was handling cluster events,
            // we need to make sure another client takes over.
            if (ClearClusterEventsClient(client))
                AssignClusterEventsClient().Wait(); // FIXME: async oops!
        }

        // rename GetAddresses gets the address to try to connect to
        public IEnumerable<NetworkAddress> GetPossibleMemberAddresses()
        {
            // gather addresses from
            // - members already known by the cluster service
            // - "address provider"
            // should return an enumerable of "address"
            // shuffling is an option

            // temp. work with a fixed address
            //yield return new NetworkAddress("127.0.0.1");
            yield return new NetworkAddress("sgay-l4");

            //return new List<string>().Shuffle();
        }
    }
}
