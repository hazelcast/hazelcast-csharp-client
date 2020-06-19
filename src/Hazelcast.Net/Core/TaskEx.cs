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
    /// Provides utilities for running <see cref="Task"/>.
    /// </summary>
    public static class TaskEx
    {
        // NOTES
        //
        // every 'WithTimeout' method executes the function passed as an argument with a cancellation token that
        // cancels once the timeout is reached - and the function is then expected to abort within a reasonable
        // delay - because we do not want to leave background tasks unattended, we do *not* want to implement
        // timeouts via awaiting a parallel Task.Delay task - because then... we still need to wait for the
        // original task to complete somehow
        //
        // WithTimeout methods accept a TimeSpan timeout parameter which can be
        // - InfiniteTimeSpan (anything <0 ms), in which case WithTimeout simply executes the task with the default
        //     cancellation token, i.e. without a timeout, and without the timeout management overhead
        // - Zero (0 ms), meaning "the default timeout", in which case WithTimeout will try to use the supplied
        //     default timeout, if it is greater than zero, or run without a timeout if it is <= 0
        // - anything else, in which case WithTimeout executes with that timeout
        //
        // if missing, the default timeout is -1, meaning that a zero timeout will mean 'infinite' when no default
        // timeout is supplied
        //
        // also not that we expose these methods publicly and *want* to keep all the overloads (vs using optional
        // parameters) for the same reason MS does it in the BCL (more future-proof)

        private const string NullTaskMessage = "The function produced a null Task.";
        private const string TimeoutMessage = "Operation timed out (see inner exception).";

        // FIXME: implement missing overloads + support for ValueTask

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (CancellationTokenSource, CancellationToken) GetTimeoutCancellation(TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            // is supplied timeout is zero (i.e. 'use default'), use the default value
            var timeoutMilliseconds = (int) timeout.TotalMilliseconds;
            if (timeoutMilliseconds == 0) timeoutMilliseconds = defaultTimeoutMilliseconds;

            // if the obtained timeout is infinite, or if it still zero (in which case
            // somebody probably made a mistake somewhere), run without a timeout
            if (timeoutMilliseconds < 0) return (null, default);

            // else create a cancellation token source which cancels after the timeout delay
            var timeoutCancellation = new CancellationTokenSource(timeoutMilliseconds);
            return (timeoutCancellation, timeoutCancellation.Token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (CancellationTokenSource, CancellationTokenSource, CancellationToken) GetTimeoutCancellation(TimeSpan timeout, int defaultTimeoutMilliseconds, CancellationToken cancellationToken)
        {
            // is supplied timeout is zero (i.e. 'use default'), use the default value
            var timeoutMilliseconds = (int) timeout.TotalMilliseconds;
            if (timeoutMilliseconds == 0) timeoutMilliseconds = defaultTimeoutMilliseconds;

            // if the obtained timeout is infinite, or if it still zero (in which case
            // somebody probably made a mistake somewhere), run without a timeout
            if (timeoutMilliseconds <= 0) return (null, null, default);

            // else create a cancellation token source which cancels after the timeout delay,
            // and combine it with the supplied cancellation token so we also respect it
            var timeoutCancellation = new CancellationTokenSource(timeoutMilliseconds);
            var combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(timeoutCancellation.Token, cancellationToken);
            return (timeoutCancellation, combinedCancellation, combinedCancellation.Token);
        }

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task<TResult> WithTimeout<TResult>(Func<CancellationToken, Task<TResult>> function,
            TimeSpan timeout)
            => WithTimeout(function, timeout, -1);

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static async Task<TResult> WithTimeout<TResult>(Func<CancellationToken, Task<TResult>> function,
            TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var (cancellation, token) = GetTimeoutCancellation(timeout, defaultTimeoutMilliseconds);

            try
            {
                // trusting the function to abort if the token cancels
                var task = function(token);
                if (task == null) throw new InvalidOperationException(NullTaskMessage);
                return await task.CAF();
            }
            catch (OperationCanceledException c)
            {
                if (cancellation == null || !cancellation.IsCancellationRequested)
                    throw; // not caused by the timeout cancellation

                throw new TimeoutException(TimeoutMessage, c);
            }
            finally
            {
                cancellation?.Dispose();
            }
        }

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task<TResult> WithTimeout<TResult, TArg1>(Func<TArg1, CancellationToken, Task<TResult>> function,
            TArg1 arg1,
            TimeSpan timeout)
            => WithTimeout(function, arg1, timeout, -1);

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static async Task<TResult> WithTimeout<TResult, TArg1>(Func<TArg1, CancellationToken, Task<TResult>> function,
            TArg1 arg1,
            TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var (cancellation, token) = GetTimeoutCancellation(timeout, defaultTimeoutMilliseconds);

            try
            {
                // trusting the function to abort if the token cancels
                var task = function(arg1, token);
                if (task == null) throw new InvalidOperationException(NullTaskMessage);
                return await task.CAF();
            }
            catch (OperationCanceledException c)
            {
                if (cancellation == null || !cancellation.IsCancellationRequested)
                    throw; // not caused by the timeout cancellation

                throw new TimeoutException(TimeoutMessage, c);
            }
            finally
            {
                cancellation?.Dispose();
            }
        }

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task<TResult> WithTimeout<TResult, TArg1, TArg2>(Func<TArg1, TArg2, CancellationToken, Task<TResult>> function,
            TArg1 arg1, TArg2 arg2,
            TimeSpan timeout)
            => WithTimeout(function, arg1, arg2, timeout, -1);

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static async Task<TResult> WithTimeout<TResult, TArg1, TArg2>(Func<TArg1, TArg2, CancellationToken, Task<TResult>> function,
            TArg1 arg1, TArg2 arg2,
            TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var (cancellation, token) = GetTimeoutCancellation(timeout, defaultTimeoutMilliseconds);

            try
            {
                // trusting the function to abort if the token cancels
                var task = function(arg1, arg2, token);
                if (task == null) throw new InvalidOperationException(NullTaskMessage);
                return await task.CAF();
            }
            catch (OperationCanceledException c)
            {
                if (cancellation == null || !cancellation.IsCancellationRequested)
                    throw; // not caused by the timeout cancellation

                throw new TimeoutException(TimeoutMessage, c);
            }
            finally
            {
                cancellation?.Dispose();
            }
        }

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <typeparam name="TArg3">The type of the third argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task<TResult> WithTimeout<TResult, TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, CancellationToken, Task<TResult>> function,
            TArg1 arg1, TArg2 arg2, TArg3 arg3,
            TimeSpan timeout)
            => WithTimeout(function, arg1, arg2, arg3, timeout, -1);

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <typeparam name="TArg3">The type of the third argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static async Task<TResult> WithTimeout<TResult, TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, CancellationToken, Task<TResult>> function,
            TArg1 arg1, TArg2 arg2, TArg3 arg3,
            TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var (cancellation, token) = GetTimeoutCancellation(timeout, defaultTimeoutMilliseconds);

            try
            {
                // trusting the function to abort if the token cancels
                var task = function(arg1, arg2, arg3, token);
                if (task == null) throw new InvalidOperationException(NullTaskMessage);
                return await task.CAF();
            }
            catch (OperationCanceledException c)
            {
                if (cancellation == null || !cancellation.IsCancellationRequested)
                    throw; // not caused by the timeout cancellation

                throw new TimeoutException(TimeoutMessage, c);
            }
            finally
            {
                cancellation?.Dispose();
            }
        }

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <typeparam name="TArg3">The type of the third argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task<TResult> WithTimeout<TResult, TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, CancellationToken, Task<TResult>> function,
            TArg1 arg1, TArg2 arg2, TArg3 arg3,
            TimeSpan timeout, CancellationToken cancellationToken)
            => WithTimeout(function, arg1, arg2, arg3, timeout, -1, cancellationToken);

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <typeparam name="TArg3">The type of the third argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static async Task<TResult> WithTimeout<TResult, TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, CancellationToken, Task<TResult>> function,
            TArg1 arg1, TArg2 arg2, TArg3 arg3,
            TimeSpan timeout, int defaultTimeoutMilliseconds, CancellationToken cancellationToken)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var (timeoutCancellation, combinedCancellation, token) = GetTimeoutCancellation(timeout, defaultTimeoutMilliseconds, cancellationToken);

            try
            {
                // trusting the function to abort if the token cancels
                var task = function(arg1, arg2, arg3, token);
                if (task == null) throw new InvalidOperationException(NullTaskMessage);
                return await task.CAF();
            }
            catch (OperationCanceledException c)
            {
                if (timeoutCancellation == null || !timeoutCancellation.IsCancellationRequested)
                    throw; // not caused by the timeout cancellation

                throw new TimeoutException(TimeoutMessage, c);
            }
            finally
            {
                combinedCancellation?.Dispose();
                timeoutCancellation?.Dispose();
            }
        }

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <typeparam name="TArg3">The type of the third argument.</typeparam>
        /// <typeparam name="TArg4">The type of the fourth argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <param name="arg4">The third argument.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task<TResult> WithTimeout<TResult, TArg1, TArg2, TArg3, TArg4>(Func<TArg1, TArg2, TArg3, TArg4, CancellationToken, Task<TResult>> function,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4,
            TimeSpan timeout)
            => WithTimeout(function, arg1, arg2, arg3, arg4, timeout, -1);

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <typeparam name="TArg3">The type of the third argument.</typeparam>
        /// <typeparam name="TArg4">The type of the fourth argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <param name="arg4">The third argument.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static async Task<TResult> WithTimeout<TResult, TArg1, TArg2, TArg3, TArg4>(Func<TArg1, TArg2, TArg3, TArg4, CancellationToken, Task<TResult>> function,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4,
            TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var (cancellation, token) = GetTimeoutCancellation(timeout, defaultTimeoutMilliseconds);

            try
            {
                // trusting the function to abort if the token cancels
                var task = function(arg1, arg2, arg3, arg4, token);
                if (task == null) throw new InvalidOperationException(NullTaskMessage);
                return await task.CAF();
            }
            catch (OperationCanceledException c)
            {
                if (cancellation == null || !cancellation.IsCancellationRequested)
                    throw; // not caused by the timeout cancellation

                throw new TimeoutException(TimeoutMessage, c);
            }
            finally
            {
                cancellation?.Dispose();
            }
        }

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <typeparam name="TArg3">The type of the third argument.</typeparam>
        /// <typeparam name="TArg4">The type of the fourth argument.</typeparam>
        /// <typeparam name="TArg5">The type of the fifth argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <param name="arg4">The fourth argument.</param>
        /// <param name="arg5">The fifth argument.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task<TResult> WithTimeout<TResult, TArg1, TArg2, TArg3, TArg4, TArg5>(Func<TArg1, TArg2, TArg3, TArg4, TArg5, CancellationToken, Task<TResult>> function,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5,
            TimeSpan timeout)
            => WithTimeout(function, arg1, arg2, arg3, arg4, arg5, timeout, -1);

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <typeparam name="TArg3">The type of the third argument.</typeparam>
        /// <typeparam name="TArg4">The type of the fourth argument.</typeparam>
        /// <typeparam name="TArg5">The type of the fifth argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <param name="arg4">The fourth argument.</param>
        /// <param name="arg5">The fifth argument.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static async Task<TResult> WithTimeout<TResult, TArg1, TArg2, TArg3, TArg4, TArg5>(Func<TArg1, TArg2, TArg3, TArg4, TArg5, CancellationToken, Task<TResult>> function,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5,
            TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var (cancellation, token) = GetTimeoutCancellation(timeout, defaultTimeoutMilliseconds);

            try
            {
                // trusting the function to abort if the token cancels
                var task = function(arg1, arg2, arg3, arg4, arg5, token);
                if (task == null) throw new InvalidOperationException(NullTaskMessage);
                return await task.CAF();
            }
            catch (OperationCanceledException c)
            {
                if (cancellation == null || !cancellation.IsCancellationRequested)
                    throw; // not caused by the timeout cancellation

                throw new TimeoutException(TimeoutMessage, c);
            }
            finally
            {
                cancellation?.Dispose();
            }
        }

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <typeparam name="TArg3">The type of the third argument.</typeparam>
        /// <typeparam name="TArg4">The type of the fourth argument.</typeparam>
        /// <typeparam name="TArg5">The type of the fifth argument.</typeparam>
        /// <typeparam name="TArg6">The type of the sixth argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <param name="arg4">The fourth argument.</param>
        /// <param name="arg5">The fifth argument.</param>
        /// <param name="arg6">The sixth argument.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task<TResult> WithTimeout<TResult, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, CancellationToken, Task<TResult>> function,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6,
            TimeSpan timeout)
            => WithTimeout(function, arg1, arg2, arg3, arg4, arg5, arg6, timeout, -1);

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <typeparam name="TArg3">The type of the third argument.</typeparam>
        /// <typeparam name="TArg4">The type of the fourth argument.</typeparam>
        /// <typeparam name="TArg5">The type of the fifth argument.</typeparam>
        /// <typeparam name="TArg6">The type of the sixth argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <param name="arg4">The fourth argument.</param>
        /// <param name="arg5">The fifth argument.</param>
        /// <param name="arg6">The sixth argument.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static async Task<TResult> WithTimeout<TResult, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, CancellationToken, Task<TResult>> function,
            TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6,
            TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var (cancellation, token) = GetTimeoutCancellation(timeout, defaultTimeoutMilliseconds);

            try
            {
                // trusting the function to abort if the token cancels
                var task = function(arg1, arg2, arg3, arg4, arg5, arg6, token);
                if (task == null) throw new InvalidOperationException(NullTaskMessage);
                return await task.CAF();
            }
            catch (OperationCanceledException c)
            {
                if (cancellation == null || !cancellation.IsCancellationRequested)
                    throw; // not caused by the timeout cancellation

                throw new TimeoutException(TimeoutMessage, c);
            }
            finally
            {
                cancellation?.Dispose();
            }
        }

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task WithTimeout(Func<CancellationToken, Task> function,
            TimeSpan timeout)
            => WithTimeout(function, timeout, -1);

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static async Task WithTimeout(Func<CancellationToken, Task> function,
            TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var (cancellation, token) = GetTimeoutCancellation(timeout, defaultTimeoutMilliseconds);

            try
            {
                // trusting the function to abort if the token cancels
                var task = function(token);
                if (task == null) throw new InvalidOperationException(NullTaskMessage);
                await task.CAF();
            }
            catch (OperationCanceledException c)
            {
                if (cancellation == null || !cancellation.IsCancellationRequested)
                    throw; // not caused by the timeout cancellation

                throw new TimeoutException(TimeoutMessage, c);
            }
            finally
            {
                cancellation?.Dispose();
            }
        }

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task WithTimeout<TArg1>(Func<TArg1, CancellationToken, Task> function,
            TArg1 arg1,
            TimeSpan timeout)
            => WithTimeout(function, arg1, timeout, -1);

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static async Task WithTimeout<TArg1>(Func<TArg1, CancellationToken, Task> function,
            TArg1 arg1,
            TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var (cancellation, token) = GetTimeoutCancellation(timeout, defaultTimeoutMilliseconds);

            try
            {
                // trusting the function to abort if the token cancels
                var task = function(arg1, token);
                if (task == null) throw new InvalidOperationException(NullTaskMessage);
                await task.CAF();
            }
            catch (OperationCanceledException c)
            {
                if (cancellation == null || !cancellation.IsCancellationRequested)
                    throw; // not caused by the timeout cancellation

                throw new TimeoutException(TimeoutMessage, c);
            }
            finally
            {
                cancellation?.Dispose();
            }
        }

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task WithTimeout<TArg1, TArg2>(Func<TArg1, TArg2, CancellationToken, Task> function,
            TArg1 arg1, TArg2 arg2,
            TimeSpan timeout)
            => WithTimeout(function, arg1, arg2, timeout, -1);

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static async Task WithTimeout<TArg1, TArg2>(Func<TArg1, TArg2, CancellationToken, Task> function,
            TArg1 arg1, TArg2 arg2,
            TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var (cancellation, token) = GetTimeoutCancellation(timeout, defaultTimeoutMilliseconds);

            try
            {
                // trusting the function to abort if the token cancels
                var task = function(arg1, arg2, token);
                if (task == null) throw new InvalidOperationException(NullTaskMessage);
                await task.CAF();
            }
            catch (OperationCanceledException c)
            {
                if (cancellation == null || !cancellation.IsCancellationRequested)
                    throw; // not caused by the timeout cancellation

                throw new TimeoutException(TimeoutMessage, c);
            }
            finally
            {
                cancellation?.Dispose();
            }
        }

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task WithTimeout<TArg1, TArg2>(Func<TArg1, TArg2, CancellationToken, Task> function,
            TArg1 arg1, TArg2 arg2,
            TimeSpan timeout, CancellationToken cancellationToken)
            => WithTimeout(function, arg1, arg2, timeout, -1, cancellationToken);

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static async Task WithTimeout<TArg1, TArg2>(Func<TArg1, TArg2, CancellationToken, Task> function,
            TArg1 arg1, TArg2 arg2,
            TimeSpan timeout, int defaultTimeoutMilliseconds, CancellationToken cancellationToken)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var (timeoutCancellation, combinedCancellation, token) = GetTimeoutCancellation(timeout, defaultTimeoutMilliseconds, cancellationToken);

            try
            {
                // trusting the function to abort if the token cancels
                var task = function(arg1, arg2, token);
                if (task == null) throw new InvalidOperationException(NullTaskMessage);
                await task.CAF();
            }
            catch (OperationCanceledException c)
            {
                if (timeoutCancellation == null || !timeoutCancellation.IsCancellationRequested)
                    throw; // not caused by the timeout cancellation

                throw new TimeoutException(TimeoutMessage, c);
            }
            finally
            {
                combinedCancellation?.Dispose();
                timeoutCancellation?.Dispose();
            }
        }

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <typeparam name="TArg3">The type of the second argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task WithTimeout<TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, CancellationToken, Task> function,
            TArg1 arg1, TArg2 arg2, TArg3 arg3,
            TimeSpan timeout)
            => WithTimeout(function, arg1, arg2, arg3, timeout, -1);

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second argument.</typeparam>
        /// <typeparam name="TArg3">The type of the second argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static async Task WithTimeout<TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, CancellationToken, Task> function,
            TArg1 arg1, TArg2 arg2, TArg3 arg3,
            TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var (cancellation, token) = GetTimeoutCancellation(timeout, defaultTimeoutMilliseconds);

            try
            {
                // trusting the function to abort if the token cancels
                var task = function(arg1, arg2, arg3, token);
                if (task == null) throw new InvalidOperationException(NullTaskMessage);
                await task.CAF();
            }
            catch (OperationCanceledException c)
            {
                if (cancellation == null || !cancellation.IsCancellationRequested)
                    throw; // not caused by the timeout cancellation

                throw new TimeoutException(TimeoutMessage, c);
            }
            finally
            {
                cancellation?.Dispose();
            }
        }
    }
}