using System;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Messaging;

namespace Hazelcast.Clustering
{
    // partial: general events
    public partial class Cluster
    {
        // NOTES - FIXME: move this out!
        //
        // original hazelcast client internals - on start
        //
        // - create configured listeners
        //     creates the listener classes from configuration
        //
        // - start lifecycle service w/listeners
        //     registers the ILifecycleListener listeners = adds them to _lifecycleListeners
        //     fire 'starting' lifecycle event
        //     set 'active'
        //     fire 'started' lifecycle event
        //
        // - start invocation service
        //     schedules with fixed delay a 'clean resources' task
        //     which kills the pending 'invocations' when their connection is dead
        //
        // - start cluster service w/listeners
        //     registers the IMembershipListener listeners = adds them to _listeners
        //     registers the cluster service as a connection listener
        //       'connection added' => use as 'cluster client' if needed
        //       'connection removed' => find a new 'cluster client' if needed
        //
        // - start connection manager
        //     start the heartbeat task
        //       ?
        //     connects to the cluster
        //       tries all known addresses until it can obtain a client for that address,
        //       and that client becomes the 'cluster client' which will handle the 'members
        //       view' and 'partition view' events
        //     if smart routing is enabled, schedules with fixed delay a 'connect task'
        //       which periodically reconnects to all cluster members
        //     fires 'connection added' / 'connection removed' events
        //
        // - wait for initial list of members from cluster service ('members view' event)
        //     wait for cluster service _initialListFetchedLatch
        //
        // - connect to all cluster members
        //     get or connect to each member (by address)
        //     todo: maybe this could be lazy?
        //
        // - start listener service
        //     registers the listener service as a connection listener
        //       'connection added' => for all _registrations, register w/server => _eventHandlers
        //       'connection removed' => for all _registrations, clear _eventHandlers
        //
        // - initialize proxy manager
        //     fixme: ?
        //
        // - initialize load balancer
        //     fixme: ?
        //
        // - add client config listeners
        //     registers the IDistributedObjectListener listeners = via proxy manager,
        //       installs the remote subscription on all known clients (in listener service _registrations)
        //     registers the IPartitionLostListener listeners = via partition service,
        //       installs the remote subscription on all known clients (in listener service _registrations)
        //
        // when a remote subscription is installed via listener service, it goes in _registrations
        //   this is for any type of subscriptions but cluster events
        //   ie 'distributed object' and 'partition lost' cluster events, and all dist. object events
        //   they are installed / maintained on every client
        //
        //
        // obtaining a client for an address means
        // - establishing a socket connection to that address
        // - authenticate
        //     todo: implement retry-able auth for kerberos etc
        // - if there are no other connections, and cluster id has changed (HandleSuccessfulAuth)
        //     ha.client.OnClusterRestart fixme=? dispose all 'onClusterChangeDisposables' (none) + clear member list version
        //     ha.client is 'CONNECTED' + triggers InitializeClientOnCluster
        //       which "sends state" ie do factory . createAll
        //       and then, hz.client is 'INITIALIZES' + triggers 'connected' life cycle event
        //   else if cluster id has not changed, ha.client is 'INITIALIZED' + trigger 'connected' life cycle event
        // - fire 'connection added'
        //     cluster considers it for new 'cluster client' if there is none
        //     listener adds registrations
        //
        // the 'cluster client' listens to 'members view' and 'partitions view' events in order to
        // manage the list of known members and partitions in the cluster
        //
        //   handling the 'members view' event means
        //   - if it's the first time, signal _initialListFetchedLatch
        //   - fire 'member added' / 'member removed' events
        //   - which are only used to notify the load balancer
        //
        //   handling the 'partition view' event means
        //   // ?
        //
        //
        // on successful auth, something happens w/cluster?
        // todo: handle cluster id change (see connection manager)
        //
        // when a client is removed,
        // - properly removed = ListenerService.ConnectionRemoved = ?
        // - lost = its subscriptions are cleared (but not uninstalled)
        //
        // if that client is the 'cluster client', the cluster polls addresses again, to get a new
        // 'cluster client' - either using the existing client for that address, or connecting a
        // new client - and then this 'cluster client' subscribes to the 'members view' and
        // 'partitions view'
        // todo: it may be faster to consider existing clients first?
        //
        //
        // FIXME and then, create new connections, or re-use existing???
        //
        // in 'unisocket' mode, the cluster uses only 1 client - in 'smart routing' mode it
        // directly talks to the member owning the partition, when relevant.
        //
        // wants to talk to a particular member and there is no client for that member yet,
        //
        // when a user subscribes to a distributed object (eg, map entry added) event, a subscription
        // is installed on the cluster, which in turns installs the subscription on each client
        //
        // when the user unsubscribes, the subscription is removed on each client

        /// <summary>
        /// Installs a subscription on the cluster.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <returns>A task that will complete when the subscription has been installed.</returns>
        public async Task InstallSubscriptionAsync(ClusterSubscription subscription)
        {
            // register the subscription - but verify that the id really is unique
            if (!_eventSubscriptions.TryAdd(subscription.Id, subscription))
                throw new InvalidOperationException("A subscription with the same identifier already exists.");

            try
            {
                // subscribe each client: each client will send a subscription request,
                // with its own correlation id that is used to register a new instance of
                // the handler function, and then add itself to the list of registered
                // clients
                // FIXME: lock the clients list?
                foreach (var (_, client) in _memberClients)
                    await InstallSubscriptionAsync(subscription, client);
            }
            catch
            {
                // FIXME leak!
                // some clients may have subscribed and registered handlers!
                _eventSubscriptions.TryRemove(subscription.Id, out _);
                throw;
            }
        }

        /// <summary>
        /// Installs a subscription on one client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="subscription">The subscription.</param>
        /// <returns>A task that will complete when the client has subscribed to the server event.</returns>
        private async ValueTask InstallSubscriptionAsync(ClusterSubscription subscription, Client client)
        {
            // FIXME try...catch, see ListenerService
            // and remove the handler if all fails

            var correlationId = _correlationIdSequence.Next;
            _correlatedSubscriptions[correlationId] = subscription;

            // we do not control the original subscription.SubscribeRequest message and it may
            // be used concurrently, and so it is not safe to alter its correlation identifier.
            // instead, we use a safe clone of the original message
            var subscribeRequest = subscription.SubscribeRequest.CloneWithNewCorrelationId(correlationId);

            try
            {
                var response = await client.SendAsync(subscribeRequest, correlationId);
                _ = subscription.AcceptSubscribeResponse(response, client);
            }
            catch
            {
                _correlatedSubscriptions.TryRemove(correlationId, out _);
                throw;
            }
        }

        /// <summary>
        /// Subscribes a client to server events.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>A task that will complete when the client has subscribed to server events.</returns>
        private async Task InstallSubscriptionsOnNewClient(Client client)
        {
            // FIXME what-if some subscriptions fail?

            foreach (var (_, subscription) in _eventSubscriptions)
                await InstallSubscriptionAsync(subscription, client);
        }

        /// <summary>
        /// Removes a subscription on the cluster.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <returns>A task that will complete when subscription has been removed.</returns>
        public async Task RemoveSubscriptionAsync(Guid subscriptionId)
        {
            if (!_eventSubscriptions.TryGetValue(subscriptionId, out var clusterSubscription))
                throw new Exception();

            // FIXME if a client goes away do we remove it from the list?!

            foreach (var (_, clientSubscription) in clusterSubscription.ClientSubscriptions)
            {
                // can we just ignore whatever is returned?
                // fixme try...catch
                await RemoveSubscriptionAsync(clientSubscription);
            }

            _eventSubscriptions.TryRemove(subscriptionId, out _);
        }

        /// <summary>
        /// Removes a subscription on one client.
        /// </summary>
        /// <param name="clientSubscription">The subscription.</param>
        /// <returns>A task that will complete when the client has unsubscribed from the server event.</returns>
        private async ValueTask RemoveSubscriptionAsync(ClientSubscription clientSubscription)
        {
            try
            {
                // ignore the response
                await clientSubscription.Client.SendAsync(clientSubscription.ClusterSubscription.CreateUnsubscribeRequest(clientSubscription.ServerSubscriptionId));
            }
            finally
            {
                // whatever happens, remove the event handler
                // if the client hasn't properly unsubscribed, it may receive more event messages,
                // which will be ignored since their correlation identifier won't match any handler.
                _correlatedSubscriptions.TryRemove(clientSubscription.CorrelationId, out _);
            }
        }

        /// <summary>
        /// Clears the subscriptions of client that is lost.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>A task that will complete when the client has unsubscribed from server events.</returns>
        private async Task ClearLostClientSubscriptions(Client client)
        {
            foreach (var (_, eventSubscription) in _eventSubscriptions)
            {
                if (eventSubscription.ClientSubscriptions.TryRemove(client, out var clientSubscription))
                    _correlatedSubscriptions.TryRemove(clientSubscription.CorrelationId, out _);
            }
        }
    }
}
