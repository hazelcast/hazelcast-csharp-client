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
    public partial interface IHMap<TKey, TValue> // Setting
    {
        /// <summary>
        /// Adds or updates an entry and returns the previous value, if any.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The value previously associated with the key in the map, if any; otherwise default(<typeparamref name="TValue"/>).</returns>
        /// <remarks>
        /// <para>If the operation times out, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task<TValue> AddOrUpdateAndReturnAsync(TKey key, TValue value, TimeSpan timeout = default);

        /// <summary>
        /// Adds or updates an entry and returns the previous value, if any.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The value previously associated with the key in the map, if any; otherwise default(<typeparamref name="TValue"/>).</returns>
        /// <remarks>
        /// <para>If the operation is cancelled, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task<TValue> AddOrUpdateAndReturnAsync(TKey key, TValue value, CancellationToken cancellationToken);

        /// <summary>
        /// Adds or updates an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>Nothing.</returns>
        /// <remarks>
        /// <para>If the operation times out, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task AddOrUpdateAsync(TKey key, TValue value, TimeSpan timeout = default);

        /// <summary>
        /// Adds or updates an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Nothing.</returns>
        /// <remarks>
        /// <para>If the operation is cancelled, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task AddOrUpdateAsync(TKey key, TValue value, CancellationToken cancellationToken);

        /// <summary>
        /// Adds or updates an entry with a time-to-live and returns the previous value.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The value previously associated with the key in the map, if any; otherwise default(<typeparamref name="TValue"/>).</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed.</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// <para>If the operation times out, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task<TValue> AndOrUpdateAndReturnTtlAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan timeout = default);

        /// <summary>
        /// Adds or updates an entry with a time-to-live and returns the previous value.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The value previously associated with the key in the map, if any; otherwise default(<typeparamref name="TValue"/>).</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed.</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// <para>If the operation is cancelled, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task<TValue> AndOrUpdateAndReturnTtlAsync(TKey key, TValue value, TimeSpan timeToLive, CancellationToken cancellationToken);

        /// <summary>
        /// Adds or updates an entry with a time-to-live.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>Nothing.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed.</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// <para>If the operation times out, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task AddOrUpdateTtlAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan timeout = default);

        /// <summary>
        /// Adds or updates an entry with a time-to-live.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Nothing.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed.</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// <para>If the operation is cancelled, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task AddOrUpdateTtlAsync(TKey key, TValue value, TimeSpan timeToLive, CancellationToken cancellationToken);

        /// <summary>
        /// Adds or updates entries.
        /// </summary>
        /// <param name="entries">Entries.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>Nothing.</returns>
        /// <remarks>
        /// <para>If the operation times out, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        /// TODO: is this transactional?
        Task AddOrUpdateAsync(IDictionary<TKey, TValue> entries, TimeSpan timeout = default);

        /// <summary>
        /// Adds or updates entries.
        /// </summary>
        /// <param name="entries">Entries.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Nothing.</returns>
        /// <remarks>
        /// <para>If the operation is cancelled, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        /// TODO: is this transactional?
        Task AddOrUpdateAsync(IDictionary<TKey, TValue> entries, CancellationToken cancellationToken);

        /// <summary>
        /// Replaces an existing entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>true if the entry was replaced; otherwise false.</returns>
        /// <remarks>
        /// <para>If the operation times out, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task<TValue> ReplaceAndReturnAsync(TKey key, TValue newValue, TimeSpan timeout = default);

        /// <summary>
        /// Replaces an existing entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>true if the entry was replaced; otherwise false.</returns>
        /// <remarks>
        /// <para>If the operation is cancelled, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task<TValue> ReplaceAndReturnAsync(TKey key, TValue newValue, CancellationToken cancellationToken);

        /// <summary>
        /// Replaces an existing entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>true if the entry was replaced; otherwise false.</returns>
        /// <remarks>
        /// <para>If the operation times out, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task<bool> ReplaceAsync(TKey key, TValue expectedValue, TValue newValue, TimeSpan timeout = default);

        /// <summary>
        /// Replaces an existing entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="expectedValue">The expected value.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>true if the entry was replaced; otherwise false.</returns>
        /// <remarks>
        /// <para>If the operation is cancelled, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task<bool> ReplaceAsync(TKey key, TValue expectedValue, TValue newValue, CancellationToken cancellationToken);

        /// <summary>
        /// Tries to set an entry within a server-side timeout.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="serverTimeout">A timeout.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>true if the entry was set; otherwise false.</returns>
        /// <remarks>
        /// <para>This method returns false when no lock on the key could be
        /// acquired within the timeout.</para>
        /// <para>If the operation times out (client-side), there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task<bool> TryAddOrUpdateAsync(TKey key, TValue value, TimeSpan serverTimeout, TimeSpan timeout = default);

        /// <summary>
        /// Tries to set an entry within a server-side timeout.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <param name="serverTimeout">A timeout.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>true if the entry was set; otherwise false.</returns>
        /// <remarks>
        /// <para>This method returns false when no lock on the key could be
        /// acquired within the timeout.</para>
        /// <para>If the operation is cancelled, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task<bool> TryAddOrUpdateAsync(TKey key, TValue value, TimeSpan serverTimeout, CancellationToken cancellationToken);

        /// <summary>
        /// Adds an entry, if no entry with the key exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The existing value, if any; otherwise the default value.</returns>
        /// <remarks>
        /// <para>If the operation times out, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task<TValue> AddAsync(TKey key, TValue value, TimeSpan timeout = default);

        /// <summary>
        /// Adds an entry, if no entry with the key exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The existing value, if any; otherwise the default value.</returns>
        /// <remarks>
        /// <para>If the operation is cancelled, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task<TValue> AddAsync(TKey key, TValue value, CancellationToken cancellationToken);

        /// <summary>
        /// Adds an entry with a time-to-live, if no entry with the key exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The existing value, if any; otherwise the default value.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed.</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// <para>If the operation times out, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task<TValue> AddTtlAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan timeout = default);

        /// <summary>
        /// Adds an entry with a time-to-live, if no entry with the key exists.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The existing value, if any; otherwise the default value.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed.</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// <para>If the operation is cancelled, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// </remarks>
        Task<TValue> AddTtlAsync(TKey key, TValue value, TimeSpan timeToLive, CancellationToken cancellationToken);

        /// <summary>
        /// Adds a transient entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <param name="timeout">A timeout.</param>
        /// <remarks>
        /// <para>If the map has a MapStore attached, the entry is added to the store but not persisted.
        /// Flushing the store is required to make sure that the entry is actually persisted.</para>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed.</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// <para>If the operation times out, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// TODO: is it really removed? or just evicted?
        /// </remarks>
        Task AddOrUpdateTransientAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan timeout = default);

        /// <summary>
        /// Adds a transient entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">The value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// <para>If the map has a MapStore attached, the entry is added to the store but not persisted.
        /// Flushing the store is required to make sure that the entry is actually persisted.</para>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed.</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// <para>If the operation is cancelled, there is no guarantee on what was actually performed,
        /// and on whether value have been modified on the servers on not.</para>
        /// TODO: is it really removed? or just evicted?
        /// </remarks>
        Task AddOrUpdateTransientAsync(TKey key, TValue value, TimeSpan timeToLive, CancellationToken cancellationToken);
    }
}
