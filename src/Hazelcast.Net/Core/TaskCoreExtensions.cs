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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Exceptions;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension method to the <see cref="Task"/> and <see cref="Task{T}"/> classes.
    /// </summary>
    internal static class TaskCoreExtensions
    {
        /// <summary>
        /// ConfigureAwait(false) = disable synchronization context and continue on any context.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>An <see cref="ConfiguredTaskAwaitable"/> object used to await the task.</returns>
        /// <remarks>
        /// <para>Configures an awaiter used to await the task, to continue on any context.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once InconsistentNaming
        public static ConfiguredTaskAwaitable CAF([NotNull] this Task task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            return task.ConfigureAwait(false);
        }

        /// <summary>
        /// ConfigureAwait(false) = disable synchronization context and continue on any context.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>An <see cref="ConfiguredTaskAwaitable"/> object used to await the task.</returns>
        /// <remarks>
        /// <para>Configures an awaiter used to await the task, to continue on any context.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once InconsistentNaming
        public static ConfiguredTaskAwaitable<T> CAF<T>([NotNull] this Task<T> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            return task.ConfigureAwait(false);
        }

        /// <summary>
        /// ConfigureAwait(false) = disable synchronization context and continue on any context.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>An <see cref="ConfiguredTaskAwaitable"/> object used to await the task.</returns>
        /// <remarks>
        /// <para>Configures an awaiter used to await the task, to continue on any context.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once InconsistentNaming
        public static ConfiguredValueTaskAwaitable CAF(this ValueTask task)
            => task.ConfigureAwait(false);

        /// <summary>
        /// ConfigureAwait(false) = disable synchronization context and continue on any context.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>An <see cref="ConfiguredTaskAwaitable"/> object used to await the task.</returns>
        /// <remarks>
        /// <para>Configures an awaiter used to await the task, to continue on any context.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once InconsistentNaming
        public static ConfiguredValueTaskAwaitable<T> CAF<T>(this ValueTask<T> task)
            => task.ConfigureAwait(false);

        /// <summary>
        /// Runs the task with a timeout.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellation">An optional cancellation token source.</param>
        /// <param name="observeException">Whether to automatically observe the task exceptions in case of a timeout.</param>
        /// <returns>A task that completes when the task completes.</returns>
        /// <exception cref="TaskTimeoutException">The task did not complete within the specified timeout.</exception>
        /// <remarks>
        /// <para>If <paramref name="timeout"/> is equivalent to a negative number of milliseconds, then
        /// the task runs without a timeout.</para>
        /// <para>When a <see cref="TaskTimeoutException"/> is thrown, the original task keeps running in the
        /// background. It is the responsibility of the caller to cancel the task, if possible, and to deal
        /// with the consequences. Or, at least, to observe exceptions that may be thrown by the task, in
        /// order to avoid unobserved exceptions.</para>
        /// <para>If <paramref name="cancellation"/> is supplied, it will be cancelled in case of a timeout, and always
        /// disposed. This can be used to cancel the task in case of a timeout.</para>
        /// <para>If <paramref name="observeException"/> is <c>true</c>, exceptions thrown by <paramref name="task"/> will
        /// be observed, so the task can be forgotten without causing problems.</para>
        /// </remarks>
        public static Task TimeoutAfter(this Task task, TimeSpan timeout, CancellationTokenSource? cancellation = null, bool observeException = false)
        {
            var timeoutMilliseconds = timeout.TimeoutMilliseconds(0); // 0 = immediate timeout
            return TimeoutAfter(task, timeoutMilliseconds, cancellation, observeException);
        }

        /// <summary>
        /// Runs the task with a timeout.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="timeoutMilliseconds">The timeout.</param>
        /// <param name="cancellation">An optional cancellation token source.</param>
        /// <param name="observeException">Whether to automatically observe the task exceptions in case of a timeout.</param>
        /// <returns>A task that completes when the task completes.</returns>
        /// <exception cref="TaskTimeoutException">The task did not complete within the specified timeout.</exception>
        /// <remarks>
        /// <para>If <paramref name="timeoutMilliseconds"/> is negative, then the task runs without a timeout.</para>
        /// <para>When a <see cref="TaskTimeoutException"/> is thrown, the original task keeps running in the
        /// background. It is the responsibility of the caller to cancel the task, if possible, and to deal
        /// with the consequences. Or, at least, to observe exceptions that may be thrown by the task, in
        /// order to avoid unobserved exceptions.</para>
        /// <para>If <paramref name="cancellation"/> is supplied, it will be cancelled in case of a timeout, and always
        /// disposed. This can be used to cancel the task in case of a timeout.</para>
        /// <para>If <paramref name="observeException"/> is <c>true</c>, exceptions thrown by <paramref name="task"/> will
        /// be observed, so the task can be forgotten without causing problems.</para>
        /// </remarks>
        public static async Task TimeoutAfter(this Task task, int timeoutMilliseconds, CancellationTokenSource? cancellation = null, bool observeException = false)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            if (timeoutMilliseconds < 0) // infinite
            {
                await task.CAF();
                return;
            }

            using var taskTimeout = new TaskTimeout(timeoutMilliseconds);
            await taskTimeout.Run(task, cancellation, observeException).CAF();
        }

        /// <summary>
        /// Runs the task with a timeout.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <param name="task">The task.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellation">An optional cancellation token source.</param>
        /// <param name="observeException">Whether to automatically observe the task exceptions in case of a timeout.</param>
        /// <returns>The result produced by the <paramref name="task"/>.</returns>
        /// <exception cref="TaskTimeoutException">The task did not complete within the specified timeout.</exception>
        /// <remarks>
        /// <para>If <paramref name="timeout"/> is equivalent to a negative number of milliseconds, then
        /// the task runs without a timeout.</para>
        /// <para>When a <see cref="TaskTimeoutException"/> is thrown, the original task keeps running in the
        /// background. It is the responsibility of the caller to cancel the task, if possible, and to deal
        /// with the consequences. Or, at least, to observe exceptions that may be thrown by the task, in
        /// order to avoid unobserved exceptions.</para>
        /// </remarks>
        public static Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout, CancellationTokenSource? cancellation = null, bool observeException = false)
        {
            var timeoutMilliseconds = timeout.TimeoutMilliseconds(0); // 0 = immediate timeout
            return TimeoutAfter(task, timeoutMilliseconds, cancellation, observeException);
        }

        /// <summary>
        /// Runs the task with a timeout.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <param name="task">The task.</param>
        /// <param name="timeoutMilliseconds">The timeout.</param>
        /// <param name="cancellation">An optional cancellation token source.</param>
        /// <param name="observeException">Whether to automatically observe the task exceptions in case of a timeout.</param>
        /// <returns>The result produced by the <paramref name="task"/>.</returns>
        /// <exception cref="TaskTimeoutException">The task did not complete within the specified timeout.</exception>
        /// <remarks>
        /// <para>If <paramref name="timeoutMilliseconds"/> is negative, then the task runs without a timeout.</para>
        /// <para>When a <see cref="TaskTimeoutException"/> is thrown, the original task keeps running in the
        /// background. It is the responsibility of the caller to cancel the task, if possible, and to deal
        /// with the consequences. Or, at least, to observe exceptions that may be thrown by the task, in
        /// order to avoid unobserved exceptions.</para>
        /// <para>If <paramref name="cancellation"/> is supplied, it will be cancelled in case of a timeout, and always
        /// disposed. This can be used to cancel the task in case of a timeout.</para>
        /// <para>If <paramref name="observeException"/> is <c>true</c>, exceptions thrown by <paramref name="task"/> will
        /// be observed, so the task can be forgotten without causing problems.</para>
        /// </remarks>
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int timeoutMilliseconds, CancellationTokenSource? cancellation = null, bool observeException = false)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            if (timeoutMilliseconds < 0) // infinite
            {
                cancellation?.Dispose();
                return await task.CAF();
            }

            using var taskTimeout = new TaskTimeout(timeoutMilliseconds);
            return await taskTimeout.Run(task, cancellation, observeException).CAF();
        }

        // TODO: document for ValueTask
        // little choice but use .AsTask() here?
        // https://stackoverflow.com/questions/45689327/task-whenall-for-valuetask

        public static async ValueTask TimeoutAfter(this ValueTask task, TimeSpan timeout, CancellationTokenSource? cancellation = null, bool observeException = false)
            => await TimeoutAfter(task.AsTask(), timeout, cancellation, observeException).CAF();

        public static async ValueTask TimeoutAfter(this ValueTask task, int timeoutMilliseconds, CancellationTokenSource? cancellation = null, bool observeException = false)
            => await TimeoutAfter(task.AsTask(), timeoutMilliseconds, cancellation, observeException).CAF();

        public static async ValueTask<TResult> TimeoutAfter<TResult>(this ValueTask<TResult> task, TimeSpan timeout, CancellationTokenSource? cancellation = null, bool observeException = false)
            => await TimeoutAfter(task.AsTask(), timeout, cancellation, observeException).CAF();

        public static async ValueTask<TResult> TimeoutAfter<TResult>(this ValueTask<TResult> task, int timeoutMilliseconds, CancellationTokenSource? cancellation = null, bool observeException = false)
            => await TimeoutAfter(task.AsTask(), timeoutMilliseconds, cancellation, observeException).CAF();
    }
}
