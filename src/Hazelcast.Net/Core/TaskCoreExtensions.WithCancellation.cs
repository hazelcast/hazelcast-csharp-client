// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Core
{
    internal static partial class TaskCoreExtensions
    {
        public static ConfiguredCancelableAsyncEnumerable<T> WithCancellation<T>(this IAsyncEnumerable<T> enumerable, bool throwOnCancel, CancellationToken cancellationToken)
            => new ConfiguredCancelableAsyncEnumerable<T>(enumerable, continueOnCapturedContext: true, throwOnCancel, cancellationToken);

        // this is a direct copy of ConfiguredAsyncEnumerable<T>
        // with added support for 'throwOnCancel'

        /// <summary>Provides an awaitable async enumerable that enables cancelable iteration and configured awaits.</summary>
        [StructLayout(LayoutKind.Auto)]
        public readonly struct ConfiguredCancelableAsyncEnumerable<T>
        {
            private readonly IAsyncEnumerable<T> _enumerable;
            private readonly CancellationToken _cancellationToken;
            private readonly bool _continueOnCapturedContext;
            public readonly bool _throwOnCancel;

            internal ConfiguredCancelableAsyncEnumerable(IAsyncEnumerable<T> enumerable, bool continueOnCapturedContext, bool throwOnCancel, CancellationToken cancellationToken)
            {
                _enumerable = enumerable;
                _continueOnCapturedContext = continueOnCapturedContext;
                _cancellationToken = cancellationToken;
                _throwOnCancel = throwOnCancel;
            }

            /// <summary>Configures how awaits on the tasks returned from an async iteration will be performed.</summary>
            /// <param name="continueOnCapturedContext">Whether to capture and marshal back to the current context.</param>
            /// <returns>The configured enumerable.</returns>
            /// <remarks>This will replace any previous value set by <see cref="ConfigureAwait(bool)"/> for this iteration.</remarks>
            public ConfiguredCancelableAsyncEnumerable<T> ConfigureAwait(bool continueOnCapturedContext) =>
                new ConfiguredCancelableAsyncEnumerable<T>(_enumerable, continueOnCapturedContext, _throwOnCancel, _cancellationToken);

            /// <summary>Sets the <see cref="CancellationToken"/> to be passed to <see cref="IAsyncEnumerable{T}.GetAsyncEnumerator(CancellationToken)"/> when iterating.</summary>
            /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use.</param>
            /// <returns>The configured enumerable.</returns>
            /// <remarks>This will replace any previous <see cref="CancellationToken"/> set by <see cref="WithCancellation(CancellationToken)"/> for this iteration.</remarks>
            public ConfiguredCancelableAsyncEnumerable<T> WithCancellation(CancellationToken cancellationToken) =>
                new ConfiguredCancelableAsyncEnumerable<T>(_enumerable, _continueOnCapturedContext, throwOnCancel: true, cancellationToken);

            public Enumerator GetAsyncEnumerator() =>
                // as with other "configured" awaitable-related type in CompilerServices, we don't null check to defend against
                // misuse like `default(ConfiguredCancelableAsyncEnumerable<T>).GetAsyncEnumerator()`, which will null ref by design.
                new Enumerator(_enumerable.GetAsyncEnumerator(_cancellationToken), _continueOnCapturedContext, _throwOnCancel);

            /// <summary>Provides an awaitable async enumerator that enables cancelable iteration and configured awaits.</summary>
            [StructLayout(LayoutKind.Auto)]
            public readonly struct Enumerator
            {
                private readonly IAsyncEnumerator<T> _enumerator;
                private readonly bool _continueOnCapturedContext;
                private readonly bool _throwOnCancel;

                internal Enumerator(IAsyncEnumerator<T> enumerator, bool continueOnCapturedContext, bool throwOnCancel)
                {
                    _enumerator = enumerator;
                    _continueOnCapturedContext = continueOnCapturedContext;
                    _throwOnCancel = throwOnCancel;
                }

                /// <summary>Advances the enumerator asynchronously to the next element of the collection.</summary>
                /// <returns>
                /// A <see cref="ValueTask{Boolean}"/> that will complete with a result of <c>true</c>
                /// if the enumerator was successfully advanced to the next element, or <c>false</c> if the enumerator has
                /// passed the end of the collection.
                /// </returns>
                //public ConfiguredValueTaskAwaitable2<bool> MoveNextAsync()
                public async ValueTask<bool> MoveNextAsync()
                {
                    // was: _enumerator.MoveNextAsync().ConfigureAwait(_continueOnCapturedContext);
                    //
                    // must return anything that can be awaited ie has a GetAwaiter() method
                    // this probably is OK *but* it creates an async state machine + try/catch
                    // we'd rather want to implement our own ConfiguredValueTaskAwaitable
                    // but - that beast is tricky now?
                    //
                    // TODO: ?

                    try
                    {
                        return await _enumerator.MoveNextAsync().ConfigureAwait(_continueOnCapturedContext);
                    }
                    catch (OperationCanceledException)
                    {
                        if (_throwOnCancel) throw;
                        return false;
                    }
                }

                /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
                public T Current => _enumerator.Current;

                /// <summary>
                /// Performs application-defined tasks associated with freeing, releasing, or
                /// resetting unmanaged resources asynchronously.
                /// </summary>
                public ConfiguredValueTaskAwaitable DisposeAsync() =>
                    _enumerator.DisposeAsync().ConfigureAwait(_continueOnCapturedContext);
            }
        }
    }
}
