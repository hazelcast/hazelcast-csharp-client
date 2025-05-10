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
using Hazelcast.Core;
using Hazelcast.Exceptions;

namespace Hazelcast.CP
{
    /// <summary>
    /// Represents a linearizable, distributed, reentrant implementation of the Java Lock.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="IFencedLock"/> is CP with respect to the CAP principle.
    /// It works on top of the Raft consensus algorithm. It offers linearizability during crash-stop
    /// failures and network partitions. If a network partition occurs, it remains  available on at
    /// most one side of the partition.</para>
    /// <para>A <see cref="IFencedLock"/> works within the context of a <see cref="LockContext"/>.</para>
    /// </remarks>
    public interface IFencedLock : ICPDistributedObject
    {
        /// <summary>
        /// Gets the identifier representing an invalid fence.
        /// </summary>
        long InvalidFence { get; }

        /// <summary>
        /// Acquires the lock for the specified <paramref name="lockContext"/> context.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When the <paramref name="lockContext"/> already holds the lock and the current <see cref="LockAsync"/>
        /// call is reentrant, the call can throw <see cref="LockAcquireLimitReachedException"/> if the lock
        /// acquisition limit is already reached.</para>
        /// </remarks>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        Task LockAsync(LockContext lockContext);

        /// <summary>
        /// Acquires the lock and returns the fencing token assigned to the specified <paramref name="lockContext"/>
        /// context for this lock acquisition.
        /// </summary>
        /// <returns>The fencing token if the lock was acquired; otherwise <see cref="InvalidFence"/>.</returns>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        /// <remarks>
        /// <para>If the lock is acquired in a reentrant way, the same fencing token is returned, or the
        /// <see cref="LockAsync"/> call can throw <see cref="LockAcquireLimitReachedException"/> if the
        /// lock acquisition limit is already reached.</para>
        /// <para>Fencing tokens are monotonic numbers that are incremented each time the lock switches
        /// from the free state to the acquired state. They are simply used for ordering lock holders.
        /// A lock holder can pass its fencing to the shared resource to fence off previous lock holders.
        /// When this resource receives an operation, it can validate the fencing token in the operation.</para>
        /// </remarks>
        Task<long> LockAndGetFenceAsync(LockContext lockContext);

        /// <summary>
        /// Tries to acquire the lock for the specified <paramref name="lockContext"/> context.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for the lock.</param>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        /// <returns><c>true</c> if the lock was acquired; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>If the lock is available or already held by the current specified <paramref name="lockContext"/>
        /// at the time of invocation and the acquisition limit is not exceeded, the method immediately returns
        /// <c>true</c>. If the lock is not immediately available, the method waits for the specified
        /// <paramref name="timeout"/>, and eventually returns <c>false</c>.
        /// </para>
        /// </remarks>
        Task<bool> TryLockAsync(LockContext lockContext, TimeSpan timeout);

        /// <summary>
        /// Tries to acquire the lock for the specified <paramref name="lockContext"/> context.
        /// </summary>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        /// <returns><c>true</c> if the lock was acquired; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>If the lock is available or already held by the current specified <paramref name="lockContext"/>
        /// at the time of invocation and the acquisition limit is not exceeded, the method immediately returns
        /// <c>true</c>. If the lock is not immediately available, the method immediately returns <c>false</c>.
        /// </para>
        /// </remarks>
        Task<bool> TryLockAsync(LockContext lockContext);

        /// <summary>
        /// Tries to acquire the lock and return the fencing token assigned to the specified <paramref name="lockContext"/>
        /// context for this lock acquisition.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for the lock.</param>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        /// <returns>The fencing token if the lock was acquired; otherwise <see cref="InvalidFence"/>.</returns>
        /// <remarks>
        /// <para>If the lock is available or already held by the current specified <paramref name="lockContext"/>
        /// at the time of invocation and the acquisition limit is not exceeded, the method immediately returns
        /// the fencing token assigned to this acquisition. If the lock is not immediately available, the method
        /// immediately returns <see cref="InvalidFence"/> representing a failed lock attempt. </para>
        /// </remarks>
        Task<long> TryLockAndGetFenceAsync(LockContext lockContext, TimeSpan timeout);

        /// <summary>
        /// Tries to acquire the lock and return the fencing token assigned to the specified <paramref name="lockContext"/>
        /// context for this lock acquisition.
        /// </summary>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        /// <returns>The fencing token if the lock was acquired; otherwise <see cref="InvalidFence"/>.</returns>
        /// <remarks>
        /// <para>If the lock is available or already held by the current specified <paramref name="lockContext"/>
        /// at the time of invocation and the acquisition limit is not exceeded, the method immediately returns
        /// the fencing token assigned to this acquisition. If the lock is not immediately available, the method
        /// immediately returns <see cref="InvalidFence"/> representing a failed lock attempt. </para>
        /// </remarks>
        Task<long> TryLockAndGetFenceAsync(LockContext lockContext);

        /// <summary>
        /// Releases the lock if the lock is currently held by the specified <paramref name="lockContext"/> context.
        /// </summary>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        Task UnlockAsync(LockContext lockContext);

        /// <summary>
        /// Determines whether this lock is held by any context or not.
        /// </summary>
        /// <returns><c>true</c> if the lock is held by any context; otherwise <c>false</c>.</returns>
        Task<bool> IsLockedAsync(LockContext lockContext);

        /// <summary>
        /// Gets the fencing token, if the lock is held by the specified <paramref name="lockContext"/> context.
        /// </summary>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        /// <returns>The fencing token, if the lock is held by the specified <paramref name="lockContext"/> context; otherwise <see cref="InvalidFence"/>.</returns>
        Task<long> GetFenceAsync(LockContext lockContext);

        /// <summary>
        /// Gets the reentrant lock count of the lock, for whichever context is locking it.
        /// </summary>
        /// <returns>The reentrant lock count of the lock, or zero if it is not locked.</returns>
        Task<int> GetLockCountAsync(LockContext lockContext);

        /// <summary>
        /// Determines whether the lock is held by the specified <paramref name="lockContext"/> context or not.
        /// </summary>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        /// <returns><c>true</c> if the lock is held by the specified <paramref name="lockContext"/> context; otherwise <c>false</c>.</returns>
        Task<bool> IsLockedByContextAsync(LockContext lockContext);
    }
}
