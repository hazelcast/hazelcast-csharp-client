// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics.CodeAnalysis;
namespace Hazelcast.Models
{
    /// <summary>
    /// Represents an entry in the vector search result.
    /// </summary>
    /// <typeparam name="TKey">The type of the key associated with the entry.</typeparam>
    /// <typeparam name="TVal">The type of the value associated with the entry.</typeparam>
    public class VectorSearchResultEntry<TKey, TVal>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VectorSearchResultEntry{TKey, TVal}"/> class with the specified key,
        /// value, vector values, and score.
        /// </summary>
        /// <param name="key">The key associated with the entry.</param>
        /// <param name="value">The value associated with the entry.</param>
        /// <param name="vectors">The vector values associated with the entry.</param>
        /// <param name="score">The score of the entry.</param>
        internal VectorSearchResultEntry([NotNull] TKey key, [NotNull] TVal value, [NotNull] VectorValues vectors, float score)
        {
            Key = key;
            Value = value;
            Vectors = vectors;
            Score = score;
        }

        /// <summary>
        /// Gets the key associated with the entry.
        /// </summary>
        [NotNull]
        public TKey Key { get; }

        /// <summary>
        /// Gets the value associated with the entry.
        /// </summary>
        [NotNull]
        public TVal Value { get; }

        /// <summary>
        /// Gets the vector values associated with the entry.
        /// </summary>
        [NotNull]
        public VectorValues Vectors { get; }

        /// <summary>
        /// Gets the score of the entry.
        /// </summary>
        public float Score { get; }
    }
}
