using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Security;

namespace Hazelcast.Clustering
{
    public class Cluster
    {
        private readonly ConcurrentDictionary<NetworkAddress, Client> _clientsBAD
            = new ConcurrentDictionary<NetworkAddress, Client>();
        private readonly ConcurrentDictionary<Guid, Client> _clients
            = new ConcurrentDictionary<Guid, Client>();
        private readonly ConcurrentDictionary<Guid, ClusterEventSubscription> _eventSubscriptions
            = new ConcurrentDictionary<Guid, ClusterEventSubscription>();
        private readonly ConcurrentDictionary<long, Action<ClientMessage>> _eventHandlers
            = new ConcurrentDictionary<long, Action<ClientMessage>>();

        private readonly bool _localOnly; // FIXME initialize from configuration - and then what?


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
        // but! only if we register to the event!

        #region Messaging

        /// <summary>
        /// Sends a message to a random target.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async ValueTask<ClientMessage> SendAsync(ClientMessage message)
        {
            // FIXME was
            /*
            if (IsSmartRoutingEnabled)
            {
                var member = _loadBalancer.Next();
                if (member != null)
                {
                    var connection = GetConnection(member.Uuid);
                    if (connection != null)
                    {
                        return connection;
                    }
                }
            }
            return _connections.Values.SingleOrDefault();
            */

            var client = _clientsBAD.First().Value;
            return await client.SendAsync(message);
        }

        /// <summary>
        /// Sends a message to a target.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="targetId">The identifier of the target.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async ValueTask<ClientMessage> SendAsync(ClientMessage message, Guid targetId)
        {
            if (!_clients.TryGetValue(targetId, out var client))
                throw new Exception(); // fixme
            return await client.SendAsync(message);
        }

        /// <summary>
        /// Sends a message to a target.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="client">The target.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async ValueTask<ClientMessage> SendAsync(ClientMessage message, Client client)
            => await client.SendAsync(message);

        /// <summary>
        /// Sends a message to the target owning a key.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="key">The key.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public ValueTask<ClientMessage> SendToKeyOwner(ClientMessage message, object key)
        {
            // use partition service to do key -> partition Id
            // use partition service to do partition id -> target guid
            // invoke on target
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends a message to the target owning a partition.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="partitionId">The identifier of the partition.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public ValueTask<ClientMessage> SendToPartitionOwner(ClientMessage message, int partitionId)
        {
            // validate partition id
            // use partition service to do partition id -> target guid
            // invoke on target
            throw new NotImplementedException();
        }

        #endregion

        public async ValueTask Connect()
        {
            await ConnectToCluster();
        }

        #region Eventing

        /// <summary>
        /// Subscribes the cluster to a server event.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <returns>A task that will complete when the cluster has subscribed to the server event.</returns>
        public async Task SubscribeAsync(ClusterEventSubscription subscription)
        {
            _eventSubscriptions.TryAdd(subscription.Id, subscription);

            foreach (var (_, client) in _clientsBAD)
            {
                // fixme try...catch
                var response = await client.SendAsync(subscription.SubscribeRequest);
                var clientSubscription = subscription.AcceptSubscribeResponse(response, client);
                _eventHandlers[clientSubscription.CorrelationId] = subscription.EventHandler;
            }
        }

        /// <summary>
        /// Subscribes a client to server events.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>A task that will complete when the client has subscribed to server events.</returns>
        public async Task SubscribeClient(Client client)
        {
            foreach (var (_, clusterSubscription) in _eventSubscriptions)
            {
                await SubscribeClient(client, clusterSubscription); // FIXME try...catch, see ListenerService
            }
        }

        private async ValueTask SubscribeClient(Client client, ClusterEventSubscription subscription)
        {
            var response = await client.SendAsync(subscription.SubscribeRequest);
            var clientSubscription = subscription.AcceptSubscribeResponse(response, client);
            _eventHandlers[clientSubscription.CorrelationId] = subscription.EventHandler;
        }

        /// <summary>
        /// Unsubscribes the cluster from a server event.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <returns>A task that will complete when the cluster has unsubscribed from the server event.</returns>
        public async Task UnsubscribeAsync(Guid subscriptionId)
        {
            if (!_eventSubscriptions.TryGetValue(subscriptionId, out var clusterSubscription))
                throw new Exception();

            foreach (var (_, clientSubscription) in clusterSubscription.ClientSubscriptions)
            {
                // can we just ignore whatever is returned?
                // fixme try...catch
                await UnsubscribeClient(clientSubscription);
            }

            _eventSubscriptions.TryRemove(subscriptionId, out _);
        }

        /// <summary>
        /// Unsubscribes a client from server events.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>A task that will complete when the client has unsubscribed from server events.</returns>
        public async Task UnsubscribeClient(Client client)
        {
            foreach (var (_, eventSubscription) in _eventSubscriptions)
            {
                if (eventSubscription.ClientSubscriptions.TryRemove(client, out var clientSubscription))
                    _eventHandlers.TryRemove(clientSubscription.CorrelationId, out _);
            }
        }

        private async ValueTask UnsubscribeClient(ClientEventSubscription clientSubscription)
        {
            await clientSubscription.Client.SendAsync(clientSubscription.ClusterSubscription.CreateUnsubscribeRequest(clientSubscription.ServerSubscriptionId));
            _eventHandlers.TryRemove(clientSubscription.CorrelationId, out _);
        }

        #endregion

        #region Networking

        // implement connect-to-cluster
        //  what's the address provider?
        //    deals with config to get configured addresses
        //    can also run a configured cloud discovery

        public async ValueTask ConnectToCluster()
        {
            // we probably could try to connect in parallel?
            // no 'cos we stop at the first one that is successful

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
            if (_clientsBAD.TryGetValue(address, out _)) // can the client be inactive?
                return true;

            // should lock

            try
            {
                var client = _clientsBAD[address] = await ConnectAsync(address);

                // fixme temp
                //_clients[address].SubscribeAsync(new ClusterEventSubscription());

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
                client.ReceiveEventMessage = ReceiveEventMessage;
                await client.ConnectAsync(); // may throw
                var info = await authenticator.AuthenticateAsync(client); // may throw

                // FIXME info may be null
                _clients[info.MemberId] = client;
                //client.MemberId = info.MemberId;
                // deal with partition and stuff

                // FIXME but we might want to do this in the background, see ListenerService
                await SubscribeClient(client);

                // FIXME we also need to handle clients going down!

                return client;
            }
            finally
            {
                s?.Release();
            }
        }

        // fixme document
        private void ReceiveEventMessage(ClientMessage message)
        {
            // TODO threading? handle events in scheduled tasks?
            if (!_eventHandlers.TryGetValue(message.CorrelationId, out var eventHandler))
            {
                // TODO log a warning
                XConsole.WriteLine(this, $"No completion for ID:{message.CorrelationId}");
                return;
            }

            eventHandler(message);
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
            yield return new NetworkAddress("127.0.0.1");

            //return new List<string>().Shuffle();
        }

        #endregion
    }
}
