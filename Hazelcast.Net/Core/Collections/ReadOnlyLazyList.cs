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

using System.Collections;
using System.Collections.Generic;
using Hazelcast.Serialization;

namespace Hazelcast.Core.Collections
{
    /// <summary>
    /// Represents a lazy list of values.
    /// </summary>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <typeparam name="TSource">The type of the source values.</typeparam>
    internal sealed class ReadOnlyLazyList<TValue, TSource> : IReadOnlyList<TValue>
    {
        private readonly List<CacheEntry<TValue, TSource>> _content = new List<CacheEntry<TValue, TSource>>();
        private readonly ISerializationService _serializationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyLazyList{TValue,T}"/> class.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="serializationService"></param>
        public ReadOnlyLazyList(IEnumerable<TSource> values, ISerializationService serializationService)
        {
            _serializationService = serializationService;
            foreach (var value in values)
                _content.Add(new CacheEntry<TValue, TSource> { Source = value });
        }

        /// <summary>
        /// Ensures that a cache entry has a value.
        /// </summary>
        /// <param name="cacheEntry">The cache entry.</param>
        private void EnsureValue(CacheEntry<TValue, TSource> cacheEntry)
        {
            if (cacheEntry.HasValue) return;

            // TODO: this is not thread-safe since Source becomes default: lock?
            cacheEntry.Value = _serializationService.ToObject<TValue>(cacheEntry.Source);
        }

        /// <inheritdoc />
        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var cacheEntry in _content)
            {
                EnsureValue(cacheEntry);
                yield return cacheEntry.Value;
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public int Count => _content.Count;

        /// <inheritdoc />
        public TValue this[int index]
        {
            get
            {
                var cacheEntry = _content[index];
                EnsureValue(cacheEntry);
                return cacheEntry.Value;
            }
        }
    }
}