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
using Hazelcast.Messaging;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    public partial class Cluster // EventTriggers
    {
        /// <summary>
        /// Triggers an object lifecycle event.
        /// </summary>
        /// <param name="eventType">The type of the events.</param>
        /// <param name="args">The event arguments.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        private ValueTask OnObjectLifecycleEvent(ClusterObjectLifecycleEventType eventType, ClusterObjectLifecycleEventArgs args, CancellationToken cancellationToken)
        {
            return ForEachHandler<ClusterObjectLifecycleEventHandler, ClusterObjectLifecycleEventArgs>((handler, sender, a, token) =>
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
        private ValueTask OnMemberLifecycleEvent(ClusterMemberLifecycleEventType eventType, ClusterMemberLifecycleEventArgs args, CancellationToken cancellationToken)
        {
            return ForEachHandler<ClusterMemberLifecycleEventHandler, ClusterMemberLifecycleEventArgs>((handler, sender, a, token) =>
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
        private ValueTask OnClientLifecycleEvent(ClientLifecycleState state, CancellationToken cancellationToken)
        {
            return ForEachHandler<ClientLifecycleEventHandler, ClientLifecycleEventArgs>((handler, sender, args, token) =>
                handler.HandleAsync(sender, args, token),
                new ClientLifecycleEventArgs(state),
                cancellationToken);
        }

        /// <summary>
        /// Triggers a partitions updated event.
        /// </summary>
        private ValueTask OnPartitionsUpdated(CancellationToken cancellationToken)
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
        private ValueTask OnPartitionLost(PartitionLostEventArgs args, CancellationToken cancellationToken)
        {
            return ForEachHandler<PartitionLostEventHandler, PartitionLostEventArgs>((handler, sender, a, token) =>
                handler.HandleAsync(sender, a, token),
                args,
                cancellationToken);
        }

        /// <summary>
        /// Triggers a connection added event.
        /// </summary>
        /// <param name="client">The new client.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        private ValueTask OnConnectionAdded(Client client, CancellationToken cancellationToken)
        {
            return ForEachHandler<ConnectionLifecycleEventHandler, ConnectionLifecycleEventArgs>((handler, sender, args, token) =>
                handler.EventType == ConnectionLifecycleEventType.Added
                    ? handler.HandleAsync(sender, args, token)
                    : default,
                new ConnectionLifecycleEventArgs(client),
                cancellationToken);
        }

        /// <summary>
        /// Triggers a connection removed event.
        /// </summary>
        /// <param name="client">The removed client.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        private ValueTask OnConnectionRemoved(Client client, CancellationToken cancellationToken)
        {
            return ForEachHandler<ConnectionLifecycleEventHandler, ConnectionLifecycleEventArgs>((handler, sender, args, token) =>
                handler.EventType == ConnectionLifecycleEventType.Removed
                    ? handler.HandleAsync(sender, args, token)
                    : default,
                new ConnectionLifecycleEventArgs(client),
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
        private async ValueTask ForEachHandler<THandler, TArgs>(Func<THandler, Cluster, TArgs, CancellationToken, ValueTask> action, TArgs args, CancellationToken cancellationToken)
        {
            // TODO: consider running on background threads + limiting concurrency

            foreach (var (_, clusterEvents) in _clusterHandlers)
            foreach (var handler in clusterEvents.OfType<THandler>())
            {
                try
                {
                    await action(handler, this, args, cancellationToken).CAF();
                }
                catch (Exception e)
                {
                    Instrumentation.CountExceptionInEventHandler(e);
                    _logger.LogError(e, "Caught exception in event handler.");
                }
            }
        }

        /// <summary>
        /// Handles an event message and trigger the appropriate events via the subscriptions.
        /// </summary>
        /// <param name="message">The event message.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        private async ValueTask OnEventMessage(ClientMessage message, CancellationToken cancellationToken)
        {
            HConsole.WriteLine(this, "Handle event message");

            if (!_correlatedSubscriptions.TryGetValue(message.CorrelationId, out var subscription))
            {
                Instrumentation.CountMissedEvent(message);
                _logger.LogWarning($"No event handler for [{message.CorrelationId}]");
                HConsole.WriteLine(this, $"No event handler for [{message.CorrelationId}]");
                return;
            }

            // FIXME: consider running event handler on background thread, limiting concurrency, setting a cancellation token

            // exceptions are handled by caller (see Client.ReceiveEvent)
            await subscription.HandleAsync(message, cancellationToken).CAF();
        }
    }
}
