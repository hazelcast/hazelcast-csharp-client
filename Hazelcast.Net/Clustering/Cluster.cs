using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Security;

namespace Hazelcast.Clustering
{
    public class Cluster
    {

        // connection manager   <- would be 'ClusterConnections'
        //   has a set of member id (guid) -> connections
        //   can return connections
        //   handles connection added/removed events
        //   maintains these connections, connect to whole cluster?

        // partition service
        //   has a set of partition id (int) -> member id (guid)
        //   TODO understand the relation between partitions and members

        // invocation service   <- more or less our 'Client'
        //  assigns correlation id
        //  invokes on a connection, obtained from the connection manager
        //   seems to manage some retry strategy?
        //  handles incoming messages
        //   route event message to listener service
        //   completes the invocation with result / exception etc   <- what our 'Client' does

        // is clusterService a singleton? NO
        // maybe there is only 1 load balancer
        // but everything is "one per client" - how can this ever makes sense?!
        // never mind, each "client" fully manages its cluster

        // manages a list of members
        // is a member what we call a 'client'?
        private IMember[] members;

        // not fully: members are added based on received events?
        // so it's (a) open a connection, corresponding event will register the member?

        public void Invoke(ClientMessage message) { }

        private readonly ClusterConnections _connections = new ClusterConnections();

        public async ValueTask Connect()
        {
            await _connections.ConnectToCluster();
        }

        public async Task<ClientMessage> SendAsync(ClientMessage request)
            => await _connections.SendAsync(request);

        private class ClusterConnections // aka ConnectionManager?
        {
            // implement connect-to-cluster
            //  what's the address provider?
            //    deals with config to get configured addresses
            //    can also run a configured cloud discovery

            public async Task<ClientMessage> SendAsync(ClientMessage request)
            {
                return await _clients.First().Value.SendAsync(request);
            }

            public async ValueTask ConnectToCluster()
            {
                // we probably could try to connect in parallel?

                var tried = new HashSet<NetworkAddress>();
                var exceptions = new List<Exception>();

                // arguments come from the configuration
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

                var clusterName = "";
                var aggregate = new AggregateException(exceptions);
                throw new InvalidOperationException($"Unable to connect to the cluster \"{clusterName}\". " +
                    $"The following addresses where tried: {string.Join(", ", tried)}.", aggregate);
            }

            private void EnsureActive() { } // supposed to validate that the client is still active?

            private async ValueTask<bool> TryConnectAsync(NetworkAddress address, List<Exception> exceptions)
            {
                if (_clients.TryGetValue(address, out _)) // can the client be inactive?
                    return true;

                // should lock

                try
                {
                    _clients[address] = await ConnectAsync(address);
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

            // TODO think!
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

                IAuthenticator authenticator = Services.Get.Authenticator();

                SemaphoreSlim s = null;
                try
                {
                    // fixme: don't understand why we have 1 per address?!
                    s = await LockAsync(address.IPEndPoint);

                    // translate address
                    // + de-duplicate them somehow
                    // fixme: IPEndPoint is lazily dns-resolved each time! not cached!

                    var client = new Client(address.IPEndPoint);
                    await client.ConnectAsync(); // may throw
                    await authenticator.AuthenticateAsync(client); // may throw
                    return client;
                }
                finally
                {
                    s?.Release();
                }
            }

            private readonly ConcurrentDictionary<NetworkAddress, Client> _clients
                 = new ConcurrentDictionary<NetworkAddress, Client>();

            // rename GetAddresses gets the address to try to connect to
            public IEnumerable<NetworkAddress> GetPossibleMemberAddresses()
            {
                // gather addresses from
                // - members already known by the cluster service
                // - "address provider"
                // should return an enumerable of "address"
                // shuffling is an option

                // temp. work with a fixed address
                yield return new NetworkAddress("127.0.0.1");

                //return new List<string>().Shuffle();
            }

        }
    }
}
