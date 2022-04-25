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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Exceptions;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Protocol.Models;

namespace Hazelcast.CP
{
    /// <summary>
    /// Implementation of <see cref="IFencedLock"/>
    /// </summary>
    internal partial class FencedLock : CPDistributedObjectBase, IFencedLock
    {
        /// <summary>
        /// Gets the current context identifier.
        /// </summary>
        /// <remarks>
        /// Hazelcast APIs call this the thread identified and maintain locks "per threads",
        /// so we are keeping the name here internally, but in reality this is not a thread
        /// identifier anymore - it is attached to the async context so it can flow with
        /// async operations.
        /// </remarks>
        private static long ContextId => AsyncContext.Current.Id;
        private readonly ConcurrentDictionary<long, long> _lockedThreadToSession = new ConcurrentDictionary<long, long>();
        private readonly ConcurrentDictionary<long, SemaphoreSlim> _contextSemaphore = new ConcurrentDictionary<long, SemaphoreSlim>();
        private readonly CPSessionManager _cpSessionManager;
        private readonly CPGroupId _groupId;
        private int _destroyed;
        //Holds whether current async flow took the semaphore
        private static AsyncLocal<bool> LocalSemaphoreContext { get; } = new AsyncLocal<bool>() { Value = false };
        public const long InvalidFence = 0;
        ICPGroupId ICPDistributedObject.GroupId => _groupId;
        long IFencedLock.InvalidFence => InvalidFence;

        public FencedLock(string name, CPGroupId groupId, Cluster cluster, CPSessionManager subsystemSession) : base(ServiceNames.FencedLock, name, groupId, cluster)
        {
            _cpSessionManager = subsystemSession;
            _groupId = groupId;
            HConsole.Configure(x => x.Configure<FencedLock>().SetIndent(2).SetPrefix("FENCEDLOCK"));
        }

        #region IFencedLock Methods
        /// <inheritdoc/>  
        public async Task<long> GetFenceAsync()
        {
            var threadId = ContextId;
            var sessionId = _cpSessionManager.GetSessionId(CPGroupId);
            var semaphore = VerifyNoLockOnThread(threadId, sessionId, false);
            HConsole.WriteLine(this, $"GetFenceAsync->Thread{threadId}, Session:{sessionId}, Semaphore:{semaphore.GetHashCode()}");

            try
            {
                if (sessionId == CPSessionManager.NoSessionId)
                {
                    _lockedThreadToSession.TryRemove(threadId, out var _);
                    throw new SynchronizationLockException();
                }

                var ownership = await RequestLockOwnershipStateAsync().CfAwait();

                if (ownership.LockedBy(threadId, sessionId))
                {
                    AsyncContext.RequireNew();
                    _lockedThreadToSession[threadId] = sessionId;
                    return ownership.Fence;
                }

                VerifyNoLockedSessionExist(threadId);
                throw new SynchronizationLockException();
            }
            finally
            {
                if(LocalSemaphoreContext.Value)
                    semaphore.Release();
            }
        }

        /// <inheritdoc/>  
        public async Task<int> GetLockCountAsync()
        {
            var threadId = ContextId;
            var sessionId = _cpSessionManager.GetSessionId(CPGroupId);
            var semaphore = VerifyNoLockOnThread(threadId, sessionId, false);

            try
            {
                var ownership = await RequestLockOwnershipStateAsync().CfAwait();

                if (ownership.LockedBy(threadId, sessionId))
                    _lockedThreadToSession[threadId] = sessionId;
                else
                    VerifyNoLockedSessionExist(threadId);

                return ownership.LockCount;
            }
            finally
            {
                if (LocalSemaphoreContext.Value)
                    semaphore.Release();
            }
        }

        /// <inheritdoc/>  
        public async Task<bool> IsLockedAsync()
        {
            var threadId = ContextId;
            var sessionId = _cpSessionManager.GetSessionId(CPGroupId);
            var semaphore = VerifyNoLockOnThread(threadId, sessionId, false);
            HConsole.WriteLine(this, $"IsLockedAsync->Thread{threadId}, Session:{sessionId}, Semaphore:semaphore.GetHashCode()");
            try
            {
                var ownership = await RequestLockOwnershipStateAsync().CfAwait();

                if (ownership.LockedBy(threadId, sessionId))
                {
                    _lockedThreadToSession[threadId] = sessionId;
                    return true;
                }

                VerifyNoLockedSessionExist(threadId);

                return ownership.Locked;
            }
            finally
            {
                if (LocalSemaphoreContext.Value)
                    semaphore.Release();
            }
        }

        public async Task<bool> IsLockedByCurrentContext()
        {
            var threadId = ContextId;
            var sessionId = _cpSessionManager.GetSessionId(CPGroupId);
            var semaphore = VerifyNoLockOnThread(threadId, sessionId, false);
            HConsole.WriteLine(this, $"IsLockedByCurrentContext->Thread{threadId}, Session:{sessionId}, Semaphore:{semaphore.GetHashCode()}");
            try
            {
                var ownership = await RequestLockOwnershipStateAsync().CfAwait();

                var lockedByCurrent = ownership.LockedBy(threadId, sessionId);

                if (lockedByCurrent)
                    _lockedThreadToSession[threadId] = sessionId;
                else
                    VerifyNoLockedSessionExist(threadId);

                return lockedByCurrent;
            }
            finally
            {
                if (LocalSemaphoreContext.Value)
                    semaphore.Release();
            }
        }

        /// <inheritdoc/>        
        public async Task<long> LockAndGetFenceAsync()
        {
            var threadId = ContextId;
            var invocationId = Guid.NewGuid();// required by server, to make the call idempotetent?

            while (true)
            {
                var sessionId = await _cpSessionManager.AcquireSessionAsync(CPGroupId).CfAwait();
                var semaphore = VerifyNoLockOnThread(threadId, sessionId, true);
                HConsole.WriteLine(this, $"LockAndGetFenceAsync->Thread{threadId}, Invocation:{invocationId}, Session:{sessionId}, Semaphore:{semaphore.GetHashCode()}");
                try
                {
                    long fence = await RequestLockAsync(sessionId, threadId, invocationId).CfAwait();

                    if (fence != InvalidFence)
                    {
                        _lockedThreadToSession[threadId] = sessionId;
                        return fence;
                    }

                    throw new LockAcquireLimitReachedException($"Lock[{Name}] reentrant lock limit is already reached!");

                }
                catch (RemoteException e)
                {
                    if (e is RemoteException { Error: RemoteError.SessionExpiredException })
                    {
                        _cpSessionManager.InvalidateSession(CPGroupId, sessionId);
                        VerifyNoLockedSessionExist(threadId);
                    }
                    else if (e is RemoteException { Error: RemoteError.WaitKeyCancelledException })
                    {
                        _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                        throw;
                    }
                    else
                    {
                        throw;
                    }
                }
                catch
                {
                    _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                    throw;
                }
                finally
                {
                    if (LocalSemaphoreContext.Value)
                        semaphore.Release();
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
            var threadId = ContextId;
            Guid invocationId = Guid.NewGuid();
            var timeoutMilliseconds = (long)Math.Round(Math.Max(0, timeout.TotalMilliseconds));

            while (true)
            {
                var start = Clock.Milliseconds;
                var sessionId = await _cpSessionManager.AcquireSessionAsync(CPGroupId).CfAwait();
                var semaphore = VerifyNoLockOnThread(threadId, sessionId);
                HConsole.WriteLine(this, $"TryLockAndGetFenceAsync->Thread{threadId}, Invocation:{invocationId}, Session:{sessionId}, Semaphore:{semaphore.GetHashCode()}");

                try
                {
                    long fence = await RequestTryLockAsync(sessionId, threadId, invocationId, timeoutMilliseconds).CfAwait();

                    if (fence != InvalidFence)
                        _lockedThreadToSession[threadId] = sessionId;
                    else
                    {
                        _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                        RemoveLocks(threadId, sessionId);
                    }

                    return fence;
                }
                catch (RemoteException e)
                {
                    if (e is RemoteException { Error: RemoteError.SessionExpiredException })
                    {
                        _cpSessionManager.InvalidateSession(CPGroupId, sessionId);
                        VerifyNoLockedSessionExist(threadId);

                        long duration = Clock.Milliseconds - start;

                        if (duration <= 0)
                            return InvalidFence;
                    }
                    else if (e is RemoteException { Error: RemoteError.WaitKeyCancelledException })
                    {
                        _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                        RemoveLocks(threadId, sessionId);
                        return InvalidFence;
                    }
                    else
                    {
                        _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                        RemoveLocks(threadId, sessionId);
                        throw;
                    }
                }
                catch
                {
                    _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
                    RemoveLocks(threadId, sessionId);
                    throw;
                }
                finally
                {
                    if (LocalSemaphoreContext.Value)
                        semaphore.Release();
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
            var threadId = ContextId;
            var sessionId = _cpSessionManager.GetSessionId(CPGroupId);

            var semaphore = VerifyNoLockOnThread(threadId, sessionId, false);

            if (sessionId == CPSessionManager.NoSessionId)
            {
                RemoveLocks(threadId, sessionId);
                throw new SynchronizationLockException();
            }

            try
            {
                Guid invocationId = Guid.NewGuid();
                HConsole.WriteLine(this, $"UnlockAsync->Thread{threadId}, Invocation:{invocationId}, Session:{sessionId}, Semaphore:{semaphore.GetHashCode()}");
                bool stillLockedByCurrentThread = await RequestUnlockAsync(sessionId, threadId, invocationId).CfAwait();

                if (stillLockedByCurrentThread)
                    _lockedThreadToSession[threadId] = sessionId;
                else
                {
                    RemoveLocks(threadId, sessionId);
                }

                _cpSessionManager.ReleaseSession(CPGroupId, sessionId);
            }
            catch (RemoteException e)
            {
                if (e is RemoteException { Error: RemoteError.SessionExpiredException })
                {
                    _cpSessionManager.InvalidateSession(CPGroupId, sessionId);
                    RemoveLocks(threadId, sessionId);
                }
                else if (e is RemoteException { Error: RemoteError.IllegalMonitorState })
                {
                    RemoveLocks(threadId, sessionId);
                }

                throw;
            }
            finally
            {
                if (LocalSemaphoreContext.Value)
                    semaphore.Release();
            }
        }

        private void RemoveLocks(long threadId, long sessionId)
        {
            HConsole.WriteLine(this, $"RemoveLocks->Thread{threadId}, Session:{sessionId}, CanRemove:{LocalSemaphoreContext.Value}");
            //Only holder of the semaphore can release it. 
            if (_contextSemaphore.TryGetValue(threadId, out var semaphore) && LocalSemaphoreContext.Value)
            {
                _lockedThreadToSession.TryRemove(threadId, out var _);
                HConsole.WriteLine(this, $"Remove lock, Thread:{AsyncContext.Current.Id}");
                semaphore.Release();                
                LocalSemaphoreContext.Value = false;
            }
        }
        #endregion

        /// <summary>
        /// Verifies there is no lock between ThreadId->SessionId. Otherwise throws.
        /// </summary>
        /// <param name="threadId"></param>
        /// <param name="sessionId"></param>
        /// <param name="releaseSession"></param>
        /// <exception cref="LockOwnershipLostException"></exception>
        private SemaphoreSlim VerifyNoLockOnThread(long threadId, long sessionId, bool releaseSession = true, bool force = false)
        {
            if (!_contextSemaphore.TryGetValue(threadId, out var semaphore))
            {
                semaphore = new SemaphoreSlim(1, 1);
                _contextSemaphore[threadId] = semaphore;
                HConsole.WriteLine(this, $"Verified by Thread:{ Environment.CurrentManagedThreadId }, Semaphore: {semaphore.GetHashCode() }, Count: { semaphore.CurrentCount}");
            }

            var isLocked = semaphore.Wait(0);
            //Flag it if current flow got the resource.
            LocalSemaphoreContext.Value = isLocked;

            HConsole.WriteLine(this, $"Context {AsyncContext.Current.Id} Semaphore: {semaphore.GetHashCode() } can take the lock -> {isLocked}");

            if ((_lockedThreadToSession.TryGetValue(threadId, out var lockedSessionId) && lockedSessionId != sessionId) || !isLocked)
            {
                RemoveLocks(threadId, sessionId);
                HConsole.WriteLine(this, $"Release session, Context {AsyncContext.Current.Id}, Thread { Environment.CurrentManagedThreadId }  { semaphore.GetHashCode() }, Count: {semaphore.CurrentCount}");
                if (releaseSession)
                    _cpSessionManager.ReleaseSession(CPGroupId, sessionId);

                throw new LockOwnershipLostException($"Current thread/context/async flow is not owner of the Lock[{Name}] because its Session[{lockedSessionId}] is closed by server!");
            }

            return semaphore;
        }

        /// <summary>
        /// Verifies there is no ThreadId -> SessionId map. Otherwise throws.
        /// </summary>
        /// <param name="threadId"></param>
        /// <exception cref="LockOwnershipLostException"></exception>
        private void VerifyNoLockedSessionExist(long threadId)
        {
            if (_lockedThreadToSession.TryRemove(threadId, out var lockedSessionId))
            {
                RemoveLocks(threadId, lockedSessionId);
                throw new LockOwnershipLostException($"Current thread/context is not owner of the Lock[{Name}] because its Session[{lockedSessionId}] is closed by server!");
            }
        }

        public async override ValueTask DestroyAsync()
        {
            if (Interlocked.CompareExchange(ref _destroyed, 1, 0) == 1) return;

            try
            {
                await RequestDestroyAsync().CfAwait();
            }
            finally
            {
                _lockedThreadToSession.Clear();

                foreach (var s in _contextSemaphore.Values)
                {
                    s.Dispose();
                }

                _contextSemaphore.Clear();
            }
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
