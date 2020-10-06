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
    public partial interface IHDictionary<TKey, TValue> // Events
    {
        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="events">An event handlers collection builder.</param>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<DictionaryEventHandlers<TKey, TValue>> events, bool includeValues = true);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="events">An event handlers collection builder.</param>
        /// <param name="key">A key to filter events.</param>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<DictionaryEventHandlers<TKey, TValue>> events, TKey key, bool includeValues = true);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="events">An event handlers collection builder.</param>
        /// <param name="predicate">A predicate to filter events.</param>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<DictionaryEventHandlers<TKey, TValue>> events, IPredicate predicate, bool includeValues = true);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="events">An event handlers collection builder.</param>
        /// <param name="key">A key to filter events.</param>
        /// <param name="predicate">A predicate to filter events.</param>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<DictionaryEventHandlers<TKey, TValue>> events, TKey key, IPredicate predicate, bool includeValues = true);

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
        /// <returns><c>true</c> if subscription is removed successfully, <c>false</c> if there is no such subscription</returns>
        ValueTask<bool> UnsubscribeAsync(Guid subscriptionId);
    }
}
