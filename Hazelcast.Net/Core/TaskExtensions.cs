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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension method to the <see cref="Task"/> and <see cref="Task{T}"/> classes.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Gets the cancellation source that never cancels.
        /// </summary>
        /// <remarks>
        /// <para>This cancellation source should *not* ever be canceled, completed, anything.</para>
        /// </remarks>
        public static readonly CancellationTokenSource NeverCanceledSource = new CancellationTokenSource();

        /// <summary>
        /// ConfigureAwait(false) = disable synchronization context and continue on any context.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>An <see cref="ConfiguredTaskAwaitable"/> object used to await the task.</returns>
        /// <remarks>
        /// <para>Configures an awaiter used to await the task, to continue on any context.</para>
        /// </remarks>
        // ReSharper disable once InconsistentNaming
        public static ConfiguredTaskAwaitable CAF(this Task task)
            => task.ConfigureAwait(false);

        /// <summary>
        /// ConfigureAwait(false) = disable synchronization context and continue on any context.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>An <see cref="ConfiguredTaskAwaitable"/> object used to await the task.</returns>
        /// <remarks>
        /// <para>Configures an awaiter used to await the task, to continue on any context.</para>
        /// </remarks>
        // ReSharper disable once InconsistentNaming
        public static ConfiguredTaskAwaitable<T> CAF<T>(this Task<T> task)
            => task.ConfigureAwait(false);

        /// <summary>
        /// ConfigureAwait(false) = disable synchronization context and continue on any context.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>An <see cref="ConfiguredTaskAwaitable"/> object used to await the task.</returns>
        /// <remarks>
        /// <para>Configures an awaiter used to await the task, to continue on any context.</para>
        /// </remarks>
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
        // ReSharper disable once InconsistentNaming
        public static ConfiguredValueTaskAwaitable<T> CAF<T>(this ValueTask<T> task)
            => task.ConfigureAwait(false);

        /// <summary>
        /// Configures a task to handle timeouts.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="cts">A cancellation token source controlling the timeout.</param>
        /// <returns>A task.</returns>
        public static Task OrTimeout(this Task task, TimeoutCancellationTokenSource cts)
        {
            if (!cts.HasTimeout) return task;

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

        /// <summary>
        /// Configures a task to handle timeouts.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="cts">A cancellation token source controlling the timeout.</param>
        /// <returns>A task.</returns>
        public static Task<T> OrTimeout<T>(this Task<T> task, TimeoutCancellationTokenSource cts)
        {
            if (!cts.HasTimeout) return task;

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

        /// <summary>
        /// Configures a task to handle timeouts.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="cts">A cancellation token source controlling the timeout.</param>
        /// <returns>A task.</returns>
        public static Task OrTimeout(this Task task, CancellationTokenSource cts)
        {
            if (cts == NeverCanceledSource) return task;

            return task.ContinueWith(x =>
            {
                var notTimedOut = !x.IsCanceled;
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

        /// <summary>
        /// Configures a task to handle timeouts.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="cts">A cancellation token source controlling the timeout.</param>
        /// <returns>A task.</returns>
        public static Task<T> OrTimeout<T>(this Task<T> task, CancellationTokenSource cts)
        {
            if (cts == NeverCanceledSource) return task;

            return task.ContinueWith(x =>
            {
                var notTimedOut = !x.IsCanceled;
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

        /// <summary>
        /// Configures a task to dispose a resource after it completes.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="disposable">The disposable resource.</param>
        /// <returns>A task.</returns>
        public static Task ThenDispose(this Task task, IDisposable disposable)
        {
            return task.ContinueWith(x =>
            {
                disposable.Dispose();
                return x;
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }

        /// <summary>
        /// Configures a task to dispose resources after it completes.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="disposables">The disposable resources.</param>
        /// <returns>A task.</returns>
        public static Task ThenDispose(this Task task, params IDisposable[] disposables)
        {
            return task.ContinueWith(x =>
            {
                foreach (var disposable in disposables)
                    disposable.Dispose();
                return x;
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }

        /// <summary>
        /// Configures a task to dispose a resource after it completes.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="disposable">The disposable resource.</param>
        /// <returns>A task.</returns>
        public static Task<T> ThenDispose<T>(this Task<T> task, IDisposable disposable)
        {
            return task.ContinueWith(x =>
            {
                disposable.Dispose();
                return x;
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }

        /// <summary>
        /// Configures a task to dispose resources after it completes.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="disposables">The disposable resources.</param>
        /// <returns>A task.</returns>
        public static Task<T> ThenDispose<T>(this Task<T> task, params IDisposable[] disposables)
        {
            return task.ContinueWith(x =>
            {
                foreach (var disposable in disposables)
                    disposable.Dispose();
                return x;
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }
    }
}
