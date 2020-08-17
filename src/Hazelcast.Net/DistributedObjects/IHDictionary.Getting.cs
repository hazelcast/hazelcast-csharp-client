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

using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Data;
using Hazelcast.Predicates;

namespace Hazelcast.DistributedObjects
{
    public partial interface IHDictionary<TKey, TValue> // Getting
    {
        /// <summary>
        /// Gets the value for a key, or <c>null</c> if the map does not contain an entry with this key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value for the specified key, or <c>null</c> if the map does not contain an entry with this key.</returns>
        Task<TValue> GetAsync(TKey key);

        /// <summary>
        /// Gets all entries for keys.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns>The values for the specified keys.</returns>
        Task<IReadOnlyDictionary<TKey, TValue>> GetAsync(ICollection<TKey> keys);

        /// <summary>
        /// Gets an entry for a key, or <c>null</c> if the map does not contain an entry with this key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>An entry for the specified key, or <c>null</c> if the map does not contain an entry with this key.</returns>
        Task<IHDictionaryEntry<TKey, TValue>> GetEntryAsync(TKey key);

        /// <summary>
        /// Queries entries.
        /// </summary>
        /// <returns>All entries.</returns>
        Task<IReadOnlyDictionary<TKey, TValue>> GetAsync();

        /// <summary>
        /// Queries entries.
        /// </summary>
        /// <param name="predicate">A predicate to filter the entries with.</param>
        /// <returns>Entries matching the <paramref name="predicate"/>.</returns>
        /// <remarks>
        /// <para>The <paramref name="predicate"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<IReadOnlyDictionary<TKey, TValue>> GetAsync(IPredicate predicate);

        /// <summary>
        /// Gets keys.
        /// </summary>
        /// <returns>All keys.</returns>
        Task<IReadOnlyList<TKey>> GetKeysAsync();

        /// <summary>
        /// Gets keys.
        /// </summary>
        /// <param name="predicate">An predicate to filter the entries with.</param>
        /// <returns>All keys matching the predicate.</returns>
        Task<IReadOnlyList<TKey>> GetKeysAsync(IPredicate predicate);

        /// <summary>
        /// Gets all values.
        /// </summary>
        /// <returns>All values.</returns>
        Task<IReadOnlyList<TValue>> GetValuesAsync();

        /// <summary>
        /// Gets values for entries matching a predicate.
        /// </summary>
        /// <param name="predicate">An optional predicate to filter the entries.</param>
        /// <returns>All values.</returns>
        /// <remarks>
        /// <para>The <paramref name="predicate"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<IReadOnlyList<TValue>> GetValuesAsync(IPredicate predicate);

        /// <summary>
        /// Gets the number of entries.
        /// </summary>
        /// <returns>The total number of entries in the map.</returns>
        Task<int> CountAsync();

        /// <summary>
        /// Determines whether this map is empty.
        /// </summary>
        /// <returns><c>true</c> if the map does not contain entries; otherwise <c>false</c>.</returns>
        Task<bool> IsEmptyAsync();

        /// <summary>
        /// Determines whether this map contains an entry for a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the map contains an entry for the specified key; otherwise <c>false</c>.</returns>
        Task<bool> ContainsKeyAsync(TKey key);

        /// <summary>
        /// Determines whether this map contains at least one entry with a value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the map contains at least an entry with the specified value; otherwise <c>false</c>.</returns>
        Task<bool> ContainsAsync(TValue value);
    }
}
