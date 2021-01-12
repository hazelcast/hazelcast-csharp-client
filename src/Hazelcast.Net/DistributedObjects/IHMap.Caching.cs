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

namespace Hazelcast.DistributedObjects
{
    // ReSharper disable once UnusedTypeParameter
    public partial interface IHMap<TKey, TValue> // Caching
    {
        /// <summary>
        /// Evicts the specified key from the map.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the entry was evicted; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// If a <c>MapStore</c> on server is defined for this map,
        /// then the entry is not deleted from the underlying <c>MapStore</c>,
        /// evict only removes the entry from the memory.
        /// Use <see cref="DeleteAsync"/> or <see cref="GetAndRemoveAsync(TKey)"/>
        /// if <c>MapStore.delete(object)</c> needs to be called.
        /// </para>
        /// <para>
        /// This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        /// the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        /// defined in <c>key</c>'s class.
        /// </para>
        /// </remarks>
        Task<bool> EvictAsync(TKey key);

        /// <summary>
        /// Evicts all entries but the locked entries from the cache.
        /// </summary>
        /// <returns>A task that will complete when all entries have been evicted.</returns>
        /// <remarks>
        /// <para>
        /// If a <c>MapStore</c> is defined on server for this map,
        /// then <c>MapStore.deleteAll</c> is not called by this method,
        /// If you do want <c>MapStore.deleteAll</c> to be called use the <see cref="ClearAsync"/> method.
        /// </para>
        /// </remarks>
        Task EvictAllAsync();

        /// <summary>
        /// Flushes the <c>MapStore</c> on server, if any.
        /// </summary>
        /// <returns>A task that will complete when the map store has been flushed.</returns>
        /// <remarks>
        /// <para>If a <c>MapStore</c> is defined for this map, this method flushes
        /// all dirty entries by deleting or storing them.</para>
        /// </remarks>
        Task FlushAsync();

        /// <summary>
        /// Loads all keys into the store.
        /// </summary>
        /// <param name="replaceExistingValues">
        /// when <c>true</c>, existing values in the <see cref="IHMap{TKey,TValue}"/> will be replaced by those loaded from the MapLoader
        /// </param>
        /// <returns>A task that will complete when the map store has been loaded.</returns>
        Task LoadAllAsync(bool replaceExistingValues);

        /// <summary>
        /// Loads the given keys into the store.
        /// </summary>
        /// <param name="keys">keys of the values entries to load (keys inside the collection cannot be null)</param>
        /// <param name="replaceExistingValues">
        /// when <c>true</c>, existing values in the <see cref="IHMap{TKey,TValue}"/> will be replaced by those loaded from the MapLoader
        /// </param>
        /// <returns>A task that will complete when the map store has been loaded.</returns>
        Task LoadAllAsync(ICollection<TKey> keys, bool replaceExistingValues);
    }
}
