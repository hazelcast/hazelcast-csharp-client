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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents an asynchronous concurrent dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    internal class ConcurrentAsyncDictionary<TKey, TValue> : IAsyncEnumerable<KeyValuePair<TKey, TValue>>
    {
        // must put Lazy in the dictionary to avoid creating the factory multiple times

        // 'GetOrAdd' call on the dictionary is not thread safe and we might end up creating the pipeline more
        // once. To prevent this Lazy<> is used. In the worst case multiple Lazy<> objects are created for multiple
        // threads but only one of the objects succeeds in creating a pipeline.

        private readonly ConcurrentDictionary<TKey, Lazy<ValueTask<TValue>>> _dictionary = new ConcurrentDictionary<TKey, Lazy<ValueTask<TValue>>>();

        /// <summary>
        /// Adds a key/value pair if the key does not already exists, or return the existing value if the key exists.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        public ValueTask<TValue> GetOrAddAsync(TKey key, Func<TKey, ValueTask<TValue>> factory)
        {
            var lazy = _dictionary.GetOrAdd(key, k => new Lazy<ValueTask<TValue>>(() => Factory(k, factory)));
            return lazy.Value; // won't throw - any exception bubbles to the task
        }

        /// <summary>
        /// Attempts to add a value.
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <param name="factory">A value factory.</param>
        /// <returns>An attempt at adding a value associated with the specified key.</returns>
        public Attempt<ValueTask<TValue>> TryAdd(TKey key, Func<TKey, ValueTask<TValue>> factory)
        {
            var lazy = new Lazy<ValueTask<TValue>>(() => factory(key));
            if (_dictionary.TryAdd(key, lazy))
                return lazy.Value;
            return Attempt.Failed;
        }

        private ValueTask<TValue> Factory(TKey key, Func<TKey, ValueTask<TValue>> factory)
        {
            // the task will not be created, and therefore the factory will not run, until lazy.Value is retrieved,
            // and that can only happen once the dictionary entry has been created, so it is safe to assume here
            // that the entry exists (and that we can remove it if we need to)

            ValueTask<TValue> task;
            try
            {
                task = factory(key);
            }
            catch (Exception e)
            {
                _dictionary.TryRemove(key, out _); // don't leave faulted entries in the dictionary

                // cannot create a ValueTask that carries an exception
                return new ValueTask<TValue>(Task.FromException<TValue>(e));
            }

            // only wait to have a proper continuation is by allocating a task
            task.AsTask().ContinueWith(t =>
                {
                    _ = t.Exception; // observe
                    _dictionary.TryRemove(key, out _); // don't leave faulted entries in the dictionary

                }, default,
                TaskContinuationOptions.NotOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current);

            return task;
        }

        /// <summary>
        /// Attempts to get the value associated with a key.
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <returns>An attempt at getting the value associated with the specified key.</returns>
        public async ValueTask<Attempt<TValue>> TryGetValue(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var lazy)) return Attempt.Failed;

            try
            {
                return await lazy.Value.CAF();
            }
            catch // bogus entry is taken care of elsewhere
            {
                return Attempt.Failed;
            }
        }

        /// <summary>
        /// Tries to remove an entry.
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <returns>true if the entry was removed; otherwise false.</returns>
        public bool TryRemove(TKey key)
        {
            return _dictionary.TryRemove(key, out _); // could it be we never await the task? and produce unobserved whatever?!
        }

        /// <summary>
        /// Determines whether the dictionary contains an entry for a key.
        /// </summary>
        /// <param name="key">The key identifying the entry.</param>
        /// <returns>true if the dictionary contains an entry for the specified key; otherwise false.</returns>
        public async ValueTask<bool> ContainsKey(TKey key)
        {
            if (!_dictionary.TryGetValue(key, out var lazy)) return false;
            try
            {
                await lazy.Value.CAF();
                return true;
            }
            catch
            {
                return false;
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
            private readonly IEnumerator<KeyValuePair<TKey, Lazy<ValueTask<TValue>>>> _enumerator;
            private readonly CancellationToken _cancellationToken;
            private KeyValuePair<TKey, TValue> _current;

            public AsyncEnumerator(IEnumerator<KeyValuePair<TKey, Lazy<ValueTask<TValue>>>> enumerator, CancellationToken cancellationToken)
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
                    var lazyTask = _enumerator.Current.Value;
                    try
                    {
                        var value = await lazyTask.Value.CAF();
                        _current = new KeyValuePair<TKey, TValue>(key, value);
                        return true;
                    }
                    catch // bogus entry is taken care of elsewhere
                    {
                        // skip
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
    }
}
