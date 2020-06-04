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
using System.Linq;
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
        private void OnObjectLifecycleEvent(ClusterObjectLifecycleEventType eventType, ClusterObjectLifecycleEventArgs args)
        {
            ForEachHandler<ClusterObjectLifecycleEventHandler>(handler =>
            {
                if (handler.EventType == eventType)
                    handler.Handle(this, args);
            });
        }

        /// <summary>
        /// Triggers a member lifecycle event.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="args">The event arguments.</param>
        private void OnMemberLifecycleEvent(ClusterMemberLifecycleEventType eventType, ClusterMemberLifecycleEventArgs args)
        {
            ForEachHandler<ClusterMemberLifecycleEventHandler>(handler =>
            {
                if (handler.EventType == eventType)
                    handler.Handle(this, args);
            });
        }

        /// <summary>
        /// Triggers a client lifecycle event.
        /// </summary>
        /// <param name="state">The new state.</param>
        private void OnClientLifecycleEvent(ClientLifecycleState state)
        {
            ForEachHandler<ClientLifecycleEventHandler>(handler =>
            {
                handler.Handle(this, state);
            });
        }

        /// <summary>
        /// Triggers a partitions updated event.
        /// </summary>
        private void OnPartitionsUpdated()
        {
            ForEachHandler<PartitionsUpdatedEventHandler>(handler =>
            {
                handler.Handle(this, EventArgs.Empty);
            });
        }

        /// <summary>
        /// Triggers a partition list event.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        private void OnPartitionLost(PartitionLostEventArgs args)
        {
            ForEachHandler<PartitionLostEventHandler>(handler =>
            {
                handler.Handle(this, args);
            });
        }

        /// <summary>
        /// Triggers a connection added event.
        /// </summary>
        /// <param name="client">The new client.</param>
        private void OnConnectionAdded(Client client)
        {
            var args = new ConnectionLifecycleEventArgs(client);
            ForEachHandler<ConnectionLifecycleEventHandler>(handler =>
            {
                if (handler.EventType == ConnectionLifecycleEventType.Added)
                    handler.Handle(this, args);
            });
        }

        /// <summary>
        /// Triggers a connection removed event.
        /// </summary>
        /// <param name="client">The removed client.</param>
        private void OnConnectionRemoved(Client client)
        {
            var args = new ConnectionLifecycleEventArgs(client);
            ForEachHandler<ConnectionLifecycleEventHandler>(handler =>
            {
                if (handler.EventType == ConnectionLifecycleEventType.Removed)
                    handler.Handle(this, args);
            });
        }

        /// <summary>
        /// Triggers events.
        /// </summary>
        /// <typeparam name="THandler">The type of the handlers to trigger.</typeparam>
        /// <param name="action">The trigger action.</param>
        private void ForEachHandler<THandler>(Action<THandler> action)
        {
            // TODO: consider async handlers + running on background threads + limiting concurrency

            foreach (var (_, clusterEvents) in _clusterHandlers)
            foreach (var handler in clusterEvents.OfType<THandler>())
            {
                try
                {
                    action(handler);
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
        private void OnEventMessage(ClientMessage message)
        {
            HzConsole.WriteLine(this, "Handle event message");

            // FIXME: consider async handlers + running on background threads + limiting concurrency

            if (!_correlatedSubscriptions.TryGetValue(message.CorrelationId, out var subscription))
            {
                Instrumentation.CountMissedEvent(message);
                _logger.LogWarning($"No event handler for [{message.CorrelationId}]");
                HzConsole.WriteLine(this, $"No event handler for [{message.CorrelationId}]");
                return;
            }

            // exceptions are handled by caller (see Client.ReceiveEvent)
            subscription.Handle(message);
        }
    }
}
