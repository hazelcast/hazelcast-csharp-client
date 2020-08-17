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
    public partial interface IHDictionary<TKey, TValue> // Setting
    {
        /// <summary>
        /// Sets (adds or updates) an entry and gets the updated value, if any.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <returns>The value previously associated with the key in the map, if any; otherwise default(<typeparamref name="TValue"/>).</returns>
        /// <remarks>
        /// <para>If the operation is cancelled, there is no guarantee on what is actually performed,
        /// i.e. on whether the entry is set on the servers or not.</para>
        /// </remarks>
        Task<TValue> GetAndSetAsync(TKey key, TValue value);

        /// <summary>
        /// Sets (adds or updates) an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <returns>A that that will complete when the entry has been set.</returns>
        /// <remarks>
        /// <para>If the operation is cancelled, there is no guarantee on what is actually performed,
        /// i.e. on whether the entry is set on the servers or not.</para>
        /// </remarks>
        Task SetAsync(TKey key, TValue value);

        /// <summary>
        /// Sets (adds or updates) an entry with a time-to-live and gets the updated value, if any.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>The value previously associated with the key in the map, if any; otherwise default(<typeparamref name="TValue"/>).</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed.</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// <para>If the operation is cancelled, there is no guarantee on what is actually performed,
        /// i.e. on whether the entry is set on the servers or not.</para>
        /// </remarks>
        Task<TValue> GetAndSetAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Sets (adds or updates) an entry with a time-to-live.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>Nothing.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed.</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// <para>If the operation is cancelled, there is no guarantee on what is actually performed,
        /// i.e. on whether the entry is set on the servers or not.</para>
        /// </remarks>
        Task SetAsync(TKey key, TValue value, TimeSpan timeToLive);

        /// <summary>
        /// Sets (adds or updates) entries.
        /// </summary>
        /// <param name="entries">Entries.</param>
        /// <returns>A that that will complete when the entries have been set.</returns>
        /// <remarks>
        /// <para>If the operation is cancelled, there is no guarantee on what is actually performed,
        /// i.e. on whether entries have been set on the servers on not. On the other hand, either all
        /// entries have been set, or none. The operation cannot be cancelled while only some entries
        /// have been set.</para>
        /// </remarks>
        Task SetAsync(IDictionary<TKey, TValue> entries);

        /// <summary>
        /// Replaces an existing entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>The updated value, or <c>default(TValue)</c> is no entry is updated.</returns>
        /// <remarks>
        /// <para>If an existing entry with the specified key is found, then its value is
        /// updated with the new value, and the updated value is returned. Otherwise, nothing
        /// happens, and <c>default(TValue)</c> is returned.</para>
        /// <para>If the operation is cancelled, there is no guarantee on what is actually performed,
        /// i.e. on whether the entry is added on the servers or not.</para>
        /// </remarks>
        Task<TValue> ReplaceAsync(TKey key, TValue newValue);

        /// <summary>
        /// Replaces an existing entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns><c>true</c> if the entry was replaced; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>If an existing entry with the specified key and expected value is found, then its
        /// value is updated with the new value. Otherwise, nothing happens.</para>
        /// <para>If the operation is cancelled, there is no guarantee on what is actually performed,
        /// i.e. on whether the entry is added on the servers or not.</para>
        /// </remarks>
        Task<bool> ReplaceAsync(TKey key, TValue expectedValue, TValue newValue);

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
        /// <para>If the operation is cancelled, there is no guarantee on what is actually performed,
        /// i.e. on whether the entry is added on the servers or not.</para>
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
        /// <remarks>
        /// <para>If the operation is cancelled, there is no guarantee on what is actually performed,
        /// i.e. on whether the entry is added on the servers or not.</para>
        /// </remarks>
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
        /// <para>If the operation is cancelled, there is no guarantee on what is actually performed,
        /// i.e. on whether the entry is added on the servers or not.</para>
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
        /// <para>If the operation is cancelled, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// TODO: is it really removed? or just evicted?
        /// </remarks>
        Task SetTransientAsync(TKey key, TValue value, TimeSpan timeToLive);
    }
}
