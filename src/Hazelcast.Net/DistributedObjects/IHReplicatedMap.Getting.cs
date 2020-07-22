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
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    public partial interface IHReplicatedMap<TKey, TValue> // Getting
    {
        /// <summary>
        ///     Returns the value for the specified key, or <c>null</c> if this map does not contain this key.
        /// </summary>
        /// <remarks>
        ///     Returns the value for the specified key, or <c>null</c> if this map does not contain this key.
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method returns a clone of original value, modifying the returned value does not change
        ///         the actual value in the map. One should put modified value back to make changes visible to all nodes.
        ///         <code>
        /// var value = map.Get(key);
        /// value.UpdateSomeProperty();
        /// map.put(key, value);
        /// </code>
        ///     </p>
        ///     <p />
        ///     <p>
        ///         <b>Warning-2:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        ///     <p />
        /// </remarks>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        Task<TValue> GetAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>Returns a set clone of the keys contained in this map.</summary>
        /// <remarks>
        ///     Returns a set clone of the keys contained in this map.
        ///     The set is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the set, and vice-versa.
        /// </remarks>
        /// <returns>a <see cref="ISet{E}" /> clone of the keys contained in this map</returns>
        Task<IReadOnlyList<TKey>> GetKeysAsync(CancellationToken cancellationToken = default);

        /// <summary>Returns a collection clone of the values contained in this map.</summary>
        /// <remarks>
        ///     Returns a collection clone of the values contained in this map.
        ///     The collection is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the collection, and vice-versa.
        /// </remarks>
        /// <returns>a collection clone of the values contained in this map</returns>
        Task<IReadOnlyList<TValue>> GetValuesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a <see cref="ISet{E}" /> clone of the mappings contained in this map.
        /// </summary>
        /// <remarks>
        ///     Returns a
        ///     <see cref="ISet{E}" />
        ///     clone of the mappings contained in this map.
        ///     The set is <b>NOT</b> backed by the map,
        ///     so changes to the map are <b>NOT</b> reflected in the set, and vice-versa.
        /// </remarks>
        /// <returns>a set clone of the keys mappings in this map</returns>
        Task<IReadOnlyDictionary<TKey, TValue>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>Returns the number of entries in this map.</summary>
        /// <remarks>Returns the number of entries in this map.</remarks>
        /// <returns>the number of entries in this map</returns>
        Task<int> CountAsync(CancellationToken cancellationToken = default);

        /// <summary>Returns <c>true</c> if this map contains no entries.</summary>
        /// <returns><c>true</c> if this map contains no entries</returns>
        Task<bool> IsEmptyAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Determines whether this map contains an entry for the specified key.
        /// </summary>
        /// <remarks>
        ///     Determines whether this map contains an entry for the specified key.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         Ë†
        ///         This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///         the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///         defined in <c>key</c>'s class.
        ///     </p>
        /// </remarks>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the specified key contains key; otherwise, <c>false</c>.</returns>
        Task<bool> ContainsKeyAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Determines whether this map contains one or more keys to the specified value
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns <c>true</c> if this map contains one or more keys to the specified value</returns>
        Task<bool> ContainsValueAsync(TValue value, CancellationToken cancellationToken = default);
    }
}
