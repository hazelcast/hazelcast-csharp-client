// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
        internal class ClusterViewProperties
        {
            public ClusterViewProperties(Func<MemberConnection, long, CancellationToken, Task<bool>> subscribeAsync, Type viewType)
            {
                SubscribeAsync = subscribeAsync;
                ViewType = viewType;
            }

            public object Mutex { get; } = new();
            public MemberConnection Connection { get; set; }
            public long CorrelationId { get; set; }
            public Task ViewTask { get; set; }
            public Type ViewType { get; }
            public Func<MemberConnection, long, CancellationToken, Task<bool>> SubscribeAsync { get; }
        }

        private readonly TerminateConnections _terminateConnections;

        private readonly ClusterState _clusterState;
        private readonly ClusterMessaging _clusterMessaging;
        private readonly ClusterMembers _clusterMembers;
        private readonly DistributedEventScheduler _scheduler;
        private readonly ILogger _logger;

        private readonly CancellationTokenSource _cancel = new();
        private readonly object _mutex = new(); // subscriptions and connections

        private Func<ValueTask> _partitionsUpdated;
        private Func<MembersUpdatedEventArgs, ValueTask> _membersUpdated;
        private Func<ValueTask> _memberPartitionGroupsUpdated;

        // Holds properties required for managing cluster views -> cluster view and CP view
        private readonly ConcurrentDictionary<Type, ClusterViewProperties> _clusterViewProperties = new();

        private volatile int _disposed;

        // connections
        private readonly HashSet<MemberConnection> _connections = new();
        private TaskCompletionSource<MemberConnection> _connectionOpened;

        // subscription id -> subscription
        // the master subscriptions list
        private readonly ConcurrentDictionary<Guid, ClusterSubscription> _subscriptions = new();

        // subscribe tasks
        private readonly object _subscribeTasksMutex = new();
        private Dictionary<MemberConnection, Task> _subscribeTasks = new(); // the tasks that subscribe new connections

        // correlation id -> subscription
        // used to match a subscription to an incoming event message
        // each connection has its own correlation id, so there can be many entries per cluster subscription
        private readonly ConcurrentDictionary<long, ClusterSubscription> _correlatedSubscriptions = new();

        // ghost subscriptions, to be collected
        // subscriptions that have failed to properly unsubscribe and now we need to take care of them
        private readonly HashSet<MemberSubscription> _collectSubscriptions = new();
        private readonly object _collectMutex = new();
        private Task _collectTask; // the task that collects ghost subscriptions

        static ClusterEvents()
        {
            HConsole.Configure(x => x.Configure<ClusterEvents>().SetPrefix("CLUST.EVTS"));
        }

        public ClusterEvents(ClusterState clusterState, ClusterMessaging clusterMessaging, TerminateConnections terminateConnections, ClusterMembers clusterMembers)
        {
            _clusterState = clusterState;
            _clusterMessaging = clusterMessaging;
            _clusterMembers = clusterMembers;

            // Register cluster views
            _clusterViewProperties[typeof(ClientAddClusterViewListenerCodec)] = new ClusterViewProperties(SubscribeToClusterViewsAsync, typeof(ClientAddClusterViewListenerCodec));

            // Subscribe when CP direct to leader is enabled
            if (_clusterState.Options.Networking.CPDirectToLeaderEnabled)
                _clusterViewProperties[typeof(ClientAddCPGroupViewListenerCodec)] = new ClusterViewProperties(SubscribeToClusterCPViewAsync, typeof(ClientAddCPGroupViewListenerCodec));

            _logger = _clusterState.LoggerFactory.CreateLogger<ClusterEvents>();
            _scheduler = new DistributedEventScheduler(_clusterState.LoggerFactory);
            _terminateConnections = terminateConnections;

            _objectLifecycleEventSubscription = new ObjectLifecycleEventSubscription(_clusterState, this)
            {
                ObjectCreated = args => _objectCreated.AwaitEach(args),
                ObjectDestroyed = args => _objectDestroyed.AwaitEach(args)
            };

            _partitionLostEventSubscription = new PartitionLostEventSubscription(_clusterState, this, clusterMembers)
            {
                PartitionLost = args => _partitionLost.AwaitEach(args)
            };
        }

        /// <summary>
        /// (internal for tests only) Gets the subscriptions.
        /// </summary>
        internal ConcurrentDictionary<Guid, ClusterSubscription> Subscriptions => _subscriptions;

        /// <summary>
        /// (internal for tests only) Gets the correlated subscriptions.
        /// </summary>
        internal ConcurrentDictionary<long, ClusterSubscription> CorrelatedSubscriptions => _correlatedSubscriptions;

        /// <summary>
        /// (internal for tests only) Gets the ghost subscriptions that need to be collected.
        /// </summary>
        internal HashSet<MemberSubscription> CollectSubscriptions => _collectSubscriptions;

        #region Add/Remove Subscriptions

        // _connections is the list of known member connections
        //   connections are added & removed by handling the ConnectionOpened and ConnectionClosed events
        //   note: a connection may be opened yet not correspond to any member
        //
        // _subscriptions is the list of known cluster subscriptions
        //   subscriptions are added & removed by invoking Add/RemoveSubscriptionAsync
        //   each subscription in _subscriptions must be added to each connection in _connections
        //
        // when a subscription is added,
        // - (mutex): capture _connections connections, add the subscription to _subscriptions
        // - for each connection
        //   - add a correlated subscription (before adding on server!)
        //   - add the subscription to the connection on server
        //     - fails
        //       - remove the correlated subscription
        //       - because
        //         - the connection is not active anymore = skip & continue with other connections
        //         - any other reason = queue all member connections for collection
        //       - fail
        //   - try-add a member connection to subscription
        //     - fails (because the subscription is not active anymore)
        //       - remove the correlated subscription
        //       - nothing else to do: the subscription has been de-activated = clean
        //       - fail
        //
        // when a connection is added
        // - (mutex): capture _subscriptions subscriptions, add the connection to _connections
        // - for each subscription
        //   - add a correlated subscription (before adding on server!)
        //   - add the subscription to the connection on server
        //     - fails
        //       - remove the correlated subscription
        //       - because
        //         - the connection is not active anymore = queue all created member subscriptions for collection
        //         - for any other reason = terminate the connection
        //       - exit
        //   - try-add the corresponding member connection to the subscription
        //     - fails (because the subscription is not active anymore)
        //       - remove the correlated subscription
        //       - queue the member connection for collection
        //       - skip & continue with other subscriptions
        //
        //
        // when a subscription is removed
        // - (mutex): remove the subscription from _subscriptions
        // - de-activate the subscription (cannot add member subscriptions anymore)
        // - for each member connection in the subscription,
        //   - clear the correlated subscription
        //   - remove from server
        //     - fails because the connection is not active anymore = consider it a success
        //     - fails for any other reason = queue the member subscription for collection
        //
        // note: meanwhile, if a connection is
        // - added: it will not see the subscription, or see it de-activated
        // - removed: removing from server will be considered a success
        //
        //
        // when a connection is removed
        // - (mutex): capture _subscriptions subscriptions, remove the connection from _connections
        // - for each subscription
        //   - remove the member subscription for the removed connection (cannot remove from server, connection is down)
        //   - remove the corresponding correlated subscription
        // - if it is the cluster views connection
        //   - clear
        //   - remove the corresponding correlated subscription
        //   - start assigning another connection
        //
        // note: meanwhile, if a subscription is
        // - added: it will not see the connection
        // - removed: never mind, we just have nothing to remove

        /// <summary>
        /// Adds a subscription.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the subscription has been added.</returns>
        public async Task AddSubscriptionAsync(ClusterSubscription subscription, CancellationToken cancellationToken = default)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));

            // atomically get connections and add the subscription
            List<MemberConnection> connections;
            lock (_mutex)
            {
                // capture connections
                connections = _connections.ToList();

                // failing would be a nasty internal error but better report it
                if (!_subscriptions.TryAdd(subscription.Id, subscription))
                    throw new InvalidOperationException("A subscription with the same identifier already exists.");
            }

            // add the subscription to each captured connection
            // TODO: consider adding in parallel
            foreach (var connection in connections)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    CollectSubscription(subscription); // undo what has been done already
                    cancellationToken.ThrowIfCancellationRequested(); // and throw
                }

                // this never throws
                var attempt = await AddSubscriptionAsync(subscription, connection, cancellationToken).CfAwait();

                switch (attempt.Value)
                {
                    case InstallResult.Success: // good
                    case InstallResult.ConnectionNotActive: // ignore it
                        continue;

                    case InstallResult.SubscriptionNotActive:
                        // not active = has been de-activated = what has been done already has been undone
                        throw new HazelcastException("Failed to add the subscription because it was removed.");

                    case InstallResult.Failed: // also if canceled
                        CollectSubscription(subscription); // undo what has been done already
                        throw new HazelcastException("Failed to add subscription (see inner exception).", attempt.Exception);

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        // adds a subscription on one member
        private async ValueTask<Attempt<InstallResult>> AddSubscriptionAsync(ClusterSubscription subscription, MemberConnection connection, CancellationToken cancellationToken)
        {
            // if we already know the connection is not active anymore, ignore it
            // otherwise, install on this member - may throw if the connection goes away in the meantime
            if (!connection.Active) return Attempt.Fail(InstallResult.ConnectionNotActive);

            // add correlated subscription now so it is ready when the first events come
            var correlationId = _clusterState.GetNextCorrelationId();
            _correlatedSubscriptions[correlationId] = subscription;

            // the original subscription.SubscribeRequest message may be used concurrently,
            // we need a safe clone so we can use our own correlation id in a safe way.
            var subscribeRequest = subscription.SubscribeRequest.CloneWithNewCorrelationId(correlationId);

            // talk to the server
            ClientMessage response;
            try
            {
                response = await _clusterMessaging.SendToMemberAsync(subscribeRequest, connection, correlationId, cancellationToken).CfAwait();
            }
            catch (Exception e)
            {
                _correlatedSubscriptions.TryRemove(correlationId, out _);
                return connection.Active
                    ? Attempt.Fail(InstallResult.Failed, e) // also if canceled
                    : Attempt.Fail(InstallResult.ConnectionNotActive);
            }

            // try to add the member subscription to the cluster subscription
            // fails if the cluster subscription is not active anymore
            var memberSubscription = subscription.ReadSubscriptionResponse(response, connection);
            var added = subscription.TryAddMemberSubscription(memberSubscription);
            if (added) return InstallResult.Success;

            // the subscription is not active anymore
            _correlatedSubscriptions.TryRemove(correlationId, out _);
            CollectSubscription(memberSubscription);
            return Attempt.Fail(InstallResult.SubscriptionNotActive);
        }

        // (background) adds subscriptions on one member - when a connection is added
        private async Task AddSubscriptionsAsync(MemberConnection connection, IReadOnlyCollection<ClusterSubscription> subscriptions, CancellationToken cancellationToken)
        {
            // this is a background task and therefore should never throw!

            foreach (var subscription in subscriptions)
            {
                if (cancellationToken.IsCancellationRequested) return;

                // this never throws
                var attempt = await AddSubscriptionAsync(subscription, connection, cancellationToken).CfAwait();

                switch (attempt.Value)
                {
                    case InstallResult.Success: // ok
                    case InstallResult.SubscriptionNotActive: // ignore it
                        continue;

                    case InstallResult.ConnectionNotActive:
                        // not active = has been removed = what has been done already has been undone
                        break; // simply exit

                    case InstallResult.Failed:
                        // failed to talk to the server - this connection is not working
                        _terminateConnections.Add(connection);
                        break; // exit

                    default:
                        continue;
                }
            }

            // we are done now
            lock (_subscribeTasksMutex) _subscribeTasks.Remove(connection);
        }

        /// <summary>
        /// Removes a subscription.
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
            // get and remove the subscription
            ClusterSubscription subscription;
            lock (_mutex)
            {
                if (!_subscriptions.TryRemove(subscriptionId, out subscription))
                    return false; // unknown subscription
            }

            await RemoveSubscriptionAsync(subscription, cancellationToken).CfAwait();
            return true;
        }

        // removes a subscription
        private async ValueTask RemoveSubscriptionAsync(ClusterSubscription subscription, CancellationToken cancellationToken)
        {
            // de-activate the subscription: all further events will be ignored
            subscription.Deactivate();

            // for each member subscription
            foreach (var memberSubscription in subscription)
            {
                // runs them all regardless of cancellation

                // remove the correlated subscription
                _correlatedSubscriptions.TryRemove(memberSubscription.CorrelationId, out _);

                // remove from the server
                // and, if it fails, enqueue for collection
                if (await RemoveSubscriptionAsync(memberSubscription, cancellationToken).CfAwait())
                    subscription.Remove(memberSubscription);
                else
                    CollectSubscription(memberSubscription);
            }
        }

        // remove a subscription from one member
        private async ValueTask<bool> RemoveSubscriptionAsync(MemberSubscription subscription, CancellationToken cancellationToken)
        {
            // fast: if the connection is down, consider the subscription removed
            if (!subscription.Connection.Active) return true;

            try
            {
                // remove the member subscription = trigger the server-side un-subscribe
                // this *may* throw if we fail to talk to the member
                // this *may* return false for some reason
                var unsubscribeRequest = subscription.ClusterSubscription.CreateUnsubscribeRequest(subscription.ServerSubscriptionId);
                var responseMessage = await _clusterMessaging.SendToMemberAsync(unsubscribeRequest, subscription.Connection, cancellationToken).CfAwait();
                var removed = subscription.ClusterSubscription.ReadUnsubscribeResponse(responseMessage);
                return removed;
            }
            catch (Exception e)
            {
                // if the connection is down, consider the subscription removed
                if (!subscription.Connection.Active) return true;

                // otherwise something went wrong and maybe we want to try again
                _logger.LogError(e, "Caught an exception while unsubscribing to events.");
                return false;
            }
        }

        // clears the subscriptions of a member that is gone fishing
        // the connection is down, no way to unsubscribe, just clear the data structures
        private void ClearMemberSubscriptions(IEnumerable<ClusterSubscription> subscriptions, MemberConnection connection)
        {
            foreach (var subscription in subscriptions)
            {
                // remove the correlated subscription
                // remove the client subscription
                if (subscription.TryRemove(connection, out var memberSubscription))
                    _correlatedSubscriptions.TryRemove(memberSubscription.CorrelationId, out _);
            }
        }

        #endregion

        #region Cluster Members/Partitions Views

        /// <summary>
        /// Clears the connection currently supporting the cluster and CP view event, if it matches the specified <paramref name="connection"/>.
        /// </summary>
        /// <param name="connection">A connection.</param>
        /// <remarks>
        /// <para>If <paramref name="connection"/> was supporting the cluster view event, and was not the last connection,
        /// this starts a background task to assign another connection to support the cluster view event.</para>
        /// </remarks>
        private void ClearClusterViewsConnection(MemberConnection connection)
        {
            // note: we do not "unsubscribe" - if we come here, the connection is gone
            foreach (var viewProp in _clusterViewProperties)
            {
                lock (viewProp.Value.Mutex)
                {
                    // if the specified client is *not* the cluster events client, ignore
                    if (viewProp.Value.Connection != connection)
                        continue;

                    // otherwise, clear the connection
                    viewProp.Value.Connection = null;
                    _correlatedSubscriptions.TryRemove(viewProp.Value.CorrelationId, out _);
                    viewProp.Value.CorrelationId = 0;

                    _logger.IfDebug()?.LogDebug("Cleared cluster views connection (was {ConnectionId}).", connection.Id.ToShortString());

                    // assign another connection (async)
                    viewProp.Value.ViewTask ??= AssignClusterViewsConnectionAsync(null, viewProp.Value, _cancel.Token);
                }
            }
        }

        /// <summary>
        /// Proposes a connection to support the cluster view event.
        /// </summary>
        /// <param name="connection">A connection.</param>
        /// <remarks>
        /// <para>if there is no connection currently supporting the cluster view event, then this starts a background
        /// task to assign a connection to support the event, trying the supplied <paramref name="connection"/> first.</para>
        /// </remarks>
        private void ProposeClusterViewsConnection(MemberConnection connection)
        {
            foreach (var viewProperty in _clusterViewProperties)
            {
                lock (viewProperty.Value.Mutex)
                {
                    if (viewProperty.Value.Connection == null)
                    {
                        viewProperty.Value.ViewTask ??= AssignClusterViewsConnectionAsync(connection, viewProperty.Value, _cancel.Token);
                    }
                }
            }
        }

        private ValueTask<MemberConnection> WaitForConnection(CancellationToken cancellationToken)
        {
            var c = _clusterMembers.GetRandomConnection();
            return c == null
                ? WaitForConnectionAsync(cancellationToken)
                : new ValueTask<MemberConnection>(c);

            async ValueTask<MemberConnection> WaitForConnectionAsync(CancellationToken token)
            {
                MemberConnection c = null;
                while (!token.IsCancellationRequested && ((c = _clusterMembers.GetRandomConnection()) == null || !c.Active))
                {
                    lock (_mutex) _connectionOpened = new TaskCompletionSource<MemberConnection>();
                    using var reg = token.Register(() => _connectionOpened.TrySetCanceled());
                    c = await _connectionOpened.Task.CfAwait();
                    lock (_mutex) _connectionOpened = null;
                    if (c is { Active: true }) break; // return the connection that was opened
                }
                return c;
            }
        }

        /// <summary>
        /// Assigns a connection to support the cluster view event.
        /// </summary>
        /// <param name="connection">An optional candidate connection.</param>
        /// <param name="viewProperty">The cluster view properties.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when a connection has been assigned to handle the cluster views event.</returns>
        private async Task AssignClusterViewsConnectionAsync(MemberConnection connection, ClusterViewProperties viewProperty, CancellationToken cancellationToken)
        {
            // this will only exit once a connection is assigned, or the task is
            // cancelled, when the cluster goes down (and never up again)
            while (!cancellationToken.IsCancellationRequested && _disposed == 0)
            {
                connection ??= await WaitForConnection(cancellationToken).CfAwait();

                // try to subscribe, relying on the default invocation timeout,
                // so this is not going to last forever - we know it will end
                var correlationId = _clusterState.GetNextCorrelationId();

                // We can't use null connection here. Cancellation could be requested if it's null.
                if (connection == null) break;

                if (!await viewProperty.SubscribeAsync(connection, correlationId, cancellationToken).CfAwait()) // does not throw
                {
                    // failed => try another connection
                    connection = null;
                    continue;
                }

                // success!
                lock (viewProperty.Mutex)
                {
                    if (connection.Active)
                    {
                        viewProperty.Connection = connection;
                        viewProperty.CorrelationId = correlationId;
                        viewProperty.ViewTask = null;
                        HConsole.WriteLine(this, $"ClusterViews for {viewProperty.ViewType}: connection {connection.Id.ToShortString()} [{correlationId}]");
                        break;
                    }
                }

                // if the connection was not active anymore, we have rejected it
                // if the connection was active, and we have accepted it, and it de-activates,
                // then ClearClusterViewsConnection will deal with it
            }
        }

        /// <summary>
        /// Subscribes a connection to the cluster CP view event.
        /// </summary>
        /// <param name="connection">Candidate connection </param>
        /// <param name="correlationId">Correlation id</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>true if subscribed</returns>
        private async Task<bool> SubscribeToClusterCPViewAsync(MemberConnection connection, long correlationId, CancellationToken cancellationToken)
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger.IfDebug()?.LogDebug("Subscribe to cluster CP view on connection {ConnectionId}.", connection.Id.ToShortString());

            ValueTask HandleEventAsync(ClientMessage message, object _)
                => ClientAddCPGroupViewListenerCodec.HandleEventAsync(message,
                    HandleCodecCPGroupViewEvent,
                    connection.Id,
                    _clusterState.LoggerFactory);

            try
            {
                var request = ClientAddCPGroupViewListenerCodec.EncodeRequest();
                request.InvocationFlags |= InvocationFlags.InvokeWhenNotConnected; // run even if client not 'connected'
                _correlatedSubscriptions[correlationId] = new ClusterSubscription(HandleEventAsync);
                _ = await _clusterMessaging.SendToMemberAsync(request, connection, correlationId, cancellationToken).CfAwait();
                _logger.IfDebug()?.LogDebug("Subscribed to cluster CP view on connection {ConnectionId}.", connection.Id.ToShortString());
                return true;
            }
            catch (Exception e) when (e is TargetDisconnectedException or ClientOfflineException)
            {
                _correlatedSubscriptions.TryRemove(correlationId, out _);
                // if the connection has died... and that can happen when switching members... no need to worry the
                // user with a warning, a debug message should be enough
                _logger.IfDebug()?.LogDebug("Failed to subscribe to cluster CP view on connection {ConnectionId)} ({Reason}), may retry.", connection.Id.ToShortString(),
                    e is ClientOfflineException o ? ("offline, " + o.State) : "disconnected");
                return false;
            }
            catch (Exception e)
            {
                _correlatedSubscriptions.TryRemove(correlationId, out _);
                _logger.IfWarning()?.LogWarning(e, "Failed to subscribe to cluster CP view on connection {ConnectionId}, may retry.", connection.Id.ToShortString());
                return false;
            }
        }

        /// <summary>
        /// Subscribes a connection to the cluster view event.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the subscription has been processed, and represent whether it was successful.</returns>
        private async Task<bool> SubscribeToClusterViewsAsync(MemberConnection connection, long correlationId, CancellationToken cancellationToken)
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            // aka subscribe to member/partition view events
            _logger.IfDebug()?.LogDebug("Subscribe to cluster views on connection {ConnectionId}.", connection.Id.ToShortString());

            // handles the event
            ValueTask HandleEventAsync(ClientMessage message, object _)
                => ClientAddClusterViewListenerCodec.HandleEventAsync(message,
                    HandleCodecMemberViewEvent,
                    HandleCodecPartitionViewEvent,
                    (version, memberGroups, state) => HandleCodecMemberGroupsViewEvent(version, memberGroups, state, connection.ClusterId, connection.MemberId),
                    HandleCodecClusterVersionEvent,
                    connection.Id,
                    _clusterState.LoggerFactory);

            try
            {
                var subscribeRequest = ClientAddClusterViewListenerCodec.EncodeRequest();
                subscribeRequest.InvocationFlags |= InvocationFlags.InvokeWhenNotConnected; // run even if client not 'connected'
                _correlatedSubscriptions[correlationId] = new ClusterSubscription(HandleEventAsync);
                _ = await _clusterMessaging.SendToMemberAsync(subscribeRequest, connection, correlationId, cancellationToken).CfAwait();
                _logger.IfDebug()?.LogDebug("Subscribed to cluster views on connection {ConnectionId)}", connection.Id.ToShortString());

                return true;
            }
            catch (Exception e) when (e is TargetDisconnectedException or ClientOfflineException)
            {
                _correlatedSubscriptions.TryRemove(correlationId, out _);
                // if the connection has died... and that can happen when switching members... no need to worry the
                // user with a warning, a debug message should be enough
                _logger.IfDebug()?.LogDebug("Failed to subscribe to cluster views on connection {ConnectionId)} ({Reason}), may retry.", connection.Id.ToShortString(),
                    e is ClientOfflineException o ? ("offline, " + o.State) : "disconnected");
                return false;
            }
            catch (Exception e)
            {
                _correlatedSubscriptions.TryRemove(correlationId, out _);
                _logger.IfWarning()?.LogWarning(e, "Failed to subscribe to cluster views on connection {ConnectionId}, may retry.", connection.Id.ToShortString());
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
            _logger.LogDebug("Handle MemberView event.");
            var eventArgs = await _clusterMembers.SetMembersAsync(version, members).CfAwait();

            // nothing to do if members have been skipped (due to version)
            if (eventArgs == null) return;

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

        private ValueTask HandleCodecClusterVersionEvent(ClusterVersion version, object state)
        {
            _logger.IfDebug()?.LogDebug("Handle ClusterVersion event, current version: {Current} received version:{Version}", _clusterState.ClusterVersion, version);
            _clusterState.ChangeClusterVersion(version);
            return default;
        }

        private async ValueTask HandleCodecMemberGroupsViewEvent(int version, IList<IList<Guid>> memberGroups, object state, Guid clusterId, Guid memberId)
        {
            _logger.IfDebug()?.LogDebug("Handle MemberGroups event for cluster {ClusterId} and member {MemberId}. Received version:{Version} Member Groups: [{Groups}]",
                clusterId, memberId, version, (memberGroups == null ? "null" : string.Join(", ", memberGroups.Select(x => $"[{string.Join(", ", x)}]"))));
            
            _clusterMembers.SubsetClusterMembers
                .SetSubsetMembers(new MemberGroups(memberGroups, version, clusterId, memberId));
            await _memberPartitionGroupsUpdated.AwaitEach().CfAwait();
        }

        private ValueTask HandleCodecCPGroupViewEvent(long version, ICollection<Hazelcast.CP.CPGroupInfo> groups, IList<KeyValuePair<Guid, Guid>> cpToApUuids, object state)
        {
            _logger.IfDebug()?.LogDebug("Handle CP Group event");
            _clusterMembers.HandleCPGroupInfoUpdated(version, groups, cpToApUuids, state);
            return default;
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

        #endregion

        #region Collect Ghosts

        // add all member subscriptions of a cluster subscription to be collected, start the collect task if needed
        private void CollectSubscription(ClusterSubscription subscription)
        {
            lock (_collectMutex)
            {
                foreach (var memberSubscription in subscription)
                    _collectSubscriptions.Add(memberSubscription);
                _collectTask ??= CollectSubscriptionsAsync(_cancel.Token);
            }
        }

        // add a member subscription to be collected, start the collect task if needed
        private void CollectSubscription(MemberSubscription subscription)
        {
            lock (_collectMutex)
            {
                _collectSubscriptions.Add(subscription);
                _collectTask ??= CollectSubscriptionsAsync(_cancel.Token);
            }
        }

        // body of the subscription collection task
        private async Task CollectSubscriptionsAsync(CancellationToken cancellationToken)
        {
            List<MemberSubscription> removedSubscriptions = null;

            HConsole.WriteLine(this, "CollectSubscription starting");

            // if canceled, will be awaited properly
            await Task.Delay(_clusterState.Options.Events.SubscriptionCollectDelay, cancellationToken).CfAwait();

            while (!cancellationToken.IsCancellationRequested)
            {
                // capture subscriptions to collect
                List<MemberSubscription> subscriptions;
                lock (_collectMutex)
                {
                    subscriptions = _collectSubscriptions.ToList();
                }

                HConsole.WriteLine(this, $"CollectSubscription loop for {subscriptions.Count} member subscriptions");

                // try to remove captured subscriptions
                // if canceled, will be awaited properly
                removedSubscriptions?.Clear();
                var timeLimit = DateTime.Now - _clusterState.Options.Events.SubscriptionCollectTimeout;
                foreach (var subscription in subscriptions)
                {
                    HConsole.WriteLine(this, "CollectSubscription collects");

                    try
                    {
                        var removed = await RemoveSubscriptionAsync(subscription, cancellationToken).CfAwait();
                        if (removed || subscription.ClusterSubscription.DeactivateTime < timeLimit)
                        {
                            subscription.ClusterSubscription.Remove(subscription);
                            (removedSubscriptions ??= new List<MemberSubscription>()).Add(subscription);
                        }
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

                HConsole.WriteLine(this, $"CollectSubscription collected {removedSubscriptions?.Count ?? 0} subscriptions");

                // update subscriptions to collect
                // none remaining = exit the task
                lock (_collectMutex)
                {
                    if (removedSubscriptions != null)
                    {
                        foreach (var subscription in removedSubscriptions)
                            _collectSubscriptions.Remove(subscription);
                    }

                    if (_collectSubscriptions.Count == 0)
                    {
                        HConsole.WriteLine(this, "CollectSubscription exits");
                        _collectTask = null;
                        return;
                    }
                }

                HConsole.WriteLine(this, "CollectSubscription waits");

                // else, wait + loop / try again
                // if canceled, will be awaited properly
                await Task.Delay(_clusterState.Options.Events.SubscriptionCollectPeriod, cancellationToken).CfAwait();
            }
        }

        #endregion

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

        /// <summary>
        /// Internal event to emit when member partition groups have been updated.
        /// </summary>
        internal Func<ValueTask> MemberPartitionGroupsUpdated
        {
            get => _memberPartitionGroupsUpdated;
            set
            {
                _clusterState.ThrowIfPropertiesAreReadOnly();
                _memberPartitionGroupsUpdated = value;
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
#pragma warning disable IDE0060 // Remove unused parameters
#pragma warning disable CA1801 // Review unused parameters
        // unused parameters are required, this is an event handler
        public ValueTask OnConnectionOpened(MemberConnection connection, bool isFirstEver, bool isFirst, bool isNewCluster, ClusterVersion clusterVersion)
#pragma warning restore CA1801
#pragma warning restore IDE0060
        {
            HConsole.WriteLine(this, $"Added connection {connection.Id.ToShortString()} to {connection.MemberId.ToShortString()} at {connection.Address}");

            // atomically add the connection and capture known subscriptions
            List<ClusterSubscription> subscriptions;
            lock (_mutex)
            {
                _connections.Add(connection);
                subscriptions = _subscriptions.Values.ToList();
                _connectionOpened?.TrySetResult(connection);
            }

            // in case we don't have one already...
            ProposeClusterViewsConnection(connection);

            // for this new connection... we need to add all known subscriptions to it, and this is
            // going to happen in the background - yes, it means that the connection could be used
            // even before all subscriptions have been added and thus some events may fail to trigger,
            // we don't offer any strict guarantee on events anyways

            lock (_subscribeTasksMutex)
            {
                if (_subscribeTasks != null)
                    _subscribeTasks[connection] = AddSubscriptionsAsync(connection, subscriptions, _cancel.Token);
            }

            return default;
        }

        /// <summary>
        /// Handles a connection being closed.
        /// </summary>
        public ValueTask OnConnectionClosed(MemberConnection connection)
        {
            HConsole.WriteLine(this, $"Removed connection {connection.Id.ToShortString()} to {connection.MemberId.ToShortString()} at {connection.Address}");

            // atomically remove the connection and capture known subscriptions
            List<ClusterSubscription> subscriptions;
            lock (_mutex)
            {
                _connections.Remove(connection);
                subscriptions = _subscriptions.Values.ToList();
            }

            // just clear subscriptions,
            // cannot unsubscribes from the server since the client is not connected anymore
            ClearMemberSubscriptions(subscriptions, connection);

            // clear, if that was the cluster views connection,
            // and then start the task to assign another one)
            ClearClusterViewsConnection(connection);

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
                _logger.IfWarning()?.LogWarning("No event handler for [{CorrelationId}]", message.CorrelationId);
                HConsole.WriteLine(this, $"No event handler for [{message.CorrelationId}]");
                return;
            }

            // schedule the event - will run async, but sequentially per-partition
            // (queues the event, returns immediately, does not await on handlers)
            _scheduler.Add(subscription, message);
        }

        #endregion

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1)
                return;

            HConsole.WriteLine(this, "Dispose scheduler.");
            await _scheduler.DisposeAsync().CfAwait();
            HConsole.WriteLine(this, "Dispose subscriptions.");
            await _objectLifecycleEventSubscription.DisposeAsync().CfAwait();
            await _partitionLostEventSubscription.DisposeAsync().CfAwait();

            _cancel.Cancel();

            HConsole.WriteLine(this, "Await cluster views task.");

            foreach (var viewProp in _clusterViewProperties)
            {
                await viewProp.Value.ViewTask.MaybeNull().CfAwaitCanceled();
            }

            HConsole.WriteLine(this, "Dispose collect task.");
            await _collectTask.MaybeNull().CfAwaitCanceled();

            HConsole.WriteLine(this, "Await subscribe tasks.");
            Task[] tasks;
            lock (_subscribeTasksMutex)
            {
                tasks = _subscribeTasks.Values.ToArray();
                _subscribeTasks = null;
            }
            await Task.WhenAll(tasks).CfAwait();

            _cancel.Dispose();

            // connection is going down
            // it will be disposed as well as all other connections
            // and subscriptions will terminate
            foreach (var viewProp in _clusterViewProperties)
            {
                viewProp.Value.Connection = null;
            }

            HConsole.WriteLine(this, "Down.");
        }
    }
}
