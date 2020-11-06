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
using System.Threading.Tasks;
using Hazelcast.Predicates;

namespace Hazelcast.DistributedObjects
{
    public partial interface IHMap<TKey, TValue> // Removing
    {
        /// <summary>
        /// Tries to remove the entry with the given key from this dictionary
        /// within the specified time to wait value.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="timeToWait">The time to wait for a lock on the key.</param>
        /// <returns><c>true</c> if the entry was removed; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>If the key is already locked by another thread and/or member, then this operation
        /// will wait for the <paramref name="timeToWait"/> for acquiring the lock. If the key
        /// is still locked, this operation returns <c>false</c>.</para>
        /// <para>The operation also returns <c>false</c> when no entry with the specified
        /// <paramref name="key"/> exists.</para>
        /// </remarks>
        Task<bool> TryRemoveAsync(TKey key, TimeSpan timeToWait);

        /// <summary>
        /// Removes an entry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if an entry with the specified key and value was removed;
        /// otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>This method removes an entry if the key and the value both match the
        /// specified key and value.</para>
        /// <para>This method does not consider the removed value at all, which breaks the events
        /// contract: any event that would be filtered on the value (for instance via a predicate),
        /// would not trigger here.</para>
        /// </remarks>
        Task<bool> RemoveAsync(TKey key, TValue value);

        /// <summary>
        /// Removes an entry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <remarks>
        /// <para>For performance reasons, this method does not return the removed value. Prefer
        /// <see cref="IHMapBase{TKey,TValue}.GetAndRemoveAsync"/> if the value is required.</para>
        /// <para>However, note that <see cref="IHMapBase{TKey,TValue}.GetAndRemoveAsync"/> may
        /// breaks the events contract: this method does not consider the removed value at all, which
        /// means that any event that would be filtered on the value (for instance via a predicate),
        /// and would trigger with <see cref="IHMapBase{TKey,TValue}.GetAndRemoveAsync"/>,
        /// will not trigger here.</para>
        /// </remarks>
        Task RemoveAsync(TKey key);

        /// <summary>
        /// Removes all entries which match with the supplied predicate.
        /// </summary>
        /// <param name="predicate">A predicate used to select entries to be removed.</param>
        /// <returns>A task that will complete when entries have been removed.</returns>
        /// <remarks>
        /// <para>This method does not consider the removed value at all, which breaks the events
        /// contract: any event that would be filtered on the value (for instance via a predicate),
        /// would not trigger here.</para>
        /// </remarks>
        Task RemoveAsync(IPredicate predicate);
    }
}
