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
        private readonly CPSubsystemSession _subsystemSession;
        private readonly CPGroupId _groupId;
        private int _destroyed;

        public const long InvalidFence = 0;
        ICPGroupId ICPDistributedObject.GroupId => _groupId;
        long IFencedLock.InvalidFence => InvalidFence;

        public FencedLock(string name, CPGroupId groupId, Cluster cluster, CPSubsystemSession subsystemSession) : base(ServiceNames.FencedLock, name, groupId, cluster)
        {
            _subsystemSession = subsystemSession;
            _groupId = groupId;
            HConsole.Configure(x => x.Configure<FencedLock>().SetIndent(2).SetPrefix("FENCEDLOCK"));
        }

        #region IFencedLock Methods
        /// <inheritdoc/>  
        public async Task<long> GetFenceAsync()
        {
            var threadId = ContextId;
            var sessionId = _subsystemSession.GetSessionId(CPGroupId);
            HConsole.WriteLine(this, $"Session:{sessionId} ThreadId:{threadId} -> GetFence");
            VerifyNoLockOnThread(threadId, sessionId, false);

            if (sessionId == CPSubsystemSession.NoSessionId)
            {
                _lockedThreadToSession.TryRemove(threadId, out var _);
                throw new SynchronizationLockException();
            }

            var ownership = await RequestLockOwnershipStateAsync().CfAwait();

            if (ownership.LockedBy(threadId, sessionId))
            {
                _lockedThreadToSession[threadId] = sessionId;
                return ownership.Fence;
            }

            VerifyNoLockedSessionExist(threadId);
            throw new SynchronizationLockException();
        }

        /// <inheritdoc/>  
        public async Task<int> GetLockCountAsync()
        {
            var threadId = ContextId;
            var sessionId = _subsystemSession.GetSessionId(CPGroupId);

            VerifyNoLockOnThread(threadId, sessionId, false);

            var ownership = await RequestLockOwnershipStateAsync().CfAwait();

            if (ownership.LockedBy(threadId, sessionId))
                _lockedThreadToSession[threadId] = sessionId;
            else
                VerifyNoLockedSessionExist(threadId);

            return ownership.LockCount;
        }

        /// <inheritdoc/>  
        public async Task<bool> IsLockedAsync()
        {
            var threadId = ContextId;
            var sessionId = _subsystemSession.GetSessionId(CPGroupId);

            VerifyNoLockOnThread(threadId, sessionId, false);

            var ownership = await RequestLockOwnershipStateAsync().CfAwait();

            if (ownership.LockedBy(threadId, sessionId))
            {
                _lockedThreadToSession[threadId] = sessionId;
                return true;
            }

            VerifyNoLockedSessionExist(threadId);

            return ownership.Locked;
        }

        public async Task<bool> IsLockedByCurrentContext()
        {
            var threadId = ContextId;
            var sessionId = _subsystemSession.GetSessionId(CPGroupId);

            VerifyNoLockOnThread(threadId, sessionId, false);

            var ownership = await RequestLockOwnershipStateAsync().CfAwait();

            var lockedByCurrent = ownership.LockedBy(threadId, sessionId);

            if (lockedByCurrent)
                _lockedThreadToSession[threadId] = sessionId;
            else
                VerifyNoLockedSessionExist(threadId);

            return lockedByCurrent;
        }

        /// <inheritdoc/>        
        public async Task<long> LockAndGetFenceAsync()
        {
            var threadId = ContextId;
            var invocationId = Guid.NewGuid();// required by server, to make the call idempotetent?

            while (true)
            {
                var sessionId = await _subsystemSession.AcquireSessionAsync(CPGroupId).CfAwait();

                VerifyNoLockOnThread(threadId, sessionId, true);

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
                        _subsystemSession.InvalidateSession(CPGroupId, sessionId);
                        VerifyNoLockedSessionExist(threadId);
                    }
                    else if (e is RemoteException { Error: RemoteError.DistributedObjectDestroyed })
                    {
                        throw new DistributedObjectDestroyedException(e);
                    }
                    else if (e is RemoteException { Error: RemoteError.WaitKeyCancelledException })
                    {
                        _subsystemSession.ReleaseSession(CPGroupId, sessionId);
                        throw new SynchronizationLockException($"Lock[{Name}] not acquired because the lock call "
                            + "on the CP group is cancelled, possibly because of another indeterminate call from the same context.");
                    }
                }
                catch
                {
                    _subsystemSession.ReleaseSession(CPGroupId, sessionId);
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
            var threadId = ContextId;
            Guid invocationId = Guid.NewGuid();
            var timeoutMilliseconds = (long)Math.Round(Math.Max(0, timeout.TotalMilliseconds));

            while (true)
            {
                var start = Clock.Milliseconds;
                var sessionId = await _subsystemSession.AcquireSessionAsync(CPGroupId).CfAwait();
                HConsole.WriteLine(this, $"SessionId: {sessionId} ThreadId:{threadId} InvocationId:{invocationId} -> TryLockAndGetFenceAsync");
                VerifyNoLockOnThread(threadId, sessionId);

                try
                {
                    long fence = await RequestTryLockAsync(sessionId, threadId, invocationId, timeoutMilliseconds).CfAwait();

                    if (fence != InvalidFence)
                        _lockedThreadToSession[threadId] = sessionId;
                    else
                        _subsystemSession.ReleaseSession(CPGroupId, sessionId);

                    return fence;
                }
                catch (RemoteException e)
                {
                    if (e is RemoteException { Error: RemoteError.SessionExpiredException })
                    {
                        _subsystemSession.InvalidateSession(CPGroupId, sessionId);
                        VerifyNoLockedSessionExist(threadId);

                        long duration = Clock.Milliseconds - start;

                        if (duration <= 0)
                            return InvalidFence;

                    }
                    else if (e is RemoteException { Error: RemoteError.DistributedObjectDestroyed })
                    {
                        throw new DistributedObjectDestroyedException(e);
                    }
                    else if (e is RemoteException { Error: RemoteError.WaitKeyCancelledException })
                    {
                        _subsystemSession.ReleaseSession(CPGroupId, sessionId);
                        return InvalidFence;
                    }
                }
                catch
                {
                    _subsystemSession.ReleaseSession(CPGroupId, sessionId);
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
            var threadId = ContextId;
            var sessionId = _subsystemSession.GetSessionId(CPGroupId);

            VerifyNoLockOnThread(threadId, sessionId, false);

            if (sessionId == CPSubsystemSession.NoSessionId)
            {
                _lockedThreadToSession.TryRemove(threadId, out var _);
                throw new SynchronizationLockException();
            }

            try
            {
                Guid invocationId = Guid.NewGuid();
                bool stillLockedByCurrentThread = await RequestUnlockAsync(sessionId, threadId, invocationId).CfAwait();

                if (stillLockedByCurrentThread)
                    _lockedThreadToSession[threadId] = sessionId;
                else
                    _lockedThreadToSession.TryRemove(threadId, out var _);

                _subsystemSession.ReleaseSession(CPGroupId, sessionId);
            }
            catch (RemoteException e)
            {
                if (e is RemoteException { Error: RemoteError.SessionExpiredException })
                {
                    _subsystemSession.InvalidateSession(CPGroupId, sessionId);
                    _lockedThreadToSession.TryRemove(threadId, out var _);
                    throw new SessionExpiredException(e);
                }
                else if (e is RemoteException { Error: RemoteError.IllegalMonitorState })
                {
                    _lockedThreadToSession.TryRemove(threadId, out var _);
                    throw new SynchronizationLockException("IllegalMonitorState exception is thrown by server. Are you trying to unlock without holding?", e);
                }
            }
            catch (Exception)
            {
                throw;
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
        private void VerifyNoLockOnThread(long threadId, long sessionId, bool releaseSession = true)
        {
            if (_lockedThreadToSession.TryGetValue(threadId, out var lockedSessionId) && lockedSessionId != sessionId)
            {
                _lockedThreadToSession.TryRemove(threadId, out var _);

                if (releaseSession)
                    _subsystemSession.ReleaseSession(CPGroupId, sessionId);

                throw new LockOwnershipLostException($"Current thread/context is not owner of the Lock[{Name}] because its Session[{lockedSessionId}] is closed by server!");
            }
        }

        /// <summary>
        /// Verifies there is no ThreadId -> SessionId map. Otherwise throws.
        /// </summary>
        /// <param name="threadId"></param>
        /// <exception cref="LockOwnershipLostException"></exception>
        private void VerifyNoLockedSessionExist(long threadId)
        {
            if (_lockedThreadToSession.TryRemove(threadId, out var lockedSessionId) && lockedSessionId != default)
            {
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
