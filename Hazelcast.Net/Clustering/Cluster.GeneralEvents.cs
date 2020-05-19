using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Microsoft.Extensions.Logging;

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
            // capture clients, and register the subscription - atomically.
            // if new clients are added, we won't deal with them here, but they will
            // subscribe on their own since the subscription is now lister.
            // if a captured client goes away while we install subscriptions, we
            // will just ignore the associated errors and skip it entirely.
            //
            List<Client> clients;
            lock (_clientsLock)
            {
                clients = _clients.Values.ToList();

                if (!_subscriptions.TryAdd(subscription.Id, subscription))
                    throw new InvalidOperationException("A subscription with the same identifier already exists.");
            }

            Exception exception = null;

            // subscribe each client
            // TODO: could we install in parallel?
            foreach (var client in clients)
            {
                try
                {
                    await InstallSubscriptionAsync(subscription, client);
                }
                catch (Exception e)
                {
                    // FIXME: if it throws because the client is gone, that's ok!
                    exception = e;
                    break; // no need to continue, we're going to fail anyways
                }
            }

            // success?
            if (exception == null)
                return;

            // otherwise, remove whatever has been installed
            try
            {
                await RemoveTemp(subscription);

            }
            catch
            {
                // in any case, remove the subscription
                // one member may still believe there's a subscribed client, and send events,
                // which will be ignored - accepting this for the moment, though later on
                // we may want to try again later?
                _subscriptions.TryRemove(subscription.Id, out _);
                _logger.LogWarning("Failed to subscribe, and failed to properly clean things up.");
            }

            throw new HazelcastException("Failed to subscribe.", exception);
        }

        /// <summary>
        /// Installs a subscription on one client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="subscription">The subscription.</param>
        /// <returns>A task that will complete when the client has subscribed to the server event.</returns>
        private async ValueTask InstallSubscriptionAsync(ClusterSubscription subscription, Client client)
        {
            // if we already know the client is not active anymore, ignore it
            // otherwise, install on this client - may throw if the client goes away in the meantime
            if (!client.Active)
                return;

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
                // hopefully the client is still active, else this will throw
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
            // install all active subscriptions
            Exception exception = null;
            foreach (var (_, subscription) in _subscriptions)
            {
                if (!subscription.Active) continue;
                try
                {
                    await InstallSubscriptionAsync(subscription, client);
                }
                catch (Exception e)
                {
                    exception = e;
                    break;
                }
            }

            // success?
            if (exception == null)
                return;

            // otherwise... throw
            // meaning the client is not properly initiated and should be killed

            // FIXME: must try to remove existing client subscriptions & cleanup
            throw new HazelcastException("Failed.", exception);
        }

        /// <summary>
        /// Removes a subscription on the cluster.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <returns>A task that will complete when subscription has been removed.</returns>
        /// <remarks>
        /// <para>This may throw in something goes wrong. In this case, the subscription
        /// is de-activated but remains in the lists, so that it is possible to try again.</para>
        /// </remarks>
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

        private async ValueTask RemoveTemp(ClusterSubscription subscription)
        {
            // deactivate the subscription, so that new clients don't use it
            subscription.Deactivate();

            // FIXME if a client goes away do we remove it from the list?!
            // FIXME some locking is required here?!
            // FIXME race condition on deactivate too!

            List<Exception> exceptions = null;
            var allRemoved = true;

            // un-subscribe each client
            foreach (var (_, clientSubscription) in subscription.ClientSubscriptions)
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
                _subscriptions.TryRemove(subscription.Id, out _);
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
