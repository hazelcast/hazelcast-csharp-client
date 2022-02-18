﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.NearCaching;

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
    internal sealed class ReadOnlyLazyList<TValue> : IReadOnlyList<TValue>, IAsyncEnumerable<TValue>
    {
        private readonly List<ReadOnlyLazyEntry<TValue>> _content = new List<ReadOnlyLazyEntry<TValue>>();
        private readonly SerializationService _serializationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyLazyList{TValue}"/> class.
        /// </summary>
        /// <param name="serializationService">The serialization service.</param>
        public ReadOnlyLazyList(SerializationService serializationService)
        {
            _serializationService = serializationService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyLazyList{TValue}"/> class.
        /// </summary>
        /// <param name="valueData">Value data items.</param>
        /// <param name="serializationService">The serialization service.</param>
        public ReadOnlyLazyList(IEnumerable<IData> valueData, SerializationService serializationService)
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

        // FIXME - document
        public async IAsyncEnumerator<TValue> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            using var enumerator = _content.GetEnumerator();
            while (!cancellationToken.IsCancellationRequested && enumerator.MoveNext())
            {
                var cacheEntry = enumerator.Current!;

                // accepted race-condition here
                if (!cacheEntry.HasValue) await DeserializeAsync(cacheEntry).CfAwait();
                yield return cacheEntry.Value;
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        // FIXME - document
        public ValueTask<TValue> GetItemAsync(int index)
        {
            var cacheEntry = _content[index];

            // accepted race-condition here
            return cacheEntry.HasValue
                ? new ValueTask<TValue>(cacheEntry.Value)
                : DeserializeAsync(cacheEntry);
        }

        private async ValueTask<TValue> DeserializeAsync(ReadOnlyLazyEntry<TValue> entry)
        {
            while (true)
            {
                try
                {
                    return entry.Value = _serializationService.ToObject<TValue>(entry.ValueData);
                }
                catch (MissingCompactSchemaException e)
                {
                    await e.FetchSchemaAsync().CfAwait(); // throws if not successful
                }
            }
        }
    }
}
