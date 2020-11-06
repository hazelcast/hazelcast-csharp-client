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
    public partial interface IHMap<TKey, TValue> // Setting
    {
        /// <summary>
        /// Sets (adds or updates) an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <returns>A task that will complete when the entry has been added or updated.</returns>
        Task SetAsync(TKey key, TValue value);

        /// <summary>
        /// Sets (adds or updates) an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>A task that will complete when the entry has been added or updated.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed.</para>
        /// <para>
        /// If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives as much as
        /// the default value configured on server map configuration.</para>
        /// <para>
        /// If the <paramref name="timeToLive"/> is <see cref="TimeSpan.MaxValue"/>, the entry lives forever.
        /// </para>
        /// </remarks>
        Task SetAsync(TKey key, TValue value, TimeSpan timeToLive);

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
        /// <param name="newValue">The new value.</param>
        /// <param name="comparisonValue">The value that is compared with the value of the entry.</param>
        /// <returns><c>true</c> if the entry was updated; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>If an existing entry with the specified key and expected value is found, then its
        /// value is updated with the new value. Otherwise, nothing happens.</para>
        /// </remarks>
        Task<bool> TryUpdateAsync(TKey key, TValue newValue, TValue comparisonValue);

        /// <summary>
        /// Tries to set (add or update) an entry within a server-side timeout.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="serverTimeout">A timeout.</param>
        /// <returns><c>true</c> if the entry was set; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>This method returns <c>false</c> when no lock on the key could be
        /// acquired within the specified server-side timeout.</para>
        /// </remarks>
        Task<bool> TrySetAsync(TKey key, TValue value, TimeSpan serverTimeout);

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
        /// <para>
        /// If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives as much as
        /// the default value configured on server map configuration.</para>
        /// <para>
        /// If the <paramref name="timeToLive"/> is <see cref="TimeSpan.MaxValue"/>, the entry lives forever.
        /// </para>
        /// </remarks>
        Task<TValue> GetOrAddAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Sets (adds or updates) an entry without calling the <c>MapStore</c> on the server side, if defined.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <remarks>
        /// <para>If the dictionary has a <c>MapStore</c> attached, the entry is added to the store but not persisted.
        /// Flushing the store is required to make sure that the entry is actually persisted.</para>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed.</para>
        /// <para>
        /// Time resolution for <param name="timeToLive"></param> is seconds. The given value is rounded to the next closest second value.
        /// </para>
        /// <para>
        /// If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives as much as
        /// the default value configured on server map configuration.</para>
        /// <para>
        /// If the <paramref name="timeToLive"/> is <see cref="TimeSpan.MaxValue"/>, the entry lives forever.
        /// </para>
        /// </remarks>
        Task SetTransientAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Updates the time to live value of the entry specified by <paramref name="key"/> with a new value.
        /// </summary>
        /// <remarks>
        /// <para>
        /// New TTL value is valid starting from the time this operation is invoked, not since the time the entry was created.
        /// </para>
        /// <para>
        /// If the entry does not exist or is already expired, this call has no effect.
        /// </para>
        /// <para>
        /// If there is no entry with key <paramref name="key"/> or is already expired,
        /// this call makes no changes to entries stored in this dictionary.
        /// </para>
        /// <para>
        /// Time resolution for <param name="timeToLive"></param> is seconds. The given value is rounded to the next closest second value.
        /// </para>
        /// <para>
        /// If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives as much as
        /// the default value configured on server map configuration.</para>
        /// <para>
        /// If the <paramref name="timeToLive"/> is <see cref="TimeSpan.MaxValue"/>, the entry lives forever.
        /// </para>
        /// </remarks>
        /// <param name="key">A key.</param>
        /// <param name="timeToLive">maximum time for this entry to stay in the dictionary.</param>
        /// <returns><c>true</c> if the entry exists and its ttl value is changed, <c>false</c> otherwise</returns>
        Task<bool> UpdateTimeToLive(TKey key, TimeSpan timeToLive);
    }
}
