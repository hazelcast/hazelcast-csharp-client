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

namespace Hazelcast.Clustering
{
    internal partial class Cluster // ClusterEvents
    {
        /// <summary>
        /// Initializes the object lifecycle event.
        /// </summary>
        /// <returns>The object lifecycle event manager.</returns>
        private ObjectLifecycleEventSubscription InitializeObjectLifecycleEventSubscription()
        {
            return new ObjectLifecycleEventSubscription(this, _loggerFactory, IsSmartRouting)
            {
                Handle = OnObjectLifecycleEvent
            };
        }

        /// <summary>
        /// Initializes the partition lost event.
        /// </summary>
        /// <returns>The object lifecycle event manager.</returns>
        private PartitionLostEventSubscription InitializePartitionLostEventSubscription()
        {
            return new PartitionLostEventSubscription(this, _loggerFactory, IsSmartRouting)
            {
                Handle = OnPartitionLost
            };
        }

        /// <summary>
        /// Adds an object lifecycle event subscription.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        public Task AddObjectLifecycleEventSubscription(CancellationToken cancellationToken)
            => _objectLifecycleEventSubscription.AddSubscription(cancellationToken);

        /// <summary>
        /// Adds a partition lost event subscription.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        public Task AddPartitionLostEventSubscription(CancellationToken cancellationToken)
            => _partitionLostEventSubscription.AddSubscription(cancellationToken);

        /// <summary>
        /// Removes an object lifecycle event subscription.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        public Task RemoveObjectLifecycleEventSubscription(CancellationToken cancellationToken)
            => _objectLifecycleEventSubscription.RemoveSubscription(cancellationToken);

        /// <summary>
        /// Removes a partition lost event subscription.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        public Task RemovePartitionLostEventSubscription(CancellationToken cancellationToken)
            => _partitionLostEventSubscription.RemoveSubscription(cancellationToken);
    }
}
