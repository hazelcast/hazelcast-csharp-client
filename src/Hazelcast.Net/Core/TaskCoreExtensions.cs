// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
    /// Provides extension method to the <see cref="Task"/> and <see cref="Task{TResult}"/> classes,
    /// and the <see cref="ValueTask"/> and <see cref="ValueTask{TResult}"/> structs.
    /// </summary>
    /// <remarks>
    /// <para>See: https://devblogs.microsoft.com/dotnet/configureawait-faq/.</para>
    /// </remarks>
    internal static class TaskCoreExtensions
    {
        // NOTES
        //
        // in this class, we trust our code that [NotNull] arguments will not be null,
        // and do *not* check arguments for null.

        /// <summary>
        /// Gets this task if it is not <c>null</c>, or a completed task.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>The <paramref name="task"/> if it is not <c>null</c>; otherwise <c>Task.CompletedTask</c>.</returns>
        /// <remarks>
        /// <para>Use this method to await a task that may be <c>null</c>.</para>
        /// </remarks>
        public static Task MaybeNull(this Task? task)
            => task ?? Task.CompletedTask;

        /// <summary>
        /// Configures an awaiter used to await this <see cref="Task"/>, continuing on
        /// any synchronization context.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>An object used to await this <see cref="Task"/>.</returns>
        /// <remarks>
        /// <para>This is equivalent to <c>ConfigureAwait(false)</c>.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable CfAwait([NotNull] this Task task)
            => task.ConfigureAwait(false);

        /// <summary>
        /// Configures an awaiter used to await this <see cref="Task{TResult}"/>, continuing on
        /// any synchronization context.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>An object used to await this <see cref="Task{TResult}"/>.</returns>
        /// <remarks>
        /// <para>This is equivalent to <c>ConfigureAwait(false)</c>.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable<T> CfAwait<T>([NotNull] this Task<T> task)
            => task.ConfigureAwait(false);

        /// <summary>
        /// Configures an awaiter used to await this <see cref="ValueTask"/>, continuing on
        /// any synchronization context.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>An object used to await this <see cref="ValueTask"/>.</returns>
        /// <remarks>
        /// <para>This is equivalent to <c>ConfigureAwait(false)</c>.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredValueTaskAwaitable CfAwait(this ValueTask task)
            => task.ConfigureAwait(false);

        /// <summary>
        /// Configures an awaiter used to await this <see cref="ValueTask{TResult}"/>, continuing on
        /// any synchronization context.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>An object used to await this <see cref="ValueTask{TResult}"/>.</returns>
        /// <remarks>
        /// <para>This is equivalent to <c>ConfigureAwait(false)</c>.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredValueTaskAwaitable<T> CfAwait<T>(this ValueTask<T> task)
            => task.ConfigureAwait(false);

        /// <summary>
        /// Configures an awaiter used to await this <see cref="Task"/>, continuing on
        /// any synchronization context, and not throwing any exception.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>An object used to await this <see cref="Task"/>.</returns>
        /// <remarks>
        /// <para>This is equivalent to <c>ConfigureAwait(false)</c>, and in addition
        /// any exception thrown by the task is swallowed and observed (in order not
        /// to become an unobserved exception).</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskNoThrowAwaitable CfAwaitNoThrow([NotNull] this Task task)
            => new ConfiguredTaskNoThrowAwaitable(task);

        /// <summary>
        /// Configures an awaiter used to await this <see cref="ValueTask"/>, continuing on
        /// any synchronization context, and not throwing any exception.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>An object used to await this <see cref="ValueTask"/>.</returns>
        /// <remarks>
        /// <para>This is equivalent to <c>ConfigureAwait(false)</c>, and in addition
        /// any exception thrown by the task is swallowed and observed (in order not
        /// to become an unobserved exception).</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredValueTaskNoThrowAwaitable CfAwaitNoThrow(this ValueTask task)
            => new ConfiguredValueTaskNoThrowAwaitable(task);

        /// <summary>
        /// Configures an awaiter used to await this <see cref="Task"/>, continuing on
        /// any synchronization context, and not throwing an exception if the task is
        /// canceled.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>An object used to await this <see cref="Task"/>.</returns>
        /// <remarks>
        /// <para>This is equivalent to <c>ConfigureAwait(false)</c>, and in addition
        /// any cancelled exception thrown by the task is swallowed and observed (in order not
        /// to become an unobserved exception). Other exceptions are thrown.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskNoThrowCanceledAwaitable CfAwaitCanceled([NotNull] this Task task)
            => new ConfiguredTaskNoThrowCanceledAwaitable(task);

        /// <summary>
        /// Configures an awaiter used to await this <see cref="Task"/>, no longer than the
        /// specified <paramref name="timeout"/>, and continuing on any synchronization context.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellation"></param>
        /// <returns>An object used to await this <see cref="Task"/>.</returns>
        /// <remarks>
        /// <para>This is equivalent to <c>ConfigureAwait(false)</c>, and in addition
        /// it throws a <see cref="TaskTimeoutException"/> if the task runs longer than the
        /// specified <paramref name="timeout"/>.</para>
        /// <para>In this case, it is only the <c>await</c> that is aborted: the original
        /// <paramref name="task"/> keeps running in the background. However, if a
        /// <paramref name="cancellation"/> source is provided, it is canceled, thus giving
        /// an opportunity to the task to cancel its operations in the background.</para>
        /// <para>In case of a timeout, exceptions throw by the <paramref name="task"/> are
        /// observed, i.e. they will not come out as unobserved exceptions.</para>
        /// </remarks>
        public static ConfiguredTaskAwaitable CfAwait(this Task task, TimeSpan timeout, CancellationTokenSource? cancellation = null)
            => AwaitWithTimeout(task, timeout.RoundedMilliseconds().ClampToInt32(), cancellation).ConfigureAwait(false);

        /// <summary>
        /// Configures an awaiter used to await this <see cref="Task"/>, continuing on
        /// any synchronization context, until a timeout is reached.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="timeoutMilliseconds">The timeout.</param>
        /// <param name="cancellation"></param>
        /// <returns>An object used to await this <see cref="Task"/>.</returns>
        /// <remarks>
        /// <para>This is equivalent to <c>ConfigureAwait(false)</c>, and in addition
        /// it throws a <see cref="TaskTimeoutException"/> if the task runs longer than the
        /// specified <paramref name="timeoutMilliseconds"/>.</para>
        /// <para>In this case, it is only the <c>await</c> that is aborted: the original
        /// <paramref name="task"/> keeps running in the background. However, if a
        /// <paramref name="cancellation"/> source is provided, it is canceled, thus giving
        /// an opportunity to the task to cancel its operations in the background.</para>
        /// <para>In case of a timeout, exceptions throw by the <paramref name="task"/> are
        /// observed, i.e. they will not come out as unobserved exceptions.</para>
        /// </remarks>
        public static ConfiguredTaskAwaitable CfAwait(this Task task, int timeoutMilliseconds, CancellationTokenSource? cancellation = null)
            => AwaitWithTimeout(task, timeoutMilliseconds, cancellation).ConfigureAwait(false);

        /// <summary>
        /// Configures an awaiter used to await this <see cref="Task{TResult}"/>, continuing on
        /// any synchronization context, until a timeout is reached.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellation"></param>
        /// <returns>An object used to await this <see cref="Task{TResult}"/>.</returns>
        /// <remarks>
        /// <para>This is equivalent to <c>ConfigureAwait(false)</c>, and in addition
        /// it throws a <see cref="TaskTimeoutException"/> if the task runs longer than the
        /// specified <paramref name="timeout"/>.</para>
        /// <para>In this case, it is only the <c>await</c> that is aborted: the original
        /// <paramref name="task"/> keeps running in the background. However, if a
        /// <paramref name="cancellation"/> source is provided, it is canceled, thus giving
        /// an opportunity to the task to cancel its operations in the background.</para>
        /// <para>In case of a timeout, exceptions throw by the <paramref name="task"/> are
        /// observed, i.e. they will not come out as unobserved exceptions.</para>
        /// </remarks>
        public static ConfiguredTaskAwaitable<TResult> CfAwait<TResult>(this Task<TResult> task, TimeSpan timeout, CancellationTokenSource? cancellation = null)
            => AwaitWithTimeout(task, timeout.RoundedMilliseconds().ClampToInt32(), cancellation).ConfigureAwait(false);

        /// <summary>
        /// Configures an awaiter used to await this <see cref="Task{TResult}"/>, continuing on
        /// any synchronization context, until a timeout is reached.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="timeoutMilliseconds">The timeout.</param>
        /// <param name="cancellation"></param>
        /// <returns>An object used to await this <see cref="Task{TResult}"/>.</returns>
        /// <remarks>
        /// <para>This is equivalent to <c>ConfigureAwait(false)</c>, and in addition
        /// it throws a <see cref="TaskTimeoutException"/> if the task runs longer than the
        /// specified <paramref name="timeoutMilliseconds"/>.</para>
        /// <para>In this case, it is only the <c>await</c> that is aborted: the original
        /// <paramref name="task"/> keeps running in the background. However, if a
        /// <paramref name="cancellation"/> source is provided, it is canceled, thus giving
        /// an opportunity to the task to cancel its operations in the background.</para>
        /// <para>In case of a timeout, exceptions throw by the <paramref name="task"/> are
        /// observed, i.e. they will not come out as unobserved exceptions.</para>
        /// </remarks>
        public static ConfiguredTaskAwaitable<TResult> CfAwait<TResult>(this Task<TResult> task, int timeoutMilliseconds, CancellationTokenSource? cancellation = null)
            => AwaitWithTimeout(task, timeoutMilliseconds, cancellation).ConfigureAwait(false);

        private static async Task AwaitWithTimeout(Task task, int timeoutMilliseconds, CancellationTokenSource? cancellation)
        {
            if (timeoutMilliseconds < 0)
            {
                await task.ConfigureAwait(false);
                return;
            }

            using var delayCancel = new CancellationTokenSource();
            var delay = Task.Delay(timeoutMilliseconds, delayCancel.Token);

            await Task.WhenAny(task, delay).ConfigureAwait(false);

            // if the delay is not completed, cancel it & observe the corresponding exception
            if (!delay.IsCompleted)
            {
                delayCancel.Cancel(); // task cancellation are never unobserved
            }

            // if the task is completed (success or fault...), return
            if (task.IsCompleted)
            {
                await task.CfAwait();
                return;
            }

            // signal the task it should cancel
            cancellation?.Cancel();

            // task cancellation are never unobserved
            // OTOH the task *could* throw another exception
            // and... most people will probably ignore them or forget to pay
            // attention, so it feels safer to observe these exceptions
            task.ObserveException();

            // else timeout
            throw new TaskTimeoutException(ExceptionMessages.Timeout, task);
        }

        private static async Task<TResult> AwaitWithTimeout<TResult>(Task<TResult> task, int timeoutMilliseconds, CancellationTokenSource? cancellation)
        {
            if (timeoutMilliseconds < 0)
            {
                return await task.ConfigureAwait(false);
            }

            using var delayCancel = new CancellationTokenSource();
            var delay = Task.Delay(timeoutMilliseconds, delayCancel.Token);

            await Task.WhenAny(task, delay).ConfigureAwait(false);

            // if the delay is not completed, cancel it & observe the corresponding exception
            if (!delay.IsCompleted)
            {
                delayCancel.Cancel(); // task cancellation are never unobserved
            }

            // if the task is completed (success or fault...), return
            if (task.IsCompleted)
            {
                return await task.CfAwait();
            }

            // signal the task it should cancel
            cancellation?.Cancel();

            // task cancellation are never unobserved
            // OTOH the task *could* throw another exception
            // and... most people will probably ignore them or forget to pay
            // attention, so it feels safer to observe these exceptions
            task.ObserveException();

            // else timeout
            throw new TaskTimeoutException(ExceptionMessages.Timeout, task);
        }

        /// <summary>
        /// Observes the exception of this <see cref="Task"/>.
        /// </summary>
        /// <param name="task">The task.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ObserveException([NotNull] this Task task)
#pragma warning disable CS4014 // Because this call is not awaited...
#pragma warning disable CA2012 // Use ValueTasks correctly - no we don't
            => ObserveExceptionInternal(task);
#pragma warning restore CS4014
#pragma warning restore CA2012

        // this indirect method will end up inlined, and exists only to
        // prevent CS4014 to pop on the code calling ObserveException
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static async ValueTask ObserveExceptionInternal([NotNull] Task task)
            => await new ConfiguredTaskNoThrowAwaitable(task);

        /// <summary>
        /// Provides an awaitable object that allows for configured awaits on <see cref="Task"/>.
        /// </summary>
        public readonly struct ConfiguredTaskNoThrowAwaitable : ICriticalNotifyCompletion
        {
            private readonly Task _task;

            /// <summary>
            /// Initializes a new instance of the <see cref="ConfiguredTaskNoThrowAwaitable"/> struct.
            /// </summary>
            /// <param name="task">The <see cref="Task"/>.</param>
            internal ConfiguredTaskNoThrowAwaitable([NotNull] Task task)
            {
                _task = task;
            }

            /// <summary>
            /// Gets the awaiter for this awaitable.
            /// </summary>
            /// <returns>The awaiter.</returns>
            public ConfiguredTaskNoThrowAwaitable GetAwaiter() => this;

            /// <summary>
            /// Gets whether the task being awaited is completed.
            /// </summary>
            /// <returns>Whether the task being awaited is completed.</returns>
            public bool IsCompleted
                => _task.IsCompleted;

            /// <summary>Ends the await on the completed <see cref="Task"/>.</summary>
            // [StackTraceHidden] is framework-internal
            public void GetResult()
            {
                // this is where we swallowed the task's possible exception
                // still, must observe the exception else it remains unobserved
                if (!_task.IsCompletedSuccessfully()) { _ = _task.Exception; }
            }

            // ConfiguredTaskAwaitable is a simple class that just returns a
            // ConfiguredTaskAwaitable.ConfiguredTaskAwaiter for GetAwaiter()
            //
            // That awaiter invokes the TaskAwaiter.OnCompletedInternal method
            // for OnCompleted and UnsafeOnCompleted - that is all - but that
            // method is internal, so the only way for us to invoke it is
            // through a ConfiguredTaskAwaiter - which only has an internal ctor,
            // so through a ConfiguredTaskAwaitable.
            //
            // so, the methods below use ConfigureAwait(false) to create the
            // correct ConfiguredTaskAwaitable.ConfiguredTaskAwaiter, and get
            // it to complete as expected.
            //
            // references:
            // https://github.com/dotnet/runtime/issues/22144
            // https://github.com/dotnet/runtime/issues/27723

            /// <summary>
            /// Schedules the continuation onto the <see cref="Task"/> associated with this <see cref="TaskAwaiter"/>.
            /// </summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            public void OnCompleted(Action continuation)
                => _task.ConfigureAwait(false).GetAwaiter().OnCompleted(continuation);

            /// <summary>
            /// Schedules the continuation onto the <see cref="Task"/> associated with this <see cref="TaskAwaiter"/>.
            /// </summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            public void UnsafeOnCompleted(Action continuation)
                => _task.ConfigureAwait(false).GetAwaiter().OnCompleted(continuation);
        }

        /// <summary>
        /// Provides an awaitable object that allows for configured awaits on <see cref="ValueTask"/>.
        /// </summary>
        public readonly struct ConfiguredValueTaskNoThrowAwaitable : ICriticalNotifyCompletion
        {
            private readonly ValueTask _task;

            /// <summary>
            /// Initializes a new instance of the <see cref="ConfiguredValueTaskNoThrowAwaitable"/> struct.
            /// </summary>
            /// <param name="task">The <see cref="ValueTask"/>.</param>
            internal ConfiguredValueTaskNoThrowAwaitable(ValueTask task)
            {
                _task = task;
            }

            /// <summary>
            /// Gets the awaiter for this awaitable.
            /// </summary>
            /// <returns>The awaiter.</returns>
            public ConfiguredValueTaskNoThrowAwaitable GetAwaiter() => this;

            /// <summary>
            /// Gets whether the task being awaited is completed.
            /// </summary>
            /// <returns>Whether the task being awaited is completed.</returns>
            public bool IsCompleted
                => _task.IsCompleted;

            /// <summary>Ends the await on the completed <see cref="Task"/>.</summary>
            // [StackTraceHidden] is framework-internal
            public void GetResult()
            {
                // this is where we swallowed the task's possible exception
                // still, must observe the exception else it remains unobserved

                // value tasks are different from tasks: they don't hold the
                // exception, if there's an exception then there has to be
                // an underlying task holding the exception

                if (!_task.IsCompletedSuccessfully) { _ = _task.AsTask().Exception; }
            }

            // see notes in ConfiguredTaskNoThrowAwaitable

            /// <summary>
            /// Schedules the continuation onto the <see cref="Task"/> associated with this <see cref="TaskAwaiter"/>.
            /// </summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            public void OnCompleted(Action continuation)
                => _task.ConfigureAwait(false).GetAwaiter().OnCompleted(continuation);

            /// <summary>
            /// Schedules the continuation onto the <see cref="Task"/> associated with this <see cref="TaskAwaiter"/>.
            /// </summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            public void UnsafeOnCompleted(Action continuation)
                => _task.ConfigureAwait(false).GetAwaiter().OnCompleted(continuation);
        }

        /// <summary>
        /// Provides an awaitable object that allows for configured awaits on <see cref="Task"/>.
        /// </summary>
        public readonly struct ConfiguredTaskNoThrowCanceledAwaitable : ICriticalNotifyCompletion
        {
            private readonly Task _task;

            /// <summary>
            /// Initializes a new instance of the <see cref="ConfiguredTaskNoThrowCanceledAwaitable"/> struct.
            /// </summary>
            /// <param name="task">The <see cref="Task"/>.</param>
            public ConfiguredTaskNoThrowCanceledAwaitable([NotNull] Task task)
            {
                _task = task;
            }

            /// <summary>
            /// Gets the awaiter for this awaitable.
            /// </summary>
            /// <returns>The awaiter.</returns>
            public ConfiguredTaskNoThrowCanceledAwaitable GetAwaiter() => this;

            /// <summary>
            /// Gets whether the task being awaited is completed.
            /// </summary>
            /// <returns>Whether the task being awaited is completed.</returns>
            public bool IsCompleted
                => _task.IsCompleted;

            /// <summary>Ends the await on the completed <see cref="Task"/>.</summary>
            // [StackTraceHidden] is framework-internal
            public void GetResult()
            {
                if (_task.IsCanceled)
                {
                    // this is where we swallowed the task's exception
                    // still, must observe it else it remains unobserved
                    _ = _task.Exception;
                }
                else
                {
                    // whatever happens, happens
                    _task.ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }

            // see notes in ConfiguredTaskNoThrowAwaitable

            /// <summary>
            /// Schedules the continuation onto the <see cref="Task"/> associated with this <see cref="TaskAwaiter"/>.
            /// </summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            public void OnCompleted(Action continuation)
                => _task.ConfigureAwait(false).GetAwaiter().OnCompleted(continuation);

            /// <summary>
            /// Schedules the continuation onto the <see cref="Task"/> associated with this <see cref="TaskAwaiter"/>.
            /// </summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            public void UnsafeOnCompleted(Action continuation)
                => _task.ConfigureAwait(false).GetAwaiter().OnCompleted(continuation);
        }
    }
}
