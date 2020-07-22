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
    /// Provides utility methods for running <see cref="ValueTask"/>.
    /// </summary>
    public static class ValueTaskEx
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
        public static ValueTask WithNewContext(Func<ValueTask> function)
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
        public static ValueTask WithNewContext(Func<CancellationToken, ValueTask> function, CancellationToken cancellationToken)
            => AsyncContext.WithNewContextInternal(function, cancellationToken);

        /// <summary>
        /// Starts an asynchronous task with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="function">A function starting and returning an asynchronous task.</param>
        /// <returns>The asynchronous task that was started.</returns>
        /// <remarks>
        /// <para>This is equivalent to doing <c>return function();</c> except that the task starts with a new <see cref="AsyncContext"/></para>
        /// </remarks>
        public static ValueTask<TResult> WithNewContext<TResult>(Func<ValueTask<TResult>> function)
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
        public static ValueTask<TResult> WithNewContext<TResult>(Func<CancellationToken, ValueTask<TResult>> function, CancellationToken cancellationToken)
            => AsyncContext.WithNewContextInternal(function, cancellationToken);
    }
}
