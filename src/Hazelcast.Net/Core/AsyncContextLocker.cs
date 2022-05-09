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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Core
{
    internal class AsyncContextLocker : IDisposable
    {
        private readonly Dictionary<long, ContextLock> _contextLocks = new Dictionary<long, ContextLock>();
        private int _disposed;

        public async Task<IDisposable> LockAsync(long contextId)
        {
            ContextLock contextLock;

            // there is one unique ContextLock instance (in _contextLocks) per contextId, and
            // then each LockAsync caller gets one different instance of ContextLocked, and
            // each one increments the reference count, and when each ContextLocked is disposed
            // it releases the lock = next one can have it + decrements the reference count,
            // and when the reference count is zero we know that there are no more tasks
            // waiting and we remove the ContextLock. so it works like this:
            // - first call creates a ContextLock (ref count == 1), locks, returns
            // - second call gets the ContextLock (ref count == 2), waits for lock
            // - first call completes = releases lock, releases ContextLock (ref count == 1),
            //   keep ContextLock since ref count > 0 and it's referenced by second call
            // - second call locks, returns
            // - second call completes = releases lock, releases ContextLock (ref count == 0),
            //   removes ContextLock since ref count == 0 and it's not referenced anymore

            // atomically get from dictionary & increment the reference count
            lock (_contextLocks)
            {
                if (!_contextLocks.TryGetValue(contextId, out contextLock))
                    contextLock = _contextLocks[contextId] = new ContextLock(contextId);

                contextLock.ReferenceCount++;
            }

            // may throw an ObjectDisposedException if the whole AsyncContextLocker is
            // disposed while some tasks are still trying to obtain a lock, and that is
            // normal behavior
            await contextLock.WaitAsync().CfAwait();
            return new ContextLocked(this, contextLock);
        }

        private void Release(ContextLock contextLock)
        {
            // release the lock
            contextLock.Release();

            // atomically decrement the reference count & remove from dictionary
            lock (_contextLocks)
            {
                contextLock.ReferenceCount--;
                if (contextLock.ReferenceCount == 0)
                {
                    _contextLocks.Remove(contextLock.ContextId);
                    contextLock.Dispose(); // no references = no waiting tasks
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed.InterlockedZeroToOne()) return;
            foreach (var contextLock in _contextLocks.Values)
                contextLock.Dispose();
            _contextLocks.Clear();
        }

        private class ContextLock : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            public ContextLock(long contextId)
            {
                _semaphore = new SemaphoreSlim(1, 1);
                ContextId = contextId;
            }

            public long ContextId { get; }

            public int ReferenceCount { get; set; }

            public Task WaitAsync()
            {
                return _semaphore.WaitAsync();
            }

            public void Release()
            {
                _semaphore.Release();
            }

            public void Dispose()
            {
                _semaphore.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        private class ContextLocked : IDisposable
        {
            private readonly AsyncContextLocker _locker;
            private readonly ContextLock _contextLock;

            public ContextLocked(AsyncContextLocker locker, ContextLock contextLock)
            {
                _locker = locker;
                _contextLock = contextLock;
            }

            public void Dispose()
            {
                _locker.Release(_contextLock);
                GC.SuppressFinalize(this);
            }
        }
    }
}
