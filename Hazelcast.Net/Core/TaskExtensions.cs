using System;
using System.Collections.Generic;
using System.Text;
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
            using (var timeoutCancel = new CancellationTokenSource())
            {
                var timeoutTask = Task.Delay(timeoutMilliseconds, timeoutCancel.Token);

                await Task.WhenAny(task, timeoutTask);

                if (task.IsCompleted)
                {
                    // timeoutTask is never awaited,
                    // results & exceptions will be ignored
                    timeoutCancel.Cancel();
                }

                // cancel the task
                taskCancel?.Cancel();

                throw new TimeoutException();
            }
        }

        public static async Task<T> WithTimeout<T>(this Task<T> task, int timeoutMilliseconds, CancellationTokenSource taskCancel = null)
        {
            using (var timeoutCancel = new CancellationTokenSource())
            {
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
        }

        public static async ValueTask<T> WithTimeout<T>(this ValueTask<T> task, int timeoutMilliseconds, CancellationTokenSource taskCancel = null)
        {
            using (var timeoutCancel = new CancellationTokenSource())
            {
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
        }
    }
}
