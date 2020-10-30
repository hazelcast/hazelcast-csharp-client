﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Exceptions;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides utility methods for running <see cref="Task"/>.
    /// </summary>
    public static class TaskEx
    {
        // TODO: consider having overloads for arguments to avoid capturing
        // see ArgsVsCaptureAndStateMachine - it would be more efficient but requires a lot of code

        /// <summary>
        /// Starts an asynchronous task with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="function">A function starting and returning an asynchronous task.</param>
        /// <returns>The asynchronous task that was started.</returns>
        /// <remarks>
        /// <para>This is equivalent to doing <c>return function();</c> except that the task starts with a new <see cref="AsyncContext"/></para>
        /// </remarks>
        public static Task RunWithNewContext(Func<Task> function)
            => AsyncContext.WithNewContextInternal(function);

        /// <summary>
        /// Starts an asynchronous task with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="function">A function starting and returning an asynchronous task.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The asynchronous task that was started.</returns>
        /// <remarks>
        /// <para>This is equivalent to doing <c>return function(cancellationToken);</c> except that the task starts with a new <see cref="AsyncContext"/></para>
        /// </remarks>
        public static Task RunWithNewContext(Func<CancellationToken, Task> function, CancellationToken cancellationToken)
            => AsyncContext.WithNewContextInternal(function, cancellationToken);

        /// <summary>
        /// Starts an asynchronous task with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="function">A function starting and returning an asynchronous task.</param>
        /// <returns>The asynchronous task that was started.</returns>
        /// <remarks>
        /// <para>This is equivalent to doing <c>return function();</c> except that the task starts with a new <see cref="AsyncContext"/></para>
        /// </remarks>
        public static Task<TResult> RunWithNewContext<TResult>(Func<Task<TResult>> function)
            => AsyncContext.WithNewContextInternal(function);

        /// <summary>
        /// Starts an asynchronous task with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="function">A function starting and returning an asynchronous task.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The asynchronous task that was started.</returns>
        /// <remarks>
        /// <para>This is equivalent to doing <c>return function(cancellationToken);</c> except that the task starts with a new <see cref="AsyncContext"/></para>
        /// </remarks>
        public static Task<TResult> RunWithNewContext<TResult>(Func<CancellationToken, Task<TResult>> function, CancellationToken cancellationToken)
            => AsyncContext.WithNewContextInternal(function, cancellationToken);

        /// <summary>
        /// Awaits a task that is expected to be canceled.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>A task that will complete when the <paramref name="task"/> has completed.</returns>
        /// <remarks>
        /// <para>If the <paramref name="task"/> throws a <see cref="OperationCanceledException"/>, that exception is swallowed,
        /// as the task is expected to have been canceled. Any other exception is rethrown.</para>
        /// </remarks>
        public static async ValueTask AwaitCanceled(Task task)
        {
            if (task == null) return;
            try
            {
                await task.CAF();
            }
            catch (OperationCanceledException) { /* expected */ }
        }

        public static Task RunWithTimeout(Func<CancellationToken, Task> taskAction, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var timeoutMs = timeout.RoundedMilliseconds(0, -1).ClampInt32();
            return RunWithTimeout(taskAction, timeoutMs, cancellationToken);
        }

        public static async Task RunWithTimeout(Func<CancellationToken, Task> taskAction, int timeoutMilliseconds, CancellationToken cancellationToken = default)
        {
            if (taskAction == null) throw new ArgumentNullException(nameof(taskAction));

            if (timeoutMilliseconds < 0) // infinite
            {
                await taskAction(cancellationToken).CAF();
                return;
            }

            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var task = taskAction(cancellation.Token);
            using var delayCancel = new CancellationTokenSource();
            var delay = Task.Delay(timeoutMilliseconds, delayCancel.Token);

            await Task.WhenAny(task, delay).CAF();

            // if the delay is not completed, cancel it & observe the corresponding exception
            if (!delay.IsCompleted)
            {
                delayCancel.Cancel();
                _ = delay.ObserveException();
            }

            // if the task is completed, return
            if (task.IsCompleted)
            {
                await task.CAF(); // might have thrown
                return;
            }

            // else, this is a timeout
            // signal the task & observe any exception it may throw
            cancellation.Cancel();
            _ = task.ObserveException(); // should we do this?

            throw new TaskTimeoutException(ExceptionMessages.Timeout, task);
        }

        public static Task RunWithTimeout<TArg>(Func<TArg, CancellationToken, Task> taskAction, TArg arg, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var timeoutMs = timeout.RoundedMilliseconds(0, -1).ClampInt32();
            return RunWithTimeout(taskAction, arg, timeoutMs, cancellationToken);
        }

        public static async Task RunWithTimeout<TArg>(Func<TArg, CancellationToken, Task> taskAction, TArg arg, int timeoutMilliseconds, CancellationToken cancellationToken = default)
        {
            if (taskAction == null) throw new ArgumentNullException(nameof(taskAction));

            if (timeoutMilliseconds < 0) // infinite
            {
                await taskAction(arg, cancellationToken).CAF();
                return;
            }

            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var task = taskAction(arg, cancellation.Token);
            using var delayCancel = new CancellationTokenSource();
            var delay = Task.Delay(timeoutMilliseconds, delayCancel.Token);

            await Task.WhenAny(task, delay).CAF();

            // if the delay is not completed, cancel it & observe the corresponding exception
            if (!delay.IsCompleted)
            {
                delayCancel.Cancel();
                _ = delay.ObserveException();
            }

            // if the task is completed, return
            if (task.IsCompleted)
            {
                await task.CAF(); // might have thrown
                return;
            }

            // else, this is a timeout
            // signal the task & observe any exception it may throw
            cancellation.Cancel();
            _ = task.ObserveException(); // should we do this?

            throw new TaskTimeoutException(ExceptionMessages.Timeout, task);
        }

        public static Task<TResult> RunWithTimeout<TResult>(Func<CancellationToken, Task<TResult>> taskAction, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var timeoutMs = timeout.RoundedMilliseconds(0, -1).ClampInt32();
            return RunWithTimeout(taskAction, timeoutMs, cancellationToken);
        }

        public static async Task<TResult> RunWithTimeout<TResult>(Func<CancellationToken, Task<TResult>> taskAction, int timeoutMilliseconds, CancellationToken cancellationToken = default)
        {
            if (taskAction == null) throw new ArgumentNullException(nameof(taskAction));

            if (timeoutMilliseconds < 0) // infinite
            {
                return await taskAction(cancellationToken).CAF();
            }

            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var task = taskAction(cancellation.Token);
            using var delayCancel = new CancellationTokenSource();
            var delay = Task.Delay(timeoutMilliseconds, delayCancel.Token);

            await Task.WhenAny(task, delay).CAF();

            // if the delay is not completed, cancel it & observe the corresponding exception
            if (!delay.IsCompleted)
            {
                delayCancel.Cancel();
                _ = delay.ObserveException();
            }

            // if the task is completed, return
            if (task.IsCompleted)
            {
                return await task.CAF(); // might have thrown
            }

            // else, this is a timeout
            // signal the task & observe any exception it may throw
            cancellation.Cancel();
            _ = task.ObserveException(); // should we do this?

            throw new TaskTimeoutException(ExceptionMessages.Timeout, task);
        }

        public static Task<TResult> RunWithTimeout<TArg, TResult>(Func<TArg, CancellationToken, Task<TResult>> taskAction, TArg arg, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var timeoutMs = timeout.RoundedMilliseconds(0, -1).ClampInt32();
            return RunWithTimeout(taskAction, arg, timeoutMs, cancellationToken);
        }

        public static async Task<TResult> RunWithTimeout<TResult, TArg>(Func<TArg, CancellationToken, Task<TResult>> taskAction, TArg arg, int timeoutMilliseconds, CancellationToken cancellationToken = default)
        {
            if (taskAction == null) throw new ArgumentNullException(nameof(taskAction));

            if (timeoutMilliseconds < 0) // infinite
            {
                return await taskAction(arg, cancellationToken).CAF();
            }

            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var task = taskAction(arg, cancellation.Token);
            using var delayCancel = new CancellationTokenSource();
            var delay = Task.Delay(timeoutMilliseconds, delayCancel.Token);

            await Task.WhenAny(task, delay).CAF();

            // if the delay is not completed, cancel it & observe the corresponding exception
            if (!delay.IsCompleted)
            {
                delayCancel.Cancel();
                _ = delay.ObserveException();
            }

            // if the task is completed, return
            if (task.IsCompleted)
            {
                return await task.CAF(); // might have thrown
            }

            // else, this is a timeout
            // signal the task & observe any exception it may throw
            cancellation.Cancel();
            _ = task.ObserveException(); // should we do this?

            throw new TaskTimeoutException(ExceptionMessages.Timeout, task);
        }
    }
}
