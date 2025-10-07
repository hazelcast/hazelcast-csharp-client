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
        /// <remarks>
        /// <para>The value has an infinite time-to-live.</para>
        /// <para>The value becomes idle after the server-configured idle time.</para>
        /// </remarks>
        Task SetAsync(TKey key, TValue value);

        /// <summary>
        /// Sets (adds or updates) an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live (0ms to live forever; -1ms to use the server-configured value).</param>
        /// <returns>A task that will complete when the entry has been added or updated.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the
        /// <paramref name="timeToLive"/> has elapsed. If <paramref name="timeToLive"/> is0ms, the value is retained
        /// indefinitely. If it is -1ms, it lives for the duration of the server-configured time-to-live.</para>
        /// <para>The value becomes idle after the server-configured idle time.</para>
        /// </remarks>
        Task SetAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Sets (adds or updates) an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live (0ms to live forever; -1ms to use the server-configured value).</param>
        /// <param name="maxIdle">A max-idle time (0ms to never become idle).</param>
        /// <returns>A task that will complete when the entry has been added or updated.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the
        /// <paramref name="timeToLive"/> has elapsed. If <paramref name="timeToLive"/> is 0ms, the value
        /// is retained indefinitely. If it is -1ms, it lives for the duration of the server-configured time-to-live.</para>
        /// <para>The value is considered idle after the <paramref name="maxIdle"/> has elapsed. If it is
        /// 0ms, the value never becomes idle.</para>
        /// </remarks>
        Task SetAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan maxIdle);

        /// <summary>
        /// Sets (adds or updates) an entry, and returns the previous value, if any.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">A time to live (0ms to live forever; -1ms to use the server-configured value).</param>
        /// <param name="maxIdle">A max-idle time (0ms to never become idle).</param>
        /// <returns>The previous value for the specified key, if any; otherwise <c>default(TValue)</c>.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the
        /// <paramref name="timeToLive"/> has elapsed. If <paramref name="timeToLive"/> is 0ms, the value is
        /// retained indefinitely. If it is -1ms, it lives for the duration of the server-configured time-to-live.</para>
        /// <para>The value is considered idle after the <paramref name="maxIdle"/> has elapsed. If it is
        /// 0ms, the value never becomes idle.</para>
        /// </remarks>
        // TODO: document MapStore behavior
        Task<TValue> PutAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan maxIdle);

        /// <summary>
        /// Updates an entry if it exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>The existing value, if any; otherwise <c>default(TValue)</c>.</returns>
        /// <remarks>
        /// <para>If an existing entry with the specified key is found, then its value is
        /// updated with the new value, and the existing value is returned. Otherwise, nothing
        /// happens.</para>
        /// <para>This methods <strong>interacts with the server-side <c>MapStore</c></strong>.
        /// If no value for the specified key is found in memory, <c>MapLoader.load(...)</c> is invoked
        /// to try to load the value from the <c>MapStore</c> backing the map, if any.
        /// If write-through persistence is configured, before the value is stored in memory,
        /// <c>MapStore.store</c> is invoked to write the value to the store.</para>
        /// </remarks>
        Task<TValue> ReplaceAsync(TKey key, TValue newValue);

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
        /// <para>This methods <strong>interacts with the server-side <c>MapStore</c></strong>.
        /// If no value for the specified key is found in memory, <c>MapLoader.load(...)</c> is invoked
        /// to try to load the value from the <c>MapStore</c> backing the map, if any.
        /// If write-through persistence is configured, before the value is stored in memory,
        /// <c>MapStore.store</c> is invoked to write the value to the store.</para>
        /// </remarks>
        Task<bool> ReplaceAsync(TKey key, TValue newValue, TValue comparisonValue);

        /// <summary>
        /// Tries to set (add or update) an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToWait">How long to wait (-1ms to wait forever; 0ms to not wait at all).</param>
        /// <returns><c>true</c> if the entry was set; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>If the entry is not immediately available, because a lock is set on the key, this will wait
        /// for the specified <paramref name="timeToWait"/> for the lock. If the lock cannot be acquired in time,
        /// returns <c>null</c>. If <paramref name="timeToWait"/> is -1ms, waits forever. If it is 0ms, does
        /// not wait at all.</para>
        /// </remarks>
        Task<bool> TryPutAsync(TKey key, TValue value, TimeSpan timeToWait);

        /// <summary>
        /// Adds an entry if no entry with the key already exists.
        /// Returns the new value, or the existing value if the entry already exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The value for the key. This will be either the existing value for the key if the entry
        /// already exists, or the new value if the no entry with the key already existed.</returns>
        /// <remarks>
        /// <para>The value never expires.</para>
        /// <para>The value becomes idle after the server-configured idle time.</para>
        /// <para>This methods <strong>interacts with the server-side <c>MapStore</c></strong>.
        /// If no value for the specified key is found in memory, <c>MapLoader.load(...)</c> is invoked
        /// to try to load the value from the <c>MapStore</c> backing the map, if any.
        /// If write-through persistence is configured, before the value is stored in memory,
        /// <c>MapStore.store</c> is invoked to write the value to the store.</para>
        /// </remarks>
        Task<TValue> PutIfAbsentAsync(TKey key, TValue value);

        /// <summary>
        /// Adds an entry with a time-to-live if no entry with the key already exists.
        /// Returns the new value, or the existing value if the entry already exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live (0ms to live forever; -1ms to use the server-configured value).</param>
        /// <returns>The value for the key. This will be either the existing value for the key if the entry
        /// already exists, or the new value if the no entry with the key already existed.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the
        /// <paramref name="timeToLive"/> has elapsed. If <paramref name="timeToLive"/> is 0ms, the value
        /// is retained indefinitely. If it is -1ms, it lives for the duration of the server-configured time-to-live.</para>
        /// <para>The value becomes idle after the server-configured idle time.</para>
        /// <para>This methods <strong>interacts with the server-side <c>MapStore</c></strong>.
        /// If no value for the specified key is found in memory, <c>MapLoader.load(...)</c> is invoked
        /// to try to load the value from the <c>MapStore</c> backing the map, if any.
        /// If write-through persistence is configured, before the value is stored in memory,
        /// <c>MapStore.store</c> is invoked to write the value to the store.</para>
        /// </remarks>
        Task<TValue> PutIfAbsentAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Adds an entry with a time-to-live and a max-idle if no entry with the key already exists.
        /// Returns the new value, or the existing value if the entry already exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live (0ms to live forever; -1ms to use the server-configured value).</param>
        /// <param name="maxIdle">A max-idle time (0ms to never become idle).</param>
        /// <returns>The value for the key. This will be either the existing value for the key if the entry
        /// already exists, or the new value if the no entry with the key already existed.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the
        /// <paramref name="timeToLive"/> has elapsed. If <paramref name="timeToLive"/> is 0ms, the value
        /// is retained indefinitely. If it is -1ms, it lives for the duration of the server-configured time-to-live.</para>
        /// <para>The value is considered idle after the <paramref name="maxIdle"/> has elapsed. If it is
        /// 0ms, the value never becomes idle.</para>
        /// <para>This methods <strong>interacts with the server-side <c>MapStore</c></strong>.
        /// If no value for the specified key is found in memory, <c>MapLoader.load(...)</c> is invoked
        /// to try to load the value from the <c>MapStore</c> backing the map, if any.
        /// If write-through persistence is configured, before the value is stored in memory,
        /// <c>MapStore.store</c> is invoked to write the value to the store.</para>
        /// </remarks>
        Task<TValue> PutIfAbsentAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan maxIdle);

        /// <summary>
        /// Sets (adds or updates) an entry without calling the <c>MapStore</c> on the server side, if defined.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live (0ms to live forever; -1ms to use the server-configured value).</param>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the
        /// <paramref name="timeToLive"/> has elapsed. If <paramref name="timeToLive"/> is 0ms, the value
        /// is retained indefinitely. If it is -1ms, it lives for the duration of the server-configured time-to-live.</para>
        /// <para>The value becomes idle after the server-configured idle time.</para>
        /// <para>If the dictionary has a <c>MapStore</c> attached, the entry is added to the store but not persisted.
        /// Flushing the store is required to make sure that the entry is actually persisted.</para>
        /// <para>
        /// Time resolution for <paramref name="timeToLive"/> is seconds. The given value is rounded to the next closest second value.
        /// </para>
        /// </remarks>
        Task PutTransientAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Sets (adds or updates) an entry without calling the <c>MapStore</c> on the server side, if defined.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live (0ms to live forever; -1ms to use the server-configured value).</param>
        /// <param name="maxIdle">A max-idle time (0ms to never become idle).</param>
        /// <returns>The value for the key. This will be either the existing value for the key if the entry
        /// already exists, or the new value if the no entry with the key already existed.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the
        /// <paramref name="timeToLive"/> has elapsed. If <paramref name="timeToLive"/> is <paramref name="TimeToLive.Infinite"/>
        /// i.e. 0ms, the value is retained indefinitely. If it is -1ms,
        /// it lives for the duration of the server-configured time-to-live.</para>
        /// <para>The value is considered idle after the <paramref name="maxIdle"/> has elapsed. If it is
        /// 0ms, the value never becomes idle.</para>
        /// <para>If the dictionary has a <c>MapStore</c> attached, the entry is added to the store but not persisted.
        /// Flushing the store is required to make sure that the entry is actually persisted.</para>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed.</para>
        /// <para>
        /// Time resolution for <paramref name="timeToLive"/> is seconds. The given value is rounded to the next closest second value.
        /// </para>
        /// </remarks>
        Task PutTransientAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan maxIdle);

        /// <summary>
        /// Updates the time-to-live of an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="timeToLive">A time to live (0ms to live
        /// forever; -1ms to use the server-configured value).</param>
        /// <returns><c>true</c> if the entry exists and its time-to-live value is changed; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the
        /// <paramref name="timeToLive"/> has elapsed. If <paramref name="timeToLive"/> is <see cref="TimeToLive.Infinite"/>
        /// i.e. 0ms, the value is retained indefinitely. If it is -1ms,
        /// it lives for the duration of the server-configured time-to-live.</para>
        /// <para>The new time-to-live value is valid starting from the time this operation is invoked,
        /// not since the time the entry was created.</para>
        /// <para>
        /// If the entry does not exist or is already expired, this call has no effect.
        /// </para>
        /// <para>
        /// If there is no entry with key <paramref name="key"/> or is already expired,
        /// this call makes no changes to entries stored in this dictionary.
        /// </para>
        /// <para>
        /// Time resolution for <paramref name="timeToLive"/> is seconds. The given value is rounded to the next closest second value.
        /// </para>
        /// </remarks>
        Task<bool> UpdateTimeToLive(TKey key, TimeSpan timeToLive);
    }
}
