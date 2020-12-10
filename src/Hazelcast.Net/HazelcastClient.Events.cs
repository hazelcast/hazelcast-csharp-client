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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Events;
using Microsoft.Extensions.Logging;

namespace Hazelcast
{
    internal partial class HazelcastClient // Events
    {
        // subscription id -> event handlers
        // for cluster client-level events i.e. events that do not directly correspond to an
        // event message received from the cluster, but from things happening in the client
        private readonly ConcurrentDictionary<Guid, HazelcastClientEventHandlers> _handlers
            = new ConcurrentDictionary<Guid, HazelcastClientEventHandlers>();

        /// <inheritdoc />
        public async Task<Guid> SubscribeAsync(Action<HazelcastClientEventHandlers> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            var handlers = new HazelcastClientEventHandlers();
            events(handlers);

            foreach (var handler in handlers)
            {
                switch (handler)
                {
                    case DistributedObjectCreatedEventHandler _:
                    case DistributedObjectDestroyedEventHandler _:
                        await Cluster.ClusterEvents.AddObjectLifecycleSubscription().CAF();
                        break;

                    case PartitionLostEventHandler _:
                        await Cluster.ClusterEvents.AddPartitionLostSubscription().CAF();
                        break;

                    case MembersUpdatedEventHandler _ :
                    case PartitionsUpdatedEventHandler _:
                    case ConnectionOpenedEventHandler _:
                    case ConnectionClosedEventHandler _:
                    case StateChangedEventHandler _:
                        // nothing to do (but don't throw)
                        break;

                    default:
                        throw new NotSupportedException($"Handler of type {handler.GetType()} is not supported here.");
                }
            }

            var id = Guid.NewGuid();
            _handlers[id] = handlers;
            return id;
        }

        /// <inheritdoc />
        public async ValueTask<bool> UnsubscribeAsync(Guid subscriptionId)
        {
            if (!_handlers.TryGetValue(subscriptionId, out var clusterHandlers))
                return true;

            var allRemoved = true;
            var removedHandlers = new List<IHazelcastClientEventHandler>();
            foreach (var handler in clusterHandlers)
            {
                var removed = handler switch
                {
                    DistributedObjectCreatedEventHandler _ => await Cluster.ClusterEvents.RemoveObjectLifecycleSubscription().CAF(),
                    DistributedObjectDestroyedEventHandler _ => await Cluster.ClusterEvents.RemoveObjectLifecycleSubscription().CAF(),
                    PartitionLostEventHandler _ => await Cluster.ClusterEvents.RemovePartitionLostSubscription().CAF(),
                    MembersUpdatedEventHandler _ => true,
                    PartitionsUpdatedEventHandler _ => true,
                    ConnectionOpenedEventHandler _ => true,
                    ConnectionClosedEventHandler _ => true,
                    StateChangedEventHandler _ => true,
                    _ => throw new NotSupportedException($"Handler of type {handler.GetType()} is not supported here.")
                };

                allRemoved &= removed;

                if (removed) removedHandlers.Add(handler);
            }

            foreach (var handler in removedHandlers)
                clusterHandlers.Remove(handler);

            if (allRemoved)
                _handlers.TryRemove(subscriptionId, out _);

            return allRemoved;
        }

        /// <summary>
        /// Handles an object being created.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        public ValueTask OnObjectCreated(DistributedObjectCreatedEventArgs args)
        {
            // triggers ObjectLifecycle event
            return ForEachHandler<DistributedObjectCreatedEventHandler, DistributedObjectCreatedEventArgs>(
                (handler, sender, a) => handler.HandleAsync(sender, a), args);
        }

        /// <summary>
        /// Handles an object being destroyed.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        public ValueTask OnObjectDestroyed(DistributedObjectDestroyedEventArgs args)
        {
            // triggers ObjectLifecycle event
            return ForEachHandler<DistributedObjectDestroyedEventHandler, DistributedObjectDestroyedEventArgs>(
                (handler, sender, a) => handler.HandleAsync(sender, a), args);
        }

        /// <summary>
        /// Handles an update of the members.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        public ValueTask OnMembersUpdated(MembersUpdatedEventArgs args)
        {
            // triggers MemberLifecycle event
            return ForEachHandler<MembersUpdatedEventHandler, MembersUpdatedEventArgs>(
                (handler, sender, a) => handler.HandleAsync(sender, a), args);
        }

        /// <summary>
        /// Handles a client state change (lifecycle).
        /// </summary>
        /// <param name="state">The new state.</param>
        public ValueTask OnStateChanged(ClientState state)
        {
            var args = new StateChangedEventArgs(state);

            // triggers StateChanged event
            return ForEachHandler<StateChangedEventHandler, StateChangedEventArgs>(
                (handler, sender, a) => handler.HandleAsync(sender, a), args);
        }

        /// <summary>
        /// Handles an update of the partitions.
        /// </summary>
        public ValueTask OnPartitionsUpdated()
        {
            // triggers PartitionsUpdated event
            return ForEachHandler<PartitionsUpdatedEventHandler, EventArgs>(
                (handler, sender, a) => handler.HandleAsync(sender, a), EventArgs.Empty);
        }

        /// <summary>
        /// Handles a lost partition.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        public ValueTask OnPartitionLost(PartitionLostEventArgs args)
        {
            // triggers PartitionLost event
            return ForEachHandler<PartitionLostEventHandler, PartitionLostEventArgs>(
                (handler, sender, a) => handler.HandleAsync(sender, a), args);
        }

        /// <summary>
        /// Handles an opened connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="isFirst">Whether the connection is the first one.</param>
        /// <param name="isNewCluster">Whether the cluster is a new/different cluster.</param>
        public async ValueTask OnConnectionOpened(MemberConnection connection, bool isFirst, bool isNewCluster)
        {
            // if it is the first connection, subscribe to events according to options
            if (isFirst)
            {
                // FIXME should this be cancelable?
                var cancellationToken = CancellationToken.None;
                foreach (var subscriber in _options.Subscribers)
                    await subscriber.SubscribeAsync(this, cancellationToken).CAF();
            }

            var args = new ConnectionOpenedEventArgs(isFirst);

            // trigger ConnectionOpened event
            await ForEachHandler<ConnectionOpenedEventHandler, ConnectionOpenedEventArgs>(
                (handler, sender, a) => handler.HandleAsync(sender, a), args).CAF();
        }

        /// <summary>
        /// Handles a closed connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="wasLast">Whether the connection was the last one.</param>
        public ValueTask OnConnectionClosed(MemberConnection connection, bool wasLast)
        {
            var args = new ConnectionClosedEventArgs(wasLast);

            // trigger ConnectionRemoved event
            return ForEachHandler<ConnectionClosedEventHandler, ConnectionClosedEventArgs>(
                (handler, sender, a) => handler.HandleAsync(sender, a), args);
        }

        /// <summary>
        /// Triggers events.
        /// </summary>
        /// <typeparam name="THandler">The type of the handlers to trigger.</typeparam>
        /// <typeparam name="TArgs">The type of the event data.</typeparam>
        /// <param name="action">The trigger action.</param>
        /// <param name="args">Event data.</param>
        private async ValueTask ForEachHandler<THandler, TArgs>(Func<THandler, IHazelcastClient, TArgs, ValueTask> action, TArgs args)
        {
            // TODO: consider running on background threads + limiting concurrency

            foreach (var (_, clusterEvents) in _handlers)
            foreach (var handler in clusterEvents.OfType<THandler>())
            {
                try
                {
                    await action(handler, this, args).CAF();
                }
                catch (Exception e)
                {
                    // TODO: refactor instrumentation
                    Cluster.Instrumentation.CountExceptionInEventHandler(e);
                    _logger.LogError(e, "Caught exception in event handler.");
                }
            }
        }
    }
}
