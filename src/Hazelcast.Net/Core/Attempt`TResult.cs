// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

// have to comment this out, C# 8 will not allow us to write TResult?
// without specifying whether TResult is a class or a struct
// TODO: uncomment + make exception Exception? eventually (requires C# 9)
//#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents the result of attempting an operation to produce a result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <remarks>
    /// <para>An <see cref="Attempt{TResult}"/> is either successful or failed, it
    /// carries a <typeparamref name="TResult"/> result, and an exception.</para>
    /// <para>An attempt can be used explicitly:  <code>
    /// var attempt = await TryAsync(...);
    /// if (attempt.Success)
    /// {
    ///     // use attempt.Value
    /// }
    /// </code> or implicitly via deconstruction: <code>
    /// var (success, value) = await TryAsync(...);
    /// if (success)
    /// {
    ///     // use value
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    internal readonly struct Attempt<TResult> : IEquatable<Attempt<TResult>>, IEquatable<TResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Attempt{TResult}"/> struct.
        /// </summary>
        /// <param name="success">Whether the attempt succeeded.</param>
        /// <param name="value">The optional value of the result.</param>
        /// <param name="exception">An optional captured exception.</param>
        internal Attempt(bool success, TResult value = default, Exception exception = default)
        {
            Success = success;
            Value = value;
            Exception = exception;
        }

        /// <summary>
        /// Represents a failed attempt with no result and no exception.
        /// </summary>
        public static Attempt<TResult> Failed => new Attempt<TResult>(false); // no such thing as 'struct' singletons!

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
        /// <remarks>
        /// <para>If not successful, the attempt may still carry a value, but this method
        /// does not return it. To check the value, regardless of success, use the <see cref="Value"/>
        /// property.</para>
        /// </remarks>
        [return: MaybeNull]
        public TResult ValueOr([AllowNull] TResult other)
            => Success ? Value : other;

        /// <summary>
        /// Gets the value of the result, if successful, else the default value for <typeparamref name="TResult"/>.
        /// </summary>
        /// <returns>The value of the result, if successful, else the default value for <typeparamref name="TResult"/>.</returns>
        /// <remarks>
        /// <para>If not successful, the attempt may still carry a value, but this method
        /// does not return it. To check the value, regardless of success, use the <see cref="Value"/>
        /// property.</para>
        /// </remarks>
        [return: MaybeNull]
        public TResult ValueOrDefault()
            => Success ? Value : default;

        /// <summary>
        /// Gets a captured exception.
        /// </summary>
        public Exception Exception { get; }

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
        /// Implicitly converts a non-generic attempt into a generic one.
        /// </summary>
        /// <param name="attempt">The attempt.</param>
        public static implicit operator Attempt<TResult>(Attempt attempt)
            => new Attempt<TResult>(attempt.Success);

        /// <summary>
        /// Implicitly converts a result value into a successful attempts.
        /// </summary>
        /// <param name="result">The result value.</param>
        public static implicit operator Attempt<TResult>(TResult result)
            => new Attempt<TResult>(true, result);

        /// <summary>
        /// Deconstruct an attempt.
        /// </summary>
        /// <param name="success">Whether the attempt succeeded.</param>
        /// <param name="value">The value of the result.</param>
        public void Deconstruct(out bool success, out TResult value)
        {
            success = Success;
            value = Value;
        }

        // NOTE
        //
        // there is no way to return Attempt.Fail(exception) and have it implicitly
        // converted to a new Attempt<TResult> as that would allocate first the non-
        // generic and second the generic attempt, and we want to avoid this - and
        // all the tricks to convert a struct into another... still imply some
        // allocations


        /// <inheritdoc />
        public bool Equals(Attempt<TResult> other)
            => (Success && other.Success && (Value is null ? other.Value is null : Value.Equals(other.Value))) ||
               (!Success && !other.Success);

        /// <inheritdoc />
        public bool Equals(TResult other)
            => Success && (Value is null ? other is null : Value.Equals(other));

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj is Attempt<TResult> attempt && Equals(attempt);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Success, Value);
        }

        public static bool operator ==(Attempt<TResult> left, Attempt<TResult> right)
            => left.Equals(right);

        public static bool operator !=(Attempt<TResult> left, Attempt<TResult> right)
            => !left.Equals(right);
    }
}
