// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
    public interface IKeyLockable<TKey>
    {
        /// <summary>
        /// Locks an entry.
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <returns>A task that will complete when the lock has been acquired.</returns>
        /// <remarks>
        /// <para>If the lock is already owned by another owner, this will wait indefinitely until the
        /// lock can be acquired.</para>
        /// <para>Locks are re-entrant, but counted: if a key is locked N times, then it should be unlocked
        /// N times before another thread can lock it.</para>
        /// <para>The lock is automatically released after the server-configured maximum lease time has elapsed.</para>
        /// </remarks>
        Task LockAsync(TKey key);

        /// <summary>
        /// Locks an entry for a given time duration (lease time),
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <param name="leaseTime">The lease time.</param>
        /// <returns>A task that will complete when the lock has been acquired.</returns>
        /// <remarks>
        /// <para>If the lock is already owned by another owner, this will wait indefinitely until the
        /// lock can be acquired.</para>
        /// <para>Locks are re-entrant, but counted: if an entry is locked N times, then it should be unlocked
        /// N times before another owner can lock it.</para>
        /// <para>The lock is automatically released after the specified <paramref name="leaseTime"/> has elapsed.</para>
        /// </remarks>
        Task LockAsync(TKey key, TimeSpan leaseTime);

        /// <summary>
        /// Tries to lock an entry immediately.
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <returns><c>true</c> if the lock was immediately acquired; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>If the entry cannot be locked, returns <c>false</c> immediately.</para>
        /// <para>Locks are re-entrant, but counted: if an entry is locked N times, then it should be unlocked
        /// N times before another owner can lock it.</para>
        /// <para>The lock is automatically released after the server-configured maximum lease time has elapsed.</para>
        /// </remarks>
        Task<bool> TryLockAsync(TKey key);

        /// <summary>
        /// Tries to lock an entry within a server-side timeout.
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <param name="timeToWait">How long to wait for the lock (-1ms to wait forever; 0ms to not wait at all).</param>
        /// <returns><c>true</c> if the lock was acquired; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>If the entry cannot be locked after <paramref name="timeToWait"/> has elapsed, returns <c>false</c>.
        /// If <paramref name="timeToWait"/> is -1ms, waits forever. If it is 0ms, does not wait at all.</para>
        /// <para>Locks are re-entrant, but counted: if an entry is locked N times, then it should be unlocked
        /// N times before another owner can lock it.</para>
        /// <para>The lock is automatically released after the server-configured maximum lease time has elapsed.</para>
        /// </remarks>
        Task<bool> TryLockAsync(TKey key, TimeSpan timeToWait);

        /// <summary>
        /// Tries to lock an entry for a given time duration (lease time), within a server-side timeout.
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <param name="timeToWait">How long to wait for the lock (-1ms to wait forever; 0ms to not wait at all).</param>
        /// <param name="leaseTime">The lease time.</param>
        /// <returns><c>true</c> if the lock was acquired; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>If the entry cannot be locked after <paramref name="timeToWait"/> has elapsed, returns <c>false</c>.
        /// If <paramref name="timeToWait"/> is -1ms, waits forever. If it is 0ms, does not wait at all.</para>
        /// <para>The lock is automatically released after the specified <paramref name="leaseTime"/> has elapsed.</para>
        /// <para>Locks are re-entrant, but counted: if an entry is locked N times, then it should be unlocked
        /// N times before another owner can lock it.</para>
        /// </remarks>
        Task<bool> TryLockAsync(TKey key, TimeSpan timeToWait, TimeSpan leaseTime);

        /// <summary>
        /// Determines whether an entry is locked.
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <returns><c>true</c> if the entry is locked; otherwise <c>false</c>.</returns>
        Task<bool> IsLockedAsync(TKey key);

        /// <summary>
        /// Unlocks an entry.
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <returns>A task that will complete when the entry has been unlocked.</returns>
        /// <remarks>
        /// <para>An entry can be unlocked only by the owner of the lock.</para>
        /// <para>Locks are re-entrant, but counted: if an entry is locked N times, then it should be unlocked
        /// N times before another owner can lock it.</para>
        /// </remarks>
        Task UnlockAsync(TKey key);

        /// <summary>
        /// Force-unlocks an entry.
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <returns>A task that will complete when the entry has been unlocked.</returns>
        /// <remarks>
        /// <para>The entry is unlocked, regardless of the lock owner.</para>
        /// <para>This always succeeds, never blocks, and returns immediately.</para>
        /// </remarks>
        Task ForceUnlockAsync(TKey key);
    }
}
