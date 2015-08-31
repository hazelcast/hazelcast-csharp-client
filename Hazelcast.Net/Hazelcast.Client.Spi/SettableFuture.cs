using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Client.Spi
{
    internal class SettableFuture<T> : IFuture<T>
    {
        private readonly object _lock = new object();
        private readonly TaskCompletionSource<T> _taskSource = new TaskCompletionSource<T>();
        private volatile Exception _exception;
        private volatile object _result;

        public Exception Exception
        {
            get
            {
                Wait();
                return _exception;
            }
            set
            {
                //TODO: make sure result is not already set 
                _exception = value;
                _taskSource.SetException(value);
                Notify();
            }
        }

        public T Result
        {
            get
            {
                Wait();
                return (T) _result;
            }
            set
            {
                //TODO: make sure result is not already set 
                _result = value;
                _taskSource.SetResult(value);
                Notify();
            }
        }

        public Task<T> ToTask()
        {
            var task = new Task<T>(() => Result);
            task.Start();
            return task;
        }

        public bool Wait()
        {
            var result = true;
            Monitor.Enter(_lock);
            if (_result == null && _exception == null)
            {
                result = Monitor.Wait(_lock);
            }
            Monitor.Exit(_lock);
            return result;
        }

        public T GetResult(int miliseconds)
        {
            var result = true;
            Monitor.Enter(_lock);
            try
            {
                if (_result == null && _exception == null)
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

        public bool Wait(int miliseconds)
        {
            var result = true;
            Monitor.Enter(_lock);
            try
            {
                if (_result == null && _exception == null)
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