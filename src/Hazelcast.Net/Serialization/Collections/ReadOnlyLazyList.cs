﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Serialization.Collections
{
    /// <summary>
    /// Represents a lazy list of values.
    /// </summary>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <remarks>
    /// <para>This class is not thread-safe for writing: it should be entirely populated
    /// in a thread-safe way, before being returned to readers.</para>
    /// <para>This class is thread-safe for reading, however for performance purposes, some values may
    /// be deserialized multiple times in multi-threaded situations.</para>
    /// </remarks>
    internal sealed class ReadOnlyLazyList<TValue> : IReadOnlyList<TValue>
    {
        private readonly List<ReadOnlyLazyEntry<TValue>> _content = new List<ReadOnlyLazyEntry<TValue>>();
        private readonly ISerializationService _serializationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyLazyList{TValue}"/> class.
        /// </summary>
        /// <param name="serializationService">The serialization service.</param>
        public ReadOnlyLazyList(ISerializationService serializationService)
        {
            _serializationService = serializationService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyLazyList{TValue}"/> class.
        /// </summary>
        /// <param name="valueData">Value data items.</param>
        /// <param name="serializationService">The serialization service.</param>
        public ReadOnlyLazyList(IEnumerable<IData> valueData, ISerializationService serializationService)
        {
            _serializationService = serializationService;
            foreach (var valueObject in valueData)
                _content.Add(new ReadOnlyLazyEntry<TValue>(valueObject));
        }

        /// <summary>
        /// Adds a value.
        /// </summary>
        /// <param name="valueData">The value data.</param>
        public void Add(IData valueData)
        {
            _content.Add(new ReadOnlyLazyEntry<TValue>(valueData));
        }

        /// <summary>
        /// Ensures that a cache entry has a value.
        /// </summary>
        /// <param name="entry">The cache entry.</param>
        private void EnsureValue(ReadOnlyLazyEntry<TValue> entry)
        {
            if (entry.HasValue) return;

            // accepted race-condition here

            entry.Value = _serializationService.ToObject<TValue>(entry.ValueData);
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
