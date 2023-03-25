// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Hazelcast.Models;

namespace Hazelcast.Core;

// LRU cache that can be used on heavy read concurrent operations. Reading and Adding is thread safe.
// During adding a value to cache, the thread that invoked Add operation can trigger eviction.
// If a thread already doing a eviction, late invokers won't be blocked. The cache evicts when threshold
// value is exceeded. No guarantee is given that cache size won't exceed the threshold but will be under eventually.
internal class ReadOptimizedLruCache<TKey, TValue> : IDisposable
{
    // internal only for tests
    internal ConcurrentDictionary<TKey, TimeBasedEntry<TValue>> Cache { get; } = new();

    // internal only for tests
    private readonly SemaphoreSlim _cleaningSlim = new(1, 1);
    private readonly int _capacity;
    private readonly int _threshold;
    private int _disposed;

    public ReadOptimizedLruCache(int capacity, int threshold)
    {
        if (capacity <= 0 || threshold <= 0) throw new ArgumentException("Threshold value or capacity cannot be negative or zero.");
        if (capacity >= threshold) throw new ArgumentException("Threshold value cannot be less than or equal capacity.");

        _capacity = capacity;
        _threshold = threshold;
    }


    /// <summary>
    /// Try get value corresponds to given key.
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="val">Value</param>
    /// <returns>True if key exists or false.</returns>
    /// <exception cref="ArgumentNullException">If key is null.</exception>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue val)
    {
        if (_disposed == 1) throw new ObjectDisposedException("Cache is disposed.");

        if (key is null) throw new ArgumentNullException(nameof(key));

        if (Cache.TryGetValue(key, out var entry))
        {
            entry.Touch();
            val = entry.Value;
            return true;
        }

        val = default;
        return false;
    }

    /// <summary>
    /// Adds the given key value to cache.
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="val">Value</param>
    /// <exception cref="ArgumentNullException">If key is null.</exception>
    public void Add(TKey key, TValue val)
    {
        if (_disposed == 1) throw new ObjectDisposedException("Cache is disposed.");

        if (key is null) throw new ArgumentNullException(nameof(key));

        var entry = new TimeBasedEntry<TValue>(val);
        var addVal = Cache.GetOrAdd(key, entry);

        if (!addVal.Equals(val) && Cache.Count > _threshold)
            DoEviction();
    }

    /// <summary>
    /// Tries to remove an entry identified by a key.
    /// </summary>
    /// <param name="key">Key to be removed</param>
    /// <param name="value">The removed value, if any, otherwise the default value.</param>
    /// <returns>True if key pair removed, otherwise false.</returns>
    /// <exception cref="ObjectDisposedException">If cache disposed</exception>
    /// <exception cref="ArgumentNullException">If key null.</exception>
    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (_disposed == 1) throw new ObjectDisposedException("Cache is disposed.");

        if (key is null) throw new ArgumentNullException(nameof(key));

        if (Cache.TryRemove(key, out var entry))
        {
            value = entry.Value;
            return true;
        }

        value = default;
        return false;
    }

    private void DoEviction()
    {
        // Don't block if a thread already doing eviction.
        if (_cleaningSlim.CurrentCount == 0) return;

        _cleaningSlim.Wait();

        try
        {
            if (Cache.Count < _threshold) return;

            var countOfEntriesToRemoved = Cache.Count - _capacity;
#if NET6_0_OR_GREATER
            var q = new PriorityQueue<long, long>();

            foreach (var val in Cache)
                q.Enqueue(val.Value.LastTouch, val.Value.LastTouch);

            for (var i = 0; i < countOfEntriesToRemoved; i++)
                q.Dequeue();

            var cutOff = q.Dequeue();
#else
            var cutOff = Cache
                .OrderBy(p => p.Value.LastTouch)
                .ElementAt(countOfEntriesToRemoved)
                .Value.LastTouch;
#endif
            foreach (var t in Cache)
            {
                if (t.Value.LastTouch < cutOff)
                     Cache.TryRemove(t.Key, out _);
            }
        }
        finally
        {
            _cleaningSlim.Release();
        }
    }

    public void Dispose()
    {
        Interlocked.CompareExchange(ref _disposed, 1, 0);

        if (_disposed == 1) return;
        Cache.Clear();
        _cleaningSlim.Dispose();
    }
}
