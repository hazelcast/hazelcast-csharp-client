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
    public partial interface IHMap<TKey, TValue> // Removing
    {
        /// <summary>
        /// Tries to remove an entry from this map.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="timeToWait">The time to wait for a lock on the key.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>true if the entry was removed; otherwise false.</returns>
        /// <remarks>
        /// <para>This method returns false when no lock on the key could be
        /// acquired within the timeout.</para>
        /// TODO or when there was no value with that key?
        /// </remarks>
        Task<bool> TryRemoveAsync(TKey key, TimeSpan timeToWait, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an entry from this map and returns the removed value, if any.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The value, if any, or <c>default(TValue)</c>.</returns>
        /// <remarks>
        /// <para>This method serializes the return value. For performance reasons, prefer
        /// <see cref="RemoveAsync(TKey,System.Threading.CancellationToken)"/> when the returned value is not used.</para>
        /// </remarks>
        Task<TValue> RemoveAndReturnAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an entry from this map.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The value, if any, or default(TValue).</returns>
        /// <remarks>
        /// <para>This method removes an entry if the key and the value both match the
        /// specified key and value.</para>
        /// </remarks>
        Task<bool> RemoveAsync(TKey key, TValue value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an entry from this map.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// <para>For performance reasons, this method does not return the value. Prefer
        /// <see cref="RemoveAndReturnAsync(TKey,System.Threading.CancellationToken)"/> if the value is required.</para>
        /// </remarks>
        Task RemoveAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes all entries from this map.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}
