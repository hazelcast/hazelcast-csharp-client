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

using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Defines an interface for classes that can subscribe to cluster events.
    /// </summary>
    public interface IClusterEventSubscriber
    {
        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when events have been subscribed to.</returns>
        Task SubscribeAsync(Cluster cluster, CancellationToken cancellationToken);
    }
}