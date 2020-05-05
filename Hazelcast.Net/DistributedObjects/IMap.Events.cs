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
using Hazelcast.Predicates;

namespace Hazelcast.DistributedObjects
{
    // partial: events
    public partial interface IMap<TKey, TValue>
    {
        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="on">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<MapEvents<TKey, TValue>> on);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <param name="on">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(bool includeValues, Action<MapEvents<TKey, TValue>> on);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="key">A key to filter events.</param>
        /// <param name="on">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(TKey key, Action<MapEvents<TKey, TValue>> on);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <param name="key">A key to filter events.</param>
        /// <param name="on">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(bool includeValues, TKey key, Action<MapEvents<TKey, TValue>> on);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="predicate">A predicate to filter events.</param>
        /// <param name="on">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(IPredicate predicate, Action<MapEvents<TKey, TValue>> on);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <param name="predicate">A predicate to filter events.</param>
        /// <param name="on">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(bool includeValues, IPredicate predicate, Action<MapEvents<TKey, TValue>> on);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="key">A key to filter events.</param>
        /// <param name="predicate">A predicate to filter events.</param>
        /// <param name="on">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(TKey key, IPredicate predicate, Action<MapEvents<TKey, TValue>> on);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <param name="key">A key to filter events.</param>
        /// <param name="predicate">A predicate to filter events.</param>
        /// <param name="on">An event handlers collection builder.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(bool includeValues, TKey key, IPredicate predicate, Action<MapEvents<TKey, TValue>> on);

        /// <summary>
        /// Unsubscribe from events.
        /// </summary>
        /// <param name="subscriptionId">The unique identifier of the subscription.</param>
        /// <returns>?</returns>
        /// TODO: what is returned? how could this fail?
        Task<bool> UnsubscribeAsync(Guid subscriptionId);
    }
}