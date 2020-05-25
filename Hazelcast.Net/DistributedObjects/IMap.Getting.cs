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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Data.Map;
using Hazelcast.Predicates;

namespace Hazelcast.DistributedObjects
{
    // partial: getting
    public partial interface IMap<TKey, TValue>
    {
        /// <summary>
        /// Gets the value for a key, or null if the map does not contain an entry with this key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The value for the specified key, or null if the map does not contain an entry with this key.</returns>
        Task<TValue> GetAsync(TKey key, TimeSpan timeout = default);

        /// <summary>
        /// Gets the value for a key, or null if the map does not contain an entry with this key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The value for the specified key, or null if the map does not contain an entry with this key.</returns>
        Task<TValue> GetAsync(TKey key, CancellationToken cancellationToken);

        // FIXME HERE CONTINUE WITH MAP

        /// <summary>
        /// Gets all entries for keys.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns>The values for the specified keys.</returns>
        Task<IReadOnlyDictionary<TKey, TValue>> GetAsync(ICollection<TKey> keys);

        /// <summary>
        /// Gets an entry for a key, or null if the map does not contain an entry with this key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>An entry for the specified key, or null if the map does not contain an entry with this key.</returns>
        Task<IMapEntry<TKey, TValue>> GetEntryAsync(TKey key);

        /// <summary>
        /// Queries entries.
        /// </summary>
        /// <param name="predicate">A predicate to filter the entries with.</param>
        /// <returns>Entries matching the <paramref name="predicate"/>, or all entries.</returns>
        /// <remarks>
        /// <para>The result it *not* backed by the map, so changes to the map are not
        /// reflected, and vice-versa.</para>
        /// <para>The <paramref name="predicate"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<IReadOnlyDictionary<TKey, TValue>> GetAsync(IPredicate predicate = null);

        /// <summary>
        /// Gets keys.
        /// </summary>
        /// <param name="predicate">An optional predicate to filter the entries with.</param>
        /// <returns>All keys.</returns>
        Task<IReadOnlyList<TKey>> GetKeysAsync(IPredicate predicate = null);

        /// <summary>
        /// Gets all values for entries matching a predicate.
        /// </summary>
        /// <param name="predicate">An optional predicate to filter the entries.</param>
        /// <returns>All values.</returns>
        Task<IReadOnlyList<TValue>> GetValuesAsync(IPredicate predicate = null);

        /// <summary>
        /// Gets the number of entries in the map.
        /// </summary>
        /// <returns>The total number of entries in the map.</returns>
        Task<int> CountAsync();

        /// <summary>
        /// Determines whether this map is empty.
        /// </summary>
        /// <returns>true if the map does not contain entries; otherwise false.</returns>
        Task<bool> IsEmptyAsync();

        /// <summary>
        /// Determines whether this map contains an entry for a key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if the map contains an entry for the specified key; otherwise false.</returns>
        Task<bool> ContainsKeyAsync(TKey key);

        /// <summary>
        /// Determines whether this map contains at least one entry with a value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if the map contains at least an entry with the specified value; otherwise false.</returns>
        Task<bool> ContainsValueAsync(TValue value);
    }
}