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
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.Serialization.Collections
{
    /// <summary>
    /// Represent a lazy dictionary of keys and values.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <remarks>
    /// <para>The key objects are always <see cref="IData"/> instances.</para>
    /// <para>This class is not thread-safe for writing: it should be entirely populated
    /// in a thread-safe way, before being returned to readers.</para>
    /// <para>This class is thread-safe for reading, however for performance purposes, some values may
    /// be deserialized multiple times in multi-threaded situations.</para>
    /// </remarks>
    internal sealed class ReadOnlyLazyKeyValuePairs<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>>
    {
        private readonly SerializationService _serializationService;

        private readonly List<ReadOnlyLazyEntry<TKey, TValue>> _entries = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyLazyDictionary{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="serializationService">The serialization service.</param>
        public ReadOnlyLazyKeyValuePairs(SerializationService serializationService)
        {
            _serializationService = serializationService;
        }

        /// <summary>
        /// Gets the entries.
        /// </summary>
        public List<ReadOnlyLazyEntry<TKey, TValue>> Entries => _entries;

        /// <summary>
        /// Adds entries.
        /// </summary>
        /// <param name="entries">Entries.</param>
        public async ValueTask AddAsync(IEnumerable<KeyValuePair<IData, IData>> entries)
        {
            foreach (var (keyData, valueData) in entries)
            {
                await _serializationService.EnsureCanDeserialize(keyData).CfAwait();
                await _serializationService.EnsureCanDeserialize(valueData).CfAwait();
                _entries.Add(new ReadOnlyLazyEntry<TKey, TValue>(keyData, valueData));
            }
        }

        /// <summary>
        /// Ensures that an entry has a key.
        /// </summary>
        /// <param name="entry">The entry.</param>
        private void EnsureKey(ReadOnlyLazyEntry<TKey, TValue> entry)
        {
            if (entry.HasKey) return;

            entry.Key = _serializationService.ToObject<TKey>(entry.KeyData);
        }

        /// <summary>
        /// Ensures that an entry has a value.
        /// </summary>
        /// <param name="entry">The entry.</param>
        private void EnsureValue(ReadOnlyLazyEntry<TValue> entry)
        {
            if (entry.HasValue) return;

            entry.Value = _serializationService.ToObject<TValue>(entry.ValueData);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var entry in _entries)
            {
                // deserialize
                EnsureKey(entry);
                EnsureValue(entry);

                yield return new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public int Count => _entries.Count;
    }
}
