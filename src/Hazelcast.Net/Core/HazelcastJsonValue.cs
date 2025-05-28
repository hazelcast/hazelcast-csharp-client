// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
    /// Represents a JSON formatted string.
    /// </summary>
    /// <remarks>
    /// <para>It is preferred to store HazelcastJsonValue instead of String for JSON formatted strings.
    /// Users can then run predicates and aggregations and use indexes on the attributes of the underlying
    /// JSON content.</para>
    /// <para>Note that the string is not validated and may be invalid JSON.</para>
    /// </remarks>
    public sealed class HazelcastJsonValue
    {
        private readonly string _json;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastJsonValue"/> with a string containing JSON.
        /// </summary>
        /// <param name="json">The string containing JSON.</param>
        public HazelcastJsonValue(string json)
        {
            _json = json ?? throw new ArgumentNullException(nameof(json));
        }

        /// <inheritdoc />
        public override string ToString() => _json;

        /// <summary>
        /// Gets string representation of JSON value.
        /// </summary>
        public string Value => _json;

        /// <inheritdoc />
        public override int GetHashCode() => _json.GetHashCode(StringComparison.Ordinal);

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is HazelcastJsonValue other && Equals(this, other);
        }

        /// <summary>
        /// Compares two instances of the <see cref="HazelcastJsonValue"/> for equality.
        /// </summary>
        /// <param name="x1">The first instance.</param>
        /// <param name="x2">The second instance.</param>
        /// <returns>true if the two instances are equal; otherwise false.</returns>
        private static bool Equals(HazelcastJsonValue x1, HazelcastJsonValue x2)
            => x1._json == x2._json;
    }
}
