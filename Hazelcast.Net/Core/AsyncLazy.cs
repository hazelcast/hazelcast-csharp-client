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
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents an asynchronous lazy.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    internal sealed class AsyncLazy<T>
    {
        private volatile State _state; // volatile ref set to null after _value has been set
        private Func<Task<T>> _asyncFactory; // set to null once used to allow GC to clean it up
        private T _value; // stores the value

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy{T}"/> class.
        /// </summary>
        /// <param name="asyncFactory">An asynchronous value factory.</param>
        public AsyncLazy(Func<Task<T>> asyncFactory)
        {
            _asyncFactory = asyncFactory;
            _state = State.Wait;
        }

        /// <summary>
        /// Represents the state of the async lazy.
        /// </summary>
        private class State
        {
            private readonly ExceptionDispatchInfo _exceptionDispatch;

            /// <summary>
            /// Initializes a new instance of the <see cref="State"/> class.
            /// </summary>
            private State()
            { }

            /// <summary>
            /// Initializes a new instance of the <see cref="State"/> class.
            /// </summary>
            /// <param name="e">An exception.</param>
            public State(Exception e)
            {
                _exceptionDispatch = ExceptionDispatchInfo.Capture(e);
            }

            /// <summary>
            /// Gets the 'wait' state singleton.
            /// </summary>
            public static State Wait { get; } = new State();

            /// <summary>
            /// Rethrows a captured exception.
            /// </summary>
            public void ThrowException()
            {
                if (_exceptionDispatch == null) throw new InvalidOperationException();
                _exceptionDispatch.Throw();
            }
        }

        /// <summary>
        /// Creates and returns the value.
        /// </summary>
        /// <returns>The value.</returns>
        /// <remarks>
        /// <para>This method must be invoked once only, other invocations would throw.</para>
        /// </remarks>
        public async Task<T> CreateValueAsync()
        {
            try
            {
                // this can run only once
                var asyncFactory = _asyncFactory;
                if (asyncFactory == null) throw new InvalidOperationException();
                _asyncFactory = null;

                _value = await asyncFactory();
                _state = null; // volatile write must occur after setting _value

                return _value;
            }
            catch (Exception e)
            {
                _state = new State(e);
                throw;
            }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <remarks>
        /// <para>Wait for <see cref="CreateValueAsync"/> to complete with a value. Rethrows
        /// any exception that would have been thrown during the creation of the value.</para>
        /// </remarks>
        public T Value => _state == null ? _value : WaitForValue();

        /// <summary>
        /// Waits for the value to become available.
        /// </summary>
        /// <returns>The value.</returns>
        private T WaitForValue()
        {
            var state = _state;

            if (state == State.Wait)
            {
                // wait for a value
                // this is how Lazy.cs waits in .NET Core
                var spinWait = new SpinWait();
                while (_state == State.Wait) spinWait.SpinOnce();
            }

            _state?.ThrowException();

            return _value;
        }
    }
}