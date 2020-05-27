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

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Provides extension to the <see cref="ClusterEventHandlers"/> class.
    /// </summary>
    public static class ClusterObjectLifecycleExtensions
    {
        /// <summary>
        /// Adds an handler for cluster object creation events.
        /// </summary>
        /// <returns>The cluster events.</returns>
        public static ClusterEventHandlers ObjectCreated(this ClusterEventHandlers handlers, Action<Cluster, ClusterObjectLifecycleEventArgs> handler)
        {
            handlers.Add(new ClusterObjectLifecycleEventHandler(ClusterObjectLifecycleEventType.Created, handler));
            return handlers;
        }

        /// <summary>
        /// Adds an handler for cluster object destruction events.
        /// </summary>
        /// <returns>The cluster events.</returns>
        public static ClusterEventHandlers ObjectDestroyed(this ClusterEventHandlers handlers, Action<Cluster, ClusterObjectLifecycleEventArgs> handler)
        {
            handlers.Add(new ClusterObjectLifecycleEventHandler(ClusterObjectLifecycleEventType.Destroyed, handler));
            return handlers;
        }
    }
}
