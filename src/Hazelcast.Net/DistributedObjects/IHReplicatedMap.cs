// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Query;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents a distributed map with weak consistency and values locally stored on every node of the cluster.
    /// </summary>
    /// <remarks>
    /// <p>Whenever a value is written asynchronously, the new value will be internally
    /// distributed to all existing cluster members, and eventually every node will have
    /// the new value.</p>
    /// <p>When a new node joins the cluster, the new node initially will request existing
    ///  values from older nodes and replicate them locally.</p>
    /// </remarks>
    /// <typeparam name="TKey">the type of keys maintained by this map</typeparam>
    /// <typeparam name="TValue">the type of mapped values</typeparam>
    public interface IHReplicatedMap<TKey, TValue> : IHMapBase<TKey, TValue>
    {
        // Events
        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="events">An event handlers collection builder.</param>
        /// <param name="state">A state object.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<ReplicatedMapEventHandlers<TKey, TValue>> events, object state = null);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="events">An event handlers collection builder.</param>
        /// <param name="key">A key to filter events.</param>
        /// <param name="state">A state object.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<ReplicatedMapEventHandlers<TKey, TValue>> events, TKey key, object state = null);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="events">An event handlers collection builder.</param>
        /// <param name="predicate">A predicate to filter events.</param>
        /// <param name="state">A state object.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<ReplicatedMapEventHandlers<TKey, TValue>> events, IPredicate predicate, object state = null);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="events">An event handlers collection builder.</param>
        /// <param name="key">A key to filter events.</param>
        /// <param name="predicate">A predicate to filter events.</param>
        /// <param name="state">A state object.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<ReplicatedMapEventHandlers<TKey, TValue>> events, TKey key, IPredicate predicate, object state = null);

        /// <summary>
        /// Unsubscribe from events.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <remarks>
        /// <para>
        /// When this method completes, handler will stop receiving events immediately.
        /// Member side event subscriptions will eventually be removed.
        /// </para>
        /// </remarks>
        /// <returns><c>true</c> if subscription is removed successfully, <c>false</c> if there is no such subscription</returns>
        ValueTask<bool> UnsubscribeAsync(Guid subscriptionId);
    }
}
