// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Exceptions;
using Hazelcast.Core;

namespace Hazelcast.CP
{
    // FIXME document lockContext

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
        /// Acquires the lock.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When the caller already holds the lock and the current <see cref="LockAsync"/> call is      
        /// reentrant, the call can throw <see cref="LockAcquireLimitReachedException"/> 
        /// if the lock acquire limit is already reached.</para>
        /// </remarks>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        Task LockAsync(LockContext lockContext);

        /// <summary>
        /// Acquires the lock and returns the fencing token assigned to the current context for this lock acquire. 
        /// </summary>
        /// <remarks>
        /// <para>If the lock is acquired reentrantly, the same fencing token is returned, or the <see cref="LockAsync"/> call can 
        /// throw <see cref="LockAcquireLimitReachedException"/>  if the lock acquire limit is already reached.</para>
        /// <para>Fencing tokens are monotonic numbers that are incremented each time the lock switches 
        /// from the free state to the acquired state. They are simply used for ordering lock holders. 
        /// A lock holder can pass its fencing to the shared resource to fence off previous lock holders. 
        /// When this resource receives an operation, it can validate the fencing token in the operation.</para>
        /// </remarks>
        /// <returns>The fencing token if the lock was acquired and <see cref="InvalidFence"/> otherwise.</returns>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        Task<long> LockAndGetFenceAsync(LockContext lockContext);

        /// <summary>
        /// Acquires the lock if it is available or already held by the current context at the time of invocation 
        /// and the acquire limit is not exceeded, and immediately returns with the value true. If the lock is not 
        /// available, then this method immediately returns with the value false. When the call is reentrant, 
        /// it can return false if the lock acquire limit is exceeded.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for the lock</param>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        /// <returns><c>true</c> if the lock was acquired otherwise <c>false</c></returns>
        Task<bool> TryLockAsync(LockContext lockContext, TimeSpan timeout);

        /// <summary>
        /// Acquires the lock if it is available or already held by the current context at the time of invocation 
        /// and the acquire limit is not exceeded, and immediately returns with the value true. If the lock is not 
        /// available, then this method immediately returns with the value false. When the call is reentrant, 
        /// it can return false if the lock acquire limit is exceeded.
        /// </summary>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        /// <returns><c>true</c> if the lock was acquired otherwise <c>false</c></returns>
        Task<bool> TryLockAsync(LockContext lockContext);

        /// <summary>
        /// Acquires the lock only if it is free or already held by the current context at the time of invocation
        /// and the acquire limit is not exceeded, and returns the fencing token assigned to the current context
        /// for this lock acquire. If the lock is acquired reentrantly, the same fencing token is returned. 
        /// If the lock is already held by another caller or the lock acquire limit is exceeded, then this method 
        /// immediately returns <see cref="InvalidFence"/> that represents a failed lock attempt.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for the lock</param>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        /// <returns>The fencing token if the lock was acquired and <see cref="InvalidFence"/> otherwise</returns>
        Task<long> TryLockAndGetFenceAsync(LockContext lockContext, TimeSpan timeout);

        /// <summary>
        /// Acquires the lock only if it is free or already held by the current context at the time of invocation
        /// and the acquire limit is not exceeded, and returns the fencing token assigned to the current context
        /// for this lock acquire. If the lock is acquired reentrantly, the same fencing token is returned. 
        /// If the lock is already held by another caller or the lock acquire limit is exceeded, then this method 
        /// immediately returns <see cref="InvalidFence"/> that represents a failed lock attempt.
        /// </summary>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        /// <returns>The fencing token if the lock was acquired and <see cref="InvalidFence"/> otherwise</returns>
        Task<long> TryLockAndGetFenceAsync(LockContext lockContext);

        /// <summary>
        /// Releases the lock if the lock is currently held by the current context.
        /// </summary>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        Task UnlockAsync(LockContext lockContext);

        /// <summary>
        /// Checks whether this lock is locked or not.
        /// </summary>
        /// <returns><c>true</c> if the lock was acquired; otherwise <c>false</c>.</returns>
        Task<bool> IsLockedAsync();

        /// <summary>
        /// Gets the fencing token, if the lock is held by the current context.
        /// </summary>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        /// <returns>The fencing token, if the lock is held by the current context; otherwise <see cref="InvalidFence"/>.</returns>
        Task<long> GetFenceAsync(LockContext lockContext);

        /// <summary>
        /// Gets the reentrant lock count of the lock.
        /// </summary>
        /// <returns>The reentrant lock count of the lock, or zero if it is not locked.</returns>
        Task<int> GetLockCountAsync();

        /// <summary>
        /// Determines whether the lock is held by the current context or not.
        /// </summary>
        /// <param name="lockContext">The <see cref="LockContext"/>.</param>
        /// <returns><c>true</c> if the lock is held by the current context; otherwise <c>false</c>.</returns>
        Task<bool> IsLockedAsync(LockContext lockContext);
    }
}
