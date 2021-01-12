// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
    /// <summary>
    /// Represents a value that may be missing.
    /// </summary>
    internal readonly struct Maybe : IEquatable<Maybe>
    {
        // note: the parameter-less constructor is always implied with structs

        #region Create

        /// <summary>
        /// Gets a <see cref="Maybe"/> with no value.
        /// </summary>
        public static Maybe None => default;

        /// <summary>
        /// Gets a <see cref="Maybe{T}"/> with a value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="Maybe{T}"/> with a value.</returns>
        public static Maybe<T> Some<T>(T value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return value;
        }

        #endregion

        #region Equality

        /// <inheritdoc />
        public override bool Equals(object obj)
            => (obj is Maybe) ||
               (!(obj is null) && obj.Equals(this)); // deals with obj being Maybe<T>

        /// <summary>
        /// Determines whether this instance is equal to another instance.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns><c>true</c> if this instance is equal to the other instance; otherwise <c>false</c>.</returns>
        public bool Equals(Maybe other) => true;

        /// <summary>
        /// Determines whether two <see cref="Maybe"/> instance are equal.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><c>true</c> if the two instances are equal; otherwise <c>false</c>.</returns>
        public static bool operator ==(Maybe left, Maybe right)
            => left.Equals(right);

        /// <summary>
        /// Determines whether two <see cref="Maybe"/> instance are different.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns><c>true</c> if the two instances are different; otherwise <c>false</c>.</returns>
        public static bool operator !=(Maybe left, Maybe right)
            => !left.Equals(right);

        /// <inheritdoc />
        public override int GetHashCode() => 0;

        #endregion
    }
}
