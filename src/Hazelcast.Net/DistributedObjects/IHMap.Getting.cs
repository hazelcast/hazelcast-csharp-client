// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Models;
using Hazelcast.Query;

namespace Hazelcast.DistributedObjects
{
    public partial interface IHMap<TKey, TValue> // Getting
    {
        /// <summary>
        /// Gets all entries for keys.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns>The values for the specified keys.</returns>
        /// <remarks>
        /// <para>
        /// The returned readonly dictionary is <b>NOT</b> backed by the hazelcast map,
        /// so changes to the returned readonly dictionary are <b>NOT</b> reflected in the <see cref="IHMap{TKey,TValue}"/>,
        /// and vice-versa.
        /// </para>
        /// </remarks>
        Task<IReadOnlyDictionary<TKey, TValue>> GetAllAsync(ICollection<TKey> keys);

        /// <summary>
        /// Queries the map based on the specified predicate and returns matching keys.
        /// </summary>
        /// <param name="predicate">A predicate to filter the entries with.</param>
        /// <returns>readonly clone of all keys matching the predicate.</returns>
        /// <remarks>
        /// <para>
        /// Specified predicate runs on all members in parallel.
        /// </para>
        /// <para>
        /// The returned collection is <b>NOT</b> backed by this dictionary,
        /// so changes to the dictionary are <b>NOT</b> reflected in the collection, and vice-versa.
        /// </para>
        /// <para>The <paramref name="predicate"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<IReadOnlyCollection<TKey>> GetKeysAsync(IPredicate predicate);

        /// <summary>
        /// Queries the map based on the specified predicate and returns a readonly collection of the values of matching entries.
        /// Gets values for entries matching a predicate.
        /// </summary>
        /// <param name="predicate">A predicate to filter the entries.</param>
        /// <returns>readonly collection of the values of matching entries.</returns>
        /// <remarks>
        /// <para>
        /// Specified predicate runs on all members in parallel.
        /// </para>
        /// <para>
        /// The returned collection is <b>NOT</b> backed by this dictionary,
        /// so changes to the dictionary are <b>NOT</b> reflected in the collection, and vice-versa.
        /// </para>
        /// <para>The <paramref name="predicate"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<IReadOnlyCollection<TValue>> GetValuesAsync(IPredicate predicate);

        /// <summary>
        /// Queries the map based on the specified predicate and returns a readonly dictionary of the matching entries.
        /// </summary>
        /// <param name="predicate">A predicate to filter the entries with.</param>
        /// <returns>readonly dictionary of the matching entries.</returns>
        /// <remarks>
        /// <para>
        /// Specified predicate runs on all members in parallel.
        /// </para>
        /// <para>
        /// The returned readonly dictionary is <b>NOT</b> backed by the hazelcast dictionary,
        /// so changes to the returned readonly dictionary are <b>NOT</b> reflected in the <see cref="IHMap{TKey,TValue}"/>,
        /// and vice-versa.
        /// </para>
        /// <para>
        /// The <paramref name="predicate"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<IReadOnlyDictionary<TKey, TValue>> GetEntriesAsync(IPredicate predicate);

        /// <summary>
        /// Gets an entry with statistics for a key, or <c>null</c> if the map does not contain an entry with this key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>An <see cref="IMapEntryStats{TKey,TValue}"/> for the specified key,
        /// or <c>null</c> if the map does not contain an entry with this key.</returns>
        Task<IMapEntryStats<TKey, TValue>> GetEntryViewAsync(TKey key);
    }
}
