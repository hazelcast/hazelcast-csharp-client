// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Client.Spi
{
    class Future
    {
        public const int NoLimit = -1;
    }

    interface IFuture<T>
    {
        void SetException(Exception ex);
        void SetResult(T value);
    }

    class SyncFuture<T> : IFuture<T>
        where T : class
    {
        /// <summary>
        /// Discriminates the state
        /// </summary>
        enum State : byte
        {
            NotCompleted = 0,
            Value = 1,
            Exception = 2
        }

        object _payload;
        State _state;

        void IFuture<T>.SetException(Exception ex) => Set(ex, State.Exception);

        void IFuture<T>.SetResult(T value) => Set(value, State.Value);

        void Set(object payload, State state)
        {
            lock (this)
            {
                _payload = payload;
                _state = state;
                Monitor.PulseAll(this);
            }
        }

        public T WaitAndGet(int timeoutMilliseconds = Future.NoLimit)
        {
            lock (this)
            {
                if (_state == State.NotCompleted)
                {
                    if (Monitor.Wait(this, timeoutMilliseconds) == false)
                    {
                        throw new TimeoutException();
                    }
                }
            }

            if (_state == State.Exception)
                throw (Exception)_payload;

            return (T)_payload;
        }
    }

    class AsyncFuture<T> : TaskCompletionSource<T>, IFuture<T>
    {
        AsyncFuture(object asyncState, TaskCreationOptions creationOptions)
            : base(asyncState, creationOptions)
        { }

        void IFuture<T>.SetException(Exception ex)
        {
            if (ex is TaskCanceledException || ex == null)
            {
                TrySetCanceled();
            }
            else
            {
                TrySetException(ex);
            }

            GC.KeepAlive(Task.Exception); // mark any exception as observed
            GC.SuppressFinalize(Task); // finalizer only exists for unobserved-exception purposes
        }

        void IFuture<T>.SetResult(T value)
        {
            TrySetResult(value);
        }

        public static IFuture<T> Create(out TaskCompletionSource<T> source, object asyncState)
        {
            var future = new AsyncFuture<T>(asyncState, TaskCreationOptions.RunContinuationsAsynchronously);
            source = future;
            return future;
        }
    }
}