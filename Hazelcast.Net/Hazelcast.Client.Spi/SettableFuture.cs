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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Util;

#pragma warning disable CS1591
namespace Hazelcast.Client.Spi
{
    //WARNING: All exceptions should returned from the _taskSource otherwise UnobservedTaskException problem occur
    internal class SettableFuture<T> : IFuture<T>
    {
        private readonly object _lock = new object();
        private readonly TaskCompletionSource<T> _taskSource = new TaskCompletionSource<T>();

        public Exception Exception
        {
            get
            {
                Wait();
                return _taskSource.Task.IsFaulted ? _taskSource.Task.Exception.Flatten().InnerExceptions.First(): null;
            }
            set
            {
                Monitor.Enter(_lock);
                _taskSource.SetException(value);
                //is there a better way to handle TaskCompletionSource's unobserved exception???
                _taskSource.Task.IgnoreExceptions();
                Notify();
                Monitor.Exit(_lock);
            }
        }

        public T Result
        {
            get
            {
                Wait();
                return _GetResult();
            }
            set
            {
                Monitor.Enter(_lock);
                _taskSource.SetResult(value);
                Notify();
                Monitor.Exit(_lock);
            }
        }

        public bool IsComplete
        {
            get { return _taskSource.Task.IsCompleted; }
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
                if (result == false)
                {
                    _taskSource.SetException(new TimeoutException("Operation timed out."));
                }
                return _GetResult();
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        private T _GetResult()
        {
            if (_taskSource.Task.IsFaulted)
            {
                throw _taskSource.Task.Exception.Flatten().InnerExceptions.First();
            }
            return _taskSource.Task.Result;
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
            Monitor.PulseAll(_lock);
        }
    }
}