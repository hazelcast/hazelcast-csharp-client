using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering.LoadBalancing;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Eventing;
using Hazelcast.Exceptions;
using Hazelcast.Logging;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Security;
using Hazelcast.Partitioning;
using Hazelcast.Serialization;
using Partitioner = Hazelcast.Partitioning.Partitioner;

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

        private readonly ISequence<long> _correlationIdSequence;

        private readonly Partitioner _partitioner;
        private readonly ILoadBalancer _loadBalancer;

        // FIXME
        // _localOnly is defined in ListenerService and initialized with IsSmartRouting so it's the same
        // it is used by ProxyManager to AddDistributedObjectListener - passing that value

        private Client _clusterEventsClient;

        // FIXME must initialize the load balancer + notify it of all added/removed members

        public Cluster()
        {
            ISerializationService serializationService = null;
            var isSmartRouting = true;

            _correlationIdSequence = new Int64Sequence();
            IsSmartRouting = isSmartRouting;
            _partitioner = new Partitioner(serializationService, isSmartRouting);
            _loadBalancer = new RandomLoadBalancer();
        }



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
        private MemberInfo[] members;

        /// <summary>
        /// Determines whether the cluster is using smart routing.
        /// </summary>
        /// <remarks>
        /// <para>In "smart mode" the clients connect to each member of the cluster. Since each
        /// data partition uses the well known and consistent hashing algorithm, each client
        /// can send an operation to the relevant cluster member, which increases the
        /// overall throughput and efficiency. Smart mode is the default mode.</para>
        /// <para>In "uni-socket mode" the clients is required to connect to a single member, which
        /// then behaves as a gateway for the other members. Firewalls, security, or some
        /// custom networking issues can be the reason for these cases.</para>
        /// </remarks>
        public bool IsSmartRouting { get; } = true;

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
            return await GetRandomClient().SendAsync(message, _correlationIdSequence.Next);
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
                throw new InvalidOperationException(ExceptionMessages.InvalidTarget);
            return await client.SendAsync(message, _correlationIdSequence.Next);
        }

        /// <summary>
        /// Sends a message to a target.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="client">The target.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async ValueTask<ClientMessage> SendAsync(ClientMessage message, Client client)
            => await client.SendAsync(message, _correlationIdSequence.Next);

        /// <summary>
        /// Sends a message to the target owning a key.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="key">The key.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async ValueTask<ClientMessage> SendToKeyOwner(ClientMessage message, object key)
        {
            var partitionId = _partitioner.GetPartitionId(key);
            var targetId = _partitioner.GetPartitionOwner(partitionId);
            if (targetId == default)
                throw new InvalidOperationException(ExceptionMessages.InvalidTarget);
            return await SendAsync(message, targetId);
        }

        /// <summary>
        /// Sends a message to the target owning a partition.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="partitionId">The identifier of the partition.</param>
        /// <returns>A task that will complete when the response is received, and represent the response message.</returns>
        public async ValueTask<ClientMessage> SendToPartitionOwner(ClientMessage message, int partitionId)
        {
            var targetId = _partitioner.GetPartitionOwner(partitionId);
            if (targetId == default)
                throw new InvalidOperationException(ExceptionMessages.InvalidTarget);
            return await SendAsync(message, targetId);
        }

        #endregion

        public async ValueTask Connect()
        {
            await ConnectToCluster();
        }

        #region Eventing / Service

        /// <summary>
        /// Subscribes the cluster to a server event.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <returns>A task that will complete when the cluster has subscribed to the server event.</returns>
        public async Task SubscribeAsync(ClusterEventSubscription subscription)
        {
            _eventSubscriptions.TryAdd(subscription.Id, subscription);

            try
            {
                foreach (var (_, client) in _clients)
                    await Subscribe(client, subscription);
            }
            catch
            {
                // FIXME what-if only some clients have subscribed?
                _eventSubscriptions.TryRemove(subscription.Id, out _);
                throw;
            }
        }

        /// <summary>
        /// Subscribes a client to server events.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>A task that will complete when the client has subscribed to server events.</returns>
        private async Task SubscribeClient(Client client)
        {
            // FIXME what-if some fail?
            foreach (var (_, subscription) in _eventSubscriptions)
                await Subscribe(client, subscription);
        }

        private async ValueTask Subscribe(Client client, ClusterEventSubscription subscription)
        {
            // FIXME try...catch, see ListenerService
            // and remove the handler if all fails

            var correlationId = _correlationIdSequence.Next;
            _eventHandlers[correlationId] = subscription.EventHandler;

            try
            {
                var response = await client.SendAsync(subscription.SubscribeRequest, correlationId);
                _ = subscription.AcceptSubscribeResponse(response, client);
            }
            catch
            {
                _eventHandlers.TryRemove(correlationId, out _);
                throw;
            }
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
        public async Task UnsubscribeClient(Client client) // fixme private
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

        /// <summary>
        /// Handles an event message.
        /// </summary>
        /// <param name="message">The event message.</param>
        private void ReceiveEventMessage(ClientMessage message)
        {
            XConsole.WriteLine(this, "Handle event message.\n" + message.Dump("EVENT"));

            // TODO threading? handle events in scheduled tasks?
            if (!_eventHandlers.TryGetValue(message.CorrelationId, out var eventHandler))
            {
                // TODO log a warning
                XConsole.WriteLine(this, $"No event handler for ID:{message.CorrelationId}");
                return;
            }

            eventHandler(message);
        }

        #endregion

        #region Eventing / Cluster

        /// <summary>
        /// Clears the client currently handling cluster events, if it has not changed.
        /// </summary>
        /// <param name="client">The expected client.</param>
        /// <returns>true if the current client matched the expected client, and was cleared; otherwise false.</returns>
        private bool ClearClusterEventsClient(Client client)
            => Interlocked.CompareExchange(ref _clusterEventsClient, null, client) == client;

        /// <summary>
        /// Assigns a new client to handle cluster events.
        /// </summary>
        /// <param name="client">An optional candidate client.</param>
        /// <returns>A task that will complete when a new client has been assigned to handle cluster events.</returns>
        private async Task AssignClusterEventsClient(Client client = null)
        {
            // todo - log when ending unsuccessfully

            if (client == null) client = GetRandomClient();
            if (client == null) return; // running out of clients, end

            // try to become the new client by replacing the current value, which is expected to be 'null'
            // if it is not 'null', it means that 'someone else' has taken over the assignment and we can end
            if (Interlocked.CompareExchange(ref _clusterEventsClient, client, null) != null)
                return;

            // arbitrarily decide to end after some amount of trying
            int GetMaxAttempts() => _clients.Count * 3;

            var failedAttempts = 0;
            while (!await SubscribeToClusterEvents(client))
            {
                // the client we tried failed to subscribe to events, try another client,
                // but ensure we don't enter some sort of infinite loop by counting attempts
                // todo: consider using a timeout, instead?
                var nextClient = GetRandomClient();
                if (nextClient == null || ++failedAttempts > GetMaxAttempts())
                {
                    // running out of clients, or tried too many times - end
                    Interlocked.CompareExchange(ref _clusterEventsClient, null, client);
                    return;
                }

                // try to become the new client by replacing the current value, which is expected to be 'client'
                // if it is not 'client', it means that 'someone else' has taken over the assignment and we can end
                if (Interlocked.CompareExchange(ref _clusterEventsClient, nextClient, client) != client)
                    return;

                client = nextClient;
            }

            // success
        }

        /// <summary>
        /// Subscribes a client to cluster events.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>A task that will complete when the subscription has been processed, and represent whether it was successful.</returns>
        private async Task<bool> SubscribeToClusterEvents(Client client) // aka subscribe to member/partition view events?
        {
            XConsole.WriteLine(this, "subscribe");

            // handles the "partition view" event
            void HandlePartitionViewEvent(int version, ICollection<KeyValuePair<Guid, IList<int>>> partitions)
                => _partitioner.HandlePartitionViewEvent(client.Id, version, MapPartitions(partitions));

            // handles the event
            void HandleEvent(ClientMessage message)
                => ClientAddClusterViewListenerCodec.EventHandler.HandleEvent(message, HandleMemberViewEvent, HandlePartitionViewEvent);

            try
            {
                var subscribeRequest = ClientAddClusterViewListenerCodec.EncodeRequest();
                var correlationId = _correlationIdSequence.Next;
                _eventHandlers[correlationId] = HandleEvent;
                _ = await client.SendAsync(subscribeRequest, correlationId);
                XConsole.WriteLine(this, "subscribed");
                return true;

                // FIXME but then are we ever going to clear that event handler?
            }
            catch
            {
                // todo log the exception?
                return false;
            }
        }

        /// <summary>
        /// Maps partitions from the event representation to our internal representation.
        /// </summary>
        /// <param name="partitions">The event representation of partitions.</param>
        /// <returns>Our internal representation of partitions.</returns>
        private static Dictionary<int, Guid> MapPartitions(IEnumerable<KeyValuePair<Guid, IList<int>>> partitions)
        {
            var map = new Dictionary<int, Guid>();
            foreach (var (memberId, partitionIds) in partitions)
                foreach (var partitionId in partitionIds)
                    map[partitionId] = memberId;
            return map;
        }

        /// <summary>
        /// Handles the "member view" event.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="members">The members.</param>
        private void HandleMemberViewEvent(int version, ICollection<MemberInfo> members)
        {
            // FIXME threading

            // notify the load balancer of the new list of members
            _loadBalancer.NotifyMembers(members.Select(x => x.Id));

            // get a new table
            var members2 = members.ToDictionary(x => x.Id, x => x);
            var table = new MemberTable(version, members2);

            // compute changes
            // count 1 for old members, 2 for new members, and then the result is
            // that 1=removed, 2=added, 3=unchanged
            // MemberInfo overrides GetHashCode and can be used as a key here
            var diff = new Dictionary<MemberInfo, int>();
            if (_memberTable == null)
            {
                foreach (var m in table.Members.Values)
                    diff[m] = 2;
            }
            else
            {
                foreach (var m in _memberTable.Members.Values)
                    diff[m] = 1;
                foreach (var m in table.Members.Values)
                    if (diff.ContainsKey(m)) diff[m] += 2;
                    else diff[m] = 2;
            }

            // replace the table
            _memberTable = table;

            // process changes, gather events
            var events = new List<MembershipEvent>();
            foreach (var (member, status) in diff)
            {
                switch (status)
                {
                    case 1: // old but not new = removed
                        XConsole.WriteLine(this, $"Removed member {member.Id}");
                        events.Add(new MembershipEvent(MembershipEventType.Removed, member));
                        if (_clients.TryGetValue(member.Id, out var client))
                            client.ShutdownAsync(); // will self-remove once down FIXME async
                        break;

                    case 2: // new but not old = added
                        XConsole.WriteLine(this, $"Added member {member.Id}");
                        events.Add(new MembershipEvent(MembershipEventType.Added, member));
                        break;

                    default: // unchanged
                        break;
                }
            }

            // raise events
            foreach (var eventData in events)
            {
                MembershipEvent.Raise(eventData);
                /*
                foreach (var handler in handlers)
                {
                    // FIXME async events? true event?
                    // cluster.AddHandler(...)
                    // cluster.MemberEvent += new MemberEventHandler(...)
                    if (eventData.EventType == MembershipEvent.MemberAdded)
                        handler.MemberAdded(eventData);
                    else
                        handler.MemberRemoved(eventData);
                }
                */
            }
        }

        // fixme move this!
        private MemberTable _memberTable;
        public IEventHandlers2<MembershipEvent> MembershipEvent { get; } = new EventHandlers2<MembershipEvent>();

        /// <summary>
        /// Represents a cluster member table.
        /// </summary>
        private class MemberTable
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MemberTable"/> class.
            /// </summary>
            /// <param name="version">The version of the member table.</param>
            /// <param name="members">The members.</param>
            public MemberTable(int version, Dictionary<Guid, MemberInfo> members)
            {
                Version = version;
                Members = members;
            }

            /// <summary>
            /// Gets the version of the member table.
            /// </summary>
            public int Version { get; }

            /// <summary>
            /// Gets the number of members in the table.
            /// </summary>
            public int Count => Members.Count;

            /// <summary>
            /// Gets the members.
            /// </summary>
            public Dictionary<Guid, MemberInfo> Members { get; }

            /// <inheritdoc />
            public override string ToString()
            {
                var text = new StringBuilder();
                text.Append("MemberTable (");
                text.Append(Count);
                text.Append(" members: ");
                text.Append(string.Join(", ", Members.Values));
                text.Append(")");
                return text.ToString();
            }
        }

        #endregion

        #region Networking

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

                // todo: what shall we do, actually?
                // SingleOrDefault or FirstOrDefault?
            }

            // todo: nicer exception
            return _clients.Values.SingleOrDefault();
        }

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
                //await SubscribeToClusterEvents(client);

                // fixme not tracking client's death?

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

                var client = new Client(address.IPEndPoint, _correlationIdSequence);
                client.ReceiveEventMessage = ReceiveEventMessage;
                client.OnShutdown = HandleClientShutdown;
                await client.ConnectAsync(); // may throw
                var info = await authenticator.AuthenticateAsync(client); // may throw

                // FIXME info may be null
                client.Update(info);
                _clients[info.MemberId] = client;
                //client.MemberId = info.MemberId;
                // deal with partition and stuff

                // FIXME but we might want to do this in the background, see ListenerService
                await AssignClusterEventsClient(client);
                await SubscribeClient(client); // fixme wtf?

                // FIXME we also need to handle clients going down!

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
                AssignClusterEventsClient().Wait(); // fixme async!
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
