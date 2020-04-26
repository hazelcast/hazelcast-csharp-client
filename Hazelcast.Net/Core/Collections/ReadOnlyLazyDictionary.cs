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
    /// <typeparam name="TSource">The type of the source values.</typeparam>
    /// <remarks>
    /// <para>The key objects are always <see cref="IData"/> instances.</para>
    /// <para>This class is not thread-safe for writing: it should be entirely populated
    /// in a thread-safe way, before being returned to readers.</para>
    /// </remarks>
    internal sealed class ReadOnlyLazyDictionary<TKey, TValue, TSource> : IReadOnlyDictionary<TKey, TValue>
        where TSource : class
    {
        private readonly Dictionary<IData, TSource> _content = new Dictionary<IData, TSource>();
        private readonly Dictionary<TKey, CacheEntry<TValue, TSource>> _cache = new Dictionary<TKey, CacheEntry<TValue, TSource>>();
        private readonly ISerializationService _serializationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyLazyDictionary{TKey,TValue,T}"/> class.
        /// </summary>
        /// <param name="serializationService">The serialization service.</param>
        public ReadOnlyLazyDictionary(ISerializationService serializationService)
        {
            _serializationService = serializationService;
        }

        /// <summary>
        /// Adds key-value pairs.
        /// </summary>
        /// <param name="values">Values.</param>
        public void Add(IEnumerable<KeyValuePair<IData, TSource>> values)
        {
            foreach (var (keyData, valueData) in values)
                _content.Add(keyData, valueData);
        }

        /// <summary>
        /// Adds a key-value pair.
        /// </summary>
        /// <param name="keyData">The key object.</param>
        /// <param name="valueData">The value object.</param>
        public void Add(IData keyData, TSource valueData)
        {
            _content.Add(keyData, valueData);
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
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var (keyData, valueData) in _content)
            {
                var key = _serializationService.ToObject<TKey>(keyData);
                if (_cache.TryGetValue(key, out var cacheEntry))
                {
                    EnsureValue(cacheEntry);
                }
                else
                {
                    // FIXME: this is not thread safe for reading either?
                    cacheEntry = _cache[key] = new CacheEntry<TValue, TSource>
                    {
                        Value = _serializationService.ToObject<TValue>(valueData)
                    };

                }
                yield return new KeyValuePair<TKey, TValue>(key, cacheEntry.Value);
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
        public bool ContainsKey(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (_cache.ContainsKey(key)) return true;

            var keyData = _serializationService.ToData(key);
            return _content.ContainsKey(keyData);
        }

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default;

            if (_cache.TryGetValue(key, out var cacheEntry))
            {
                EnsureValue(cacheEntry);
                value = cacheEntry.Value;
                return true;
            }

            var keyData = _serializationService.ToData(key);
            if (!_content.TryGetValue(keyData, out var valueData))
                return false;

            // FIXME: this is not thread safe for reading either?
            _cache[key] = new CacheEntry<TValue, TSource>
            {
                Value = value = _serializationService.ToObject<TValue>(valueData)
            };

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
                foreach (var (keyData, valueData) in _content)
                {
                    var key = _serializationService.ToObject<TKey>(keyData);
                    // FIXME: this is not thread safe for reading either?
                    if (!_cache.ContainsKey(key)) _cache[key] = new CacheEntry<TValue, TSource> { Source = valueData };
                    yield return key;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<TValue> Values
        {
            get
            {
                foreach (var (keyData, valueData) in _content)
                {
                    var key = _serializationService.ToObject<TKey>(keyData);
                    if (_cache.TryGetValue(key, out var cacheEntry))
                    {
                        EnsureValue(cacheEntry);
                    }
                    else
                    {
                        // FIXME: this is not thread safe for reading either?
                        cacheEntry = _cache[key] = new CacheEntry<TValue, TSource>
                        {
                            Value = _serializationService.ToObject<TValue>(valueData)
                        };

                    }
                    yield return cacheEntry.Value;
                }
            }
        }
    }
}
