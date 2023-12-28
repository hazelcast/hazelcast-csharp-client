// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Exceptions;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Models;
using Hazelcast.Serialization;

namespace Hazelcast.CP
{
    /// <summary>
    /// Implementation of <see cref="IFencedLock"/>
    /// </summary>
    internal partial class FencedLock : CPDistributedObjectBase, IFencedLock
    {
        private readonly Dictionary<long, LockState> _locks = new Dictionary<long, LockState>();
        private readonly object _locksMutex = new object();

        private readonly CPSessionManager _cpSessionManager;
        private readonly CPGroupId _groupId;
        private readonly string _fullName;
        private int _destroyed;
        public const long InvalidFence = 0;

        public FencedLock(string fullName, string objectName, CPGroupId groupId, Cluster cluster, CPSessionManager subsystemSession, SerializationService serializationService)
            : base(ServiceNames.FencedLock, objectName, groupId, cluster, serializationService)
        {
            _fullName = fullName; // TODO: this should be a base class property
            _groupId = groupId;
            _cpSessionManager = subsystemSession;
        }

        /// <inheritdoc />
        ICPGroupId ICPDistributedObject.GroupId => _groupId;

        /// <inheritdoc />
        long IFencedLock.InvalidFence => InvalidFence;

        #region LockState

        private class LockState
        {
            public LockState(long contextId, long sessionId)
            {
                ContextId = contextId;
                SessionId = sessionId;
            }

            public long ContextId { get; }

            public long SessionId { get; }

            public int Count { get; set; }
        }

        // gets the LockState associated with a contextId
        // if no LockState was associated with the contextId, create a new one
        // otherwise, ensure that the sessionIds match
        // and, if they don't, remove the LockState and throw (optionally release the session)
        //
        // once a LockState has been returned by GetOrCreateLockState,
        // it should either be entered, exited, collected or removed
        private LockState GetOrCreateLockState(long contextId, long sessionId, bool releaseSession = false, bool acceptNoSession = false)
        {
            lock (_locksMutex)
            {
                if (_locks.TryGetValue(contextId, out var lockState))
                {
                    if (lockState.SessionId != sessionId) // includes sessionId being CPSessionManager.NoSessionId
                    {
                        _locks.Remove(contextId);
                        if (releaseSession) _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                        throw new LockOwnershipLostException($"Context {contextId} lost lock {_fullName} because the associated session {lockState.SessionId} was closed.");
                    }
                }
                else if (sessionId == CPSessionManager.NoSessionId)
                {
                    if (acceptNoSession)
                    {
                        lockState = new LockState(contextId, sessionId);
                    }
                    else
                    {
                        throw new SynchronizationLockException($"Context {contextId} does not own lock {_fullName}."); // note: Java throws new IllegalMonitorStateException
                    }
                }
                else
                {
                    lockState = _locks[contextId] = new LockState(contextId, sessionId);
                }

                return lockState;
            }
        }

        // removes a LockState entirely, because it is not valid anymore
        // so if it is actually removed, with a count > 0, throw
        private void RemoveLock(LockState lockState)
        {
            lock (_locksMutex)
            {
                if (_locks.TryGetValue(lockState.ContextId, out var ls) && lockState == ls)
                {
                    _locks.Remove(lockState.ContextId);
                    if (lockState.Count > 0)
                        throw new LockOwnershipLostException($"Context {lockState.ContextId} lost lock {_fullName} because the associated session {lockState.SessionId} was closed.");
                }
            }
        }

        // enters a lock = increments the reference count of its LockState
        // if the count becomes zero, removes the LockState
        // (due to concurrency, we may ExitLock following an unlock, before EnterLock following a lock,
        // and then the count would go 0 -> -1 on ExitLock -> 0 on EnterLock, so we have to test for
        // zero here too and not only in ExitLock)
        private void EnterLock(LockState lockState)
        {
            lock (_locksMutex)
            {
                lockState.Count++;
                if (lockState.Count == 0 && _locks.TryGetValue(lockState.ContextId, out var ls) && lockState == ls)
                    _locks.Remove(lockState.ContextId);
            }
        }

        // exits a lock = decrements the reference count of its LockState
        // if the count becomes zero, removes the LockState
        private void ExitLock(LockState lockState)
        {
            lock (_locksMutex)
            {
                lockState.Count--;
                if (lockState.Count == 0 && _locks.TryGetValue(lockState.ContextId, out var ls) && lockState == ls)
                    _locks.Remove(lockState.ContextId);
            }
        }

        // collects a lock = removes its LockState if the count is zero
        private void CollectLock(LockState lockState)
        {
            lock (_locksMutex)
            {
                if (lockState.Count == 0 && _locks.TryGetValue(lockState.ContextId, out var ls) && lockState == ls)
                    _locks.Remove(lockState.ContextId);
            }
        }

        #endregion

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
        // We cannot simply serialize FencedLock methods as a blocked LockAsync call could then
        // block a TryLockAsync call - which should instead return immediately - in other words
        // we cannot put an exclusive lock around cluster-side operations - thus, we have to
        // leave with race conditions and try our best to mitigate them.

        /// <inheritdoc/>
        public async Task<long> GetFenceAsync(LockContext lockContext)
        {
            if (lockContext == null) throw new ArgumentNullException(nameof(lockContext));

            var contextId = lockContext.Id; // (equivalent to Java thread identifier)
            var sessionId = _cpSessionManager.GetSessionId(CPGroupId);

            var lockState = GetOrCreateLockState(contextId, sessionId); // note: handles sessionId being NoSessionId

            // if count is zero then we cannot possibly get a fence
            if (lockState.Count == 0)
            {
                CollectLock(lockState);
                throw new SynchronizationLockException($"Context {contextId} does not own lock {_fullName}.");
            }

            var ownership = await RequestLockOwnershipStateAsync().CfAwait();
            var lockedByCurrent = ownership.LockedBy(contextId, sessionId);

            // we *cannot* trust the returned value (lockedByCurrent) for any LockState-related
            // operations, since another task may lock/unlock anytime, so lockedByCurrent could
            // be false and yet we *already* are locked by current. all we can do is collect
            // the LockState.
            CollectLock(lockState);

            if (lockedByCurrent) return ownership.Fence;

            // now it becomes tricky
            // server says we do not own the lock, and what shall we do?
            // see notes in IsLockedAsync for a complete discussion,
            // there is nothing we can do about lockState

            // throw for this call
            throw new SynchronizationLockException($"Context {contextId} does not own lock {_fullName}.");
        }

        /// <inheritdoc/>
        public async Task<int> GetLockCountAsync(LockContext lockContext)
        {
            // the original Java code does some sanity-checking on this operation but
            // due to race conditions linked to contexts, we totally cannot do it.
            //
            // the lockContext parameter is kept for consistency so all IFencedLock
            // methods require a LockContext - but it is pointless here and not used.

            var ownership = await RequestLockOwnershipStateAsync().CfAwait();
            return ownership.LockCount;

            /*
            var contextId = lockContext.Id; // (equivalent to Java thread identifier)
            var sessionId = _cpSessionManager.GetSessionId(CPGroupId);
            var lockState = GetOrCreateLockState(contextId, sessionId); // note: handles sessionId being NoSessionId

            var ownership = await RequestLockOwnershipStateAsync().CfAwait();
            var lockedByCurrent = ownership.LockedBy(contextId, sessionId);
            CollectLock(lockState);

            if (lockedByCurrent)
            {
                // we cannot make any decision here!
            }

            return ownership.LockCount;
            */
        }

        /// <inheritdoc/>
        public async Task<bool> IsLockedAsync(LockContext lockContext)
        {
            // the original Java code does some sanity-checking on this operation but
            // due to race conditions linked to contexts, we totally cannot do it.
            //
            // the lockContext parameter is kept for consistency so all IFencedLock
            // methods require a LockContext - but it is pointless here and not used.

            var ownership = await RequestLockOwnershipStateAsync().CfAwait();
            return ownership.Locked;

            /*
            var contextId = lockContext.Id; // (equivalent to Java thread identifier)
            var sessionId = _cpSessionManager.GetSessionId(CPGroupId);

            VerifyNoLockOrValidSession(contextId, sessionId, false);

            var ownership = await RequestLockOwnershipStateAsync().CfAwait();
            var lockedByCurrent = ownership.LockedBy(contextId, sessionId);
            CollectLock(lockState);

            if (lockedByCurrent)
            {
                // we cannot make any decision here!
            }

            return ownership.Locked;
            */
        }

        /// <inheritdoc/>
        public async Task<bool> IsLockedByContextAsync(LockContext lockContext)
        {
            if (lockContext == null) throw new ArgumentNullException(nameof(lockContext));

            var contextId = lockContext.Id; // (equivalent to Java thread identifier)
            var sessionId = _cpSessionManager.GetSessionId(CPGroupId);

            var lockState = GetOrCreateLockState(contextId, sessionId, acceptNoSession: true);
            if (sessionId == CPSessionManager.NoSessionId) return false;

            var count0 = lockState.Count;
            var ownership = await RequestLockOwnershipStateAsync().CfAwait();
            var lockedByCurrent = ownership.LockedBy(contextId, sessionId);

            // we *cannot* trust the returned value (lockedByCurrent) for any LockState-related
            // operations, since another task may lock/unlock anytime, so lockedByCurrent could
            // be false and yet we *already* are locked by current. all we can do is collect
            // the LockState.
            CollectLock(lockState);

            if (!lockedByCurrent)
            {
                // now it becomes tricky
                //
                // Java can immediately assume that if we think we are locked, we have lost
                // the lock, and we must throw + clear our internal structures. .NET is
                // different due to contexts vs. threads.
                //
                // and, we cannot prevent the race conditions, because we would need to block
                // on locks, and locking can block for a long time, so it's all not good.
                //
                // if current lockState.Count is zero, then we don't think we own the lock, so
                // everything is all right. on the other hand, if lockState.Count is >0, then
                // we think we own the lock. but, due to concurrency, maybe the current context
                // acquired the lock *after* we RequestLockOwnershipStateAsync and everything
                // is actually all right too.
                //
                // using the following heuristics: if count is now >0 and it was zero *before*
                // RequestLockOwnershipStateAsync, then probably the lock was acquired in the
                // meantime and things are ok - on the other hand if it was >0 before, then we
                // can conclude that something is wrong.
                //
                // still, consider the following sequence:
                // - count "before" is 1
                // - lock is unlocked
                // - we RequestLockOwnershipStateAsync and lockedByCurrent is false
                // - locked is locked
                // - count "after" is 1
                //
                // yet we still own the lock - ok, at the time of testing, we had temporarily
                // lost it, so it's OK to throw, but we CANNOT make a decision for lockState
                // as, in this example, it is perfectly valid. we simply don't know.
                //
                // so we have to leave it unchanged. if we actually lost the lock, that HAS
                // to be a session issue, so a later call to any method will cause an
                // exception to be thrown in GetOrCreateLockState - compared to Java, we may
                // throw more exceptions, or throw later.

                if (lockState.Count > 0 && count0 > 0)
                    throw new LockOwnershipLostException($"Context {contextId} lost lock {_fullName} because the associated session {lockState.SessionId} was closed.");
            }

            return lockedByCurrent;
        }

        /// <inheritdoc/>
        public async Task<long> LockAndGetFenceAsync(LockContext lockContext)
        {
            if (lockContext == null) throw new ArgumentNullException(nameof(lockContext));

            var contextId = lockContext.Id; // (equivalent to Java thread identifier)
            var invocationId = Guid.NewGuid(); // required by server, to make the call idempotent

            while (true)
            {
                var sessionId = await _cpSessionManager.AcquireSessionAsync(CPGroupId).CfAwait();
                var lockState = GetOrCreateLockState(contextId, sessionId, true);

                try
                {
                    // go to the server to lock - if we get a valid fence, return
                    var fence = await RequestLockAsync(sessionId, contextId, invocationId).CfAwait();
                    if (fence != InvalidFence)
                    {
                        EnterLock(lockState);
                        return fence;
                    }

                    // going to be caught & rethrown below & it will release the session & collect the lock
                    throw new LockAcquireLimitReachedException($"Lock[{_fullName}] re-entrant lock limit has been reached.");
                }
                catch (RemoteException e) when (e.Error == RemoteError.SessionExpiredException)
                {
                    _cpSessionManager.InvalidateSession(CPGroupId, sessionId);
                    RemoveLock(lockState);
                    // loop and try again
                }
                catch (RemoteException e) when (e.Error == RemoteError.WaitKeyCancelledException)
                {
                    _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                    CollectLock(lockState);
                    throw; // note: Java throws new IllegalMonitorStateException
                }
                catch
                {
                    _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                    CollectLock(lockState);
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public Task LockAsync(LockContext lockContext)
        {
            return LockAndGetFenceAsync(lockContext);
        }

        /// <inheritdoc/>
        public Task<long> TryLockAndGetFenceAsync(LockContext lockContext)
        {
            return TryLockAndGetFenceAsync(lockContext, TimeSpan.FromMilliseconds(0));
        }

        /// <inheritdoc/>
        public async Task<long> TryLockAndGetFenceAsync(LockContext lockContext, TimeSpan timeout)
        {
            if (lockContext == null) throw new ArgumentNullException(nameof(lockContext));

            var contextId = lockContext.Id; // (equivalent to Java thread identifier)
            var invocationId = Guid.NewGuid(); // required by server, to make the call idempotent

            var timeoutMilliseconds = (long) Math.Round(Math.Max(0, timeout.TotalMilliseconds));
            var start = Clock.Milliseconds;

            while (true)
            {
                var sessionId = await _cpSessionManager.AcquireSessionAsync(CPGroupId).CfAwait();
                var lockState = GetOrCreateLockState(contextId, sessionId, true);

                try
                {
                    var fence = await RequestTryLockAsync(sessionId, contextId, invocationId, timeoutMilliseconds).CfAwait();
                    if (fence != InvalidFence)
                    {
                        EnterLock(lockState);
                    }
                    else
                    {
                        _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                        CollectLock(lockState);
                    }

                    return fence;
                }
                catch (RemoteException e) when (e.Error == RemoteError.SessionExpiredException)
                {
                    _cpSessionManager.InvalidateSession(CPGroupId, sessionId);
                    RemoveLock(lockState);
                    var elapsed = Clock.Milliseconds - start;
                    if (elapsed > timeoutMilliseconds) return InvalidFence;
                    // else loop and try again
                }
                catch (RemoteException e) when (e.Error == RemoteError.WaitKeyCancelledException)
                {
                    _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                    CollectLock(lockState);
                    return InvalidFence;
                }
                catch
                {
                    _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                    CollectLock(lockState);
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<bool> TryLockAsync(LockContext lockContext, TimeSpan timeout)
        {
            var fence = await TryLockAndGetFenceAsync(lockContext, timeout).CfAwait();
            return fence != InvalidFence;
        }

        /// <inheritdoc/>
        public async Task<bool> TryLockAsync(LockContext lockContext)
        {
            var fence = await TryLockAndGetFenceAsync(lockContext, TimeSpan.FromMilliseconds(0)).CfAwait();
            return fence != InvalidFence;
        }

        /// <inheritdoc/>
        public async Task UnlockAsync(LockContext lockContext)
        {
            if (lockContext == null) throw new ArgumentNullException(nameof(lockContext));

            var contextId = lockContext.Id; // (equivalent to Java thread identifier)
            var sessionId = _cpSessionManager.GetSessionId(CPGroupId);

            var lockState = GetOrCreateLockState(contextId, sessionId); // note: handles sessionId being NoSessionId

            try
            {
                var invocationId = Guid.NewGuid(); // required by server, to make the call idempotent
                _ = await RequestUnlockAsync(sessionId, contextId, invocationId).CfAwait();

                // note: unlocking when not-locked causes a RemoteException w/ Error IllegalMonitorState
                // and message "Current thread is not owner of the lock!" so we *know* that if we go past
                // the RequestUnlockAsync call, then we *have* effectively decreased the lock count by 1.

                // we *cannot* trust the returned value (stillLockedByCurrentContext) since another task
                // may lock/unlock anytime, all we can do is decrement the reference count, which will
                // remove the state when count reaches zero.
                // it *may* be that the lockState has been removed already, but it does not really matter.
                ExitLock(lockState);

                // release session
                _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
            }
            catch (RemoteException e) when (e.Error == RemoteError.SessionExpiredException)
            {
                _cpSessionManager.InvalidateSession(CPGroupId, sessionId);
                RemoveLock(lockState);
                throw;
            }
            catch (RemoteException e) when (e.Error == RemoteError.IllegalMonitorState)
            {
                RemoveLock(lockState);
                throw;
            }
        }

        #endregion

        public override async ValueTask DestroyAsync()
        {
            if (!_destroyed.InterlockedZeroToOne()) return;

            await base.DestroyAsync().CfAwait();

            // note: still needs to be disposed to clear the _contextLocker
        }

        internal class LockOwnershipState
        {
            public long Fence { get; }

            private long SessionId { get; }

            private long ThreadId { get; }

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
