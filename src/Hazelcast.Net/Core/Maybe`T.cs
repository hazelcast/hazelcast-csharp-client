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

using System;

namespace Hazelcast.Core
{
    // TODO: consider implementing IStructuralEquatable

    // <pedant>
    // "A monad is just a monoid in the category of endofunctors."
    // </pedant>
    //
    // In many cases we would like to return a T? value where T can just be
    // anything, but C# 8 does not supporting nullable "anything", and we
    // have to fall back to using our own structure.
    //
    // references:
    // https://mikhail.io/2018/07/monads-explained-in-csharp-again/
    // https://gist.github.com/johnazariah/d95c03e2c56579c11272a647bab4bc38
    // https://github.com/bert2/Nullable.Extensions
    // https://github.com/AndreyTsvetkov/Functional.Maybe
    // https://habr.com/en/post/458692/

    /// <summary>
    /// Represents a value that may be missing.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    internal readonly struct Maybe<T> : IEquatable<Maybe<T>>, IEquatable<T>
    {
        private readonly bool _hasValue;
        private readonly T _value;

        // note: the parameter-less constructor is always implied with structs

        /// <summary>
        /// Initializes a new instance of the <see cref="Maybe{T}"/> class with a value.
        /// </summary>
        /// <param name="value">The value.</param>
        private Maybe(T value)
        {
            _value = value;
            _hasValue = true;
        }

        #region Create

#pragma warning disable CA1000 // Do not declare static members on generic types

        /// <summary>
        /// Gets a <see cref="Maybe{T}"/> with no value.
        /// </summary>
        /// <returns>A <see cref="Maybe{T}"/> with no value.</returns>
        public static Maybe<T> None => default;

        /// <summary>
        /// Gets a <see cref="Maybe{T}"/> with a value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="Maybe{T}"/> with a value.</returns>
        public static Maybe<T> Some(T value) => new Maybe<T>(value);

#pragma warning restore CA1000

        #endregion

        #region Conversions

        /// <summary>
        /// Implicitly converts a <typeparamref cref="T"/> value into a corresponding <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        public static implicit operator Maybe<T>(T value)
            => value == null ? default : new Maybe<T>(value);

        /// <summary>
        /// Implicitly converts a non-generic <see cref="Maybe"/> none value into a
        /// corresponding <see cref="Maybe{T}"/> with no value.
        /// </summary>
        /// <param name="_"></param>
        public static implicit operator Maybe<T>(Maybe _)
            => default;

        /// <summary>
        /// Explicitly converts a <see cref="Maybe{T}"/> into its value.
        /// </summary>
        /// <param name="maybe">The attempt.</param>
        public static explicit operator T(Maybe<T> maybe)
            => maybe._value;

        #endregion

        #region Value

        /// <summary>
        /// Tries to get the value.
        /// </summary>
        /// <param name="value">The value if any; otherwise the default value for <typeparamref name="T"/>.</param>
        /// <returns>Whether this instance has a value.</returns>
        public bool TryGetValue(out T value)
        {
            value = _hasValue ? _value : default;
            return _hasValue;
        }

        /// <summary>
        /// Gets this value, if it exists; otherwise the default value for <typeparamref name="T"/>.
        /// </summary>
        /// <returns>This value, if it exists; otherwise the default value for <typeparamref name="T"/>.</returns>
        public T ValueOrDefault()
            => _hasValue ? _value : default;

        /// <summary>
        /// Gets this value, if it exists; otherwise the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>This value, if it exists; otherwise the specified value.</returns>
        public T ValueOr(T value)
            => _hasValue ? _value : value;

        /// <summary>
        /// Deconstruct this instance.
        /// </summary>
        /// <param name="hasValue">Whether this instance has a value.</param>
        /// <param name="value">The value if any; otherwise the default value for <typeparamref name="T"/>.</param>
        public void Deconstruct(out bool hasValue, out T value)
        {
            hasValue = _hasValue;
            value = _value;
        }

        /// <summary>
        /// Whether this instance does not have a value.
        /// </summary>
        /// <returns><c>true</c> if this instance does not have a value; otherwise <c>false</c>.</returns>
        public bool IsNone => !_hasValue;

        /// <summary>
        /// Whether this instance has a value.
        /// </summary>
        /// <returns><c>true</c> if this instance has a value; otherwise <c>false</c>.</returns>
        public bool IsValue => _hasValue;

        #endregion

        #region Operations

        /// <summary>
        /// Maps this instance.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the map operation.</typeparam>
        /// <param name="map">The map function.</param>
        /// <returns>The <see cref="Maybe{TResult}"/> resulting from the map.</returns>
        public Maybe<TResult> Map<TResult>(Func<T, TResult> map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            return _hasValue ? map(_value) : default;
        }

        /// <summary>
        /// Combines this instance.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the combination operation.</typeparam>
        /// <param name="bind">The combinator function.</param>
        /// <returns>The <see cref="Maybe{TResult}"/> resulting from the combinations.</returns>
        public Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> bind)
        {
            if (bind == null) throw new ArgumentNullException(nameof(bind));
            return _hasValue ? bind(_value) : default;
        }

        /// <summary>
        /// Matches this instance.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the match operation.</typeparam>
        /// <param name="ifSome">The value to match if this instance has a value.</param>
        /// <param name="ifNone">The value to match if this instance does not have a value.</param>
        /// <returns>The <typeparamref name="TResult"/> value resulting from the match.</returns>
        public TResult Match<TResult>(TResult ifSome, TResult ifNone)
        {
            return _hasValue ? ifSome : ifNone;
        }

        /// <summary>
        /// Matches this instance.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the match operation.</typeparam>
        /// <param name="ifSome">The function to match if this instance has a value.</param>
        /// <param name="ifNone">The function to match if this instance does not have a value.</param>
        /// <returns>The <typeparamref name="TResult"/> value resulting from the match.</returns>
        public TResult Match<TResult>(Func<T, TResult> ifSome, Func<TResult> ifNone)
        {
            if (ifSome == null) throw new ArgumentNullException(nameof(ifSome));
            if (ifNone == null) throw new ArgumentNullException(nameof(ifNone));

            return _hasValue ? ifSome(_value) : ifNone();
        }

        /// <summary>
        /// Matches this instance.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the match operation.</typeparam>
        /// <param name="ifSome">The function to match if this instance has a value.</param>
        /// <param name="ifNone">The value to match if this instance does not have a value.</param>
        /// <returns>The <typeparamref name="TResult"/> value resulting from the match.</returns>
        public TResult Match<TResult>(Func<T, TResult> ifSome, TResult ifNone)
        {
            if (ifSome == null) throw new ArgumentNullException(nameof(ifSome));

            return _hasValue ? ifSome(_value) : ifNone;
        }

        /// <summary>
        /// Matches this instance.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of the match operation.</typeparam>
        /// <param name="ifSome">The function to match if this instance has a value.</param>
        /// <param name="value">The value to match if this instance does not have a value.</param>
        /// <returns>The <typeparamref name="TResult"/> value resulting from the match.</returns>
        public TResult Match<TResult>(Func<TResult, T, TResult> ifSome, TResult value)
        {
            if (ifSome == null) throw new ArgumentNullException(nameof(ifSome));

            // note: this is also known as "fold"

            return _hasValue ? ifSome(value, _value) : value;
        }

        /// <summary>
        /// Matches this instance.
        /// </summary>
        /// <param name="ifSome">The action to match if this instance has a value.</param>
        /// <param name="ifNone">The action to match if this instance does not have a value.</param>
        public void Match(Action<T> ifSome, Action ifNone)
        {
            if (ifSome == null) throw new ArgumentNullException(nameof(ifSome));
            if (ifNone == null) throw new ArgumentNullException(nameof(ifNone));

            // note: purists may object to the 'Match' name

            if (_hasValue) ifSome(_value); else ifNone();
        }

        #endregion

        #region Equality

        // '==' compares references (object.ReferenceEquals) for reference types,
        // unless it is implemented for the type, whereas 'Equals' compares values.
        //
        // we do not need == for Maybe<T> and T, because of the above implicit and
        // explicit operators for converting types.
        //
        // == is ok if
        // both sides are reference types (compares references)
        // or
        // '==' has been defined for the type (which one?)
        //
        // '==' performs value-equality comparison for value types,
        // and reference-equality comparison for reference types, by default


        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return (obj is Maybe && !_hasValue) ||
                   (obj is Maybe<T> other && Equals(other));
        }

        /// <summary>
        /// Determines whether this instance is equal to another instance.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns><c>true</c> if this instance is equal to the other instance; otherwise <c>false</c>.</returns>
        public bool Equals(Maybe<T> other) // IEquatable<Maybe<T>>
        {
            return _hasValue == other._hasValue &&
                   (!_hasValue || _value.Equals(other._value));
        }

        /// <summary>
        /// Determines whether this instance has a value which is is equal to another value.
        /// </summary>
        /// <param name="value">The other value.</param>
        /// <returns><c>true</c> if this instance has a value which is equal to the other value; otherwise <c>false</c>.</returns>
        public bool Equals(T value) // IEquatable<T>
        {
            return _hasValue && _value.Equals(value);
        }

        /// <summary>
        /// Determines whether two <see cref="Maybe{T}"/> instance are equal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><c>true</c> if the two instances are equal; otherwise <c>false</c>.</returns>
        public static bool operator ==(Maybe<T> left, Maybe<T> right)
            => left.Equals(right);

        /// <summary>
        /// Determines whether two <see cref="Maybe{T}"/> instance are different.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><c>true</c> if the two instances are different; otherwise <c>false</c>.</returns>
        public static bool operator !=(Maybe<T> left, Maybe<T> right)
            => !left.Equals(right);

        /// <inheritdoc />
        public override int GetHashCode() => _hasValue ? _value.GetHashCode() : 0;

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"HasValue = {(_hasValue ? "true" : "false")}{(_hasValue ? ", Value = " + _value : "")}";
        }
    }
}
