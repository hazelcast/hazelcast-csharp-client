// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    public partial class Cluster // GeneralEvents
    {
        /// <summary>
        /// Installs a subscription on the cluster.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the subscription has been installed.</returns>
        public async Task InstallSubscriptionAsync(ClusterSubscription subscription, CancellationToken cancellationToken)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));

            // capture active clients, and adds the subscription - atomically.
            List<Client> clients;
            using (await _clusterLock.AcquireAsync(CancellationToken.None).CAF())
            {
                clients = _clients.Values.Where(x => x.Active).ToList();

                if (!_subscriptions.TryAdd(subscription.Id, subscription))
                    throw new InvalidOperationException("A subscription with the same identifier already exists.");
            }

            // from now on,
            // - if new clients are added, we won't deal with them here, but they will
            //   subscribe on their own since the subscription is now listed.
            // - if a captured client goes away while we install subscriptions, we
            //   will just ignore the associated errors and skip it entirely.

            // subscribe each captured client
            // TODO: could we install in parallel?
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var client in clients)
            {
                // don't even try clients that became inactive
                if (!client.Active) continue;

                // this never throws
                var attempt = await InstallSubscriptionOnClientAsync(subscription, client, cancellationToken).CAF();

                switch (attempt.Result)
                {
                    case InstallResult.Success:
                    case InstallResult.ClientNotActive:
                        continue;

                    case InstallResult.SubscriptionNotActive:
                    case InstallResult.ConfusedServer:
                        // not active: some other code must have
                        // - removed the subscriptions from _subscriptions
                        // - dealt with its existing clients
                        // nothing left to do here
                        throw new HazelcastException(attempt.Result == InstallResult.SubscriptionNotActive
                            ? "Failed to install the subscription because it was removed."
                            : "Failed to install the subscription because it was removed (and the server may be confused).", attempt.Exception);

                    case InstallResult.Failed:
                        // failed: client is active but installing the subscription failed
                        // however, we might have installed it on other clients
                        var allRemoved = await RemoveSubscriptionAsync(subscription, cancellationToken).CAF();
                        throw new HazelcastException(allRemoved
                            ? "Failed to install subscription (see inner exception)."
                            : "Failed to install subscription (see inner exception - and the server may be confused).", attempt.Exception);

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        private async ValueTask<bool> RemoveSubscriptionAsync(ClusterSubscription subscription, CancellationToken cancellationToken)
        {
            subscription.Deactivate();

            List<Exception> exceptions = null;
            var allRemoved = true;

            // un-subscribe each client
            foreach (var clientSubscription in subscription)
            {
                // if one client fails, keep the exception but continue with other clients
                try
                {
                    // this does
                    // - remove the correlated subscription
                    // - tries to properly unsubscribe from the server
                    allRemoved &= await RemoveSubscriptionAsync(clientSubscription, cancellationToken).CAF();
                }
                catch (Exception e)
                {
                    exceptions ??= new List<Exception>();
                    exceptions.Add(e);
                    allRemoved = false;
                }
            }

            // remove the subscription
            // so whatever happens, nothing remains of the original subscription
            // (but the client may be "dirty" ie the server may still think it needs
            // to send events, which will be ignored)
            using (await _clusterLock.AcquireAsync(CancellationToken.None).CAF())
            {
                _subscriptions.TryRemove(subscription.Id, out _);
            }
            return allRemoved;
        }

        /// <summary>
        /// Subscribes a client to server events.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="subscriptions">The subscriptions</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the client has subscribed to server events.</returns>
        private async Task InstallSubscriptionsOnNewClient(Client client, IReadOnlyCollection<ClusterSubscription> subscriptions, CancellationToken cancellationToken)
        {
            // the client has been added to _clients, and subscriptions have been
            // captured already, all within a _clientsLock, but the caller

            // install all active subscriptions
            foreach (var subscription in subscriptions)
            {
                // don't even try subscriptions that became inactive
                if (!subscription.Active) continue;

                // this never throws
                var attempt = await InstallSubscriptionOnClientAsync(subscription, client, cancellationToken).CAF();

                switch (attempt.Result)
                {
                    case InstallResult.Success:
                    case InstallResult.SubscriptionNotActive:
                        continue;

                    case InstallResult.ClientNotActive:
                        // not active: some other code must have:
                        // - removed the client from _clients
                        // - dealt with its existing subscriptions
                        // nothing left to do here
                        throw new HazelcastException("Failed to install the new client because it was removed.");

                    case InstallResult.ConfusedServer:
                        // same as subscription not active, but we failed to remove the subscription
                        // on the server side - the client is dirty - just kill it entirely
                        ClearClientSubscriptions(subscriptions, client);
                        throw new HazelcastException("Failed to install the new client.");

                    case InstallResult.Failed:
                        // failed to talk to the client - nothing works - kill it entirely
                        ClearClientSubscriptions(subscriptions, client);
                        throw new HazelcastException("Failed to install the new client.");

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        private void ClearClientSubscriptions(IEnumerable<ClusterSubscription> subscriptions, Client client)
        {
            foreach (var subscription in subscriptions)
            {
                // remove the correlated subscription
                // remove the client subscription
                if (subscription.TryRemove(client, out var clientSubscription))
                    _correlatedSubscriptions.TryRemove(clientSubscription.CorrelationId, out _);
            }
        }

        /// <summary>
        /// Installs a subscription on one client.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="subscription">The subscription.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the client has subscribed to the server event.</returns>
        private async ValueTask<InstallAttempt> InstallSubscriptionOnClientAsync(ClusterSubscription subscription, Client client, CancellationToken cancellationToken)
        {
            // if we already know the client is not active anymore, ignore it
            // otherwise, install on this client - may throw if the client goes away in the meantime
            if (!client.Active) return new InstallAttempt(InstallResult.ClientNotActive);

            // add immediately, we don't know when the events will start to come
            var correlationId = _correlationIdSequence.GetNext();
            _correlatedSubscriptions[correlationId] = subscription;

            // we do not control the original subscription.SubscribeRequest message and it may
            // be used concurrently, and so it is not safe to alter its correlation identifier.
            // instead, we use a safe clone of the original message
            var subscribeRequest = subscription.SubscribeRequest.CloneWithNewCorrelationId(correlationId);

            ClientMessage response;
            try
            {
                // hopefully the client is still active, else this will throw
                response = await SendToClientAsync(subscribeRequest, client, correlationId, cancellationToken).CAF();
            }
            catch (Exception e)
            {
                _correlatedSubscriptions.TryRemove(correlationId, out _);
                if (!client.Active) return new InstallAttempt(InstallResult.ClientNotActive);

                _logger.LogError(e, "Caught exception while cleaning up after failing to install a subscription.");
                return new InstallAttempt(InstallResult.Failed, e);
            }

            // try to add the client subscription
            var (added, id) = subscription.TryAddClientSubscription(response, client);
            if (added) return InstallAttempt.Success;

            // otherwise, the client subscription could not be added, which means that the
            // cluster subscription is not active anymore, and so we need to undo the
            // server-side subscription

            // if the client is gone already it may be that the subscription has been
            // removed already, in which case... just give up now
            if (!_correlatedSubscriptions.TryRemove(correlationId, out _))
                return new InstallAttempt(InstallResult.SubscriptionNotActive);

            var unsubscribeRequest = subscription.CreateUnsubscribeRequest(id);

            try
            {
                var unsubscribeResponse = await SendToClientAsync(unsubscribeRequest, client, cancellationToken).CAF();
                var unsubscribed = subscription.DecodeUnsubscribeResponse(unsubscribeResponse);
                return unsubscribed
                    ? new InstallAttempt(InstallResult.SubscriptionNotActive)
                    : new InstallAttempt(InstallResult.ConfusedServer);
            }
            catch (Exception e)
            {
                // otherwise, we failed to undo the server-side subscription - end result is that
                // the client is fine (won't handle events, we've removed the correlated subscription
                // etc) but the server maybe confused.
                _logger.LogError(e, "Caught exception while cleaning up after failing to install a subscription.");
                return new InstallAttempt(InstallResult.ConfusedServer, e);
            }
        }

        /// <summary>
        /// Removes a subscription on the cluster.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when subscription has been removed.</returns>
        /// <remarks>
        /// <para>This may throw in something goes wrong. In this case, the subscription
        /// is de-activated but remains in the lists, so that it is possible to try again.</para>
        /// </remarks>
        public async ValueTask RemoveSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken)
        {
            // ignore unknown subscriptions
            // don't remove it now - will remove it only if all goes well
            ClusterSubscription subscription;
            using (await _clusterLock.AcquireAsync(CancellationToken.None).CAF())
            {
                if (!_subscriptions.TryGetValue(subscriptionId, out subscription))
                    return;
            }

            // deactivate the subscription, so that new clients don't use it
            subscription.Deactivate();

            List<Exception> exceptions = null;
            var allRemoved = true;

            // un-subscribe each client
            foreach (var clientSubscription in subscription)
            {
                // if one client fails, keep the exception but continue with other clients
                try
                {
                    // this does
                    // - remove the correlated subscription
                    // - tries to properly unsubscribe from the server
                    allRemoved &= await RemoveSubscriptionAsync(clientSubscription, cancellationToken).CAF();
                }
                catch (Exception e)
                {
                    exceptions ??= new List<Exception>();
                    exceptions.Add(e);
                    allRemoved = false;
                }
            }

            // remove the subscription
            // so whatever happens, nothing remains of the original subscription
            // (but the client may be "dirty" ie the server may still think it needs
            // to send events, which will be ignored)
            using (await _clusterLock.AcquireAsync(CancellationToken.None).CAF())
            {
                _subscriptions.TryRemove(subscription.Id, out _);
            }

            // if at least an exception was thrown, rethrow
            if (exceptions != null)
                throw new AggregateException("Failed to fully remove the subscription (and the server may be confused).", exceptions.ToArray());

            // if !allRemoved, everything has been removed from the client side,
            // but the server may still think it needs to send events, so it's kinda dirty.
            if (!allRemoved)
                throw new HazelcastException("Failed to fully remove the subscription (and the server may be confused).");
        }

        /// <summary>
        /// Removes a subscription on one client.
        /// </summary>
        /// <param name="clientSubscription">The subscription.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Whether the operation was successful.</returns>
        /// <remarks>
        /// <para>This methods always remove the event handlers associated with the subscription, regardless
        /// of the response from the server. Even when the server returns false, meaning it failed to
        /// properly remove the subscription, no events for that subscription will be triggered anymore
        /// because the client will ignore these events when the server sends them.</para>
        /// </remarks>
        private async ValueTask<bool> RemoveSubscriptionAsync(ClientSubscription clientSubscription, CancellationToken cancellationToken)
        {
            // whatever happens, remove the event handler
            // if the client hasn't properly unsubscribed, it may receive more event messages,
            // which will be ignored since their correlation identifier won't match any handler.
            _correlatedSubscriptions.TryRemove(clientSubscription.CorrelationId, out _);

            // trigger the server-side un-subscribe
            var unsubscribeRequest = clientSubscription.ClusterSubscription.CreateUnsubscribeRequest(clientSubscription.ServerSubscriptionId);
            var responseMessage = await SendToClientAsync(unsubscribeRequest, clientSubscription.Client, cancellationToken).CAF();
            return clientSubscription.ClusterSubscription.DecodeUnsubscribeResponse(responseMessage);
        }
    }
}
