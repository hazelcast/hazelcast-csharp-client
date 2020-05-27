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

#nullable enable

using System;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents the result of attempting an operation to produce a result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <remarks>
    /// <para>An <see cref="Attempt{TResult}"/> is either successful or failed, it
    /// carries a <typeparamref name="TResult"/> result, and an exception.</para>
    /// </remarks>
    public readonly struct Attempt<TResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Attempt{TResult}"/> struct.
        /// </summary>
        /// <param name="success">Whether the attempt succeeded.</param>
        /// <param name="value">The optional value of the result.</param>
        /// <param name="exception">An optional captured exception.</param>
        internal Attempt(bool success, TResult value = default, Exception? exception = default)
        {
            Success = success;
            Value = value;
            Exception = exception;
        }

        /// <summary>
        /// Represents a failed attempt with no result and no exception.
        /// </summary>
        public static Attempt<TResult> Failed { get; } = new Attempt<TResult>(false);

        /// <summary>
        /// Gets a value indicating whether the attempt succeeded.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the value of the result.
        /// </summary>
        public TResult Value { get; }

        /// <summary>
        /// Gets the value of the result, if successful, else another value.
        /// </summary>
        /// <param name="other">The other value.</param>
        /// <returns>The value of the result, if successful, else the specified value.</returns>
        public TResult ValueOr(TResult other)
            => Success ? Value : other;

        /// <summary>
        /// Gets a captured exception.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Gets a value indicating whether the attempt contains an exception.
        /// </summary>
        public bool HasException => Exception != null;

        /// <summary>
        /// Implicitly converts an attempt into a boolean.
        /// </summary>
        /// <param name="attempt">The attempt.</param>
        public static implicit operator bool(Attempt<TResult> attempt)
            => attempt.Success;

        /// <summary>
        /// Implicitly converts an attempt into its result.
        /// </summary>
        /// <param name="attempt">The attempt.</param>
        public static implicit operator TResult(Attempt<TResult> attempt)
            => attempt.Value;

        /// <summary>
        /// Implicitly converts a non-generic failed attempt into a generic one.
        /// </summary>
        /// <param name="failedAttempt">The failed attempt.</param>
        /// <remarks>
        /// <para>The only non-generic attempt is the failed attempt.</para>
        /// </remarks>
#pragma warning disable IDE0060 // Remove unused parameter / required even though ignored
        public static implicit operator Attempt<TResult>(Attempt failedAttempt)
#pragma warning restore IDE0060 // Remove unused parameter
            => new Attempt<TResult>(false);

        /// <summary>
        /// Implicitly converts a result value into a successful attempts.
        /// </summary>
        /// <param name="result">The result value.</param>
        public static implicit operator Attempt<TResult>(TResult result)
            => new Attempt<TResult>(true, result);

        // NOTE
        //
        // there is no way to return Attempt.Fail(exception) and have it implicitly
        // converted to a new Attempt<TResult> as that would allocate first the non-
        // generic and second the generic attempt, and we want to avoid this - and
        // all the tricks to convert a struct into another... still imply some
        // allocations
    }
}
