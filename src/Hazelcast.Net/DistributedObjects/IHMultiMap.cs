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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Represents a distributed map whose keys can be associated with multiple values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>IHMultiMap</c> can be configured on Server side to allow duplicate values or not for its values collection
    /// </para>
    /// </remarks>
    public interface IHMultiMap<TKey, TValue> : IDistributedObject, IKeyLockable<TKey>, IAsyncEnumerable<KeyValuePair<TKey, TValue>>
    {
        // Events

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="events">An event handlers collection builder.</param>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <param name="state">A state object.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<MultiMapEventHandlers<TKey, TValue>> events, bool includeValues = true, object state = null);

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        /// <param name="events">An event handlers collection builder.</param>
        /// <param name="key">A key to filter events.</param>
        /// <param name="includeValues">Whether to include values in event arguments.</param>
        /// <param name="state">A state object.</param>
        /// <returns>The unique identifier of the subscription.</returns>
        Task<Guid> SubscribeAsync(Action<MultiMapEventHandlers<TKey, TValue>> events, TKey key, bool includeValues = true, object state = null);

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

        // Setting

        /// <summary>Stores a key-value pair in the multi-map.</summary>
        /// <param name="key">the key to be stored</param>
        /// <param name="value">the value to be stored</param>
        /// <returns>
        /// <c>true</c> if size of the multi-map is increased, <c>false</c> otherwise.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        /// the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        /// defined in <c>key</c>'s class.
        /// </para>
        /// </remarks>
        Task<bool> PutAsync(TKey key, TValue value);

        // Getting

        /// <summary>Returns the collection of values associated with the key.</summary>
        /// <param name="key">the key whose associated values are to be returned</param>
        /// <returns>the collection of the values associated with the key.</returns>
        Task<IReadOnlyCollection<TValue>> GetAsync(TKey key);

        /// <summary>Returns the set of key-value pairs in the multi-map.</summary>
        /// <returns>the collection of key-value pairs in the multi-map. </returns>
        Task<IReadOnlyCollection<KeyValuePair<TKey, TValue>>> GetEntriesAsync();

        /// <summary>Returns the set of keys in the multi-map.</summary>
        /// <returns>the collection of keys in the multi-map.</returns>
        Task<IReadOnlyCollection<TKey>> GetKeysAsync();

        /// <summary>Returns the collection of values in the multi-map.</summary>
        /// <returns>the collection of values in the multi-map.</returns>
        Task<IReadOnlyCollection<TValue>> GetValuesAsync();

        /// <summary>Returns whether the multi-map contains the given key-value pair.</summary>
        /// <param name="key">the key whose existence is checked.</param>
        /// <param name="value">the value whose existence is checked.</param>
        /// <returns>true if the multi-map contains the key-value pair, false otherwise.</returns>
        Task<bool> ContainsEntryAsync(TKey key, TValue value);

        /// <summary>Returns whether the multi-map contains an entry with the key.</summary>
        /// <param name="key">the key whose existence is checked.</param>
        /// <returns>true if the multi-map contains an entry with the key, false otherwise.</returns>
        Task<bool> ContainsKeyAsync(TKey key);

        /// <summary>Returns whether the multi-map contains an entry with the value.</summary>
        /// <param name="value">the value whose existence is checked.</param>
        /// <returns>true if the multi-map contains an entry with the value, false otherwise.</returns>
        Task<bool> ContainsValueAsync(TValue value);

        /// <summary>Returns the number of key-value pairs in the multi-map.</summary>
        /// <returns>the number of key-value pairs in the multi-map.</returns>
        Task<int> GetSizeAsync();

        /// <summary>Returns number of values matching to given key in the multi-map.</summary>
        /// <param name="key">the key whose values count are to be returned</param>
        /// <returns>number of values matching to given key in the multi-map.</returns>
        Task<int> GetValueCountAsync(TKey key);

        // Removing

        /// <summary>Removes the given key value pair from the multi-map.</summary>
        /// <param name="key">the key of the entry to remove</param>
        /// <param name="value">the value of the entry to remove</param>
        /// <returns>true if the size of the multi-map changed after the remove operation, false otherwise.</returns>
        Task<bool> RemoveAsync(TKey key, TValue value);

        /// <summary>Removes all the entries with the given key.</summary>
        /// <param name="key">the key of the entries to remove</param>
        /// <returns>the collection of removed values associated with the given key</returns>
        Task<IReadOnlyCollection<TValue>> RemoveAsync(TKey key);

        /// <summary>
        /// Removes all the entries with the given key.
        /// </summary>
        /// <param name="key">the key of the entries to remove</param>
        Task DeleteAsync(TKey key);

        /// <summary>Clears the multi-map. Removes all key-value pairs.</summary>
        Task ClearAsync();
    }
}
