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
using Hazelcast.Core;

namespace Hazelcast.DistributedObjects
{
    public partial interface IHDictionary<TKey, TValue> // Setting
    {
        /// <summary>
        /// Adds or updates an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="returnValue">Whether to return the updated value, if any.</param>
        /// <returns>The updated value, if any.</returns>
        /// <remarks>
        /// <para>For performance reasons, <paramref name="returnValue"/> is <c>false</c> by
        /// default and the method returns <c>default(TValue)</c>. Set <paramref name="returnValue"/>
        /// to true if you are interested in the updated value.</para>
        /// </remarks>
        Task<TValue> AddOrUpdateAsync(TKey key, TValue value, bool returnValue = false);

        /// <summary>
        /// Adds or updates an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <param name="returnValue">Whether to return the updated value, if any.</param>
        /// <returns>The updated value, if any.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed.</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// <para>For performance reasons, <paramref name="returnValue"/> is <c>false</c> by
        /// default and the method returns <c>default(TValue)</c>. Set <paramref name="returnValue"/>
        /// to true if you are interested in the updated value.</para>
        /// </remarks>
        Task<TValue> AddOrUpdateAsync(TKey key, TValue value, TimeSpan timeToLive, bool returnValue = false);

        /// <summary>
        /// Adds or updates entries.
        /// </summary>
        /// <param name="entries">Entries.</param>
        /// <returns>A task that will complete when the entries have been added or updated.</returns>
        Task AddOrUpdateAsync(IDictionary<TKey, TValue> entries);

        /// <summary>
        /// Updates an entry if it exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>The updated value, or <c>default(TValue)</c> is no entry is updated.</returns>
        /// <remarks>
        /// <para>If an existing entry with the specified key is found, then its value is
        /// updated with the new value, and the updated value is returned. Otherwise, nothing
        /// happens, and <c>default(TValue)</c> is returned.</para>
        /// </remarks>
        Task<TValue> TryUpdateAsync(TKey key, TValue newValue);

        /// <summary>
        /// Updates an entry if it exists, and its value is equal to <paramref name="comparisonValue"/>.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="comparisonValue">The value that is compared with the value of the entry.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns><c>true</c> if the entry was updated; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>If an existing entry with the specified key and expected value is found, then its
        /// value is updated with the new value. Otherwise, nothing happens.</para>
        /// </remarks>
        Task<bool> TryUpdateAsync(TKey key, TValue comparisonValue, TValue newValue);

        /// <summary>
        /// Tries to add or update an entry within a server-side timeout.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="serverTimeout">A timeout.</param>
        /// <returns><c>true</c> if the entry was set; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>This method returns <c>false</c> when no lock on the key could be
        /// acquired within the specified server-side timeout.</para>
        /// </remarks>
        Task<bool> TryAddOrUpdateAsync(TKey key, TValue value, TimeSpan serverTimeout);

        /// <summary>
        /// Adds an entry if no entry with the key already exists.
        /// Returns the new value, or the existing value if the entry already exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The value for the key. This will be either the existing value for the key if the entry
        /// already exists, or the new value if the no entry with the key already existed.</returns>
        Task<TValue> GetOrAddAsync(TKey key, TValue value);

        /// <summary>
        /// Adds an entry with a time-to-live if no entry with the key already exists.
        /// Returns the new value, or the existing value if the entry already exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>The value for the key. This will be either the existing value for the key if the entry
        /// already exists, or the new value if the no entry with the key already existed.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed.</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// </remarks>
        Task<TValue> GetOrAddAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Sets (adds or updates) a transient entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <remarks>
        /// <para>If the map has a MapStore attached, the entry is added to the store but not persisted.
        /// Flushing the store is required to make sure that the entry is actually persisted.</para>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed.</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// TODO: is it really removed? or just evicted?
        /// </remarks>
        Task AddOrUpdateTransientAsync(TKey key, TValue value, TimeSpan timeToLive);
    }
}
