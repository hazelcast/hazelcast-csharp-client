using System;
using System.Collections;
using System.Collections.Generic;
using Hazelcast.Serialization;

namespace Hazelcast.Core.Collections
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
    internal sealed class ReadOnlyLazyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly ISerializationService _serializationService;

        private readonly Dictionary<IData, ReadOnlyLazyEntry<TKey, TValue>> _entries = new Dictionary<IData, ReadOnlyLazyEntry<TKey, TValue>>();
        private readonly Dictionary<TKey, ReadOnlyLazyEntry<TKey, TValue>> _keyEntries = new Dictionary<TKey, ReadOnlyLazyEntry<TKey, TValue>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyLazyDictionary{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="serializationService">The serialization service.</param>
        public ReadOnlyLazyDictionary(ISerializationService serializationService)
        {
            _serializationService = serializationService;
        }

        /// <summary>
        /// Gets the entries.
        /// </summary>
        public Dictionary<IData, ReadOnlyLazyEntry<TKey, TValue>> Entries => _entries;

        /// <summary>
        /// Adds entries.
        /// </summary>
        /// <param name="entries">Entries.</param>
        public void Add(IEnumerable<KeyValuePair<IData, object>> entries)
        {
            foreach (var (keyData, valueObject) in entries)
                _entries.Add(keyData, new ReadOnlyLazyEntry<TKey, TValue>(keyData, valueObject));
        }

        /// <summary>
        /// Adds entries.
        /// </summary>
        /// <param name="entries">Entries.</param>
        public void Add(IEnumerable<KeyValuePair<IData, IData>> entries)
        {
            foreach (var (keyData, valueObject) in entries)
                _entries.Add(keyData, new ReadOnlyLazyEntry<TKey, TValue>(keyData, valueObject));
        }

        /// <summary>
        /// Adds a key-value pair.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <param name="valueObject">The value source object.</param>
        public void Add(IData keyData, object valueObject)
        {
            _entries.Add(keyData, new ReadOnlyLazyEntry<TKey, TValue>(keyData, valueObject));
        }

        /// <summary>
        /// Ensures that an entry has a key.
        /// </summary>
        /// <param name="entry">The entry.</param>
        private void EnsureKey(ReadOnlyLazyEntry<TKey, TValue> entry)
        {
            if (entry.HasKey) return;

            entry.Value = _serializationService.ToObject<TValue>(entry.ValueObject);
        }

        /// <summary>
        /// Ensures that an entry has a value.
        /// </summary>
        /// <param name="entry">The entry.</param>
        private void EnsureValue(ReadOnlyLazyEntry<TValue> entry)
        {
            if (entry.HasValue) return;

            entry.Value = _serializationService.ToObject<TValue>(entry.ValueObject);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var entry in _entries.Values)
            {
                // deserialize
                EnsureKey(entry);
                EnsureValue(entry);

                // while we're at it, ensure it's in the key entries too
                if (!_keyEntries.ContainsKey(entry.Key))
                    _keyEntries.Add(entry.Key, entry);

                yield return new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default;

            // fast: use key entries
            if (_keyEntries.TryGetValue(key, out var cacheEntry))
            {
                EnsureValue(cacheEntry);
                value = cacheEntry.Value;
                return true;
            }

            // slower: serialize
            var keyData = _serializationService.ToData(key);

            // exit if no corresponding entry
            if (!_entries.TryGetValue(keyData, out var entry)) return false;

            // while we're at it, update the entry + key entries
            if (!entry.HasKey) entry.Key = key;
            _keyEntries.Add(key, entry);

            EnsureValue(entry);
            value = entry.Value;

            return true;
        }

        /// <inheritdoc />
        public TValue this[TKey key]
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
        public IEnumerable<TValue> Values
        {
            get
            {
                foreach (var entry in _entries.Values)
                {
                    EnsureValue(entry);
                    yield return entry.Value;
                }
            }
        }
    }
}
