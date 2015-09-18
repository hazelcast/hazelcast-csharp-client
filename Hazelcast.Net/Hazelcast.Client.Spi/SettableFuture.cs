using System;
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
        private volatile int _isComplete;

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
            var task = new Task<T>(() => Result);
            task.Start();
            return task;
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