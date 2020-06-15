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
    public partial interface IHMap<TKey, TValue> // Locking
    {
        /// <summary>
        /// Locks an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="timeout">A timeout.</param>
        /// <remarks>
        /// TODO: document this properly (also: not distributed lock?)
        /// <para>If the lock is not available, then "the current thread becomes disabled for thread scheduling
        /// purposes and lies dormant until the lock has been acquired." The scope of the lock is this map only,
        /// and the lock is only for the specified key in this map.</para>
        /// <para>Locks are re-entrant, but counted: if a key is locked N times, then it should be unlocked
        /// N times before another thread can lock it.</para>
        /// </remarks>
        Task LockAsync(TKey key, TimeSpan timeout = default);

        /// <summary>
        /// Locks an entry.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// TODO: document this properly (also: not distributed lock?)
        /// <para>If the lock is not available, then "the current thread becomes disabled for thread scheduling
        /// purposes and lies dormant until the lock has been acquired." The scope of the lock is this map only,
        /// and the lock is only for the specified key in this map.</para>
        /// <para>Locks are re-entrant, but counted: if a key is locked N times, then it should be unlocked
        /// N times before another thread can lock it.</para>
        /// </remarks>
        Task LockAsync(TKey key, CancellationToken cancellationToken);

        /// <summary>
        /// Locks an entry for a specified lease time, 
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="leaseTime">A time span.</param>
        /// <param name="timeout">A timeout.</param>
        /// <remarks>
        /// TODO: document this properly (also: not distributed lock?)
        /// <para>If the lock is not available, then "the current thread becomes disabled for thread scheduling
        /// purposes and lies dormant until the lock has been acquired." The scope of the lock is this map only,
        /// and the lock is only for the specified key in this map.</para>
        /// <para>Locks are re-entrant, but counted: if a key is locked N times, then it should be unlocked
        /// N times before another thread can lock it.</para>
        /// <para>The lock is released after the time span.</para>
        /// </remarks>
        Task LockForAsync(TKey key, TimeSpan leaseTime, TimeSpan timeout = default);

        /// <summary>
        /// Locks an entry for a specified lease time, 
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="leaseTime">A time span.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// TODO: document this properly (also: not distributed lock?)
        /// <para>If the lock is not available, then "the current thread becomes disabled for thread scheduling
        /// purposes and lies dormant until the lock has been acquired." The scope of the lock is this map only,
        /// and the lock is only for the specified key in this map.</para>
        /// <para>Locks are re-entrant, but counted: if a key is locked N times, then it should be unlocked
        /// N times before another thread can lock it.</para>
        /// <para>The lock is released after the time span.</para>
        /// </remarks>
        Task LockForAsync(TKey key, TimeSpan leaseTime, CancellationToken cancellationToken);

        /// <summary>
        /// Tries to lock an entry immediately.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>true if the lock was acquired; otherwise false.</returns>
        /// <remarks>
        /// <para>If the entry cannot be locked, returns false immediately.</para>
        /// </remarks>
        Task<bool> TryLockAsync(TKey key, TimeSpan timeout = default);

        /// <summary>
        /// Tries to lock an entry immediately.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>true if the lock was acquired; otherwise false.</returns>
        /// <remarks>
        /// <para>If the entry cannot be locked, returns false immediately.</para>
        /// </remarks>
        Task<bool> TryLockAsync(TKey key, CancellationToken cancellationToken);

        /// <summary>
        /// Tries to lock an entry with a server-side timeout.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="timeToWait">How long to wait for the lock.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>true if the lock was acquired; otherwise false.</returns>
        /// <remarks>
        /// <para>If the entry cannot be locked after <paramref name="timeToWait"/>, returns false.</para>
        /// <para>If <paramref name="timeToWait"/> is <see cref="Timeout.InfiniteTimeSpan"/>, waits forever.</para>
        /// </remarks>
        Task<bool> WaitLockAsync(TKey key, TimeSpan timeToWait, TimeSpan timeout = default);

        /// <summary>
        /// Tries to lock an entry with a server-side timeout.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="timeToWait">How long to wait for the lock.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>true if the lock was acquired; otherwise false.</returns>
        /// <remarks>
        /// <para>If the entry cannot be locked after <paramref name="timeToWait"/>, returns false.</para>
        /// <para>If <paramref name="timeToWait"/> is <see cref="Timeout.InfiniteTimeSpan"/>, waits forever.</para>
        /// </remarks>
        Task<bool> WaitLockAsync(TKey key, TimeSpan timeToWait, CancellationToken cancellationToken);

        /// <summary>
        /// Tries to lock an entry for a specified lease time, with a server-side timeout.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="timeToWait">How long to wait for the lock.</param>
        /// <param name="leaseTime">A lease time.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>true if the lock was acquired; otherwise false.</returns>
        /// <remarks>
        /// <para>If the entry cannot be locked after <paramref name="timeToWait"/>, returns false.
        /// If <paramref name="timeToWait"/> is <see cref="Timeout.InfiniteTimeSpan"/>, waits forever.</para>
        /// <para>If acquired, the lock is automatically released after <paramref cref="leaseTime"/>.
        /// If <paramref name="leaseTime"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the lock is never
        /// released.</para>
        /// </remarks>
        Task<bool> WaitLockForAsync(TKey key, TimeSpan timeToWait, TimeSpan leaseTime, TimeSpan timeout = default);

        /// <summary>
        /// Tries to lock an entry for a specified lease time, with a server-side timeout.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="timeToWait">How long to wait for the lock.</param>
        /// <param name="leaseTime">A lease time.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>true if the lock was acquired; otherwise false.</returns>
        /// <remarks>
        /// <para>If the entry cannot be locked after <paramref name="timeToWait"/>, returns false.
        /// If <paramref name="timeToWait"/> is <see cref="Timeout.InfiniteTimeSpan"/>, waits forever.</para>
        /// <para>If acquired, the lock is automatically released after <paramref cref="leaseTime"/>.
        /// If <paramref name="leaseTime"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the lock is never
        /// released.</para>
        /// </remarks>
        Task<bool> WaitLockForAsync(TKey key, TimeSpan timeToWait, TimeSpan leaseTime, CancellationToken cancellationToken);

        /// <summary>
        /// Determines whether an entry is locked.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>true if the entry is locked; otherwise false.</returns>
        Task<bool> IsLockedAsync(TKey key, TimeSpan timeout = default);

        /// <summary>
        /// Determines whether an entry is locked.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>true if the entry is locked; otherwise false.</returns>
        Task<bool> IsLockedAsync(TKey key, CancellationToken cancellationToken);

        /// <summary>
        /// Unlocks an entry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>A task that will complete when the entry has been unlocked.</returns>
        Task UnlockAsync(TKey key, TimeSpan timeout = default);

        /// <summary>
        /// Unlocks an entry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the entry has been unlocked.</returns>
        Task UnlockAsync(TKey key, CancellationToken cancellationToken);

        /// <summary>
        /// Unlocks an entry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="timeout">A timeout.</param>
        /// <remarks>
        /// <para>Unlocks the entry identified by the key, regardless of the lock owner.</para>
        /// <para>This always succeed, never blocks, and returns immediately.</para>
        /// TODO: but, async?
        /// </remarks>
        Task ForceUnlockAsync(TKey key, TimeSpan timeout = default);

        /// <summary>
        /// Unlocks an entry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// <para>Unlocks the entry identified by the key, regardless of the lock owner.</para>
        /// <para>This always succeed, never blocks, and returns immediately.</para>
        /// TODO: but, async?
        /// </remarks>
        Task ForceUnlockAsync(TKey key, CancellationToken cancellationToken);
    }
}
