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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Events;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Clustering
{
    internal partial class Cluster // EventTriggers
    {
        private Func<DistributedObjectLifecycleEventType, DistributedObjectLifecycleEventArgs, CancellationToken, ValueTask> _onObjectLifeCycleEvent;
        private Func<MemberLifecycleEventType, MemberLifecycleEventArgs, CancellationToken, ValueTask> _onMemberLifecycleEvent;
        private Func<ClientLifecycleState, CancellationToken, ValueTask> _onClientLifecycleEvent;
        private Func<CancellationToken, ValueTask> _onPartitionUpdated;
        private Func<PartitionLostEventArgs, CancellationToken, ValueTask> _onPartitionLost;
        private Func<CancellationToken, ValueTask> _onConnectionAdded;
        private Func<CancellationToken, ValueTask> _onConnectionRemoved;

        /// <summary>
        /// Gets or sets the function that triggers an object lifecycle event.
        /// </summary>
        public Func<DistributedObjectLifecycleEventType, DistributedObjectLifecycleEventArgs, CancellationToken, ValueTask> OnObjectLifecycleEvent
        {
            get => _onObjectLifeCycleEvent;
            set
            {
                if (_readonlyProperties)
                    throw new InvalidOperationException(ExceptionMessages.PropertyIsNowReadOnly);
                _onObjectLifeCycleEvent = value;
            }
        }

        /// <summary>
        /// Gets or sets the function that triggers a member lifecycle event.
        /// </summary>
        public Func<MemberLifecycleEventType, MemberLifecycleEventArgs, CancellationToken, ValueTask> OnMemberLifecycleEvent
        {
            get => _onMemberLifecycleEvent;
            set
            {
                if (_readonlyProperties)
                    throw new InvalidOperationException(ExceptionMessages.PropertyIsNowReadOnly);
                _onMemberLifecycleEvent = value;
            }
        }

        /// <summary>
        /// Gets or sets the function that triggers a client lifecycle event.
        /// </summary>
        public Func<ClientLifecycleState, CancellationToken, ValueTask> OnClientLifecycleEvent
        {
            get => _onClientLifecycleEvent;
            set
            {
                if (_readonlyProperties)
                    throw new InvalidOperationException(ExceptionMessages.PropertyIsNowReadOnly);
                _onClientLifecycleEvent = value;
            }
        }

        /// <summary>
        /// Gets or sets the function that triggers a partitions updated event.
        /// </summary>
        public Func<CancellationToken, ValueTask> OnPartitionsUpdated
        {
            get => _onPartitionUpdated;
            set
            {
                if (_readonlyProperties)
                    throw new InvalidOperationException(ExceptionMessages.PropertyIsNowReadOnly);
                _onPartitionUpdated = value;
            }
        }

        /// <summary>
        /// Gets or sets the function that triggers a partition list event.
        /// </summary>
        public Func<PartitionLostEventArgs, CancellationToken, ValueTask> OnPartitionLost
        {
            get => _onPartitionLost;
            set
            {
                if (_readonlyProperties)
                    throw new InvalidOperationException(ExceptionMessages.PropertyIsNowReadOnly);
                _onPartitionLost = value;
            }
        }

        /// <summary>
        /// Gets or sets the function that triggers a connection added event.
        /// </summary>
        public Func<CancellationToken, ValueTask> OnConnectionAdded
        {
            get => _onConnectionAdded;
            set
            {
                if (_readonlyProperties)
                    throw new InvalidOperationException(ExceptionMessages.PropertyIsNowReadOnly);
                _onConnectionAdded = value;
            }
        }

        /// <summary>
        /// Gets or sets the function that triggers a connection removed event.
        /// </summary>
        public Func<CancellationToken, ValueTask> OnConnectionRemoved
        {
            get => _onConnectionRemoved;
            set
            {
                if (_readonlyProperties)
                    throw new InvalidOperationException(ExceptionMessages.PropertyIsNowReadOnly);
                _onConnectionRemoved = value;
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
