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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents an asynchronous concurrent dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    internal class ConcurrentAsyncDictionary<TKey, TValue> : IAsyncEnumerable<KeyValuePair<TKey, TValue>>
    {
        // usage:
        // this class is used by the DistributedObjectFactory to cache its distributed objects, and by NearCache

        private readonly ConcurrentDictionary<TKey, Entry> _dictionary = new ConcurrentDictionary<TKey, Entry>();

        /// <summary>
        /// Adds a key/value pair if the key does not already exists, or return the existing value if the key exists.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="factory">A value factory.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The value in the dictionary.</returns>
        public async ValueTask<TValue> GetOrAddAsync(TKey key, Func<TKey, CancellationToken, ValueTask<TValue>> factory, CancellationToken cancellationToken = default)
        {
            var entry = _dictionary.GetOrAdd(key, k => new Entry(k));

            // fast
            if (entry.HasValue) return entry.Value;

            try
            {
                // await - may throw
                return await entry.GetValue(factory, cancellationToken).CAF();
            }
            catch
            {
                // remove the failed entry from the dictionary
                ((ICollection<KeyValuePair<TKey, Entry>>)_dictionary).Remove(new KeyValuePair<TKey, Entry>(key, entry));
                throw;
            }
        }

        /// <summary>
        /// Adds a key/value pair if the key does not already exists.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="factory">A value factory.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns><c>true</c> if a key/value pair was added; otherwise <c>false</c>.</returns>
        public async ValueTask<bool> TryAddAsync(TKey key, Func<TKey, CancellationToken, ValueTask<TValue>> factory, CancellationToken cancellationToken = default)
        {
            var entry = new Entry(key);
            if (!_dictionary.TryAdd(key, entry)) return false;

            try
            {
                // await - may throw
                await entry.GetValue(factory, cancellationToken).CAF();
                return true;
            }
            catch
            {
                // remove the failed entry from the dictionary
                ((ICollection<KeyValuePair<TKey, Entry>>)_dictionary).Remove(new KeyValuePair<TKey, Entry>(key, entry));
                throw;
            }
        }

        /// <summary>
        /// Attempts to get the value associated with a key.
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <returns>An attempt at getting the value associated with the specified key.</returns>
        public async ValueTask<Attempt<TValue>> TryGetValueAsync(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var entry)) return Attempt.Failed;

            return entry.HasValue ? entry.Value : await entry.GetValue().CAF();
        }

        /// <summary>
        /// Tries to remove an entry, and returns the removed entry.
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <returns>An attempt at removing the value associated with the specified key.</returns>
        public async ValueTask<Attempt<TValue>> TryGetAndRemoveAsync(TKey key)
        {
            if (!_dictionary.TryRemove(key, out var entry)) return Attempt.Failed;

            try
            {
                return entry.HasValue ? entry.Value : await entry.GetValue().CAF();
            }
            catch
            {
                return Attempt.Failed; // ignore bogus entries
            }
        }

        /// <summary>
        /// Tries to remove an entry.
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <returns>true if the entry was removed; otherwise false.</returns>
        public bool TryRemove(TKey key)
        {
            return _dictionary.TryRemove(key, out _);
        }

        /// <summary>
        /// Determines whether the dictionary contains an entry for a key.
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <returns>true if the dictionary contains an entry for the specified key; otherwise false.</returns>
        public async ValueTask<bool> ContainsKeyAsync(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var entry)) return false;

            try
            {
                // ensure we really have a value, not a yet-unobserved error
                if (entry.HasValue) return true;
                await entry.GetValue().CAF();
                return true;
            }
            catch
            {
                return false; // ignore bogus entries
            }
        }

        /// <summary>
        /// Clears the dictionary.
        /// </summary>
        public void Clear()
        {
            _dictionary.Clear();
        }

        /// <summary>
        /// Gets the number of entries contained in the dictionary.
        /// </summary>
        public int Count => _dictionary.Count;

        /// <inheritdoc />
        public IAsyncEnumerator<KeyValuePair<TKey, TValue>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new AsyncEnumerator(_dictionary.GetEnumerator(), cancellationToken);

        private class AsyncEnumerator : IAsyncEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly IEnumerator<KeyValuePair<TKey, Entry>> _enumerator;
            private readonly CancellationToken _cancellationToken;
            private KeyValuePair<TKey, TValue> _current;

            public AsyncEnumerator(IEnumerator<KeyValuePair<TKey, Entry>> enumerator, CancellationToken cancellationToken)
            {
                _enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
                _cancellationToken = cancellationToken;
            }

            public ValueTask DisposeAsync()
            {
                _enumerator.Dispose();
                return default;
            }

            public async ValueTask<bool> MoveNextAsync()
            {
                while (_enumerator.MoveNext() && !_cancellationToken.IsCancellationRequested)
                {
                    var key = _enumerator.Current.Key;
                    var entry = _enumerator.Current.Value;

                    try
                    {
                        var value = entry.HasValue ? entry.Value : await entry.GetValue().CAF();
                        _current = new KeyValuePair<TKey, TValue>(key, value);
                        return true;
                    }
                    catch
                    {
                        // ignore bogus entries
                    }
                }

                return false;
            }

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    _ = _enumerator.Current; // throw if it must throw
                    return _current;
                }
            }
        }

        // internal for tests only
        internal class Entry
        {
            private readonly object _lock = new object();
            private readonly TKey _key;
            private TValue _value;
            private Task<TValue> _creating;

            public Entry(TKey key)
            {
                _key = key;
            }

            public bool HasValue { get; private set; }

            public TValue Value
            {
                get
                {
                    if (!HasValue) throw new InvalidOperationException("Entry does not have a value.");
                    return _value;
                }
            }

            public async Task<TValue> GetValue(Func<TKey, CancellationToken, ValueTask<TValue>> factory, CancellationToken cancellationToken = default)
            {
                // there is only one factory, each method is not supposed to try another factory

                lock (_lock)
                {
                    if (HasValue) return _value;
                    _creating ??= factory(_key, cancellationToken).AsTask();
                }

                _value = await _creating.CAF();

                lock (_lock)
                {
                    HasValue = true;
                    _creating = null;
                }

                return _value;
            }

            public async Task<TValue> GetValue()
            {
                return HasValue ? _value : await _creating.CAF();
            }
        }
    }
}
