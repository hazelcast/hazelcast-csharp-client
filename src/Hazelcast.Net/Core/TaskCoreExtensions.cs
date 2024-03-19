// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
    /// and to the <see cref="ValueTask"/> and <see cref="ValueTask{TResult}"/> structs.
    /// </summary>
    /// <remarks>
    /// <para>See: https://devblogs.microsoft.com/dotnet/configureawait-faq/.</para>
    /// </remarks>
    internal static partial class TaskCoreExtensions
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
#if NET8_0_OR_GREATER
            => task.ConfigureAwait(continueOnCapturedContext: false);
#else
            => task.ConfigureAwait(false);
#endif
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
        /// Configures an awaiter used to await this <see cref="Task"/>, continuing on
        /// any synchronization context, and not throwing any exception.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="exceptionValue">The value to return in case of an exception.</param>
        /// <returns>An object used to await this <see cref="Task{T}"/>.</returns>
        /// <remarks>
        /// <para>This is equivalent to <c>ConfigureAwait(false)</c>, and in addition
        /// any exception thrown by the task is swallowed and observed (in order not
        /// to become an unobserved exception) and the <paramref name="exceptionValue"/> is
        /// returned.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskNoThrowAwaitable<T> CfAwaitNoThrow<T>([NotNull] this Task<T> task, T exceptionValue)
            => new ConfiguredTaskNoThrowAwaitable<T>(task, exceptionValue);

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
        /// Configures an awaiter used to await this <see cref="ValueTask{T}"/>, continuing on
        /// any synchronization context, and not throwing any exception.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="exceptionValue">The value to return in case of an exception.</param>
        /// <returns>An object used to await this <see cref="ValueTask{T}"/>.</returns>
        /// <remarks>
        /// <para>This is equivalent to <c>ConfigureAwait(false)</c>, and in addition
        /// any exception thrown by the task is swallowed and observed (in order not
        /// to become an unobserved exception) and the <paramref name="exceptionValue"/> is
        /// returned.</para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredValueTaskNoThrowAwaitable<T> CfAwaitNoThrow<T>(this ValueTask<T> task, T exceptionValue)
            => new ConfiguredValueTaskNoThrowAwaitable<T>(task, exceptionValue);

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
            => AwaitWithTimeout(task, timeout, cancellation).ConfigureAwait(false);

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
            => AwaitWithTimeout(task, TimeSpan.FromMilliseconds(timeoutMilliseconds), cancellation).ConfigureAwait(false);

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
            => AwaitWithTimeout(task, timeout, cancellation).ConfigureAwait(false);

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
            => AwaitWithTimeout(task, TimeSpan.FromMilliseconds(timeoutMilliseconds), cancellation).ConfigureAwait(false);

        private static async Task AwaitWithTimeout(Task task, TimeSpan timeout, CancellationTokenSource? cancellation)
        {
            try
            {
                await task.WaitAsync(timeout).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                cancellation?.Cancel(); // signal the task it should cancel
                task.ObserveException(); // ensure we don't leak an unobserved exception
                throw new TaskTimeoutException(ExceptionMessages.Timeout, task);
            }
        }

#if !NET6_0_OR_GREATER
        // .NET 6 introduced the built-in WaitAsync method so let use use it everywhere,
        // and for pre-.NET 6 let us use MS' own code from https://github.com/dotnet/runtime/pull/48842

        private static async Task WaitAsync(this Task task, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (new Timer(s => ((TaskCompletionSource<bool>)s!).TrySetException(new TimeoutException()), tcs, timeout, Timeout.InfiniteTimeSpan))
            {
                await (await Task.WhenAny(task, tcs.Task).ConfigureAwait(false)).ConfigureAwait(false);
            }
        }

        public static async Task WaitAsync(this Task task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using var reg = cancellationToken.Register(() => tcs.TrySetCanceled());
            await Task.WhenAny(task, tcs.Task).ConfigureAwait(false);
        }

        private static async Task<TResult> WaitAsync<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<TResult>();
            using (new Timer(s => ((TaskCompletionSource<bool>)s!).TrySetException(new TimeoutException()), tcs, timeout, Timeout.InfiniteTimeSpan))
            {
                return await (await Task.WhenAny(task, tcs.Task).ConfigureAwait(false)).ConfigureAwait(false);
            }
        }
#endif

        private static async Task<TResult> AwaitWithTimeout<TResult>(Task<TResult> task, TimeSpan timeout, CancellationTokenSource? cancellation)
        {
            try
            {
                return await task.WaitAsync(timeout).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
                cancellation?.Cancel(); // signal the task it should cancel
                task.ObserveException(); // ensure we don't leak an unobserved exception
                throw new TaskTimeoutException(ExceptionMessages.Timeout, task);
            }
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
                if (!_task.IsCompletedSuccessfully())
                {
                    _ = _task.Exception;
                }
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
        /// Provides an awaitable object that allows for configured awaits on <see cref="Task{T}"/>.
        /// </summary>
        public readonly struct ConfiguredTaskNoThrowAwaitable<T> : ICriticalNotifyCompletion
        {
            private readonly Task<T> _task;
            private readonly T _exceptionValue;

            /// <summary>
            /// Initializes a new instance of the <see cref="ConfiguredTaskNoThrowAwaitable{T}"/> struct.
            /// </summary>
            /// <param name="task">The <see cref="Task{T}"/>.</param>
            /// <param name="exceptionValue">The value to return in case of an exception.</param>
            internal ConfiguredTaskNoThrowAwaitable([NotNull] Task<T> task, T exceptionValue)
            {
                _task = task;
                _exceptionValue = exceptionValue;
            }

            /// <summary>
            /// Gets the awaiter for this awaitable.
            /// </summary>
            /// <returns>The awaiter.</returns>
            public ConfiguredTaskNoThrowAwaitable<T> GetAwaiter() => this;

            /// <summary>
            /// Gets whether the task being awaited is completed.
            /// </summary>
            /// <returns>Whether the task being awaited is completed.</returns>
            public bool IsCompleted
                => _task.IsCompleted;

            /// <summary>Ends the await on the completed <see cref="Task"/>.</summary>
            // [StackTraceHidden] is framework-internal
            public T GetResult()
            {
                // this is where we swallowed the task's possible exception
                // still, must observe the exception else it remains unobserved
                if (_task.IsCompletedSuccessfully()) return _task.GetAwaiter().GetResult();
                _ = _task.Exception;
                return _exceptionValue;
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

                if (!_task.IsCompletedSuccessfully)
                {
                    _ = _task.AsTask().Exception;
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

        /// <summary>
        /// Provides an awaitable object that allows for configured awaits on <see cref="ValueTask{T}"/>.
        /// </summary>
        public readonly struct ConfiguredValueTaskNoThrowAwaitable<T> : ICriticalNotifyCompletion
        {
            private readonly ValueTask<T> _task;
            private readonly T _exceptionValue;

            /// <summary>
            /// Initializes a new instance of the <see cref="ConfiguredTaskNoThrowAwaitable{T}"/> struct.
            /// </summary>
            /// <param name="task">The <see cref="Task{T}"/>.</param>
            /// <param name="exceptionValue">The value to return in case of an exception.</param>
            internal ConfiguredValueTaskNoThrowAwaitable([NotNull] ValueTask<T> task, T exceptionValue)
            {
                _task = task;
                _exceptionValue = exceptionValue;
            }

            /// <summary>
            /// Gets the awaiter for this awaitable.
            /// </summary>
            /// <returns>The awaiter.</returns>
            public ConfiguredValueTaskNoThrowAwaitable<T> GetAwaiter() => this;

            /// <summary>
            /// Gets whether the task being awaited is completed.
            /// </summary>
            /// <returns>Whether the task being awaited is completed.</returns>
            public bool IsCompleted
                => _task.IsCompleted;

            /// <summary>Ends the await on the completed <see cref="Task"/>.</summary>
            // [StackTraceHidden] is framework-internal
            public T GetResult()
            {
                // this is where we swallowed the task's possible exception
                // still, must observe the exception else it remains unobserved
                if (_task.IsCompletedSuccessfully()) return _task.GetAwaiter().GetResult();
                _ = _task.AsTask().Exception;
                return _exceptionValue;
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
