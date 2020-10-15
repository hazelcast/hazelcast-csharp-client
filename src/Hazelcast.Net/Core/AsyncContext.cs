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

            HConsole.WriteLine(HConsoleObject, 10, $"AsyncContext [{Thread.CurrentThread.ManagedThreadId:00}] {ToString(obj.PreviousValue)} -> {ToString(obj.CurrentValue)} {(obj.ThreadContextChanged ? "(execution context change)" : "")}");
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

        /// <summary>
        /// Starts an asynchronous task with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="function">A function starting and returning an asynchronous task.</param>
        /// <returns>The asynchronous task that was started.</returns>
        internal static T WithNewContextInternal<T>(Func<T> function)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

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

        /// <summary>
        /// Starts an asynchronous task with a new <see cref="AsyncContext"/>.
        /// </summary>
        /// <param name="function">A function starting and returning an asynchronous task.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The asynchronous task that was started.</returns>
        internal static T WithNewContextInternal<T>(Func<CancellationToken, T> function, CancellationToken cancellationToken)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));

            var savedContext = Current.Value;
            Current.Value = new AsyncContext();
            try
            {
                return function(cancellationToken);
            }
            finally
            {
                Current.Value = savedContext;
            }
        }
    }
}
