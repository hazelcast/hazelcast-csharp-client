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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Exceptions;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides utility methods for running a <see cref="Task"/>.
    /// </summary>
    internal static class TaskEx
    {
        /// <summary>
        /// Runs a task with a timeout.
        /// </summary>
        /// <param name="taskAction">The task action.</param>
        /// <param name="timeout">A timeout.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>A task that will complete when the task action completes, or the timeout expires.</returns>
        /// <exception cref="TaskTimeoutException">The timeout has expired prior to the completion of the task.</exception>
        /// <remarks>
        /// <para>In case of a timeout, the <see cref="TaskTimeoutException"/> that is thrown contains a
        /// reference to the original task. Also, a continuation is added to the task, which observes its
        /// exceptions, thus making sure no unobserved exception can leak.</para>
        /// <para>In case of a timeout, the cancellation token that is passed to the task action is
        /// canceled, and it is expected that the task will behave and terminate nicely. The task is
        /// <em>not</em> aborted. Conversely, cancelling the task is a technical .NET-level, client-side
        /// operation, <em>not</em> a logical, application-level operation. Underlying cluster operations,
        /// if any, are not "canceled" nor "rolled back" and could have partially be executed. The exact
        /// state of the cluster after a cancellation is unspecified.</para>
        /// </remarks>
        public static Task RunWithTimeout(Func<CancellationToken, Task> taskAction, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var timeoutMs = timeout.RoundedMilliseconds().ClampToInt32();
            return RunWithTimeout(taskAction, timeoutMs, cancellationToken);
        }

        /// <summary>
        /// Runs a task with a timeout.
        /// </summary>
        /// <param name="taskAction">The task action.</param>
        /// <param name="timeoutMilliseconds">A timeout in milliseconds (-1 is infinite).</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>A task that will complete when the task action completes, or the timeout expires.</returns>
        /// <exception cref="TaskTimeoutException">The timeout has expired prior to the completion of the task.</exception>
        /// <remarks>
        /// <para>In case of a timeout, the <see cref="TaskTimeoutException"/> that is thrown contains a
        /// reference to the original task. Also, a continuation is added to the task, which observes its
        /// exceptions, thus making sure no unobserved exception can leak.</para>
        /// <para>In case of a timeout, the cancellation token that is passed to the task action is
        /// canceled, and it is expected that the task will behave and terminate nicely. The task is
        /// <em>not</em> aborted. Conversely, cancelling the task is a technical .NET-level, client-side
        /// operation, <em>not</em> a logical, application-level operation. Underlying cluster operations,
        /// if any, are not "canceled" nor "rolled back" and could have partially be executed. The exact
        /// state of the cluster after a cancellation is unspecified.</para>
        /// </remarks>
        public static async Task RunWithTimeout(Func<CancellationToken, Task> taskAction, int timeoutMilliseconds, CancellationToken cancellationToken = default)
        {
            if (taskAction == null) throw new ArgumentNullException(nameof(taskAction));

            if (timeoutMilliseconds < 0) // infinite
            {
                await taskAction(cancellationToken).CfAwait();
                return;
            }

            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var task = taskAction(cancellation.Token);
            using var delayCancel = new CancellationTokenSource();
            var delay = Task.Delay(timeoutMilliseconds, delayCancel.Token);

            await Task.WhenAny(task, delay).CfAwait();

            // if the delay is not completed, cancel it & observe the corresponding exception
            if (!delay.IsCompleted)
            {
                delayCancel.Cancel();
                _ = delay.ObserveException();
            }

            // if the task is completed (success or fault...), return
            if (task.IsCompleted)
            {
                await task.CfAwait();
                return;
            }

            // else timeout
            throw CreateTaskTimeoutException(task, cancellation);
        }

        /// <summary>
        /// Runs a task with a timeout.
        /// </summary>
        /// <typeparam name="TArg">The type of the argument passed to the task action.</typeparam>
        /// <param name="taskAction">The task action.</param>
        /// <param name="arg">An argument to pass to the task action.</param>
        /// <param name="timeout">A timeout.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>A task that will complete when the task action completes, or the timeout expires.</returns>
        /// <exception cref="TaskTimeoutException">The timeout has expired prior to the completion of the task.</exception>
        /// <remarks>
        /// <para>In case of a timeout, the <see cref="TaskTimeoutException"/> that is thrown contains a
        /// reference to the original task. Also, a continuation is added to the task, which observes its
        /// exceptions, thus making sure no unobserved exception can leak.</para>
        /// <para>In case of a timeout, the cancellation token that is passed to the task action is
        /// canceled, and it is expected that the task will behave and terminate nicely. The task is
        /// <em>not</em> aborted. Conversely, cancelling the task is a technical .NET-level, client-side
        /// operation, <em>not</em> a logical, application-level operation. Underlying cluster operations,
        /// if any, are not "canceled" nor "rolled back" and could have partially be executed. The exact
        /// state of the cluster after a cancellation is unspecified.</para>
        /// </remarks>
        public static Task RunWithTimeout<TArg>(Func<TArg, CancellationToken, Task> taskAction, TArg arg, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var timeoutMs = timeout.RoundedMilliseconds().ClampToInt32();
            return RunWithTimeout(taskAction, arg, timeoutMs, cancellationToken);
        }

        /// <summary>
        /// Runs a task with a timeout.
        /// </summary>
        /// <typeparam name="TArg">The type of the argument passed to the task action.</typeparam>
        /// <param name="taskAction">The task action.</param>
        /// <param name="arg">An argument to pass to the task action.</param>
        /// <param name="timeoutMilliseconds">A timeout in milliseconds (-1 is infinite).</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>A task that will complete when the task action completes, or the timeout expires.</returns>
        /// <exception cref="TaskTimeoutException">The timeout has expired prior to the completion of the task.</exception>
        /// <remarks>
        /// <para>In case of a timeout, the <see cref="TaskTimeoutException"/> that is thrown contains a
        /// reference to the original task. Also, a continuation is added to the task, which observes its
        /// exceptions, thus making sure no unobserved exception can leak.</para>
        /// <para>In case of a timeout, the cancellation token that is passed to the task action is
        /// canceled, and it is expected that the task will behave and terminate nicely. The task is
        /// <em>not</em> aborted. Conversely, cancelling the task is a technical .NET-level, client-side
        /// operation, <em>not</em> a logical, application-level operation. Underlying cluster operations,
        /// if any, are not "canceled" nor "rolled back" and could have partially be executed. The exact
        /// state of the cluster after a cancellation is unspecified.</para>
        /// </remarks>
        public static async Task RunWithTimeout<TArg>(Func<TArg, CancellationToken, Task> taskAction, TArg arg, int timeoutMilliseconds, CancellationToken cancellationToken = default)
        {
            if (taskAction == null) throw new ArgumentNullException(nameof(taskAction));

            if (timeoutMilliseconds < 0) // infinite
            {
                await taskAction(arg, cancellationToken).CfAwait();
                return;
            }

            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var task = taskAction(arg, cancellation.Token);
            using var delayCancel = new CancellationTokenSource();
            var delay = Task.Delay(timeoutMilliseconds, delayCancel.Token);

            await Task.WhenAny(task, delay).CfAwait();

            // if the delay is not completed, cancel it & observe the corresponding exception
            if (!delay.IsCompleted)
            {
                delayCancel.Cancel();
                _ = delay.ObserveException();
            }

            // if the task is completed (success or fault...), return
            if (task.IsCompleted)
            {
                await task.CfAwait(); // might have thrown
                return;
            }

            // else timeout
            throw CreateTaskTimeoutException(task, cancellation);
        }

        /// <summary>
        /// Runs a task with a timeout.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the task.</typeparam>
        /// <param name="taskAction">The task action.</param>
        /// <param name="timeout">A timeout.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>A task that will complete when the task action completes, or the timeout expires.</returns>
        /// <exception cref="TaskTimeoutException">The timeout has expired prior to the completion of the task.</exception>
        /// <remarks>
        /// <para>In case of a timeout, the <see cref="TaskTimeoutException"/> that is thrown contains a
        /// reference to the original task. Also, a continuation is added to the task, which observes its
        /// exceptions, thus making sure no unobserved exception can leak.</para>
        /// <para>In case of a timeout, the cancellation token that is passed to the task action is
        /// canceled, and it is expected that the task will behave and terminate nicely. The task is
        /// <em>not</em> aborted. Conversely, cancelling the task is a technical .NET-level, client-side
        /// operation, <em>not</em> a logical, application-level operation. Underlying cluster operations,
        /// if any, are not "canceled" nor "rolled back" and could have partially be executed. The exact
        /// state of the cluster after a cancellation is unspecified.</para>
        /// </remarks>
        public static Task<TResult> RunWithTimeout<TResult>(Func<CancellationToken, Task<TResult>> taskAction, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var timeoutMs = timeout.RoundedMilliseconds().ClampToInt32();
            return RunWithTimeout(taskAction, timeoutMs, cancellationToken);
        }

        /// <summary>
        /// Runs a task with a timeout.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the task.</typeparam>
        /// <param name="taskAction">The task action.</param>
        /// <param name="timeoutMilliseconds">A timeout in milliseconds (-1 is infinite).</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>A task that will complete when the task action completes, or the timeout expires.</returns>
        /// <exception cref="TaskTimeoutException">The timeout has expired prior to the completion of the task.</exception>
        /// <remarks>
        /// <para>In case of a timeout, the <see cref="TaskTimeoutException"/> that is thrown contains a
        /// reference to the original task. Also, a continuation is added to the task, which observes its
        /// exceptions, thus making sure no unobserved exception can leak.</para>
        /// <para>In case of a timeout, the cancellation token that is passed to the task action is
        /// canceled, and it is expected that the task will behave and terminate nicely. The task is
        /// <em>not</em> aborted. Conversely, cancelling the task is a technical .NET-level, client-side
        /// operation, <em>not</em> a logical, application-level operation. Underlying cluster operations,
        /// if any, are not "canceled" nor "rolled back" and could have partially be executed. The exact
        /// state of the cluster after a cancellation is unspecified.</para>
        /// </remarks>
        public static async Task<TResult> RunWithTimeout<TResult>(Func<CancellationToken, Task<TResult>> taskAction, int timeoutMilliseconds, CancellationToken cancellationToken = default)
        {
            if (taskAction == null) throw new ArgumentNullException(nameof(taskAction));

            if (timeoutMilliseconds < 0) // infinite
                return await taskAction(cancellationToken).CfAwait();

            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var task = taskAction(cancellation.Token);
            using var delayCancel = new CancellationTokenSource();
            var delay = Task.Delay(timeoutMilliseconds, delayCancel.Token);

            await Task.WhenAny(task, delay).CfAwait();

            // if the delay is not completed, cancel it & observe the corresponding exception
            if (!delay.IsCompleted)
            {
                delayCancel.Cancel();
                _ = delay.ObserveException();
            }

            // if the task is completed (success or fault...), return
            if (task.IsCompleted)
                return await task.CfAwait();

            // else timeout
            throw CreateTaskTimeoutException(task, cancellation);
        }

        /// <summary>
        /// Runs a task with a timeout.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the task.</typeparam>
        /// <typeparam name="TArg">The type of the argument passed to the task action.</typeparam>
        /// <param name="taskAction">The task action.</param>
        /// <param name="arg">An argument to pass to the task action.</param>
        /// <param name="timeout">A timeout.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>A task that will complete when the task action completes, or the timeout expires.</returns>
        /// <exception cref="TaskTimeoutException">The timeout has expired prior to the completion of the task.</exception>
        /// <remarks>
        /// <para>In case of a timeout, the <see cref="TaskTimeoutException"/> that is thrown contains a
        /// reference to the original task. Also, a continuation is added to the task, which observes its
        /// exceptions, thus making sure no unobserved exception can leak.</para>
        /// <para>In case of a timeout, the cancellation token that is passed to the task action is
        /// canceled, and it is expected that the task will behave and terminate nicely. The task is
        /// <em>not</em> aborted. Conversely, cancelling the task is a technical .NET-level, client-side
        /// operation, <em>not</em> a logical, application-level operation. Underlying cluster operations,
        /// if any, are not "canceled" nor "rolled back" and could have partially be executed. The exact
        /// state of the cluster after a cancellation is unspecified.</para>
        /// </remarks>
        public static Task<TResult> RunWithTimeout<TArg, TResult>(Func<TArg, CancellationToken, Task<TResult>> taskAction, TArg arg, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            var timeoutMs = timeout.RoundedMilliseconds().ClampToInt32();
            return RunWithTimeout(taskAction, arg, timeoutMs, cancellationToken);
        }

        /// <summary>
        /// Runs a task with a timeout.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the task.</typeparam>
        /// <typeparam name="TArg">The type of the argument passed to the task action.</typeparam>
        /// <param name="taskAction">The task action.</param>
        /// <param name="arg">An argument to pass to the task action.</param>
        /// <param name="timeoutMilliseconds">A timeout in milliseconds (-1 is infinite).</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        /// <returns>A task that will complete when the task action completes, or the timeout expires.</returns>
        /// <exception cref="TaskTimeoutException">The timeout has expired prior to the completion of the task.</exception>
        /// <remarks>
        /// <para>In case of a timeout, the <see cref="TaskTimeoutException"/> that is thrown contains a
        /// reference to the original task. Also, a continuation is added to the task, which observes its
        /// exceptions, thus making sure no unobserved exception can leak.</para>
        /// <para>In case of a timeout, the cancellation token that is passed to the task action is
        /// canceled, and it is expected that the task will behave and terminate nicely. The task is
        /// <em>not</em> aborted. Conversely, cancelling the task is a technical .NET-level, client-side
        /// operation, <em>not</em> a logical, application-level operation. Underlying cluster operations,
        /// if any, are not "canceled" nor "rolled back" and could have partially be executed. The exact
        /// state of the cluster after a cancellation is unspecified.</para>
        /// </remarks>
        public static async Task<TResult> RunWithTimeout<TResult, TArg>(Func<TArg, CancellationToken, Task<TResult>> taskAction, TArg arg, int timeoutMilliseconds, CancellationToken cancellationToken = default)
        {
            if (taskAction == null) throw new ArgumentNullException(nameof(taskAction));

            if (timeoutMilliseconds < 0) // infinite
                return await taskAction(arg, cancellationToken).CfAwait();

            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var task = taskAction(arg, cancellation.Token);
            using var delayCancel = new CancellationTokenSource();
            var delay = Task.Delay(timeoutMilliseconds, delayCancel.Token);

            await Task.WhenAny(task, delay).CfAwait();

            // if the delay is not completed, cancel it & observe the corresponding exception
            if (!delay.IsCompleted)
            {
                delayCancel.Cancel();
                _ = delay.ObserveException();
            }

            // if the task is completed (success or fault...), return
            if (task.IsCompleted)
                return await task.CfAwait();

            // else, timeout
            throw CreateTaskTimeoutException(task, cancellation);
        }

        [DoesNotReturn]
        private static Exception CreateTaskTimeoutException(Task task, CancellationTokenSource cancellation)
        {
            // try to signal the task
            cancellation.Cancel();

            // observe any exception it may throw
            _ = task.ObserveException();

            // throw
            return new TaskTimeoutException(ExceptionMessages.Timeout, task);
        }
    }
}
