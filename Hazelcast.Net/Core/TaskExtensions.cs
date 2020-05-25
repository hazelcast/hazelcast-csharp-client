using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension method to the <see cref="Task"/> and <see cref="Task{T}"/> classes.
    /// </summary>
    public static class TaskExtensions
    {
        public static async Task WithTimeout(this Task task, int timeoutMilliseconds, CancellationTokenSource taskCancel = null)
        {
            using var timeoutCancel = new CancellationTokenSource();

            var timeoutTask = Task.Delay(timeoutMilliseconds, timeoutCancel.Token);

            await Task.WhenAny(task, timeoutTask);

            if (task.IsCompleted)
            {
                // timeoutTask is never awaited,
                // results & exceptions will be ignored
                timeoutCancel.Cancel();

                // https://stackoverflow.com/questions/24623120/await-on-a-completed-task-same-as-task-result
                // return task.Result;
                await task;
            }

            if (taskCancel == null)
                throw new TimeoutException("Operation timed out.");

            // cancel the task
            taskCancel.Cancel(); // FIXME also in other methods
            try
            {
                await task;
            }
            catch (OperationCanceledException) { /* expected */ } // FIXME but we want to know where?
            catch (Exception e)
            {
                throw new TimeoutException("Operation timed out, see inner exception.", e);
            }
        }

        public static async Task<T> WithTimeout<T>(this Task<T> task, int timeoutMilliseconds, CancellationTokenSource taskCancel = null)
        {
            using var timeoutCancel = new CancellationTokenSource();

            var timeoutTask = Task.Delay(timeoutMilliseconds, timeoutCancel.Token);

            await Task.WhenAny(task, timeoutTask);

            if (task.IsCompleted)
            {
                // timeoutTask is never awaited,
                // results & exceptions will be ignored
                timeoutCancel.Cancel();

                // https://stackoverflow.com/questions/24623120/await-on-a-completed-task-same-as-task-result
                // return task.Result;
                return await task;
            }

            // cancel the task
            taskCancel?.Cancel();

            throw new TimeoutException();
        }

        public static async ValueTask<T> WithTimeout<T>(this ValueTask<T> task, int timeoutMilliseconds, CancellationTokenSource taskCancel = null)
        {
            using var timeoutCancel = new CancellationTokenSource();

            var timeoutTask = Task.Delay(timeoutMilliseconds, timeoutCancel.Token);

            await Task.WhenAny(task.AsTask(), timeoutTask);

            if (task.IsCompleted)
            {
                // timeoutTask is never awaited,
                // results & exceptions will be ignored
                timeoutCancel.Cancel();

                // https://stackoverflow.com/questions/24623120/await-on-a-completed-task-same-as-task-result
                // return task.Result;
                return await task;
            }

            // cancel the task
            taskCancel?.Cancel();

            throw new TimeoutException();
        }

        public static Task OrTimeout(this Task task, TimeoutCancellationTokenSource cts)
        {
            return task.ContinueWith(x =>
            {
                var notTimedOut = !x.IsCanceled || !cts.HasTimedOut;
                cts.Dispose();

                if (notTimedOut) return x;

                try
                {
                    // this is the way to get the original exception with correct stack trace
                    task.GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    throw new TimeoutException("Operation timed out (see inner exception).", e);
                }

                throw new TimeoutException("Operation timed out");
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }

        public static Task<T> OrTimeout<T>(this Task<T> task, TimeoutCancellationTokenSource cts)
        {
            return task.ContinueWith(x =>
            {
                var notTimedOut = !x.IsCanceled || !cts.HasTimedOut;
                cts.Dispose();

                if (notTimedOut) return x;

                try
                {
                    // this is the way to get the original exception with correct stack trace
                    task.GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    throw new TimeoutException("Operation timed out (see inner exception).", e);
                }

                throw new TimeoutException("Operation timed out");
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }

        // TODO: consider removing this code
        /*
        public static Task ThenDispose(this Task task, IDisposable disposable)
        {
            return task.ContinueWith(x =>
            {
                disposable.Dispose();
                return x;
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }

        public static Task ThenDispose(this Task task, params IDisposable[] disposables)
        {
            return task.ContinueWith(x =>
            {
                foreach (var disposable in disposables)
                    disposable.Dispose();
                return x;
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }

        public static Task<T> ThenDispose<T>(this Task<T> task, IDisposable disposable)
        {
            return task.ContinueWith(x =>
            {
                disposable.Dispose();
                return x;
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }

        public static Task<T> ThenDispose<T>(this Task<T> task, params IDisposable[] disposables)
        {
            return task.ContinueWith(x =>
            {
                foreach (var disposable in disposables)
                    disposable.Dispose();
                return x;
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }
        */
    }
}
