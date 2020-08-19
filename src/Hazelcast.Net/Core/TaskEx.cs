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
        public static Task WithNewContext(Func<Task> function)
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
        public static Task WithNewContext(Func<CancellationToken, Task> function, CancellationToken cancellationToken)
            => AsyncContext.WithNewContextInternal(function, cancellationToken);

        /// <summary>
        /// Starts an asynchronous task with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="function">A function starting and returning an asynchronous task.</param>
        /// <returns>The asynchronous task that was started.</returns>
        /// <remarks>
        /// <para>This is equivalent to doing <c>return function();</c> except that the task starts with a new <see cref="AsyncContext"/></para>
        /// </remarks>
        public static Task<TResult> WithNewContext<TResult>(Func<Task<TResult>> function)
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
        public static Task<TResult> WithNewContext<TResult>(Func<CancellationToken, Task<TResult>> function, CancellationToken cancellationToken)
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
    }
}
