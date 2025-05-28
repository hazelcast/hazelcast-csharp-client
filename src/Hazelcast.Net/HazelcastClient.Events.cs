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
using System.Threading.Tasks;
using Hazelcast.Core;
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
                        await Cluster.Events.AddObjectLifecycleSubscription().CfAwait();
                        break;

                    case PartitionLostEventHandler _:
                        await Cluster.Events.AddPartitionLostSubscription().CfAwait();
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
                    DistributedObjectCreatedEventHandler _ => await Cluster.Events.RemoveObjectLifecycleSubscription().CfAwait(),
                    DistributedObjectDestroyedEventHandler _ => await Cluster.Events.RemoveObjectLifecycleSubscription().CfAwait(),
                    PartitionLostEventHandler _ => await Cluster.Events.RemovePartitionLostSubscription().CfAwait(),
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
        /// Triggers handlers.
        /// </summary>
        /// <typeparam name="THandler">The type of the handlers.</typeparam>
        /// <returns>A task that will complete when the handlers have been triggered.</returns>
        /// <remarks>
        /// <para>Each individual handle executes within a try/catch block and exceptions
        /// are caught and logged; this method does not throw.</para>
        /// </remarks>
        private ValueTask Trigger<THandler>()
            where THandler : HazelcastClientEventHandlerBase<EventArgs>
        {
            return ForEachHandler<THandler, EventArgs>((handler, sender, a) =>
                handler.HandleAsync(sender, a), EventArgs.Empty);
        }

        /// <summary>
        /// Triggers handlers.
        /// </summary>
        /// <typeparam name="THandler">The type of the handlers.</typeparam>
        /// <typeparam name="TArgs">The type of the event arguments.</typeparam>
        /// <returns>A task that will complete when the handlers have been triggered.</returns>
        /// <remarks>
        /// <para>Each individual handle executes within a try/catch block and exceptions
        /// are caught and logged; this method does not throw.</para>
        /// </remarks>
        private ValueTask Trigger<THandler, TArgs>(TArgs args)
            where THandler : HazelcastClientEventHandlerBase<TArgs>
        {
            return ForEachHandler<THandler, TArgs>((handler, sender, a) =>
                handler.HandleAsync(sender, a), args);
        }

        /// <summary>
        /// Triggers events.
        /// </summary>
        /// <typeparam name="THandler">The type of the handlers to trigger.</typeparam>
        /// <typeparam name="TArgs">The type of the event data.</typeparam>
        /// <param name="action">The trigger action.</param>
        /// <param name="args">Event data.</param>
        /// <remarks>
        /// <para>Each individual handle executes within a try/catch block and exceptions
        /// are caught and logged; this method does not throw.</para>
        /// </remarks>
        private async ValueTask ForEachHandler<THandler, TArgs>(Func<THandler, IHazelcastClient, TArgs, ValueTask> action, TArgs args)
        {
            // TODO: consider running on background threads + limiting concurrency

            foreach (var (_, clusterEvents) in _handlers)
            foreach (var handler in clusterEvents.OfType<THandler>())
            {
                try
                {
                    await action(handler, this, args).CfAwait();
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
