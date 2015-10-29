/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Client.Spi
{
    internal class SettableFuture<T> : IFuture<T>
    {
        private readonly object _lock = new object();
        private readonly TaskCompletionSource<T> _taskSource = new TaskCompletionSource<T>();
        private Exception _exception;
        private object _result;
        private int _isComplete;

        public Exception Exception
        {
            get
            {
                Wait();
                return _exception;
            }
            set
            {
                SetComplete();
                _exception = value;
                _taskSource.SetException(value);
                Notify();
            }
        }

        private void SetComplete()
        {
            if (Interlocked.CompareExchange(ref _isComplete, 1, 0) == 1)
            {
                throw new InvalidOperationException("The result is already set.");
            }
        }

        public T Result
        {
            get
            {
                Wait();
                if (_exception != null) throw _exception;
                return (T) _result;
            }
            set
            {
                SetComplete();
                _result = value;
                _taskSource.SetResult(value);
                Notify();
            }
        }

        public bool IsComplete
        {
            get { return _isComplete == 1; }
        }

        public Task<T> ToTask()
        {
            return _taskSource.Task;
        }

        public T GetResult(int miliseconds)
        {
            var result = true;
            Monitor.Enter(_lock);
            try
            {
                if (!IsComplete)
                {
                    result = Monitor.Wait(_lock, miliseconds);
                }
                if (result == false) throw new TimeoutException("Operation timed out.");
                if (_exception != null) throw _exception;
                return (T) _result;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        public bool Wait()
        {
            var result = true;
            Monitor.Enter(_lock);
            if (!IsComplete)
            {
                result = Monitor.Wait(_lock);
            }
            Monitor.Exit(_lock);
            return result;
        }

        public bool Wait(int miliseconds)
        {
            var result = true;
            Monitor.Enter(_lock);
            try
            {
                if (!IsComplete)
                {
                    result = Monitor.Wait(_lock, miliseconds);
                }
                return result;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        private void Notify()
        {
            Monitor.Enter(_lock);
            Monitor.PulseAll(_lock);
            Monitor.Exit(_lock);
        }
    }
}