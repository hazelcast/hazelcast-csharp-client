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

namespace Hazelcast.Partitioning.Strategies
{
    /// <summary>
    /// Implements an <see cref="IPartitioningStrategy"/> that returns the object if it is a string (trimming leading '@'), else returns null.
    /// </summary>
    internal sealed class StringPartitioningStrategy : IPartitioningStrategy
    {
        /// <inheritdoc />
        public object GetPartitionKey(object o)
            => o is string s ? GetPartitionKey(s) : null;

        /// <summary>
        /// Gets the partition key of a string value.
        /// </summary>
        /// <param name="s">The string value.</param>
        /// <returns>The partition key of the string value.</returns>
        public static string GetPartitionKey(string s)
        {
            if (s == null) return null;

            var pos = s.IndexOf('@', StringComparison.Ordinal);
            return pos < 0 ? s : s[(pos + 1)..];
        }
    }
}
