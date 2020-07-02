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
        public Task<Guid> SubscribeAsync(Action<HazelcastClientEventHandlers> handle, TimeSpan timeout = default)
            => TaskEx.WithTimeout(SubscribeAsync, handle, timeout, DefaultOperationTimeoutMilliseconds);

        /// <inheritdoc />
        public async Task<Guid> SubscribeAsync(Action<HazelcastClientEventHandlers> handle, CancellationToken cancellationToken)
        {
            if (handle == null) throw new ArgumentNullException(nameof(handle));

            var handlers = new HazelcastClientEventHandlers();
            handle(handlers);

            foreach (var handler in handlers)
            {
                switch (handler)
                {
                    case DistributedObjectLifecycleEventHandler _:
                        await Cluster.AddObjectLifecycleEventSubscription(cancellationToken).CAF();
                        break;

                    case PartitionLostEventHandler _:
                        await Cluster.AddPartitionLostEventSubscription(cancellationToken).CAF();
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
        public Task UnsubscribeAsync(Guid subscriptionId, TimeSpan timeout = default)
            => TaskEx.WithTimeout(UnsubscribeAsync, subscriptionId, timeout, DefaultOperationTimeoutMilliseconds);

        /// <inheritdoc />
        public async Task UnsubscribeAsync(Guid subscriptionId, CancellationToken cancellationToken)
        {
            if (!_handlers.TryRemove(subscriptionId, out var clusterHandlers))
                return;

            foreach (var handler in clusterHandlers)
            {
                switch (handler)
                {
                    case DistributedObjectLifecycleEventHandler _:
                        await Cluster.RemoveObjectLifecycleEventSubscription(cancellationToken).CAF();
                        break;

                    case PartitionLostEventHandler _:
                        await Cluster.RemovePartitionLostEventSubscription(cancellationToken).CAF();
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Triggers an object lifecycle event.
        /// </summary>
        /// <param name="eventType">The type of the events.</param>
        /// <param name="args">The event arguments.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public ValueTask OnObjectLifecycleEvent(DistributedObjectLifecycleEventType eventType, DistributedObjectLifecycleEventArgs args, CancellationToken cancellationToken)
        {
            return ForEachHandler<DistributedObjectLifecycleEventHandler, DistributedObjectLifecycleEventArgs>((handler, sender, a, token) =>
                    handler.EventType == eventType
                        ? handler.HandleAsync(sender, a, token)
                        : default,
                args,
                cancellationToken);
        }

        /// <summary>
        /// Triggers a member lifecycle event.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="args">The event arguments.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public ValueTask OnMemberLifecycleEvent(MemberLifecycleEventType eventType, MemberLifecycleEventArgs args, CancellationToken cancellationToken)
        {
            return ForEachHandler<MemberLifecycleEventHandler, MemberLifecycleEventArgs>((handler, sender, a, token) =>
                handler.EventType == eventType
                    ? handler.HandleAsync(sender, a, token)
                    : default,
                args,
                cancellationToken);
        }

        /// <summary>
        /// Triggers a client lifecycle event.
        /// </summary>
        /// <param name="state">The new state.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public ValueTask OnClientLifecycleEvent(ClientLifecycleState state, CancellationToken cancellationToken)
        {
            return ForEachHandler<ClientLifecycleEventHandler, ClientLifecycleEventArgs>((handler, sender, args, token) =>
                handler.HandleAsync(sender, args, token),
                new ClientLifecycleEventArgs(state),
                cancellationToken);
        }

        /// <summary>
        /// Triggers a partitions updated event.
        /// </summary>
        public ValueTask OnPartitionsUpdated(CancellationToken cancellationToken)
        {
            return ForEachHandler<PartitionsUpdatedEventHandler, EventArgs>((handler, sender, args, token) =>
                handler.HandleAsync(sender, args, token),
                EventArgs.Empty,
                cancellationToken);
        }

        /// <summary>
        /// Triggers a partition list event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public ValueTask OnPartitionLost(PartitionLostEventArgs args, CancellationToken cancellationToken)
        {
            return ForEachHandler<PartitionLostEventHandler, PartitionLostEventArgs>((handler, sender, a, token) =>
                handler.HandleAsync(sender, a, token),
                args,
                cancellationToken);
        }

        /// <summary>
        /// Triggers a connection added event.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        public ValueTask OnConnectionAdded(/*Client client,*/ CancellationToken cancellationToken)
        {
            return ForEachHandler<ConnectionLifecycleEventHandler, ConnectionLifecycleEventArgs>((handler, sender, args, token) =>
                handler.EventType == ConnectionLifecycleEventType.Added
                    ? handler.HandleAsync(sender, args, token)
                    : default,
                new ConnectionLifecycleEventArgs(/*client*/),
                cancellationToken);
        }

        /// <summary>
        /// Triggers a connection removed event.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        public ValueTask OnConnectionRemoved(/*Client client,*/ CancellationToken cancellationToken)
        {
            return ForEachHandler<ConnectionLifecycleEventHandler, ConnectionLifecycleEventArgs>((handler, sender, args, token) =>
                    handler.EventType == ConnectionLifecycleEventType.Removed
                        ? handler.HandleAsync(sender, args, token)
                        : default,
                new ConnectionLifecycleEventArgs(/*client*/),
                cancellationToken);
        }

        /// <summary>
        /// Triggers events.
        /// </summary>
        /// <typeparam name="THandler">The type of the handlers to trigger.</typeparam>
        /// <typeparam name="TArgs">The type of the event data.</typeparam>
        /// <param name="action">The trigger action.</param>
        /// <param name="args">Event data.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        private async ValueTask ForEachHandler<THandler, TArgs>(Func<THandler, IHazelcastClient, TArgs, CancellationToken, ValueTask> action, TArgs args, CancellationToken cancellationToken)
        {
            // TODO: consider running on background threads + limiting concurrency

            foreach (var (_, clusterEvents) in _handlers)
            foreach (var handler in clusterEvents.OfType<THandler>())
            {
                try
                {
                    await action(handler, this, args, cancellationToken).CAF();
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