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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Defines the base interface for various Hazelcast distributed dictionaries.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the map.</typeparam>
    /// <typeparam name="TValue">The type of values in the map.</typeparam>
    /// <seealso cref="IHMap{TKey,TValue}"/>
    /// <seealso cref="IHReplicatedMap{TKey,TValue}"/>
    public interface IHMapBase<TKey, TValue> : IDistributedObject, IAsyncEnumerable<KeyValuePair<TKey, TValue>>
    {
        //getting

        /// <summary>
        /// Gets the value for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value for the specified key, if any; otherwise <c>default(TValue)</c>.</returns>
        /// <remarks>
        /// <para>This methods <strong>interacts with the server-side <c>MapStore</c></strong>.
        /// If no value for the specified key is found in memory, <c>MapLoader.load(...)</c> is invoked
        /// to try to load the value from the <c>MapStore</c> backing the map, if any.</para>
        /// </remarks>
        Task<TValue> GetAsync(TKey key);

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{TKey}" /> of the keys contained in this map.
        /// </summary>
        /// <returns>A <see cref="IReadOnlyCollection{TKey}" /> of the keys contained in this map.</returns>
        /// <remarks>
        /// <para>This method <strong>does not interact with the server-side <c>MapStore</c></strong>.
        /// It returns the keys found in memory, but does not look for more keys in the <c>MapStore</c>
        /// backing the map, if any.</para>
        /// </remarks>
        Task<IReadOnlyCollection<TKey>> GetKeysAsync();

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{TValue}" /> of the values contained in this map.
        /// </summary>
        /// <returns>A <see cref="IReadOnlyCollection{TValue}" /> of the values contained in this map.</returns>
        /// <remarks>
        /// <para>This method <strong>does not interact with the server-side <c>MapStore</c></strong>.
        /// It returns the values found in memory, but does not look for more values in the <c>MapStore</c>
        /// backing the map, if any.</para>
        /// </remarks>
        Task<IReadOnlyCollection<TValue>> GetValuesAsync();

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{TKey, TValue}" /> of the entries contained in this map.
        /// </summary>
        /// <returns>A <see cref="IReadOnlyDictionary{TKey, TValue}" /> of the <see cref="IHMapBase{TKey,TValue}"/> in this map.</returns>
        /// <remarks>
        /// <para>This method <strong>does not interact with the server-side <c>MapStore</c></strong>.
        /// It returns the entries found in memory, but does not look for more entries in the <c>MapStore</c>
        /// backing the map, if any.</para>
        /// </remarks>
        Task<IReadOnlyDictionary<TKey, TValue>> GetEntriesAsync();

        /// <summary>Gets the number of entries contained in this map.</summary>
        /// <returns>The number of entries contained in this map.</returns>
        // TODO: document MapStore behavior
        Task<int> GetSizeAsync();

        /// <summary>Determines whether this map contains no entries.</summary>
        /// <returns><c>true</c> if this map contains no entries; otherwise <c>false</c>.</returns>
        // TODO: document MapStore behavior
        Task<bool> IsEmptyAsync();

        /// <summary>
        /// Determines whether this map contains an entry for the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key contains key; otherwise <c>false</c>.</returns>
        // TODO: document MapStore behavior
        Task<bool> ContainsKeyAsync(TKey key);

        /// <summary>
        /// Determines whether this map contains one or more keys to the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if this map contains one or more keys to the specified value; otherwise <c>false</c>.</returns>
        // TODO: document MapStore behavior
        Task<bool> ContainsValueAsync(TValue value);

        //setting

        /// <summary>
        /// Sets (adds or updates) an entry, and returns the previous value, if any.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The previous value for the specified key, if any; otherwise <c>default(TValue)</c>.</returns>
        // TODO: document MapStore behavior
        Task<TValue> PutAsync(TKey key, TValue value);

        /// <summary>
        /// Sets (adds or updates) an entry with a time-to-live, and returns the previous value, if any.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">A time-to-live period.</param>
        /// <returns>The previous value for the specified key, if any; otherwise <c>default(TValue)</c>.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the
        /// <paramref name="timeToLive"/> has elapsed.</para>
        /// TODO: document zero and infinite
        /// </remarks>
        // TODO: document MapStore behavior
        Task<TValue> PutAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Sets (adds or updates) entries.
        /// </summary>
        /// <param name="entries">The entries.</param>
        /// <returns>A task that will complete when entries have been added or updated.</returns>
        Task SetAllAsync(IDictionary<TKey, TValue> entries);

        //removing

        /// <summary>
        /// Clears the map by deleting all entries.
        /// </summary>
        /// <returns>A task that will complete when the map has been cleared.</returns>
        Task ClearAsync();

        /// <summary>
        /// Removes an entry, and returns the removed value, if any.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The removed value, if any; otherwise <c>default(TValue)</c>.</returns>
        // TODO: document MapStore behavior
        Task<TValue> RemoveAsync(TKey key);
    }
}
