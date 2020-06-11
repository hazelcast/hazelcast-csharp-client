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
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    // ReSharper disable once UnusedTypeParameter
    public partial interface IHMap<TKey, TValue> // Caching
    {
        /// <summary>
        /// Evicts an entry from the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>true if the entry was evicted; otherwise false.</returns>
        /// <remarks>
        /// <para>Locked entries are not evicted (TODO: true?)</para>
        /// <para>Evicts the entry from the in-memory cache. The entry is not removed from
        /// the map. If a <see cref="MapStore"/> is defined for this map, The entry is
        /// not evicted from the map store.</para>
        /// </remarks>
        Task<bool> EvictAsync(TKey key, TimeSpan timeout = default);

        /// <summary>
        /// Evicts an entry from the cache.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>true if the entry was evicted; otherwise false.</returns>
        /// <remarks>
        /// <para>Locked entries are not evicted (TODO: true?)</para>
        /// <para>Evicts the entry from the in-memory cache. The entry is not removed from
        /// the map. If a <see cref="MapStore"/> is defined for this map, The entry is
        /// not evicted from the map store.</para>
        /// </remarks>
        Task<bool> EvictAsync(TKey key, CancellationToken cancellationToken);

        /// <summary>
        /// Evicts all entries but the locked entries from the cache.
        /// </summary>
        /// <remarks>
        /// <para>Locked entries are not evicted.</para>
        /// <para>Evicts entries from the in-memory cache. Entries are not removed from
        /// the map. If a <see cref="MapStore"/> is defined for this map, entries are
        /// not evicted from the map store.</para>
        /// </remarks>
        Task EvictAllAsync(TimeSpan timeout = default);

        /// <summary>
        /// Evicts all entries but the locked entries from the cache.
        /// </summary>
        /// <remarks>
        /// <para>Locked entries are not evicted.</para>
        /// <para>Evicts entries from the in-memory cache. Entries are not removed from
        /// the map. If a <see cref="MapStore"/> is defined for this map, entries are
        /// not evicted from the map store.</para>
        /// </remarks>
        Task EvictAllAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Flushes the map store, if any.
        /// </summary>
        /// <remarks>
        /// <para>If a <see cref="MapStore"/> is defined for this map, this method flushes
        /// all dirty entries by deleting or storing them.</para>
        /// </remarks>
        Task FlushAsync(TimeSpan timeout = default);

        /// <summary>
        /// Flushes the map store, if any.
        /// </summary>
        /// <remarks>
        /// <para>If a <see cref="MapStore"/> is defined for this map, this method flushes
        /// all dirty entries by deleting or storing them.</para>
        /// </remarks>
        Task FlushAsync(CancellationToken cancellationToken);
    }
}
