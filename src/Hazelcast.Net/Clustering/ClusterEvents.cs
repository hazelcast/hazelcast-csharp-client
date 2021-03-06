﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Events;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Models;
using Hazelcast.Protocol.Codecs;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Provides the cluster events service for a cluster.
    /// </summary>
    internal partial class ClusterEvents : IAsyncDisposable
    {
        private readonly ClusterState _clusterState;
        private readonly ClusterMessaging _clusterMessaging;
        private readonly ClusterMembers _clusterMembers;

        private readonly ILogger _logger;
        private readonly DistributedEventScheduler _scheduler;

        private readonly object _ghostLock = new object();

        private Func<ValueTask> _partitionsUpdated;
        private Func<MembersUpdatedEventArgs, ValueTask> _membersUpdated;


        private MemberConnection _clusterEventsConnection; // the client which handles 'cluster events'
        private long _clusterEventsCorrelationId; // the correlation id of the 'cluster events'
        private Task _clusterEventsTask; // the task that ensures there is a client to handle 'cluster events'
        private Task _ghostTask; // the task that clears ghost subscriptions

        // subscription id -> subscription
        // the master subscriptions list
        private readonly ConcurrentDictionary<Guid, ClusterSubscription> _subscriptions = new ConcurrentDictionary<Guid, ClusterSubscription>();

        // correlation id -> subscription
        // used to match a subscription to an incoming event message
        // each client has its own correlation id, so there can be many entries per cluster subscription
        private readonly ConcurrentDictionary<long, ClusterSubscription> _correlatedSubscriptions = new ConcurrentDictionary<long, ClusterSubscription>();

        // ghost subscriptions
        // subscriptions that have failed to properly unsubscribe and now we need to take care of them
        private readonly HashSet<ClusterSubscription> _ghostSubscriptions = new HashSet<ClusterSubscription>();

        public ClusterEvents(ClusterState clusterState, ClusterMessaging clusterMessaging, ClusterMembers clusterMembers)
        {
            _clusterState = clusterState;
            _clusterMessaging = clusterMessaging;
            _clusterMembers = clusterMembers;

            _logger = _clusterState.LoggerFactory.CreateLogger<ClusterEvents>();
            _scheduler = new DistributedEventScheduler(_clusterState.LoggerFactory);
        }

        /// <summary>
        /// (internal for tests only)
        /// Gets the subscriptions.
        /// </summary>
        internal ConcurrentDictionary<Guid, ClusterSubscription> Subscriptions => _subscriptions;

        /// <summary>
        /// (internal for tests only)
        /// Gets the correlated subscriptions.
        /// </summary>
        internal ConcurrentDictionary<long, ClusterSubscription> CorrelatedSubscriptions => _correlatedSubscriptions;

        /// <summary>
        /// (internal for tests only)
        /// Gets the ghost subscriptions.
        /// </summary>
        internal HashSet<ClusterSubscription> GhostSubscriptions => _ghostSubscriptions;

        /// <summary>
        /// Installs a subscription on the cluster, i.e. on each member.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the subscription has been installed.</returns>
        public async Task InstallSubscriptionAsync(ClusterSubscription subscription, CancellationToken cancellationToken = default)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));

            // capture active clients, and adds the subscription - atomically.
            List<MemberConnection> connections;
            lock (_clusterState.Mutex)
            {
                connections = _clusterMembers.SnapshotConnections(true);

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
            foreach (var connection in connections)
            {
                // don't even try clients that became inactive
                if (!connection.Active) continue;

                // this never throws
                var attempt = await InstallSubscriptionAsync(subscription, connection, cancellationToken).CfAwait();

                switch (attempt.Value)
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
                        throw new HazelcastException(attempt.Value == InstallResult.SubscriptionNotActive
                            ? "Failed to install the subscription because it was removed."
                            : "Failed to install the subscription because it was removed (and the server may be confused).", attempt.Exception);

                    case InstallResult.Failed:
                        // failed: client is active but installing the subscription failed
                        // however, we might have installed it on other clients
                        await RemoveSubscriptionAsync(subscription, cancellationToken).CfAwait();
                        throw new HazelcastException("Failed to install subscription (see inner exception).", attempt.Exception);

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Installs a subscription on one member.
        /// </summary>
        /// <param name="connection">The connection to the member.</param>
        /// <param name="subscription">The subscription.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the client has subscribed to the server event.</returns>
        private async ValueTask<Attempt<InstallResult>> InstallSubscriptionAsync(ClusterSubscription subscription, MemberConnection connection, CancellationToken cancellationToken)
        {
            // if we already know the client is not active anymore, ignore it
            // otherwise, install on this client - may throw if the client goes away in the meantime
            if (!connection.Active) return Attempt.Fail(InstallResult.ClientNotActive);

            // add immediately, we don't know when the events will start to come
            var correlationId = _clusterState.GetNextCorrelationId();
            _correlatedSubscriptions[correlationId] = subscription;

            // we do not control the original subscription.SubscribeRequest message and it may
            // be used concurrently, and so it is not safe to alter its correlation identifier.
            // instead, we use a safe clone of the original message
            var subscribeRequest = subscription.SubscribeRequest.CloneWithNewCorrelationId(correlationId);

            ClientMessage response;
            try
            {
                // hopefully the client is still active, else this will throw
                response = await _clusterMessaging.SendToMemberAsync(subscribeRequest, connection, correlationId, cancellationToken).CfAwait();
            }
            catch (Exception e)
            {
                _correlatedSubscriptions.TryRemove(correlationId, out _);
                if (!connection.Active) return Attempt.Fail(InstallResult.ClientNotActive);

                _logger.LogError(e, "Caught exception while cleaning up after failing to install a subscription.");
                return Attempt.Fail(InstallResult.Failed, e);
            }

            // try to add the client subscription
            var (added, id) = subscription.TryAddClientSubscription(response, connection);
            if (added) return InstallResult.Success;

            // otherwise, the client subscription could not be added, which means that the
            // cluster subscription is not active anymore, and so we need to undo the
            // server-side subscription

            // if the client is gone already it may be that the subscription has been
            // removed already, in which case... just give up now
            if (!_correlatedSubscriptions.TryRemove(correlationId, out _))
                return Attempt.Fail(InstallResult.SubscriptionNotActive);

            var unsubscribeRequest = subscription.CreateUnsubscribeRequest(id);

            try
            {
                var unsubscribeResponse = await _clusterMessaging.SendToMemberAsync(unsubscribeRequest, connection, cancellationToken).CfAwait();
                var unsubscribed = subscription.ReadUnsubscribeResponse(unsubscribeResponse);
                return unsubscribed
                    ? Attempt.Fail(InstallResult.SubscriptionNotActive)
                    : Attempt.Fail(InstallResult.ConfusedServer);
            }
            catch (Exception e)
            {
                // otherwise, we failed to undo the server-side subscription - end result is that
                // the client is fine (won't handle events, we've removed the correlated subscription
                // etc) but the server maybe confused.
                _logger.LogError(e, "Caught exception while cleaning up after failing to install a subscription.");
                return Attempt.Fail(InstallResult.ConfusedServer, e);
            }
        }

        /// <summary>
        /// Removes a subscription from the cluster, i.e. from each member.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Whether the subscription was removed.</returns>
        /// <remarks>
        /// <para>This may throw in something goes wrong. In this case, the subscription
        /// is de-activated but remains in the lists, so that it is possible to try again.</para>
        /// </remarks>
        public async ValueTask<bool> RemoveSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
        {
            // don't remove it now - will remove it only if all goes well
            ClusterSubscription subscription;
            lock (_clusterState.Mutex)
            {
                if (!_subscriptions.TryGetValue(subscriptionId, out subscription))
                    return false; // unknown subscription
            }

            await RemoveSubscriptionAsync(subscription, cancellationToken).CfAwait();
            return true;
        }

        /// <summary>
        /// Removes a subscription from the cluster, i.e. from each member.
        /// </summary>
        /// <param name="subscription">The subscription to remove.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns></returns>
        private async ValueTask RemoveSubscriptionAsync(ClusterSubscription subscription, CancellationToken cancellationToken = default)
        {
            // de-activate the subscription: events received from members will *not* be processed anymore,
            // even if we receive more event messages from the servers
            subscription.Deactivate();

            var allRemoved = await RemoveMemberSubscriptionsAsync(subscription, cancellationToken).CfAwait();

            // remove the subscription, but if some member-level subscriptions could not
            // be removed, adds the subscription to the list of ghost subscriptions.
            lock (_clusterState.Mutex)
            {
                _subscriptions.TryRemove(subscription.Id, out _);
            }

            if (allRemoved) return;

            lock (_ghostLock)
            {
                _ghostSubscriptions.Add(subscription);
                _ghostTask ??= CollectSubscriptionsAsync(_clusterState.CancellationToken);
            }
        }

        private async ValueTask<bool> RemoveMemberSubscriptionsAsync(ClusterSubscription subscription, CancellationToken cancellationToken = default)
        {
            var allRemoved = true;

            // un-subscribe each client
            var removedMemberSubscriptions = new List<MemberSubscription>();
            foreach (var memberSubscription in subscription)
            {
                // if one client fails, keep the exception but continue with other clients
                try
                {
                    // this does
                    // - remove the correlated subscription
                    // - tries to properly unsubscribe from the server
                    var removed = await RemoveMemberSubscriptionAsync(memberSubscription, cancellationToken).CfAwait();
                    if (removed)
                        removedMemberSubscriptions.Add(memberSubscription);
                    else
                        allRemoved = false;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Caught an exception while unsubscribing to events.");
                    allRemoved = false;
                }
            }

            // remove those that have effectively been removed
            foreach (var memberSubscription in removedMemberSubscriptions)
                subscription.Remove(memberSubscription);

            return allRemoved;
        }

        /// <summary>
        /// Removes a subscription from one member.
        /// </summary>
        /// <param name="memberSubscription">The subscription.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Whether the subscription was removed.</returns>
        /// <remarks>
        /// <para>This methods always remove the event handlers associated with the subscription, regardless
        /// of the response from the server. Even when the server returns false, meaning it failed to
        /// properly remove the subscription, no events for that subscription will be triggered anymore
        /// because the client will ignore these events when the server sends them.</para>
        /// </remarks>
        private async ValueTask<bool> RemoveMemberSubscriptionAsync(MemberSubscription memberSubscription, CancellationToken cancellationToken = default)
        {
            // always remove the correlated subscription
            _correlatedSubscriptions.TryRemove(memberSubscription.CorrelationId, out _);

            // trigger the server-side un-subscribe
            var unsubscribeRequest = memberSubscription.ClusterSubscription.CreateUnsubscribeRequest(memberSubscription.ServerSubscriptionId);
            var responseMessage = await _clusterMessaging.SendToMemberAsync(unsubscribeRequest, memberSubscription.Connection, cancellationToken).CfAwait();
            var removed = memberSubscription.ClusterSubscription.ReadUnsubscribeResponse(responseMessage);

            return removed;
        }

        private async Task CollectSubscriptionsAsync(CancellationToken cancellationToken)
        {
            List<ClusterSubscription> gone = null;

            HConsole.WriteLine(this, "CollectSubscription starting");

            // if canceled, will be awaited properly
            await Task.Delay(_clusterState.Options.Events.SubscriptionCollectDelay, cancellationToken).CfAwait();

            while (true)
            {
                // capture ghosts subscriptions
                List<ClusterSubscription> ghosts;
                lock (_ghostLock)
                {
                    ghosts = _ghostSubscriptions.ToList();
                }

                HConsole.WriteLine(this, $"CollectSubscription loop for {ghosts.Count} subscriptions");

                // try to remove captures subscriptions
                // if canceled, will be awaited properly
                gone?.Clear();
                var timeLimit = DateTime.Now - _clusterState.Options.Events.SubscriptionCollectTimeout;
                foreach (var subscription in ghosts)
                {
                    HConsole.WriteLine(this, "CollectSubscription collects");

                    try
                    {
                        var allRemoved = await RemoveMemberSubscriptionsAsync(subscription, cancellationToken).CfAwait();
                        if (allRemoved || subscription.DeactivateTime < timeLimit)
                            (gone ??= new List<ClusterSubscription>()).Add(subscription);
                    }
                    catch (OperationCanceledException)
                    {
                        return; // cancelled - stop everything
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "An error occurred while collecting subscriptions.");
                    }
                }

                HConsole.WriteLine(this, $"CollectSubscription collected {gone?.Count??0} subscriptions");

                // update ghost subscriptions
                // none remaining = exit the task
                lock (_ghostLock)
                {
                    if (gone != null)
                    {
                        foreach (var subscription in gone)
                            _ghostSubscriptions.Remove(subscription);
                    }

                    if (_ghostSubscriptions.Count == 0)
                    {
                        HConsole.WriteLine(this, "CollectSubscription exits");
                        _ghostTask = null;
                        return;
                    }
                }

                HConsole.WriteLine(this, "CollectSubscription waits");

                // else, wait + loop / try again
                // if canceled, will be awaited properly
                await Task.Delay(_clusterState.Options.Events.SubscriptionCollectPeriod, cancellationToken).CfAwait();
            }
        }



        /// <summary>
        /// Installs existing subscriptions on a new member.
        /// </summary>
        /// <param name="connection">The connection to the new member.</param>
        /// <param name="subscriptions">The subscriptions</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the client has subscribed to server events.</returns>
        private async Task InstallSubscriptionsOnNewMember(MemberConnection connection, IReadOnlyCollection<ClusterSubscription> subscriptions, CancellationToken cancellationToken)
        {
            // the client has been added to _clients, and subscriptions have been
            // captured already, all within a _clientsLock, but the caller...?

            // install all active subscriptions
            foreach (var subscription in subscriptions)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // don't even try subscriptions that became inactive
                if (!subscription.Active) continue;

                // this never throws
                var attempt = await InstallSubscriptionAsync(subscription, connection, cancellationToken).CfAwait();

                switch (attempt.Value)
                {
                    case InstallResult.Success:
                    case InstallResult.SubscriptionNotActive:
                        continue;

                    case InstallResult.ClientNotActive:
                        // not active: some other code must have:
                        // - removed the client from _clients
                        // - dealt with its existing subscriptions
                        // nothing left to do here
                        throw new HazelcastException("Failed to install the new connection (removed).");

                    case InstallResult.ConfusedServer:
                        // same as subscription not active, but we failed to remove the subscription
                        // on the server side - the client is dirty - just kill it entirely
                        ClearMemberSubscriptions(subscriptions, connection);
                        throw new HazelcastException("Failed to install the new connection (confused).");

                    case InstallResult.Failed:
                        // failed to talk to the client - nothing works - kill it entirely
                        ClearMemberSubscriptions(subscriptions, connection);
                        throw new HazelcastException("Failed to install the new connection (failed).");

                    default:
                        throw new NotSupportedException();
                }
            }
        }



        /// <summary>
        /// Clears the subscriptions of a member that is gone fishing.
        /// </summary>
        /// <param name="subscriptions">Cluster subscriptions.</param>
        /// <param name="connection">The connection to the member.</param>
        private void ClearMemberSubscriptions(IEnumerable<ClusterSubscription> subscriptions, MemberConnection connection)
        {
            foreach (var subscription in subscriptions)
            {
                // remove the correlated subscription
                // remove the client subscription
                if (subscription.TryRemove(connection, out var clientSubscription))
                    _correlatedSubscriptions.TryRemove(clientSubscription.CorrelationId, out _);
            }
        }



        /// <summary>
        /// Clears the connection currently handling cluster events, if it matches the specified <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">A connection.</param>
        /// <returns><c>true</c> if the current connection matched the specified connection, and was cleared; otherwise <c>false</c>.</returns>
        private bool ClearClusterEventsConnectionWithLock(MemberConnection connection)
        {
            // if the specified client is *not* the cluster events client, ignore
            if (_clusterEventsConnection != connection)
                return false;

            // otherwise, clear the cluster event client
            _clusterEventsConnection = null;
            _correlatedSubscriptions.TryRemove(_clusterEventsCorrelationId, out _);
            _clusterEventsCorrelationId = 0;
            return true;
        }

        /// <summary>
        /// Starts the task that ensures that a connection handles cluster events, if that task is not already running.
        /// </summary>
        /// <param name="connection">A candidate connection.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        private void StartSetClusterEventsConnectionWithLock(MemberConnection connection, CancellationToken cancellationToken)
        {
            // there can only be one instance of that task running at a time
            // and it runs in the background, and at any time any connection could
            // shutdown, which might clear the current cluster event connection
            //
            // the task self-removes itself when it ends

            _clusterEventsTask ??= SetClusterEventsConnectionAsync(connection, cancellationToken);
        }

        /// <summary>
        /// Sets a connection to handle cluster events.
        /// </summary>
        /// <param name="connection">An optional candidate connection.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when a new client has been assigned to handle cluster events.</returns>
        private async Task SetClusterEventsConnectionAsync(MemberConnection connection, CancellationToken cancellationToken)
        {
            // TODO: throttle
            
            // this will only exit once a client is assigned, or the task is
            // cancelled, when the cluster goes down (and never up again)
            while (!cancellationToken.IsCancellationRequested)
            {
                connection ??= await _clusterMembers.WaitRandomConnection(cancellationToken).CfAwait();

                // try to subscribe, relying on the default invocation timeout,
                // so this is not going to last forever - we know it will end
                var correlationId = _clusterState.GetNextCorrelationId();
                if (!await SubscribeToClusterEventsAsync(connection, correlationId, cancellationToken).CfAwait()) // does not throw
                {
                    // FIXME tests fail
                    // because of ClientOfflineException in ClusterMessaging.SendToMember
                    // ClusterState.GetLinkedCancellation -> ClientOfflineException
                    // but, if it's offline, how come we have a connection from WaitRandomConnection?!
                    
                    // failed => try another client
                    connection = null;
                    continue;
                }

                // success!
                lock (_clusterState.Mutex)
                {
                    _clusterEventsConnection = connection;
                    _clusterEventsCorrelationId = correlationId;

                    // avoid race conditions, this task is going to end, and if the
                    // client dies we want to be sure we restart the task
                    _clusterEventsTask = null;
                }

                break;
            }
        }

        /// <summary>
        /// Subscribes a connection to cluster events.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the subscription has been processed, and represent whether it was successful.</returns>
        private async Task<bool> SubscribeToClusterEventsAsync(MemberConnection connection, long correlationId, CancellationToken cancellationToken)
        {
            // aka subscribe to member/partition view events
            HConsole.TraceLine(this, "subscribe");

            // handles the event
            ValueTask HandleEventAsync(ClientMessage message, object _)
                => ClientAddClusterViewListenerCodec.HandleEventAsync(message,
                    HandleCodecMemberViewEvent,
                    HandleCodecPartitionViewEvent,
                    connection.Id,
                    _clusterState.LoggerFactory);

            try
            {
                var subscribeRequest = ClientAddClusterViewListenerCodec.EncodeRequest();
                _correlatedSubscriptions[correlationId] = new ClusterSubscription(HandleEventAsync);
                _ = await _clusterMessaging.SendToMemberAsync(subscribeRequest, connection, correlationId, cancellationToken).CfAwait();
                HConsole.WriteLine(this, "subscribed");
                return true;
            }
            catch (Exception e)
            {
                HConsole.WriteLine(this, "failed " + e);
                _correlatedSubscriptions.TryRemove(correlationId, out _);
                _logger.LogWarning(e, "Failed to subscribe to cluster events, may retry.");
                return false;
            }
        }

        /// <summary>
        /// Handles the 'members view' event.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="members">The members.</param>
        /// <param name="state">A state object.</param>
        private async ValueTask HandleCodecMemberViewEvent(int version, ICollection<MemberInfo> members, object state)
        {
            var eventArgs = await _clusterMembers.NotifyMembersView(version, members).CfAwait();

            // raise events (On... does not throw)
            await _membersUpdated.AwaitEach(eventArgs).CfAwait();
        }

        /// <summary>
        /// Handles the 'partitions view' event.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="partitions">The partitions.</param>
        /// <param name="state">A state object.</param>
        private async ValueTask HandleCodecPartitionViewEvent(int version, IList<KeyValuePair<Guid, IList<int>>> partitions, object state)
        {
            var clientId = (Guid) state;

            var updated = _clusterState.Partitioner.NotifyPartitionView(clientId, version, MapPartitions(partitions));
            if (!updated) return;

            // signal once
            //if (Interlocked.CompareExchange(ref _firstPartitionsViewed, 1, 0) == 0)
            //    _firstPartitionsView.Release();

            // raise event
            // On... does not throw
            await _partitionsUpdated.AwaitEach().CfAwait();
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

        #region Events

        /// <summary>
        /// Gets or sets an action that will be executed when members have been updated.
        /// </summary>
        public Func<MembersUpdatedEventArgs, ValueTask> MembersUpdated
        {
            get => _membersUpdated;
            set
            {
                _clusterState.ThrowIfPropertiesAreReadOnly();
                _membersUpdated = value;
            }
        }

        /// <summary>
        /// Gets or sets an action that will be executed when partitions have been updated.
        /// </summary>
        public Func<ValueTask> PartitionsUpdated
        {
            get => _partitionsUpdated;
            set
            {
                _clusterState.ThrowIfPropertiesAreReadOnly();
                _partitionsUpdated = value;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles a connection being created.
        /// </summary>
        /// <param name="connection"></param>
        public void OnConnectionCreated(MemberConnection connection)
        {
            // wires reception of event messages
            connection.ReceivedEvent += OnReceivedEvent;
        }

        /// <summary>
        /// Handles a connection being opened.
        /// </summary>
        public ValueTask OnConnectionOpened(MemberConnection connection, bool isFirst, bool isNewCluster)
        {
            // if we don't have a connection for cluster events yet, start a
            // single, cluster-wide task ensuring there is one
            if (_clusterEventsConnection == null)
                StartSetClusterEventsConnectionWithLock(connection, _clusterState.CancellationToken);

            // start the background initialization of the connection, installs subscriptions on
            // the corresponding member, this happens in the background (we do not wait on it) and
            // should it fail, the connection would be terminated
            // yes, it means that the connection could be used even before all subscriptions have
            // been installed and then some events may fail to trigger - we don't offer any strict
            // guarantee that events will trigger, anyways
            var subscriptions = _subscriptions.Values.Where(x => x.Active).ToList();
            connection.StartBackgroundTask(token => InstallSubscriptionsOnNewMember(connection, subscriptions, token));

            return default;
        }

        /// <summary>
        /// Handles a connection being closed.
        /// </summary>
        public ValueTask OnConnectionClosed(MemberConnection connection, bool wasLast)
        {
            // just clear subscriptions, cannot unsubscribes from the server since
            // the client is not connected anymore
            var subscriptions = _subscriptions.Values.ToList();
            ClearMemberSubscriptions(subscriptions, connection);

            // if the client was the cluster client, and we have more client, start a
            // single, cluster-wide task ensuring there is a cluster events client
            if (ClearClusterEventsConnectionWithLock(connection) && !wasLast)
                StartSetClusterEventsConnectionWithLock(connection, _clusterState.CancellationToken);

            return default;
        }

        /// <summary>
        /// Handles an event message.
        /// </summary>
        /// <param name="message">The event message.</param>
        public void OnReceivedEvent(ClientMessage message)
        {
            HConsole.WriteLine(this, "Handle event message");

            // get the matching subscription
            if (!_correlatedSubscriptions.TryGetValue(message.CorrelationId, out var subscription))
            {
                _clusterState.Instrumentation.CountMissedEvent(message);
                _logger.LogWarning($"No event handler for [{message.CorrelationId}]");
                HConsole.WriteLine(this, $"No event handler for [{message.CorrelationId}]");
                return;
            }

            // schedule the event - will run async, but serialized per-partition
            // (queues the event, returns immediately, does not await on handlers)
            _scheduler.Add(subscription, message);
        }

        #endregion

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _scheduler.DisposeAsync().CfAwait();

            await _clusterEventsTask.MaybeNull().CfAwaitCanceled();
            await _ghostTask.MaybeNull().CfAwaitCanceled();

            // connection is going down
            // it will be disposed as well as all other connections
            // and subscriptions will terminate
            _clusterEventsConnection = null;
        }
    }
}
