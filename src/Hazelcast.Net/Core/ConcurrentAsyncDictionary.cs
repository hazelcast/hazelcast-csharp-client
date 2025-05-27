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
        where TValue : class
    {
        // usage:
        // this class is used by the DistributedObjectFactory to cache its distributed objects, and by NearCache

        private readonly ConcurrentDictionary<TKey, Entry> _dictionary = new ConcurrentDictionary<TKey, Entry>();

        /// <summary>
        /// (internal for tests only)
        /// Adds an entry.
        /// </summary>
        internal bool AddEntry(TKey key, Entry entry)
        {
            return _dictionary.TryAdd(key, entry);
        }

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

            // it is possible to GetOrAdd a null value, if the factory returns a null
            // value, but that value will not stay in the cache

            // fast
            // may be null but then the entry will be removed eventually
            if (entry.HasValue) return entry.Value;

            try
            {
                // await - may throw - meaning the factory has thrown, entry is failed
                var value = await entry.GetValue(factory, cancellationToken).CfAwait();
                if (value != null) return value;

                // remove the invalid entry (with null value) from the dictionary
                _dictionary.TryRemove(key, entry);
                return null; // and the entry has been removed
            }
            catch
            {
                // remove the failed entry from the dictionary
                _dictionary.TryRemove(key, entry);
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

            // it is not possible to add a null value, if the factory returns a null
            // value then TryAdd would return false and the value does not stay in
            // the cache

            try
            {
                // await - may throw - meaning the factory has thrown, entry is failed
                var value = await entry.GetValue(factory, cancellationToken).CfAwait();
                if (value != null) return true;

                // remove the invalid entry (with null value) from the dictionary
                _dictionary.TryRemove(key, entry);
                return false; // and the entry has been removed
            }
            catch
            {
                // remove the failed entry from the dictionary
                _dictionary.TryRemove(key, entry);
                throw;
            }
        }

        /// <summary>
        /// Attempts to get the value associated with a key.
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <returns>An attempt at getting the value associated with the specified key.</returns>
        public async ValueTask<Attempt<TValue>> TryGetAsync(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var entry)) return Attempt.Failed;

            // it is not possible to get a null value
            return await TryGetEntryValueAsync(entry).CfAwait();
        }

        /// <summary>
        /// Tries to remove an entry, and returns the removed entry.
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <returns>An attempt at removing the value associated with the specified key.</returns>
        public async ValueTask<Attempt<TValue>> TryGetAndRemoveAsync(TKey key)
        {
            if (!_dictionary.TryRemove(key, out var entry)) return Attempt.Failed;

            // it is not possible to get a null value
            return await TryGetEntryValueAsync(entry).CfAwait();
        }

        internal static async ValueTask<Attempt<TValue>> TryGetEntryValueAsync(Entry entry)
        {
            // it is not possible to get a null value

            // fast
            if (entry.HasValue)
            {
                if (entry.Value != null) return entry.Value;
                return Attempt.Failed;
            }

            try
            {
                var value = await entry.GetValue().CfAwait();
                if (value != null) return value;
            }
            catch
            {
                // ignore the exception here, it's handled by whatever has added the value
            }

            return Attempt.Failed;
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

            // a null value is not a value
            // return the attempt at getting the entry value
            return await TryGetEntryValueAsync(entry).CfAwait();
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
        /// <remarks>
        /// <para>This is the total number of entries, including entries which do not yet have
        /// a value, and may eventually be removed if their factory throws or return a null value.</para>
        /// </remarks>
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
                        var value = entry.HasValue ? entry.Value : await entry.GetValue().CfAwait();
                        if (value == null) continue; // skip null values
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

        /// <summary>
        /// (internal for tests only)
        /// Represents a dictionary entry.
        /// </summary>
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

            /// <summary>
            /// (internal for tests only)
            /// Initializes a new instance of the <see cref="Entry"/> class.
            /// </summary>
            internal Entry(TKey key, bool hasValue, TValue value = default, Task<TValue> creating = default)
            {
                _key = key;
                HasValue = hasValue;
                _value = value;
                _creating = creating;
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

                Task<TValue> creating;
                lock (_lock)
                {
                    if (HasValue) return _value;
                    creating = _creating ??= factory(_key, cancellationToken).AsTask();
                }

                _value = await creating.CfAwait();

                lock (_lock)
                {
                    HasValue = true;
                    _creating = null;
                }

                return _value;
            }

            public async Task<TValue> GetValue()
            {
                Task<TValue> creating;
                lock (_lock)
                {
                    if (HasValue) return _value;
                    creating = _creating;
                }

                return await creating.CfAwait();
            }
        }
    }
}
