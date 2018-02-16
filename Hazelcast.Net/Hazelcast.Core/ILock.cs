// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Core
{
    /// <summary>
    ///     Distributed implementation of Lock
    /// </summary>
    public interface ILock : IDistributedObject
    {
        /// <summary>Releases the lock regardless of the lock owner.</summary>
        /// <remarks>
        ///     Releases the lock regardless of the lock owner.
        ///     It always successfully unlocks, never blocks  and returns immediately.
        /// </remarks>
        void ForceUnlock();

        /// <summary>Returns re-entrant lock hold count, regardless of lock ownership.</summary>
        /// <remarks>Returns re-entrant lock hold count, regardless of lock ownership.</remarks>
        /// <returns>lock hold count.</returns>
        int GetLockCount();

        /// <summary>Returns remaining lease time in milliseconds.</summary>
        /// <remarks>
        ///     Returns remaining lease time in milliseconds.
        ///     If the lock is not locked then -1 will be returned.
        /// </remarks>
        /// <returns>remaining lease time in milliseconds.</returns>
        long GetRemainingLeaseTime();

        /// <summary>Returns whether this lock is locked or not.</summary>
        /// <remarks>Returns whether this lock is locked or not.</remarks>
        /// <returns>
        ///     <code>true</code>
        ///     if this lock is locked,
        ///     <code>false</code>
        ///     otherwise.
        /// </returns>
        bool IsLocked();

        /// <summary>Returns whether this lock is locked by current thread or not.</summary>
        /// <remarks>Returns whether this lock is locked by current thread or not.</remarks>
        /// <returns>
        ///     <code>true</code>
        ///     if this lock is locked by current thread,
        ///     <code>false</code>
        ///     otherwise.
        /// </returns>
        bool IsLockedByCurrentThread();

        /// <summary>
        ///Acquires the lock.
        /// </summary>
        /// <remarks>
        ///     Acquires the lock.
        ///     <p>
        ///        If the lock is not available then
        ///        the current thread becomes disabled for thread scheduling
        ///        purposes and lies dormant until the lock has been acquired.
        ///     </p>
        /// </remarks>
        void Lock();

        /// <summary>Acquires the lock for the specified lease time.</summary>
        /// <remarks>
        ///     Acquires the lock for the specified lease time.
        ///     <p>After lease time, lock will be released.</p>
        ///     <p>
        ///        If the lock is not available then
        ///        the current thread becomes disabled for thread scheduling
        ///        purposes and lies dormant until the lock has been acquired.
        ///     </p>
        /// </remarks>
        /// <param name="leaseTime">time to wait before releasing the lock.</param>
        /// <param name="timeUnit">unit of time to specify lease time.</param>
        void Lock(long leaseTime, TimeUnit? timeUnit);

        /// <summary>
        /// Tries to acquires the lock and returns immediately.
        /// </summary>
        /// <returns><c>true</c> if acquires the lock, <c>false</c> otherwise.</returns>
        bool TryLock();

        /// <summary>Tries to acquires the lock for the specified lease time.</summary>
        /// <remarks>
        ///     Tries to acquires the lock for the specified lease time.
        ///     <p>After lease time, lock will be released.</p>
        /// </remarks>
        /// <param name="time">time to wait before releasing the lock.</param>
        /// <param name="unit">unit of time to specify lease time.</param>
        bool TryLock(long time, TimeUnit? unit);

        /// <summary>Releases the lock.</summary>
        /// <remarks>Releases the lock.</remarks>
        void Unlock();
    }
}