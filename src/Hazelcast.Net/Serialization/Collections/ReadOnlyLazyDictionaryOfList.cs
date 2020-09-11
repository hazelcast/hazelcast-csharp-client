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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hazelcast.Serialization.Collections
{
    internal sealed class ReadOnlyLazyDictionaryOfList<TKey, TValue> : IReadOnlyDictionary<TKey, IReadOnlyList<TValue>>
    {
        private readonly ISerializationService _serializationService;

        private readonly Dictionary<IData, ReadOnlyLazyEntryOfList<TKey, TValue>> _entries
            = new Dictionary<IData, ReadOnlyLazyEntryOfList<TKey, TValue>>();

        private readonly Dictionary<TKey, ReadOnlyLazyEntryOfList<TKey, TValue>> _keyEntries
            = new Dictionary<TKey, ReadOnlyLazyEntryOfList<TKey, TValue>>();


        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyLazyDictionaryOfList{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="serializationService">The serialization service.</param>
        public ReadOnlyLazyDictionaryOfList(ISerializationService serializationService)
        {
            _serializationService = serializationService;
        }

        /// <summary>
        /// Adds entries.
        /// </summary>
        /// <param name="entries">Entries.</param>
        public void Add(IEnumerable<KeyValuePair<IData, IData>> entries)
        {
            foreach (var (keyData, valueObject) in entries)
            {
                if (!_entries.TryGetValue(keyData, out var entry))
                    _entries.Add(keyData, entry = new ReadOnlyLazyEntryOfList<TKey, TValue>(keyData, new ReadOnlyLazyList<TValue>(_serializationService)));
                entry.Values.Add(valueObject);
            }
        }

        /// <summary>
        /// Ensures that an entry has a key.
        /// </summary>
        /// <param name="entry">The entry.</param>
        private void EnsureKey(ReadOnlyLazyEntryOfList<TKey, TValue> entry)
        {
            if (entry.HasKey) return;

            entry.Key = _serializationService.ToObject<TKey>(entry.KeyData);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, IReadOnlyList<TValue>>> GetEnumerator()
        {
            foreach (var entry in _entries.Values)
            {
                // deserialize
                EnsureKey(entry);

                // while we're at it, ensure it's in the key entries too
                if (!_keyEntries.ContainsKey(entry.Key))
                    _keyEntries.Add(entry.Key, entry);

                yield return new KeyValuePair<TKey, IReadOnlyList<TValue>>(entry.Key, entry.Values);
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public int Count => _entries.Count;

        /// <inheritdoc />
        public bool ContainsKey(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            // fast: use key entries
            if (_keyEntries.ContainsKey(key)) return true;

            // slower: serialize
            var keyData = _serializationService.ToData(key);

            // exit if no corresponding entry
            if (!_entries.TryGetValue(keyData, out var entry)) return false;

            // else, while we're at it, update the entry + key entries
            if (!entry.HasKey) entry.Key = key;
            _keyEntries.Add(key, entry);

            return true;
        }

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out IReadOnlyList<TValue> value)
        {
            value = default;

            // fast: use key entries
            if (_keyEntries.TryGetValue(key, out var cacheEntry))
            {
                value = cacheEntry.Values;
                return true;
            }

            // slower: serialize
            var keyData = _serializationService.ToData(key);

            // exit if no corresponding entry
            if (!_entries.TryGetValue(keyData, out var entry)) return false;

            // while we're at it, update the entry + key entries
            if (!entry.HasKey) entry.Key = key;
            _keyEntries.Add(key, entry);

            value = entry.Values;

            return true;
        }

        /// <inheritdoc />
        public IReadOnlyList<TValue> this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out var value))
                    return value;

                throw new KeyNotFoundException();
            }
        }

        /// <inheritdoc />
        public IEnumerable<TKey> Keys
        {
            get
            {
                foreach (var entry in _entries.Values)
                {
                    EnsureKey(entry);
                    if (!_keyEntries.ContainsKey(entry.Key))
                        _keyEntries.Add(entry.Key, entry);
                    yield return entry.Key;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<IReadOnlyList<TValue>> Values
            => _entries.Values.Select(entry => entry.Values);
    }
}
