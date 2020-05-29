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
using Hazelcast.Core;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents the cluster events.
    /// </summary>
    public sealed class ClusterEventHandlers : EventHandlersBase<IClusterEventHandler>
    {
        /// <summary>
        /// Adds an handler which runs when a partition is lost.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ClusterEventHandlers PartitionLost(Action<Cluster, PartitionLostEventArgs> handler)
        {
            Add(new PartitionLostEventHandler(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when partitions are updated.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ClusterEventHandlers PartitionsUpdated(Action<Cluster, EventArgs> handler)
        {
            Add(new PartitionsUpdatedEventHandler(handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a member is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ClusterEventHandlers MemberAdded(Action<Cluster, ClusterMemberLifecycleEventArgs> handler)
        {
            Add(new ClusterMemberLifecycleEventHandler(ClusterMemberLifecycleEventType.Added, handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a member is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ClusterEventHandlers MemberRemoved(Action<Cluster, ClusterMemberLifecycleEventArgs> handler)
        {
            Add(new ClusterMemberLifecycleEventHandler(ClusterMemberLifecycleEventType.Removed, handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a distributed object is created.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ClusterEventHandlers ObjectCreated(Action<Cluster, ClusterObjectLifecycleEventArgs> handler)
        {
            Add(new ClusterObjectLifecycleEventHandler(ClusterObjectLifecycleEventType.Created, handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a distributed object is destroyed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ClusterEventHandlers ObjectDestroyed(Action<Cluster, ClusterObjectLifecycleEventArgs> handler)
        {
            Add(new ClusterObjectLifecycleEventHandler(ClusterObjectLifecycleEventType.Destroyed, handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a connection is added.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ClusterEventHandlers ConnectionAdded(Action<Cluster, ConnectionLifecycleEventArgs> handler)
        {
            Add(new ConnectionLifecycleEventHandler(ConnectionLifecycleEventType.Added, handler));
            return this;
        }

        /// <summary>
        /// Adds an handler which runs when a connection is removed.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ClusterEventHandlers ConnectionRemoved(Action<Cluster, ConnectionLifecycleEventArgs> handler)
        {
            Add(new ConnectionLifecycleEventHandler(ConnectionLifecycleEventType.Removed, handler));
            return this;
        }

        // TODO: consider having 1 event per state
        // original code has 1 unique 'StateChanged' event, could we have eg ClientStarting, ClientStarted, etc...?

        /// <summary>
        /// Adds an handler which runs when the client state changes.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <returns>The handlers.</returns>
        public ClusterEventHandlers ClientStateChanged(Action<Cluster, ClientLifecycleEventArgs> handler)
        {
            Add(new ClientLifecycleEventHandler(handler));
            return this;
        }
    }
}
