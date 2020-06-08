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
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides utilities for running <see cref="Task"/>.
    /// </summary>
    public static class TaskEx
    {
        // FIXME: also for ValueTask
        // FIXME: more overloads

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task<TResult> WithTimeout<TResult>(Func<CancellationToken, Task<TResult>> function, TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

#pragma warning disable CA2000 // Dispose objects before losing scope - disposed by .OrTimeout(cancellation)
            var cancellation = timeout.AsCancellationTokenSource(defaultTimeoutMilliseconds);
#pragma warning restore CA2000

            Task<TResult> task;
            try
            {
                task = function(cancellation.Token);
            }
            catch (Exception e)
            {
                if (cancellation != TaskExtensions.NeverCanceledSource)
                    cancellation.Dispose();
                return Task.FromException<TResult>(e);
            }

            return task.OrTimeout(cancellation);
        }

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
        public static Task<TResult> WithTimeout<TResult, TArg1>(Func<TArg1, CancellationToken, Task<TResult>> function, TArg1 arg1, TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

#pragma warning disable CA2000 // Dispose objects before losing scope - disposed by .OrTimeout(cancellation)
            var cancellation = timeout.AsCancellationTokenSource(defaultTimeoutMilliseconds);
#pragma warning restore CA2000

            Task<TResult> task;
            try
            {
                task = function(arg1, cancellation.Token);
            }
            catch (Exception e)
            {
                if (cancellation != TaskExtensions.NeverCanceledSource)
                    cancellation.Dispose();
                return Task.FromException<TResult>(e);
            }

            return task.OrTimeout(cancellation);
        }

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
        public static Task<TResult> WithTimeout<TResult, TArg1, TArg2>(Func<TArg1, TArg2, CancellationToken, Task<TResult>> function, TArg1 arg1, TArg2 arg2, TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

#pragma warning disable CA2000 // Dispose objects before losing scope - disposed by .OrTimeout(cancellation)
            var cancellation = timeout.AsCancellationTokenSource(defaultTimeoutMilliseconds);
#pragma warning restore CA2000

            Task<TResult> task;
            try
            {
                task = function(arg1, arg2, cancellation.Token);
            }
            catch (Exception e)
            {
                if (cancellation != TaskExtensions.NeverCanceledSource)
                    cancellation.Dispose();
                return Task.FromException<TResult>(e);
            }

            return task.OrTimeout(cancellation);
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
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task<TResult> WithTimeout<TResult, TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, CancellationToken, Task<TResult>> function, TArg1 arg1, TArg2 arg2, TArg3 arg3, TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

#pragma warning disable CA2000 // Dispose objects before losing scope - disposed by .OrTimeout(cancellation)
            var cancellation = timeout.AsCancellationTokenSource(defaultTimeoutMilliseconds);
#pragma warning restore CA2000

            Task<TResult> task;
            try
            {
                task = function(arg1, arg2, arg3, cancellation.Token);
            }
            catch (Exception e)
            {
                if (cancellation != TaskExtensions.NeverCanceledSource)
                    cancellation.Dispose();
                return Task.FromException<TResult>(e);
            }

            return task.OrTimeout(cancellation);
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
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <param name="arg4">The third argument.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task<TResult> WithTimeout<TResult, TArg1, TArg2, TArg3, TArg4>(Func<TArg1, TArg2, TArg3, TArg4, CancellationToken, Task<TResult>> function, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

#pragma warning disable CA2000 // Dispose objects before losing scope - disposed by .OrTimeout(cancellation)
            var cancellation = timeout.AsCancellationTokenSource(defaultTimeoutMilliseconds);
#pragma warning restore CA2000

            Task<TResult> task;
            try
            {
                task = function(arg1, arg2, arg3, arg4, cancellation.Token);
            }
            catch (Exception e)
            {
                if (cancellation != TaskExtensions.NeverCanceledSource)
                    cancellation.Dispose();
                return Task.FromException<TResult>(e);
            }

            return task.OrTimeout(cancellation);
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
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <param name="arg4">The fourth argument.</param>
        /// <param name="arg5">The fifth argument.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task<TResult> WithTimeout<TResult, TArg1, TArg2, TArg3, TArg4, TArg5>(Func<TArg1, TArg2, TArg3, TArg4, TArg5, CancellationToken, Task<TResult>> function, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

#pragma warning disable CA2000 // Dispose objects before losing scope - disposed by .OrTimeout(cancellation)
            var cancellation = timeout.AsCancellationTokenSource(defaultTimeoutMilliseconds);
#pragma warning restore CA2000

            Task<TResult> task;
            try
            {
                task = function(arg1, arg2, arg3, arg4, arg5, cancellation.Token);
            }
            catch (Exception e)
            {
                if (cancellation != TaskExtensions.NeverCanceledSource)
                    cancellation.Dispose();
                return Task.FromException<TResult>(e);
            }

            return task.OrTimeout(cancellation);
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
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="arg2">The second argument.</param>
        /// <param name="arg3">The third argument.</param>
        /// <param name="arg4">The fourth argument.</param>
        /// <param name="arg5">The fifth argument.</param>
        /// <param name="arg6">The sixth argument.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task<TResult> WithTimeout<TResult, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, CancellationToken, Task<TResult>> function, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

#pragma warning disable CA2000 // Dispose objects before losing scope - disposed by .OrTimeout(cancellation)
            var cancellation = timeout.AsCancellationTokenSource(defaultTimeoutMilliseconds);
#pragma warning restore CA2000

            Task<TResult> task;
            try
            {
                task = function(arg1, arg2, arg3, arg4, arg5, arg6, cancellation.Token);
            }
            catch (Exception e)
            {
                if (cancellation != TaskExtensions.NeverCanceledSource)
                    cancellation.Dispose();
                return Task.FromException<TResult>(e);
            }

            return task.OrTimeout(cancellation);
        }

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <param name="function"></param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task WithTimeout(Func<CancellationToken, Task> function, TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

#pragma warning disable CA2000 // Dispose objects before losing scope - disposed by .OrTimeout(cancellation)
            var cancellation = timeout.AsCancellationTokenSource(defaultTimeoutMilliseconds);
#pragma warning restore CA2000

            Task task;
            try
            {
                task = function(cancellation.Token);
            }
            catch (Exception e)
            {
                if (cancellation != TaskExtensions.NeverCanceledSource)
                    cancellation.Dispose();
                return Task.FromException(e);
            }

            return task.OrTimeout(cancellation);
        }

        /// <summary>
        /// Applies a timeout to a task.
        /// </summary>
        /// <typeparam name="TArg1">The type of the first argument.</typeparam>
        /// <param name="function"></param>
        /// <param name="arg1">The first argument.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task WithTimeout<TArg1>(Func<TArg1, CancellationToken, Task> function, TArg1 arg1, TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

#pragma warning disable CA2000 // Dispose objects before losing scope - disposed by .OrTimeout(cancellation)
            var cancellation = timeout.AsCancellationTokenSource(defaultTimeoutMilliseconds);
#pragma warning restore CA2000

            Task task;
            try
            {
                task = function(arg1, cancellation.Token);
            }
            catch (Exception e)
            {
                if (cancellation != TaskExtensions.NeverCanceledSource)
                    cancellation.Dispose();
                return Task.FromException(e);
            }

            return task.OrTimeout(cancellation);
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
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task WithTimeout<TArg1, TArg2>(Func<TArg1, TArg2, CancellationToken, Task> function, TArg1 arg1, TArg2 arg2, TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

#pragma warning disable CA2000 // Dispose objects before losing scope - disposed by .OrTimeout(cancellation)
            var cancellation = timeout.AsCancellationTokenSource(defaultTimeoutMilliseconds);
#pragma warning restore CA2000

            Task task;
            try
            {
                task = function(arg1, arg2, cancellation.Token);
            }
            catch (Exception e)
            {
                if (cancellation != TaskExtensions.NeverCanceledSource)
                    cancellation.Dispose();
                return Task.FromException(e);
            }

            return task.OrTimeout(cancellation);
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
        /// <param name="defaultTimeoutMilliseconds">The default timeout.</param>
        /// <returns>The task, with a timeout applied to it.</returns>
        public static Task WithTimeout<TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, CancellationToken, Task> function, TArg1 arg1, TArg2 arg2, TArg3 arg3, TimeSpan timeout, int defaultTimeoutMilliseconds)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

#pragma warning disable CA2000 // Dispose objects before losing scope - disposed by .OrTimeout(cancellation)
            var cancellation = timeout.AsCancellationTokenSource(defaultTimeoutMilliseconds);
#pragma warning restore CA2000

            Task task;
            try
            {
                task = function(arg1, arg2, arg3, cancellation.Token);
            }
            catch (Exception e)
            {
                if (cancellation != TaskExtensions.NeverCanceledSource)
                    cancellation.Dispose();
                return Task.FromException(e);
            }

            return task.OrTimeout(cancellation);
        }
    }
}