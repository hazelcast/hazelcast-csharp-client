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
    public partial interface IHMap<TKey, TValue> // Events
    {
        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="events">An event handlers collection builder.</param>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <param name="state">A state object.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<MapEventHandlers<TKey, TValue>> events, bool includeValues = true, object state = null);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="events">An event handlers collection builder.</param>
        /// <param name="key">A key to filter events.</param>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <param name="state">A state object.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<MapEventHandlers<TKey, TValue>> events, TKey key, bool includeValues = true, object state = null);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="events">An event handlers collection builder.</param>
        /// <param name="predicate">A predicate to filter events.</param>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <param name="state">A state object.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        /// <remarks>
        /// <para>Note that some methods such as <see cref="DeleteAsync"/> may break the
        /// events contract in some situations, such as when the predicate refers to the
        /// entry value. Refer to the documentation for these methods for more details.</para>
        /// </remarks>
        Task<Guid> SubscribeAsync(Action<MapEventHandlers<TKey, TValue>> events, IPredicate predicate, bool includeValues = true, object state = null);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="events">An event handlers collection builder.</param>
        /// <param name="key">A key to filter events.</param>
        /// <param name="predicate">A predicate to filter events.</param>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <param name="state">A state object.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        /// <remarks>
        /// <para>Note that some methods such as <see cref="DeleteAsync"/> may break the
        /// events contract in some situations, such as when the predicate refers to the
        /// entry value. Refer to the documentation for these methods for more details.</para>
        /// </remarks>
        Task<Guid> SubscribeAsync(Action<MapEventHandlers<TKey, TValue>> events, TKey key, IPredicate predicate, bool includeValues = true, object state = null);

        /// <summary>
        /// Unsubscribe from events.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <remarks>
        /// <para>
        /// When this method completes, event handler will stop receiving events immediately.
        /// Member side event subscriptions will eventually be removed.
        /// </para>
        /// </remarks>
        /// <returns><c>true</c> if a subscription with the specified identifier was removed successfully; otherwise, if no subscription was found with the specified identifier, <c>false</c>.</returns>
        ValueTask<bool> UnsubscribeAsync(Guid subscriptionId);
    }
}
