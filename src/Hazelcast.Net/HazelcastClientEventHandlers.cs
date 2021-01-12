// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Events;

namespace Hazelcast
{
    /// <summary>
    /// Represents the client events.
    /// </summary>
    /// <remarks>
    /// <para>Handlers for events can be synchronous or asynchronous. Asynchronous handlers are defined by
    /// an <c>Action{IHazelcastClient, TArgs}</c> whereas... </para>
    /// </remarks>
    public sealed class HazelcastClientEventHandlers : EventHandlersBase<IHazelcastClientEventHandler>
    {
        /// <summary>
        /// Adds a handler which runs when a partition is lost.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers PartitionLost(Action<IHazelcastClient, PartitionLostEventArgs> handler)
        {
            Add(new PartitionLostEventHandler(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a partition is lost.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers PartitionLost(Func<IHazelcastClient, PartitionLostEventArgs, ValueTask> handler)
        {
            Add(new PartitionLostEventHandler(handler));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when partitions are updated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers PartitionsUpdated(Action<IHazelcastClient, EventArgs> handler)
        {
            Add(new PartitionsUpdatedEventHandler(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when partitions are updated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers PartitionsUpdated(Func<IHazelcastClient, EventArgs, ValueTask> handler)
        {
            Add(new PartitionsUpdatedEventHandler(handler));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when members are updated
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers MembersUpdated(Action<IHazelcastClient, MembersUpdatedEventArgs> handler)
        {
            Add(new MembersUpdatedEventHandler(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a member is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers MembersUpdated(Func<IHazelcastClient, MembersUpdatedEventArgs, ValueTask> handler)
        {
            Add(new MembersUpdatedEventHandler(handler));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a distributed object is created.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers ObjectCreated(Action<IHazelcastClient, DistributedObjectCreatedEventArgs> handler)
        {
            Add(new DistributedObjectCreatedEventHandler(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a distributed object is created.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers ObjectCreated(Func<IHazelcastClient, DistributedObjectCreatedEventArgs, ValueTask> handler)
        {
            Add(new DistributedObjectCreatedEventHandler(handler));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a distributed object is destroyed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers ObjectDestroyed(Action<IHazelcastClient, DistributedObjectDestroyedEventArgs> handler)
        {
            Add(new DistributedObjectDestroyedEventHandler(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a distributed object is destroyed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers ObjectDestroyed(Func<IHazelcastClient, DistributedObjectDestroyedEventArgs, ValueTask> handler)
        {
            Add(new DistributedObjectDestroyedEventHandler(handler));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a connection is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        internal HazelcastClientEventHandlers ConnectionOpened(Action<IHazelcastClient, ConnectionOpenedEventArgs> handler)
        {
            Add(new ConnectionOpenedEventHandler(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a connection is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        internal HazelcastClientEventHandlers ConnectionOpened(Func<IHazelcastClient, ConnectionOpenedEventArgs, ValueTask> handler)
        {
            Add(new ConnectionOpenedEventHandler(handler));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a connection is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        internal HazelcastClientEventHandlers ConnectionClosed(Action<IHazelcastClient, ConnectionClosedEventArgs> handler)
        {
            Add(new ConnectionClosedEventHandler(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a connection is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        internal HazelcastClientEventHandlers ConnectionClosed(Func<IHazelcastClient, ConnectionClosedEventArgs, ValueTask> handler)
        {
            Add(new ConnectionClosedEventHandler(handler));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when the client state changes.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers StateChanged(Action<IHazelcastClient, StateChangedEventArgs> handler)
        {
            Add(new StateChangedEventHandler(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when the client state changes.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers StateChanged(Func<IHazelcastClient, StateChangedEventArgs, ValueTask> handler)
        {
            Add(new StateChangedEventHandler(handler));
            return this;
        }
    }
}
