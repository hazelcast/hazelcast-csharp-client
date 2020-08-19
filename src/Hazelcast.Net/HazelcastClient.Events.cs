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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Events;
using Microsoft.Extensions.Logging;

namespace Hazelcast
{
    internal partial class HazelcastClient // Events
    {
        // subscription id -> event handlers
        // for cluster client-level events (not wired to the server)
        private readonly ConcurrentDictionary<Guid, HazelcastClientEventHandlers> _handlers
            = new ConcurrentDictionary<Guid, HazelcastClientEventHandlers>();

        /// <inheritdoc />
        public async Task<Guid> SubscribeAsync(Action<HazelcastClientEventHandlers> handle)
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));

            var handlers = new HazelcastClientEventHandlers();
            handle(handlers);

            foreach (var handler in handlers)
            {
                switch (handler)
                {
                    case DistributedObjectLifecycleEventHandler _:
                        await Cluster.ClusterEvents.AddObjectLifecycleSubscription().CAF();
                        break;

                    case PartitionLostEventHandler _:
                        await Cluster.ClusterEvents.AddPartitionLostSubscription().CAF();
                        break;

                    default:
                        throw new NotSupportedException();
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
                    DistributedObjectLifecycleEventHandler _ => await Cluster.ClusterEvents.RemoveObjectLifecycleSubscription().CAF(),
                    PartitionLostEventHandler _ => await Cluster.ClusterEvents.RemovePartitionLostSubscription().CAF(),
                    _ => throw new NotSupportedException()
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
        /// Triggers an object lifecycle event.
        /// </summary>
        /// <param name="eventType">The type of the events.</param>
        /// <param name="args">The event arguments.</param>
        public ValueTask OnObjectLifecycleEvent(DistributedObjectLifecycleEventType eventType, DistributedObjectLifecycleEventArgs args)
        {
            return ForEachHandler<DistributedObjectLifecycleEventHandler, DistributedObjectLifecycleEventArgs>((handler, sender, a) =>
                    handler.EventType == eventType
                        ? handler.HandleAsync(sender, a)
                        : default,
                args);
        }

        /// <summary>
        /// Triggers a member lifecycle event.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="args">The event arguments.</param>
        public ValueTask OnMemberLifecycleEvent(MemberLifecycleEventType eventType, MemberLifecycleEventArgs args)
        {
            return ForEachHandler<MemberLifecycleEventHandler, MemberLifecycleEventArgs>((handler, sender, a) =>
                handler.EventType == eventType
                    ? handler.HandleAsync(sender, a)
                    : default,
                args);
        }

        /// <summary>
        /// Triggers a client lifecycle event.
        /// </summary>
        /// <param name="state">The new state.</param>
        public ValueTask OnClientLifecycleEvent(ClientLifecycleState state)
        {
            return ForEachHandler<ClientLifecycleEventHandler, ClientLifecycleEventArgs>((handler, sender, args) =>
                handler.HandleAsync(sender, args),
                new ClientLifecycleEventArgs(state));
        }

        /// <summary>
        /// Triggers a partitions updated event.
        /// </summary>
        public ValueTask OnPartitionsUpdated()
        {
            return ForEachHandler<PartitionsUpdatedEventHandler, EventArgs>((handler, sender, args) =>
                handler.HandleAsync(sender, args),
                EventArgs.Empty);
        }

        /// <summary>
        /// Triggers a partition list event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        public ValueTask OnPartitionLost(PartitionLostEventArgs args)
        {
            return ForEachHandler<PartitionLostEventHandler, PartitionLostEventArgs>((handler, sender, a) =>
                handler.HandleAsync(sender, a),
                args);
        }

        /// <summary>
        /// Triggers a connection added event.
        /// </summary>
        public ValueTask OnConnectionAdded(/*Client client*/)
        {
            return ForEachHandler<ConnectionLifecycleEventHandler, ConnectionLifecycleEventArgs>((handler, sender, args) =>
                handler.EventType == ConnectionLifecycleEventType.Added
                    ? handler.HandleAsync(sender, args)
                    : default,
                new ConnectionLifecycleEventArgs(/*client*/));
        }

        /// <summary>
        /// Triggers a connection removed event.
        /// </summary>
        public ValueTask OnConnectionRemoved(/*Client client*/)
        {
            return ForEachHandler<ConnectionLifecycleEventHandler, ConnectionLifecycleEventArgs>((handler, sender, args) =>
                    handler.EventType == ConnectionLifecycleEventType.Removed
                        ? handler.HandleAsync(sender, args)
                        : default,
                new ConnectionLifecycleEventArgs(/*client*/));
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
