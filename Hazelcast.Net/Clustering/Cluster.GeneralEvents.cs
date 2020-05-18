using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Logging;
using Hazelcast.Messaging;

namespace Hazelcast.Clustering
{
    // partial: general events
    public partial class Cluster
    {
        /// <summary>
        /// Installs a subscription on the cluster.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <returns>A task that will complete when the subscription has been installed.</returns>
        public async Task InstallSubscriptionAsync(ClusterSubscription subscription)
        {
            List<Client> clients;
            lock (_clientsLock)
            {
                // capture clients
                clients = _clients.Values.ToList();

                // register the subscription - but verify that the id really is unique
                if (!_subscriptions.TryAdd(subscription.Id, subscription))
                    throw new InvalidOperationException("A subscription with the same identifier already exists.");
            }

            // if new clients are added now, we won't deal with it here, but they will
            // see the new subscriptions and will handle it just as new clients do
            //
            // if some clients go away now... FIXME: handle clients going away

            List<Exception> exceptions = null;

            // subscribe each client
            foreach (var client in clients)
            {
                try
                {
                    await InstallSubscriptionAsync(subscription, client);
                }
                catch (Exception e)
                {
                    if (exceptions == null) exceptions = new List<Exception>();
                    exceptions.Add(e);
                }
            }

            if (exceptions == null)
                return;

            // deactivate the subscription
            subscription.Deactivate();

            // remove
            // FIXME: leak, we should unregister what we can, etc
            _subscriptions.TryRemove(subscription.Id, out _);
            throw new AggregateException("Failed to subscribe.", exceptions.ToArray());
        }

        /// <summary>
        /// Installs a subscription on one client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="subscription">The subscription.</param>
        /// <returns>A task that will complete when the client has subscribed to the server event.</returns>
        private async ValueTask InstallSubscriptionAsync(ClusterSubscription subscription, Client client)
        {
            // add immediately, we don't know when the events will start to come
            var correlationId = _correlationIdSequence.Next;
            _correlatedSubscriptions[correlationId] = subscription;

            // we do not control the original subscription.SubscribeRequest message and it may
            // be used concurrently, and so it is not safe to alter its correlation identifier.
            // instead, we use a safe clone of the original message
            var subscribeRequest = subscription.SubscribeRequest.CloneWithNewCorrelationId(correlationId);

            ClientMessage response;
            try
            {
                response = await client.SendAsync(subscribeRequest, correlationId);
            }
            catch
            {
                _correlatedSubscriptions.TryRemove(correlationId, out _);
                throw;
            }

            subscription.AddClientSubscription(response, client);
        }

        /// <summary>
        /// Subscribes a client to server events.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>A task that will complete when the client has subscribed to server events.</returns>
        private async Task InstallSubscriptionsOnNewClient(Client client)
        {
            // FIXME what-if some subscriptions fail?
            // FIXME try...catch

            foreach (var (_, subscription) in _subscriptions)
            {
                // ignore inactive subscriptions
                if (!subscription.Active) continue;

                // install
                await InstallSubscriptionAsync(subscription, client); // FIXME may throw!
            }
        }

        /// <summary>
        /// Removes a subscription on the cluster.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <returns>A task that will complete when subscription has been removed.</returns>
        public async ValueTask RemoveSubscriptionAsync(Guid subscriptionId)
        {
            // ignore unknown subscriptions
            if (!_subscriptions.TryGetValue(subscriptionId, out var clusterSubscription))
                return;

            // deactivate the subscription, so that new clients don't use it
            clusterSubscription.Deactivate();

            // FIXME if a client goes away do we remove it from the list?!
            // FIXME some locking is required here?!
            // FIXME race condition on deactivate too!

            List<Exception> exceptions = null;
            var allRemoved = true;

            // un-subscribe each client
            foreach (var (_, clientSubscription) in clusterSubscription.ClientSubscriptions)
            {
                // if one client fails, keep the exception but continue with other clients
                try
                {
                    allRemoved &= await RemoveSubscriptionAsync(clientSubscription);
                }
                catch (Exception e)
                {
                    if (exceptions == null) exceptions = new List<Exception>();
                    exceptions.Add(e);
                    allRemoved = false;
                }
            }

            // if at least an exception was thrown, rethrow
            if (exceptions != null)
                throw new AggregateException("Failed to fully remove the subscription.", exceptions.ToArray());

            // if everything went well, remove the subscription
            // otherwise, keep it around and throw (may want to try again?)
            // in any case client handlers have been removed so no event will be handled
            if (allRemoved)
                _subscriptions.TryRemove(subscriptionId, out _);
            else
                throw new HazelcastException("Failed to fully remove the subscription.");
        }

        /// <summary>
        /// Removes a subscription on one client.
        /// </summary>
        /// <param name="clientSubscription">The subscription.</param>
        /// <returns>Whether the operation was successful.</returns>
        /// <remarks>
        /// <para>This methods always remove the event handlers associated with the subscription, regardless
        /// of the response from the server. Even when the server returns false, meaning it failed to
        /// properly remove the subscription, no events for that subscription will be triggered anymore
        /// because the client will ignore these events when the server sends them.</para>
        /// </remarks>
        private async ValueTask<bool> RemoveSubscriptionAsync(ClientSubscription clientSubscription)
        {
            // whatever happens, remove the event handler
            // if the client hasn't properly unsubscribed, it may receive more event messages,
            // which will be ignored since their correlation identifier won't match any handler.
            _correlatedSubscriptions.TryRemove(clientSubscription.CorrelationId, out _);

            // trigger the server-side un-subscribe
            var responseMessage = await clientSubscription.Client.SendAsync(clientSubscription.ClusterSubscription.CreateUnsubscribeRequest(clientSubscription.ServerSubscriptionId));
            return clientSubscription.ClusterSubscription.DecodeUnsubscribeResponse(responseMessage);
        }

        /// <summary>
        /// Clears the subscriptions of client that is lost.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <returns>A task that will complete when the client has unsubscribed from server events.</returns>
        private void ClearLostClientSubscriptions(Client client)
        {
            // this is for a lost client, so we don't have a connection to the server anymore,
            // so we just clear everything we know about that client, but we cannot properly
            // unsubscribe

            foreach (var (_, subscription) in _subscriptions)
            {
                if (subscription.ClientSubscriptions.TryRemove(client, out var clientSubscription))
                    _correlatedSubscriptions.TryRemove(clientSubscription.CorrelationId, out _);
            }
        }
    }
}
