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
        /// Adds a handler which runs when a member is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers MemberAdded(Action<IHazelcastClient, MemberLifecycleEventArgs> handler)
        {
            Add(new MemberLifecycleEventHandler(MemberLifecycleEventType.Added, handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a member is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers MemberAdded(Func<IHazelcastClient, MemberLifecycleEventArgs, ValueTask> handler)
        {
            Add(new MemberLifecycleEventHandler(MemberLifecycleEventType.Added, handler));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a member is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers MemberRemoved(Action<IHazelcastClient, MemberLifecycleEventArgs> handler)
        {
            Add(new MemberLifecycleEventHandler(MemberLifecycleEventType.Removed, handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a member is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers MemberRemoved(Func<IHazelcastClient, MemberLifecycleEventArgs, ValueTask> handler)
        {
            Add(new MemberLifecycleEventHandler(MemberLifecycleEventType.Removed, handler));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a distributed object is created.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers ObjectCreated(Action<IHazelcastClient, DistributedObjectLifecycleEventArgs> handler)
        {
            Add(new DistributedObjectLifecycleEventHandler(DistributedObjectLifecycleEventType.Created, handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a distributed object is created.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers ObjectCreated(Func<IHazelcastClient, DistributedObjectLifecycleEventArgs, ValueTask> handler)
        {
            Add(new DistributedObjectLifecycleEventHandler(DistributedObjectLifecycleEventType.Created, handler));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a distributed object is destroyed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers ObjectDestroyed(Action<IHazelcastClient, DistributedObjectLifecycleEventArgs> handler)
        {
            Add(new DistributedObjectLifecycleEventHandler(DistributedObjectLifecycleEventType.Destroyed, handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a distributed object is destroyed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers ObjectDestroyed(Func<IHazelcastClient, DistributedObjectLifecycleEventArgs, ValueTask> handler)
        {
            Add(new DistributedObjectLifecycleEventHandler(DistributedObjectLifecycleEventType.Destroyed, handler));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a connection is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        internal HazelcastClientEventHandlers ConnectionAdded(Action<IHazelcastClient, ConnectionLifecycleEventArgs> handler)
        {
            Add(new ConnectionLifecycleEventHandler(ConnectionLifecycleEventType.Added, handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a connection is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        internal HazelcastClientEventHandlers ConnectionAdded(Func<IHazelcastClient, ConnectionLifecycleEventArgs, ValueTask> handler)
        {
            Add(new ConnectionLifecycleEventHandler(ConnectionLifecycleEventType.Added, handler));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a connection is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        internal HazelcastClientEventHandlers ConnectionRemoved(Action<IHazelcastClient, ConnectionLifecycleEventArgs> handler)
        {
            Add(new ConnectionLifecycleEventHandler(ConnectionLifecycleEventType.Removed, handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when a connection is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        internal HazelcastClientEventHandlers ConnectionRemoved(Func<IHazelcastClient, ConnectionLifecycleEventArgs, ValueTask> handler)
        {
            Add(new ConnectionLifecycleEventHandler(ConnectionLifecycleEventType.Removed, handler));
            return this;
        }

        // TODO: consider having 1 event per state
        // original code has 1 unique 'StateChanged' event, could we have eg ClientStarting, ClientStarted, etc...?

        /// <summary>
        /// Adds a handler which runs when the client state changes.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers ClientStateChanged(Action<IHazelcastClient, ClientLifecycleEventArgs> handler)
        {
            Add(new ClientLifecycleEventHandler(handler.AsAsync()));
            return this;
        }

        /// <summary>
        /// Adds a handler which runs when the client state changes.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public HazelcastClientEventHandlers ClientStateChanged(Func<IHazelcastClient, ClientLifecycleEventArgs, ValueTask> handler)
        {
            Add(new ClientLifecycleEventHandler(handler));
            return this;
        }
    }
}
