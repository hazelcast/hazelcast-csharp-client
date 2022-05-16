﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Exceptions;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Models;

namespace Hazelcast.CP
{
    /// <summary>
    /// Implementation of <see cref="IFencedLock"/>
    /// </summary>
    internal partial class FencedLock : CPDistributedObjectBase, IFencedLock
    {
        private readonly AsyncContextLocker _contextLocker = new AsyncContextLocker();
        private readonly ConcurrentDictionary<long, long> _lockedSessionIds = new ConcurrentDictionary<long, long>(); // context -> session
        private readonly CPSessionManager _cpSessionManager;
        private readonly CPGroupId _groupId;
        private readonly string _fullName;
        private int _destroyed, _disposed;
        public const long InvalidFence = 0;

        public FencedLock(string fullName, string objectName, CPGroupId groupId, Cluster cluster, CPSessionManager subsystemSession) 
            : base(ServiceNames.FencedLock, objectName, groupId, cluster)
        {
            _fullName = fullName; // TODO: this should be a base class property
            _groupId = groupId;
            _cpSessionManager = subsystemSession;
        }

        /// <inheritdoc />
        ICPGroupId ICPDistributedObject.GroupId => _groupId;

        /// <inheritdoc />
        long IFencedLock.InvalidFence => InvalidFence;

        #region IFencedLock Methods

        // Java has a threadId -> sessionId dictionary and code is inherently thread-safe since
        // it is threadId-based. If it gets the sessionId associated with a threadId, it knows
        // that it will not change since only the current thread can change it.
        //
        // C# is not threadId-based but contextId-based and therefore unsafe, since multiple tasks
        // can run with the same contextId at the same time. If it gets the sessionId associated
        // with a contextId, it cannot assume that it will not change, since another task with
        // the same context could be running in parallel.
        //
        // In order to make sure to serializes FencedLock method, i.e. to ensure that only one
        // method at a time can access the sessionId associated with a contextId, we use a
        // AsyncContextLocker which allows us to acquire an exclusive lock over the async context
        // at the beginning of each method.
        //
        // Note that AsyncContextLocker locks are obtained within the scope of the locker, so
        // two FencedLock (i.e. two lockers) do not interfere with each other, they run with two
        // totally different locker i.e. sets of locks, and can both lock the contexts
        // independently.
        //
        // The AsyncContextLocker LockAsync method returns an IDisposable object that releases
        // the lock upon being disposed. Therefore, if the lock is declared with the 'using var'
        // pattern, we know that it will be disposed and thus released when exiting the scope of
        // the method (even in case of exceptions).
        //
        // ->  in the end we can have code very similar to Java except for the context locker.
        //
        // The AsyncContextLocker uses a reference-counting mechanism to ensure that it does not
        // leak lock objects when no task owns a lock for a given contextId.

        /// <inheritdoc/>  
        public async Task<long> GetFenceAsync()
        {
            var contextId = AsyncContext.Current.Id; // the asynchronous context identified (equivalent to Java thread identifier)
            using var contextLock = await _contextLocker.LockAsync(contextId).CfAwait(); // lock the asynchronous context
            var sessionId = _cpSessionManager.GetSessionId(CPGroupId);

            VerifyNoLockOrValidSession(contextId, sessionId, false);

            if (sessionId == CPSessionManager.NoSessionId)
            {
                _lockedSessionIds.TryRemove(contextId, out _);
                throw new SynchronizationLockException($"Context {contextId} does not own lock {_fullName}.");
            }

            var ownership = await RequestLockOwnershipStateAsync().CfAwait();
            var lockedByCurrent = ownership.LockedBy(contextId, sessionId);

            if (lockedByCurrent)
            {
                _lockedSessionIds[contextId] = sessionId;
                return ownership.Fence;
            }

            VerifyNoLock(contextId);
            throw new SynchronizationLockException($"Context {contextId} does not own lock {_fullName}.");
        }

        /// <inheritdoc/>  
        public async Task<int> GetLockCountAsync()
        {
            var contextId = AsyncContext.Current.Id; // the asynchronous context identified (equivalent to Java thread identifier)
            using var contextLock = await _contextLocker.LockAsync(contextId).CfAwait(); // lock the asynchronous context
            var sessionId = _cpSessionManager.GetSessionId(CPGroupId);

            VerifyNoLockOrValidSession(contextId, sessionId, false);

            var ownership = await RequestLockOwnershipStateAsync().CfAwait();
            var lockedByCurrent = ownership.LockedBy(contextId, sessionId);

            if (lockedByCurrent)
            {
                _lockedSessionIds[contextId] = sessionId;
            }
            else
            {
                VerifyNoLock(contextId);
            }

            return ownership.LockCount;
        }

        /// <inheritdoc/>  
        public async Task<bool> IsLockedAsync()
        {
            var contextId = AsyncContext.Current.Id; // the asynchronous context identified (equivalent to Java thread identifier)
            using var contextLock = await _contextLocker.LockAsync(contextId).CfAwait(); // lock the asynchronous context
            var sessionId = _cpSessionManager.GetSessionId(CPGroupId);

            VerifyNoLockOrValidSession(contextId, sessionId, false);

            var ownership = await RequestLockOwnershipStateAsync().CfAwait();
            var lockedByCurrent = ownership.LockedBy(contextId, sessionId);

            if (lockedByCurrent)
            {
                _lockedSessionIds[contextId] = sessionId;
                return true;
            }

            VerifyNoLock(contextId);
            return ownership.Locked;
        }

        /// <inheritdoc/>        
        public async Task<bool> IsLockedByCurrentContextAsync()
        {
            var contextId = AsyncContext.Current.Id; // the asynchronous context identified (equivalent to Java thread identifier)
            using var contextLock = await _contextLocker.LockAsync(contextId).CfAwait(); // lock the asynchronous context
            var sessionId = _cpSessionManager.GetSessionId(CPGroupId);

            VerifyNoLockOrValidSession(contextId, sessionId, false);

            var ownership = await RequestLockOwnershipStateAsync().CfAwait();
            var lockedByCurrent = ownership.LockedBy(contextId, sessionId);

            if (lockedByCurrent)
            {
                _lockedSessionIds[contextId] = sessionId;
            }
            else
            {
                VerifyNoLock(contextId);
            }

            return lockedByCurrent;
        }

        /// <inheritdoc/>        
        public async Task<long> LockAndGetFenceAsync()
        {
            var contextId = AsyncContext.Current.Id; // the asynchronous context identified (equivalent to Java thread identifier)
            using var contextLock = await _contextLocker.LockAsync(contextId).CfAwait(); // lock the asynchronous context
            var invocationId = Guid.NewGuid(); // required by server, to make the call idempotent

            while (true)
            {
                var sessionId = await _cpSessionManager.AcquireSessionAsync(CPGroupId).CfAwait();

                VerifyNoLockOrValidSession(contextId, sessionId, false);

                try
                {
                    // go to the server to lock - if we get a valid fence, return
                    var fence = await RequestLockAsync(sessionId, contextId, invocationId).CfAwait();
                    if (fence != InvalidFence)
                    {
                        _lockedSessionIds[contextId] = sessionId;
                        return fence;
                    }

                    throw new LockAcquireLimitReachedException($"Lock[{_fullName}] re-entrant lock limit has been reached.");

                }
                catch (RemoteException e) when (e.Error == RemoteError.SessionExpiredException)
                {
                    _cpSessionManager.InvalidateSession(CPGroupId, sessionId);
                    VerifyNoLock(contextId);
                    // loop and try again
                }
                catch (RemoteException e) when (e.Error == RemoteError.WaitKeyCancelledException)
                {
                    _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                    throw; // note: Java throws new IllegalMonitorStateException
                }
                catch
                {
                    _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                    throw;
                }
            }
        }

        /// <inheritdoc/> 
        public Task LockAsync()
        {
            return LockAndGetFenceAsync();
        }

        /// <inheritdoc/> 
        public Task<long> TryLockAndGetFenceAsync()
        {
            return TryLockAndGetFenceAsync(TimeSpan.FromMilliseconds(0));
        }

        /// <inheritdoc/> 
        public async Task<long> TryLockAndGetFenceAsync(TimeSpan timeout)
        {
            var contextId = AsyncContext.Current.Id; // the asynchronous context identified (equivalent to Java thread identifier)
            using var contextLock = await _contextLocker.LockAsync(contextId).CfAwait(); // lock the asynchronous context
            var invocationId = Guid.NewGuid(); // required by server, to make the call idempotent

            var timeoutMilliseconds = (long) Math.Round(Math.Max(0, timeout.TotalMilliseconds));
            var start = Clock.Milliseconds;

            while (true)
            {
                var sessionId = await _cpSessionManager.AcquireSessionAsync(CPGroupId).CfAwait();

                VerifyNoLockOrValidSession(contextId, sessionId, true);

                try
                {
                    var fence = await RequestTryLockAsync(sessionId, contextId, invocationId, timeoutMilliseconds).CfAwait();

                    if (fence != InvalidFence)
                    {
                        _lockedSessionIds[contextId] = sessionId;
                    }
                    else
                    {
                        _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                    }

                    return fence;
                }
                catch (RemoteException e) when (e.Error == RemoteError.SessionExpiredException)
                {
                    _cpSessionManager.InvalidateSession(CPGroupId, sessionId);
                    VerifyNoLock(contextId);
                    var elapsed = Clock.Milliseconds - start;
                    if (elapsed > timeoutMilliseconds) return InvalidFence;
                    // else loop and try again
                }
                catch (RemoteException e) when (e.Error == RemoteError.WaitKeyCancelledException)
                {
                    _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                    return InvalidFence;
                }
                catch
                {
                    _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                    throw;
                }
            }
        }

        /// <inheritdoc/> 
        public async Task<bool> TryLockAsync(TimeSpan timeout)
        {
            var fence = await TryLockAndGetFenceAsync(timeout).CfAwait();
            return fence != InvalidFence;
        }

        /// <inheritdoc/> 
        public async Task<bool> TryLockAsync()
        {
            var fence = await TryLockAndGetFenceAsync(TimeSpan.FromMilliseconds(0)).CfAwait();
            return fence != InvalidFence;
        }

        /// <inheritdoc/> 
        public async Task UnlockAsync()
        {
            var contextId = AsyncContext.Current.Id; // the asynchronous context identified (equivalent to Java thread identifier)
            using var contextLock = await _contextLocker.LockAsync(contextId).CfAwait(); // lock the asynchronous context
            var sessionId = _cpSessionManager.GetSessionId(CPGroupId);

            VerifyNoLockOrValidSession(contextId, sessionId, false);

            if (sessionId == CPSessionManager.NoSessionId)
            {
                _lockedSessionIds.TryRemove(contextId, out _);
                throw new SynchronizationLockException(); // note: Java throws new IllegalMonitorStateException
            }

            try
            {
                var invocationId = Guid.NewGuid(); // required by server, to make the call idempotent

                var stillLockedByCurrentThread = await RequestUnlockAsync(sessionId, contextId, invocationId).CfAwait();

                if (stillLockedByCurrentThread)
                {
                    _lockedSessionIds[contextId] = sessionId;
                }
                else
                {
                    _lockedSessionIds.TryRemove(contextId, out _);
                }

                _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
            }
            catch (RemoteException e) when (e.Error == RemoteError.SessionExpiredException)
            {
                _cpSessionManager.InvalidateSession(CPGroupId, sessionId);
                _lockedSessionIds.TryRemove(contextId, out _);
                throw;
            }
            catch (RemoteException e) when (e.Error == RemoteError.IllegalMonitorState)
            {
                _lockedSessionIds.TryRemove(contextId, out _);
                throw;
            }
        }

        #endregion

        private void VerifyNoLockOrValidSession(long contextId, long sessionId, bool releaseSession)
        {
            if (_lockedSessionIds.TryGetValue(contextId, out var lockedSessionId) && lockedSessionId != sessionId)
            {
                _lockedSessionIds.TryRemove(contextId, out _);
                if (releaseSession) _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                throw new LockOwnershipLostException($"Context {contextId} lost lock {_fullName} because the associated session {lockedSessionId} was closed.");
            }
        }

        private void VerifyNoLock(long contextId)
        {
            if (_lockedSessionIds.TryRemove(contextId, out var lockedSessionId))
            {
                throw new LockOwnershipLostException($"Context {contextId} lost lock {_fullName} because the associated session {lockedSessionId} was closed.");
            }
        }

        public override async ValueTask DestroyAsync()
        {
            if (!_destroyed.InterlockedZeroToOne()) return;

            await RequestDestroyAsync().CfAwait();

            // note: still needs to be disposed to clear the _contextLocker
        }

        /// <inheritdoc />
        public override ValueTask DisposeAsync()
        {
            if (!_disposed.InterlockedZeroToOne()) return default;

            _contextLocker.Dispose();

            return base.DisposeAsync();
        }

        internal class LockOwnershipState
        {
            public long Fence { get; }
            public long SessionId { get; }
            public long ThreadId { get; }
            public int LockCount { get; }

            public LockOwnershipState(long fence, long sessionId, long threadId, int lockCount)
            {
                Fence = fence;
                SessionId = sessionId;
                ThreadId = threadId;
                LockCount = lockCount;
            }

            public bool Locked => FencedLock.InvalidFence != Fence;

            public bool LockedBy(long threadId, long sessionId) => Locked && SessionId == sessionId && ThreadId == threadId;
        }
    }
}
