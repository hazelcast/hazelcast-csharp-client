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
using Hazelcast.Events;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Provides the cluster-level events management service for a cluster.
    /// </summary>
    internal partial class ClusterEvents
    {
        private readonly ObjectLifecycleEventSubscription _objectLifecycleEventSubscription;
        private readonly PartitionLostEventSubscription _partitionLostEventSubscription;

        private Func<DistributedObjectCreatedEventArgs, ValueTask> _objectCreated;
        private Func<DistributedObjectDestroyedEventArgs, ValueTask> _objectDestroyed;
        private Func<PartitionLostEventArgs, ValueTask> _partitionLost;

        
        #region Event

        /// <summary>
        /// Gets or sets the function that triggers when an object is created.
        /// </summary>
        public Func<DistributedObjectCreatedEventArgs, ValueTask> ObjectCreated
        {
            get => _objectCreated;
            set
            {
                _clusterState.ThrowIfPropertiesAreReadOnly();
                _objectCreated = value;
            }
        }

        /// <summary>
        /// Gets or sets the function that triggers when an object is destroyed.
        /// </summary>
        public Func<DistributedObjectDestroyedEventArgs, ValueTask> ObjectDestroyed
        {
            get => _objectDestroyed;
            set
            {
                _clusterState.ThrowIfPropertiesAreReadOnly();
                _objectDestroyed = value;
            }
        }

        /// <summary>
        /// Gets or sets the function that triggers a partition list event.
        /// </summary>
        public Func<PartitionLostEventArgs, ValueTask> PartitionLost
        {
            get => _partitionLost;
            set
            {
                _clusterState.ThrowIfPropertiesAreReadOnly();
                _partitionLost = value;
            }
        }

        #endregion


        /// <summary>
        /// Adds an object lifecycle event subscription.
        /// </summary>
        public Task AddObjectLifecycleSubscription()
            => _objectLifecycleEventSubscription.AddSubscription();

        /// <summary>
        /// Adds a partition lost event subscription.
        /// </summary>
        public Task AddPartitionLostSubscription()
            => _partitionLostEventSubscription.AddSubscription();

        /// <summary>
        /// Removes an object lifecycle event subscription.
        /// </summary>
        public ValueTask<bool> RemoveObjectLifecycleSubscription()
            => _objectLifecycleEventSubscription.RemoveSubscription();

        /// <summary>
        /// Removes a partition lost event subscription.
        /// </summary>
        public ValueTask<bool> RemovePartitionLostSubscription()
            => _partitionLostEventSubscription.RemoveSubscription();
    }
}
