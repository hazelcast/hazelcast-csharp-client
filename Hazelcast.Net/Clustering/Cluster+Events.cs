using System;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Messaging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents an Hazelcast Cluster.
    /// </summary>
    public partial class Cluster // Events
    {
        /// <summary>
        /// Subscribes the cluster to a server event.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <returns>A task that will complete when the cluster has subscribed to the server event.</returns>
        public async Task SubscribeAsync(ClusterEventSubscription subscription)
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
                foreach (var (_, client) in _clients)
                    await Subscribe(client, subscription);
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
        /// Subscribes a client to server events.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>A task that will complete when the client has subscribed to server events.</returns>
        private async Task SubscribeClientToEvents(Client client)
        {
            // FIXME what-if some subscriptions fail?

            foreach (var (_, subscription) in _eventSubscriptions)
                await Subscribe(client, subscription);
        }

        /// <summary>
        /// Subscribes a client to a server event.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="subscription">The subscription.</param>
        /// <returns>A task that will complete when the client has subscribed to the server event.</returns>
        private async ValueTask Subscribe(Client client, ClusterEventSubscription subscription)
        {
            // FIXME try...catch, see ListenerService
            // and remove the handler if all fails

            var correlationId = _correlationIdSequence.Next;
            _eventHandlers[correlationId] = subscription.EventHandler;

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

            // FIXME if a client goes away do we remove it from the list?!

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
        private async Task UnsubscribeClient(Client client)
        {
            foreach (var (_, eventSubscription) in _eventSubscriptions)
            {
                if (eventSubscription.ClientSubscriptions.TryRemove(client, out var clientSubscription))
                    _eventHandlers.TryRemove(clientSubscription.CorrelationId, out _);
            }
        }

        /// <summary>
        /// Unsubscribe a client from a cluster event.
        /// </summary>
        /// <param name="clientSubscription">The subscription.</param>
        /// <returns>A task that will complete when the client has unsubscribed from the server event.</returns>
        private async ValueTask UnsubscribeClient(ClientEventSubscription clientSubscription)
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
                _eventHandlers.TryRemove(clientSubscription.CorrelationId, out _);
            }
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
    }
}
