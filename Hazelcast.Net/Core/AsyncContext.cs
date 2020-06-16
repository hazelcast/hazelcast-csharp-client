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
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
#if HZ_CONSOLE

#endif

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents an ambient context that is local to a given asynchronous control flow, such as an asynchronous method.
    /// </summary>
    public sealed class AsyncContext
    {
        private static readonly ISequence<long> IdSequence = new Int64Sequence();

#if !HZ_CONSOLE
        private static readonly AsyncLocal<AsyncContext> Current = new AsyncLocal<AsyncContext>();
#else
        private static readonly AsyncLocal<AsyncContext> Current = new AsyncLocal<AsyncContext>(ValueChangedHandler);

        private static AsyncContext HzConsoleObject { get; } = new AsyncContext();

        private static void ValueChangedHandler(AsyncLocalValueChangedArgs<AsyncContext> obj)
        {
            static string ToString(AsyncContext context) => context?.Id.ToString(CultureInfo.InvariantCulture) ?? "x";

            HConsole.WriteLine(HzConsoleObject, $"AsyncContext [{Thread.CurrentThread.ManagedThreadId:00}] {ToString(obj.PreviousValue)} -> {ToString(obj.CurrentValue)} {(obj.ThreadContextChanged ? "(execution context change)" : "")}");
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncContext"/> class.
        /// </summary>
        private AsyncContext()
        {
            Id = IdSequence.GetNext();
        }

        /// <summary>
        /// Gets the unique identifier for the current context.
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the current asynchronous context is in a transaction.
        /// </summary>
        public bool InTransaction { get; set; } // see discussion in TransactionContext

        /// <summary>
        /// Gets the current context.
        /// </summary>
        public static AsyncContext CurrentContext => Current.Value ??= new AsyncContext();

        /// <summary>
        /// Ensures that a context exists.
        /// </summary>
        internal static void Ensure()
        {
            Current.Value ??= new AsyncContext();
        }

        /// <summary>
        /// Queues the specified work to run on the thread pool (same as <see cref="Task.Run(Action)"/>) with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        /// <returns>A task that represents the work queued to execute in the thread pool.</returns>
        public static Task RunDetached(Action action)
            => Detached(() => Task.Run(action));

        /// <summary>
        /// Queues the specified work to run on the thread pool (same as <see cref="Task.Run(Action, CancellationToken)"/>) with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="action">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the work queued to execute in the thread pool.</returns>
        public static Task RunDetached(Action action, CancellationToken cancellationToken)
            => Detached(() => Task.Run(action, cancellationToken));

        /// <summary>
        /// Queues the specified work to run on the thread pool (same as <see cref="Task.Run(Func{TResult})"/>) with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <returns>A task that represents the work queued to execute in the thread pool.</returns>
        public static Task<TResult> RunDetached<TResult>(Func<TResult> function)
            => Detached(() => Task.Run(function));

        /// <summary>
        /// Queues the specified work to run on the thread pool (same as <see cref="Task.Run(Func{TResult}, CancellationToken)"/>) with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the work queued to execute in the thread pool.</returns>
        public static Task<TResult> RunDetached<TResult>(Func<TResult> function, CancellationToken cancellationToken)
            => Detached(() => Task.Run(function, cancellationToken));

        /// <summary>
        /// Queues the specified work to run on the thread pool (same as <see cref="Task.Run(Func{Task})"/>) with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <returns>A task that represents the work queued to execute in the thread pool.</returns>
        public static Task RunDetached(Func<Task> function)
            => Detached(() => Task.Run(function));

        /// <summary>
        /// Queues the specified work to run on the thread pool (same as <see cref="Task.Run(Func{Task}, CancellationToken)"/>) with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the work queued to execute in the thread pool.</returns>
        public static Task RunDetached(Func<Task> function, CancellationToken cancellationToken)
            => Detached(() => Task.Run(function, cancellationToken));

        /// <summary>
        /// Queues the specified work to run on the thread pool (same as <see cref="Task.Run(Func{Task{TResult}})"/>) with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <returns>A task that represents the work queued to execute in the thread pool.</returns>
        public static Task<TResult> RunDetached<TResult>(Func<Task<TResult>> function)
            => Detached(() => Task.Run(function));

        /// <summary>
        /// Queues the specified work to run on the thread pool (same as <see cref="Task.Run(Func{Tash{TResult}}, CancellationToken)"/>) with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="function">The work to execute asynchronously.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the work queued to execute in the thread pool.</returns>
        public static Task<TResult> RunDetached<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken)
            => Detached(() => Task.Run(function, cancellationToken));

        /// <summary>
        /// Executes a function with a new context.
        /// </summary>
        /// <param name="function">The function to execute.</param>
        /// <returns>The result of the function.</returns>
        private static T Detached<T>(Func<T> function)
        {
            var savedContext = Current.Value;
            Current.Value = new AsyncContext();
            try
            {
                return function();
            }
            finally
            {
                Current.Value = savedContext;
            }
        }
    }
}
