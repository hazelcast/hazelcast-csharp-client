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

namespace Hazelcast.DistributedObjects
{
    // partial: setting
    public partial interface IMap<TKey, TValue>
    {
        /// <summary>
        /// Adds or replaces an entry and returns the previous value.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <returns>The value previously associated with the key in the map, if any; otherwise default(<typeparamref name="TValue"/>).</returns>
        Task<TValue> AddOrReplaceWithValueAsync(TKey key, TValue value);

        /// <summary>
        /// Adds or replaces an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <returns>Nothing.</returns>
        Task AddOrReplaceAsync(TKey key, TValue value);

        /// <summary>
        /// Adds or replaces an entry with a time-to-live and returns the previous value.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>The value previously associated with the key in the map, if any; otherwise default(<typeparamref name="TValue"/>).</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// </remarks>
        Task<TValue> AddOrReplaceWithValueAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Adds or replaces an entry with a time-to-live.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>Nothing.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// </remarks>
        Task AddOrReplaceAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Adds or replaces entries.
        /// </summary>
        /// <param name="entries">Entries.</param>
        /// <returns>Nothing.</returns>
        /// TODO: is this transactional?
        Task AddOrReplaceAsync(IDictionary<TKey, TValue> entries);

        /// <summary>
        /// Replaces an existing entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>true if the entry was replaced; otherwise false.</returns>
        Task<TValue> ReplaceAsync(TKey key, TValue newValue);

        /// <summary>
        /// Replaces an existing entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>true if the entry was replaced; otherwise false.</returns>
        Task<bool> ReplaceAsync(TKey key, TValue expectedValue, TValue newValue);

        /// <summary>
        /// Tries to set an entry within a timeout.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>true if the entry was set; otherwise false.</returns>
        /// <remarks>
        /// <para>This method returns false when no lock on the key could be
        /// acquired within the timeout.</para>
        /// </remarks>
        Task<bool> TryAddOrReplaceAsync(TKey key, TValue value, TimeSpan timeout);

        /// <summary>
        /// Adds an entry, if no entry with the key exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The existing value, if any; otherwise the default value.</returns>
        Task<TValue> AddIfMissingAsync(TKey key, TValue value);

        /// <summary>
        /// Adds an entry with a time-to-live, if no entry with the key exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>The existing value, if any; otherwise the default value.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// </remarks>
        Task<TValue> AddIfMissingAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Adds an entry, or its <see cref="MapStore"/>.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <remarks>
        /// <para>If the map has a <see cref="MapStore"/> attached, the entry is added to the store
        /// but not persisted. Flushing the store (see <see cref="Flush"/>) is required to make sure
        /// that the entry is actually persisted to the map.</para>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// TODO: is it really removed? or just evicted?
        /// </remarks>
        Task AddTransientAsync(TKey key, TValue value, TimeSpan timeToLive);
    }
}