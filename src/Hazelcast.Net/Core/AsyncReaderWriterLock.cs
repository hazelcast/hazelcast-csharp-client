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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Core;

// heavily inspired from
// https://github.com/StephenCleary/AsyncEx/blob/master/src/Nito.AsyncEx.Coordination/AsyncReaderWriterLock.cs
//
// this is a simplified version where acquiring locks cannot be canceled

internal sealed class AsyncReaderWriterLock : IAsyncDisposable
{
    private readonly Queue<TaskCompletionSource<IDisposable>> _writerQueue = new();
    private readonly Queue<TaskCompletionSource<IDisposable>> _readerQueue = new();
    private readonly object _mutex = new();

    // number of reader locks held; -1 if a writer lock is held; 0 if no locks are held.
    private int _locksHeld;

    // whether the lock has been disposed
    private bool _disposed;

    public ValueTask DisposeAsync()
    {
        lock (_mutex)
        {
            if (_disposed) return default;
            _disposed = true;

            // cancel every pending lock requests
            while (_writerQueue.Count > 0) _writerQueue.Dequeue().TrySetCanceled();
            while (_readerQueue.Count > 0) _readerQueue.Dequeue().TrySetCanceled();

            // as for the locks currently held... 
            // forget about them - the user should know better
            return default;
        }
    }

    #region ReadLock

    private Task<IDisposable> RequestReadLockAsync()
    {
        lock (_mutex)
        {
            if (_disposed) return Task.FromCanceled<IDisposable>(CancellationToken.None);

            // if the lock is available and there are no pending writers, take it immediately
            if (_locksHeld >= 0 && _writerQueue.Count == 0)
            {
                ++_locksHeld;
                return Task.FromResult<IDisposable>(new ReaderHolder(this));
            }
            else
            {
                // otherwise, enqueue a task that waits for the lock to become available or cancellation
                var completion = new TaskCompletionSource<IDisposable>(TaskCreationOptions.RunContinuationsAsynchronously);
                _readerQueue.Enqueue(completion);
                return completion.Task;
            }
        }
    }

    /// <summary>
    /// Asynchronously acquires the lock as a reader. Returns a disposable that releases the lock when disposed.
    /// </summary>
    /// <returns>A disposable that releases the lock when disposed.</returns>
    public Task<IDisposable> ReadLockAsync()
    {
        return RequestReadLockAsync();
    }

    /// <summary>
    /// Synchronously acquires the lock as a reader. Returns a disposable that releases the lock when disposed. This method may block the calling thread.
    /// </summary>
    /// <returns>A disposable that releases the lock when disposed.</returns>
    public IDisposable ReadLock()
    {
        return RequestReadLockAsync().GetAwaiter().GetResult();
    }

    #endregion

    #region WriteLock

    private Task<IDisposable> RequestWriteLockAsync()
    {
        lock (_mutex)
        {
            if (_disposed) return Task.FromCanceled<IDisposable>(CancellationToken.None);

            // if the lock is available, take it immediately
            if (_locksHeld == 0)
            {
                _locksHeld = -1;
                return Task.FromResult<IDisposable>(new WriterHolder(this));
            }
            else
            {
                // otherwise, enqueue a task that waits for the lock to become available or cancellation
                var completion = new TaskCompletionSource<IDisposable>(TaskCreationOptions.RunContinuationsAsynchronously);
                _writerQueue.Enqueue(completion);
                return completion.Task;
            }
        }
    }

    /// <summary>
    /// Asynchronously acquires the lock as a writer. Returns a disposable that releases the lock when disposed.
    /// </summary>
    /// <returns>A disposable that releases the lock when disposed.</returns>
    public Task<IDisposable> WriteLockAsync()
    {
        return RequestWriteLockAsync();
    }

    /// <summary>
    /// Asynchronously acquires the lock as a writer. Returns a disposable that releases the lock when disposed. This method may block the calling thread.
    /// </summary>
    /// <returns>A disposable that releases the lock when disposed.</returns>
    public IDisposable WriteLock()
    {
        return RequestWriteLockAsync().GetAwaiter().GetResult();
    }

    #endregion

    #region Release

    /// <summary>
    /// Grants lock(s) to waiting tasks. This method assumes the sync lock is already held.
    /// </summary>
    private void ReleaseWaiters()
    {
        if (_disposed)
            return;

        if (_locksHeld == -1)
            return;

        // if we have pending writers...
        if (_writerQueue.Count != 0)
        {
            // and no current readers...
            if (_locksHeld == 0)
            {
                // then grant lock to the next writer
                _locksHeld = -1;
                _writerQueue.Dequeue().SetResult(new WriterHolder(this));
            }
        }
        else
        {
            // we have no pending writer, grant lock to pending readers, if any
            while (_readerQueue.Count > 0)
            {
                _readerQueue.Dequeue().SetResult(new ReaderHolder(this));
                ++_locksHeld;
            }
        }
    }

    /// <summary>
    /// Releases the lock as a reader.
    /// </summary>
    internal void ReleaseReadLock()
    {
        lock (_mutex)
        {
            --_locksHeld;
            ReleaseWaiters();
        }
    }

    /// <summary>
    /// Releases the lock as a writer.
    /// </summary>
    internal void ReleaseWriteLock()
    {
        lock (_mutex)
        {
            _locksHeld = 0;
            ReleaseWaiters();
        }
    }

    #endregion

    private abstract class Holder<T> : IDisposable
    {
        private readonly T _state;

        protected Holder(T state)
        {
            _state = state;
        }

        public void Dispose()
        {
            Dispose(_state);
        }

        public abstract void Dispose(T state);
    }

    private sealed class ReaderHolder : Holder<AsyncReaderWriterLock>
    {
        public ReaderHolder(AsyncReaderWriterLock state) : base(state)
        { }

        public override void Dispose(AsyncReaderWriterLock state)
        {
            state.ReleaseReadLock();
        }
    }

    private sealed class WriterHolder : Holder<AsyncReaderWriterLock>
    {
        public WriterHolder(AsyncReaderWriterLock state) : base(state)
        { }

        public override void Dispose(AsyncReaderWriterLock state)
        {
            state.ReleaseWriteLock();
        }
    }
}