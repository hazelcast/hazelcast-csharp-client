using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Networking;

namespace Hazelcast.Clustering
{
    // partial: networking
    public partial class Cluster
    {
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

            if (IsSmartRouting)
            {
                var memberId = _loadBalancer.Select();
                if (_clients.TryGetValue(memberId, out var client))
                    return client;

                // TODO: what shall we do, actually?
                // SingleOrDefault or FirstOrDefault?
            }

            // TODO: nicer exception
            return _clients.Values.SingleOrDefault();
        }

        public async ValueTask ConnectToCluster()
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
                    if (await TryConnectAsync(address, exceptions)) return;
                }

            } while (await retryStrategy.WaitAsync());

            var clusterName = ""; // FIXME: cluster name?
            var aggregate = new AggregateException(exceptions);
            throw new InvalidOperationException($"Unable to connect to the cluster \"{clusterName}\". " +
                $"The following addresses where tried: {string.Join(", ", tried)}.", aggregate);
        }

        // TODO: implement (and for client etc)
        private void EnsureActive() { } // supposed to validate that the client is still active?

        private async ValueTask<bool> TryConnectAsync(NetworkAddress address, List<Exception> exceptions)
        {
            if (_clientsBAD.TryGetValue(address, out _)) // can the client be inactive?
                return true;

            // TODO: should lock something?

            try
            {
                // TODO: _clientsBAD? tracking client's death? other?
                var client = _clientsBAD[address] = await ConnectAsync(address);
                return true;
            }
            catch (Exception e)
            {
                exceptions.Add(e);
                return false;
            }
        }

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

        private async ValueTask<Client> ConnectAsync(NetworkAddress address)
        {
            // connect for real

            SemaphoreSlim s = null;
            try
            {
                // FIXME: don't understand why we have 1 per address?!
                s = await LockAsync(address.IPEndPoint);

                // translate address
                // + de-duplicate them somehow
                // FIXME: should IPEndPoint be lazily dns-resolved each time?

                // create the client
                var client = new Client(address.IPEndPoint, _correlationIdSequence)
                {
                    ReceiveEventMessage = ReceiveEventMessage, 
                    OnShutdown = HandleClientShutdown
                };

                // connect to the server (may throw)
                await client.ConnectAsync();

                // authenticate (may throw)
                var info = await _authenticator.AuthenticateAsync(client);



                // FIXME: can info be null?
                client.Update(info); //client.MemberId = info.MemberId;
                _clients[info.MemberId] = client;
                var serverVersion = info.ServerVersion;
                var remoteAddress = info.MemberAddress;
                var clusterId = info.ClusterId; // is this the cluster name?

                // initialize the partitioner & the load balancer
                Partitioner.InitializeCount(info.PartitionCount);
                _loadBalancer.NotifyMembers(new[] { info.MemberId });

                // FIXME: but we might want to do this in the background, see ListenerService
                // ensure there is always one client dealing with cluster events
                await AssignClusterEventsClient(client);

                // subscribe the new client to all events the cluster has subscriptions for
                await SubscribeClientToEvents(client);

                // trigger connection event
                OnConnectionAdded(client);

                // FIXME: we also need to handle clients going down!

                return client;
            }
            finally
            {
                s?.Release();
            }
        }

        private void HandleClientShutdown(Client client)
        {
            _clients.TryRemove(client.MemberId, out _);

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
