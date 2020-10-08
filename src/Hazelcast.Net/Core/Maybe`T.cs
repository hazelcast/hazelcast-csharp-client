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

namespace Hazelcast.Core
{
    internal readonly struct Maybe<T> : IEquatable<Maybe<T>>
    {
        private readonly T _value;
        private readonly bool _hasValue;

        private Maybe(T value)
        {
            _value = value;
            _hasValue = true;
        }

        public static Maybe<T> None => default;

        public static implicit operator Maybe<T>(T value)
            => value == null ? default : new Maybe<T>(value);

        public static implicit operator Maybe<T>(MaybeNone _)
            => default;

        public bool TryGetValue(out T value)
        {
            value = _hasValue ? _value : default;
            return _hasValue;
        }

        public Maybe<T1> Map<T1>(Func<T, T1> map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));
            return _hasValue ? map(_value) : default (Maybe<T1>);
        }

        public Maybe<T1> Bind<T1>(Func<T, Maybe<T1>> bind)
        {
            if (bind == null) throw new ArgumentNullException(nameof(bind));
            return _hasValue ? bind(_value) : default;
        }

        public T1 Match<T1>(T1 ifSome, T1 ifNone)
            => _hasValue ? ifSome : ifNone;

        public T ValueOrDefault()
            => _hasValue ? _value : default;

        public T ValueOr(T defaultValue)
            => _hasValue ? _value : defaultValue;

        /// <inheritdoc />
        public override bool Equals(object obj)
            => obj is Maybe<T> other && Equals(other);

        public bool Equals(Maybe<T> other)
        {
            return
                _hasValue == other._hasValue &&
                             (!_hasValue || _value.Equals(other._value));
        }

        public static bool operator ==(Maybe<T> left, Maybe<T> right)
            => left.Equals(right);

        public static bool operator !=(Maybe<T> left, Maybe<T> right)
            => !left.Equals(right);

        /// <inheritdoc />
        public override int GetHashCode() => _hasValue ? _value.GetHashCode() : 0;

        /// <inheritdoc />
        public override string ToString()
        {
            return $"HasValue = {(_hasValue ? "true" : "false")}{(_hasValue ? ", Value = " + _value : "")}";
        }
    }
}