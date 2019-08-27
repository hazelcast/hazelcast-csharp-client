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

        protected static readonly Exception DefaultException = new Exception("An error occured");
        protected static readonly object NullObject = new object();
    }

    interface IFuture<T>
    {
        void SetException(Exception ex);
        void SetResult(T value);
    }

    class SyncFuture<T> : Future, IFuture<T>
        where T : class
    {
        object _state;

        void IFuture<T>.SetException(Exception ex) => Set(ex ?? DefaultException);

        void IFuture<T>.SetResult(T value) => Set(value ?? NullObject);

        void Set(object payload)
        {
            lock (this)
            {
                _state = payload;
                Monitor.PulseAll(this);
            }
        }

        public T WaitAndGet(int timeoutMilliseconds = NoLimit)
        {
            lock (this)
            {
                if (_state == null)
                {
                    if (Monitor.Wait(this, timeoutMilliseconds) == false)
                    {
                        throw new TimeoutException();
                    }
                }
            }

            var state = _state;

            if (state is Exception ex)
                throw ex;

            if (ReferenceEquals(state, NullObject))
            {
                return null;
            }

            // potentially, return Unsafe.As<T>(state);
            return (T)state;
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

        public static IFuture<T> Create(out TaskCompletionSource<T> source, object asyncState = null)
        {
            var future = new AsyncFuture<T>(asyncState, TaskCreationOptions.RunContinuationsAsynchronously);
            source = future;
            return future;
        }
    }
}