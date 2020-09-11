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

#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Exceptions;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents a task timeout.
    /// </summary>
    internal class TaskTimeout : IDisposable
    {
        private readonly object _mutex = new object();
        private readonly Task _delay;
        private readonly Task _task;
#pragma warning disable CA2213 // Disposable fields should be disposed - it will!
        private CancellationTokenSource? _cancellation;
#pragma warning restore CA2213 // Disposable fields should be disposed

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskTimeout"/> class.
        /// </summary>
        /// <param name="timeoutMilliseconds">The timeout, in milliseconds.</param>
        public TaskTimeout(int timeoutMilliseconds)
        {
            if (timeoutMilliseconds < 0) throw new ArgumentOutOfRangeException(nameof(timeoutMilliseconds));

            _cancellation = new CancellationTokenSource();
            _delay = Task.Delay(timeoutMilliseconds, _cancellation.Token);
            _task = _delay.ContinueWith((t, state) =>
            {
                var timeout = (TaskTimeout) state;
                lock (timeout._mutex)
                {
                    timeout._cancellation!.Dispose();
                    timeout._cancellation = null;
                }
            }, this, default, TaskContinuationOptions.None, TaskScheduler.Current);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (_mutex) _cancellation?.Cancel();
        }

        /// <summary>
        /// Throws a <see cref="TaskTimeoutException"/>.
        /// </summary>
        /// <param name="task">The task that has timed out.</param>
        /// <param name="cancellation">An optional cancellation token source.</param>
        /// <param name="observeException">Whether to observe the exceptions of the task.</param>
        /// <remarks>
        /// <para>If <paramref name="cancellation"/> is provided, it will be cancelled in case of a timeout, and always disposed.</para>
        /// <para>If <paramref name="observeException"/> is <c>true</c>, exceptions thrown by <paramref name="task"/> will
        /// be observed, so the task can be forgotten without causing problems.</para>
        /// </remarks>
        public static void Throw(Task task, CancellationTokenSource? cancellation = null, bool observeException = false)
        {
            if (cancellation != null)
            {
                cancellation.Cancel();
                cancellation.Dispose();
            }

            if (observeException)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                task.ObserveException();
#pragma warning restore CS4014
            }

            throw new TaskTimeoutException(ExceptionMessages.Timeout, task);
        }

        /// <summary>
        /// Runs a task with a timeout.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="cancellation">An optional cancellation token source.</param>
        /// <param name="observeException">Whether to observe the exceptions of the task.</param>
        /// <returns>A task that will complete when the <paramref name="task"/> has completed.</returns>
        /// <exception cref="TaskTimeoutException">Occurs when the timeout expires before the <paramref name="task"/> has completed.</exception>
        /// <remarks>
        /// <para>If <paramref name="cancellation"/> is provided, it will be cancelled in case of a timeout, and always disposed.</para>
        /// <para>If <paramref name="observeException"/> is <c>true</c>, exceptions thrown by <paramref name="task"/> will
        /// be observed, so the task can be forgotten without causing problems.</para>
        /// </remarks>
        public async Task Run(Task task, CancellationTokenSource? cancellation = null, bool observeException = false)
        {
            await Task.WhenAny(task, _task).CAF();
            if (!task.IsCompletedSuccessfully() && _delay.IsCompleted)
                Throw(task, cancellation, observeException);
            await task.CAF();
        }

        /// <summary>
        /// Runs a task with a timeout.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="cancellation">An optional cancellation token source.</param>
        /// <param name="observeException">Whether to observe the exceptions of the task.</param>
        /// <returns>The result of the supplied <paramref name="task"/>.</returns>
        /// <exception cref="TaskTimeoutException">Occurs when the timeout expires before the <paramref name="task"/> has completed.</exception>
        /// <remarks>
        /// <para>If <paramref name="cancellation"/> is provided, it will be cancelled in case of a timeout, and always disposed.</para>
        /// <para>If <paramref name="observeException"/> is <c>true</c>, exceptions thrown by <paramref name="task"/> will
        /// be observed, so the task can be forgotten without causing problems.</para>
        /// </remarks>
        public async Task<TResult> Run<TResult>(Task<TResult> task, CancellationTokenSource? cancellation = null, bool observeException = false)
        {
            await Task.WhenAny(task, _task).CAF();
            if (!task.IsCompletedSuccessfully() && _delay.IsCompleted)
                Throw(task, cancellation, observeException);
            return await task.CAF();
        }
    }
}
