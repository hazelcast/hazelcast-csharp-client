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
#if HZ_CONSOLE
using System.Globalization;
#endif

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents an ambient context that is local to a given asynchronous control flow, such as an asynchronous method.
    /// </summary>
    public sealed class AsyncContext
    {
        // the sequence of unique identifiers for contexts
        private static ISequence<long> _idSequence = new Int64Sequence();

#if !HZ_CONSOLE
        private static readonly AsyncLocal<AsyncContext> Current = new AsyncLocal<AsyncContext>();
#else
        private static readonly AsyncLocal<AsyncContext> Current = new AsyncLocal<AsyncContext>(ValueChangedHandler);

        private static AsyncContext HConsoleObject { get; } = new AsyncContext();

        private static void ValueChangedHandler(AsyncLocalValueChangedArgs<AsyncContext> obj)
        {
            static string ToString(AsyncContext context) => context?.Id.ToString(CultureInfo.InvariantCulture) ?? "x";

            HConsole.WriteLine(HConsoleObject, 10, $"AsyncContext [{Thread.CurrentThread.ManagedThreadId:00}] {ToString(obj.PreviousValue)} -> {ToString(obj.CurrentValue)} {(obj.ThreadContextChanged ? "(execution context change)" : "")}\n{Environment.StackTrace}");
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncContext"/> class.
        /// </summary>
        private AsyncContext()
        {
            // assign the unique identifier using the sequence
            Id = _idSequence.GetNext();
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
        /// Whether there is a current context.
        /// </summary>
        internal static bool HasCurrent => Current.Value != null;

        /// <summary>
        /// Clears the current context (removes it).
        /// </summary>
        internal static void ClearCurrent() => Current.Value = null;

        /// <summary>
        /// (internal for tests only)
        /// Resets the sequence of unique identifiers.
        /// </summary>
        internal static void ResetSequence()
        {
            _idSequence = new Int64Sequence();
        }

        /// <summary>
        /// Ensures that a context exists.
        /// </summary>
        internal static void Ensure()
        {
            Current.Value ??= new AsyncContext();
        }

        /// <summary>
        /// (internal for tests only)
        /// Ensures that a context exists.
        /// </summary>
        internal static void EnsureNew()
        {
            Current.Value = new AsyncContext();
        }

        private static TTask RunWithNew<TTask>(Workload<TTask> workload)
            where TTask : Task
        {
            if (workload == null) throw new ArgumentNullException(nameof(workload));

            var ec = ExecutionContext.Capture();

            // may be null if execution flow has been suppressed - don't do it
            if (ec == null) throw new InvalidOperationException("Cannot run without an ExecutionContext. Has execution flow been suspended?");

            ExecutionContext.Run(ec, state =>
            {
                EnsureNew(); // force a new context
                ((Workload) state).Start();
            }, workload);

            return workload.Task;
        }

        #region Task

        /// <summary>
        /// Starts an asynchronous task with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="function">A function starting and returning an asynchronous task.</param>
        /// <returns>The asynchronous task that was started.</returns>
        public static Task RunWithNew(Func<Task> function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            return RunWithNew(Workload.Create(function));
        }

        /// <summary>
        /// Starts an asynchronous task with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <typeparam name="TState">The type of the state object.</typeparam>
        /// <param name="function">A function starting and returning an asynchronous task.</param>
        /// <param name="state">A state object that will be passed to the function.</param>
        /// <returns>The asynchronous task that was started.</returns>
        public static Task RunWithNew<TState>(Func<TState, Task> function, TState state)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            return RunWithNew(Workload.Create(function, state));
        }

        /// <summary>
        /// Starts an asynchronous task with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="function">A function starting and returning an asynchronous task.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The asynchronous task that was started.</returns>
        public static Task RunWithNew(Func<CancellationToken, Task> function, CancellationToken cancellationToken)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            return RunWithNew(Workload.Create(function, cancellationToken));
        }

        /// <summary>
        /// Starts an asynchronous task with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="function">A function starting and returning an asynchronous task.</param>
        /// <param name="state">A state object that will be passed to the function.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The asynchronous task that was started.</returns>
        public static Task RunWithNew<TState>(Func<TState, CancellationToken, Task> function, TState state, CancellationToken cancellationToken)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            return RunWithNew(Workload.Create(function, state, cancellationToken));
        }

        #endregion

        #region Task<TResult>

        /// <summary>
        /// Starts an asynchronous task with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <param name="function">A function starting and returning an asynchronous task.</param>
        /// <returns>The asynchronous task that was started.</returns>
        public static Task<TResult> RunWithNew<TResult>(Func<Task<TResult>> function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            return RunWithNew(Workload.Create(function));
        }

        /// <summary>
        /// Starts an asynchronous task with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <typeparam name="TState">The type of the state object.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <param name="function">A function starting and returning an asynchronous task.</param>
        /// <param name="state">A state object that will be passed to the function.</param>
        /// <returns>The asynchronous task that was started.</returns>
        public static Task<TResult> RunWithNew<TState, TResult>(Func<TState, Task<TResult>> function, TState state)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            return RunWithNew(Workload.Create(function, state));
        }

        /// <summary>
        /// Starts an asynchronous task with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <param name="function">A function starting and returning an asynchronous task.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The asynchronous task that was started.</returns>
        public static Task<TResult> RunWithNew<TResult>(Func<CancellationToken, Task<TResult>> function, CancellationToken cancellationToken)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            return RunWithNew(Workload.Create(function, cancellationToken));
        }

        /// <summary>
        /// Starts an asynchronous task with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <typeparam name="TState">The type of the state object.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the task.</typeparam>
        /// <param name="function">A function starting and returning an asynchronous task.</param>
        /// <param name="state">A state object that will be passed to the function.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The asynchronous task that was started.</returns>
        public static Task<TResult> RunWithNew<TState, TResult>(Func<TState, CancellationToken, Task<TResult>> function, TState state, CancellationToken cancellationToken)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            return RunWithNew(Workload.Create(function, state, cancellationToken));
        }

        #endregion

        #region Internals

        private abstract class Workload
        {
            public static Workload<TTask> Create<TTask>(Func<TTask> function)
                where TTask : Task
                => new SimpleWorkload<TTask>(function);

            public static Workload<TTask> Create<TState, TTask>(Func<TState, TTask> function, TState state)
                where TTask : Task
                => new SimpleWorkload<TState, TTask>(function, state);

            public static Workload<TTask> Create<TTask>(Func<CancellationToken, TTask> function, CancellationToken cancellationToken)
                where TTask : Task
                => new CancellableWorkload<TTask>(function, cancellationToken);

            public static Workload<TTask> Create<TState, TTask>(Func<TState, CancellationToken, TTask> function, TState state, CancellationToken cancellationToken)
                where TTask : Task
                => new CancellableWorkload<TState, TTask>(function, state, cancellationToken);

            public abstract void Start();
        }

        #endregion

        #region Task Workload

        private abstract class Workload<TTask> : Workload
            where TTask : Task
        {
            public TTask Task { get; protected set; }
        }

        private class SimpleWorkload<TTask> : Workload<TTask>
            where TTask : Task
        {
            private readonly Func<TTask> _function;

            public SimpleWorkload(Func<TTask> function)
            {
                _function = function;
            }

            public override void Start() => Task = _function();
        }

        private class SimpleWorkload<TState, TTask> : Workload<TTask>
            where TTask : Task
        {
            private readonly Func<TState, TTask> _function;
            private readonly TState _state;

            public SimpleWorkload(Func<TState, TTask> function, TState state)
            {
                _function = function;
                _state = state;
            }

            public override void Start() => Task = _function(_state);
        }

        private class CancellableWorkload<TTask> : Workload<TTask>
            where TTask : Task
        {
            private readonly Func<CancellationToken, TTask> _function;
            private readonly CancellationToken _cancellationToken;

            public CancellableWorkload(Func<CancellationToken, TTask> function, CancellationToken cancellationToken)
            {
                _function = function;
                _cancellationToken = cancellationToken;
            }

            public override void Start() => Task = _function(_cancellationToken);
        }

        private class CancellableWorkload<TState, TTask> : Workload<TTask>
            where TTask : Task
        {
            private readonly Func<TState, CancellationToken, TTask> _function;
            private readonly TState _state;
            private readonly CancellationToken _cancellationToken;

            public CancellableWorkload(Func<TState, CancellationToken, TTask> function, TState state, CancellationToken cancellationToken)
            {
                _function = function;
                _state = state;
                _cancellationToken = cancellationToken;
            }

            public override void Start() => Task = _function(_state, _cancellationToken);
        }

        #endregion
    }
}
