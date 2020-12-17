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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides a lightweight asynchronous queue for multiple providers,
    /// and one single sequential consumer.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    internal sealed class AsyncQueue<T> : IAsyncEnumerable<T>, IAsyncEnumerator<T>
    {
        private readonly ConcurrentQueue<T> _items = new ConcurrentQueue<T>();
        private readonly object _lock = new object();
        private TaskCompletionSource<bool> _waiting;
        private T _current;
        private bool _completed;

        /// <summary>
        /// Drains the queue.
        /// </summary>
        /// <remarks>
        /// <para>Removes all the items that are in the queue (while blocking writes)
        /// and have not yet been waited for with <see cref="WaitAsync"/>. If an item
        /// has been waited for, but not yet retrieved with <see cref="Read"/>, it is
        /// not drained.</para>
        /// </remarks>
        public void Drain()
        {
            lock (_lock)
            {
                while (_items.TryDequeue(out _))
                { }
            }
        }

        /// <summary>
        /// Tries to write an item to the queue.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if the item was written; otherwise (the queue is complete) <c>false</c>.</returns>
        public bool TryWrite(T item)
        {
            TaskCompletionSource<bool> waiting = null;

            lock (_lock)
            {
                if (_completed) return false;

                if (_waiting == null)
                {
                    _items.Enqueue(item);
                }
                else
                {
                    _current = item;
                    waiting = _waiting;
                    _waiting = null;
                }
            }

            waiting?.SetResult(true);
            return true;
        }

        /// <summary>
        /// Completes the queue.
        /// </summary>
        /// <remarks>
        /// <para>The queue keeps providing its items for reading, but it is not possible to write items anymore.</para>
        /// </remarks>
        public void Complete()
        {
            TaskCompletionSource<bool> waiting;

            lock (_lock)
            {
                _completed = true;
                waiting = _waiting;
                _waiting = null;
            }

            waiting?.SetResult(false);
        }

        // there is going to be only 1 reader pumping items out and processing them
        // sequentially, so if we enter this method and the queue is empty and we
        // return a task, we are not going to enter this method again until that task
        // has completed - in other words, there can only be one _waiting at a time

        /// <summary>
        /// Waits for an item to become available.
        /// </summary>
        /// <returns><c>true</c> if an item is available; otherwise (the queue is complete) <c>false</c>.</returns>
        public ValueTask<bool> WaitAsync()
        {
            if (_items.TryDequeue(out _current))
                return new ValueTask<bool>(true);

            lock (_lock)
            {
                if (_items.TryDequeue(out _current))
                    return new ValueTask<bool>(true);

                if (_completed)
                    return new ValueTask<bool>(false);

                _waiting = new TaskCompletionSource<bool>();
                return new ValueTask<bool>(_waiting.Task);
            }
        }

        /// <summary>
        /// Reads the available item.
        /// </summary>
        /// <returns>The available item, or <c>default(T)</c> if no item is available.</returns>
        public T Read()
        {
            return _current;
        }

        // ---- IAsyncEnumerable ----

        /// <inheritdoc />
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => this;

        // ---- IAsyncEnumerator ----

        /// <inheritdoc />
        public ValueTask<bool> MoveNextAsync() => WaitAsync();

        /// <inheritdoc />
        public T Current => _current;

        /// <inheritdoc />
        public ValueTask DisposeAsync() => default;
    }
}
