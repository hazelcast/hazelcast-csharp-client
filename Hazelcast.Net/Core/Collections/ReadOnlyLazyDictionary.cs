using System;
using System.Collections;
using System.Collections.Generic;
using Hazelcast.Serialization;

namespace Hazelcast.Core.Collections
{

    // FIXME thread-safety & tests
    // FIXME document

    /// <summary>
    /// Represent an <see cref="IReadOnlyDictionary{TKey,TValue}"/> that lazily de-serializes its keys and values.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <typeparam name="T">The type of the value objects.</typeparam>
    /// <remarks>
    /// <para>The key objects are always <see cref="IData"/> instances.</para>
    /// </remarks>
    internal sealed class ReadOnlyLazyDictionary<TKey, TValue, T> : IReadOnlyDictionary<TKey, TValue>
        where T : class
    {
        private readonly Dictionary<IData, T> _content = new Dictionary<IData, T>();
        private readonly Dictionary<TKey, CacheEntry<TValue, T>> _cache = new Dictionary<TKey, CacheEntry<TValue, T>>();
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
        public void Add(IEnumerable<KeyValuePair<IData, T>> values)
        {
            foreach (var (keyData, valueData) in values)
                _content.Add(keyData, valueData);
        }

        /// <summary>
        /// Adds a key-value pair.
        /// </summary>
        /// <param name="keyData">The key object.</param>
        /// <param name="valueData">The value object.</param>
        public void Add(IData keyData, T valueData)
        {
            _content.Add(keyData, valueData);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var (keyData, valueData) in _content)
            {
                var key = _serializationService.ToObject<TKey>(keyData);
                if (_cache.TryGetValue(key, out var cacheEntry))
                {
                    if (!cacheEntry.HasValue)
                    {
                        cacheEntry.Value = _serializationService.ToObject<TValue>(cacheEntry.Source);
                        cacheEntry.HasValue = true;
                    }
                }
                else
                {
                    cacheEntry = _cache[key] = new CacheEntry<TValue, T>
                    {
                        Source = valueData,
                        Value = _serializationService.ToObject<TValue>(valueData),
                        HasValue = true
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
                if (!cacheEntry.HasValue)
                {
                    cacheEntry.Value = _serializationService.ToObject<TValue>(cacheEntry.Source);
                    cacheEntry.HasValue = true;
                }
                value = cacheEntry.Value;
                return true;
            }

            var keyData = _serializationService.ToData(key);
            if (!_content.TryGetValue(keyData, out var valueData))
                return false;

            _cache[key] = new CacheEntry<TValue, T>
            {
                Source = valueData,
                Value = value = _serializationService.ToObject<TValue>(valueData),
                HasValue = true
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
                    if (!_cache.ContainsKey(key)) _cache[key] = new CacheEntry<TValue, T> { Source = valueData };
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
                        if (!cacheEntry.HasValue)
                        {
                            cacheEntry.Value = _serializationService.ToObject<TValue>(cacheEntry.Source);
                            cacheEntry.HasValue = true;
                        }
                    }
                    else
                    {
                        cacheEntry = _cache[key] = new CacheEntry<TValue, T>
                        {
                            Source = valueData,
                            Value = _serializationService.ToObject<TValue>(valueData),
                            HasValue = true
                        };

                    }
                    yield return cacheEntry.Value;
                }
            }
        }
    }
}
