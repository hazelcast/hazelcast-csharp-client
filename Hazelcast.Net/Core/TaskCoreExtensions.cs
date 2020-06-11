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

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension method to the <see cref="Task"/> and <see cref="Task{T}"/> classes.
    /// </summary>
    public static class TaskCoreExtensions
    {
        /// <summary>
        /// Gets the cancellation source that never cancels.
        /// </summary>
        /// <remarks>
        /// <para>This cancellation source should *not* ever be canceled, completed, anything.</para>
        /// </remarks>
        internal static readonly CancellationTokenSource NeverCanceledSource = new CancellationTokenSource();

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
        /// Configures a task to handle timeouts.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="cts">A cancellation token source controlling the timeout.</param>
        /// <returns>A task.</returns>
        public static Task OrTimeout([NotNull] this Task task, [NotNull] CancellationTokenSource cts)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
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
            }, default, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current).Unwrap();
        }

        /// <summary>
        /// Configures a task to handle timeouts.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="cts">A cancellation token source controlling the timeout.</param>
        /// <returns>A task.</returns>
        public static Task<T> OrTimeout<T>([NotNull] this Task<T> task, [NotNull] CancellationTokenSource cts)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
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
            }, default, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current).Unwrap();
        }
    }
}
