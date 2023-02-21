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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Hazelcast.Models;

namespace Hazelcast.Core;

internal class ReadOptimizedLruCache<TKey, TValue> : IDisposable
{
    // internal only for tests
    internal readonly ConcurrentDictionary<TKey, TimeBasedEntry<TValue>> dictionary = new();
    private readonly SemaphoreSlim _cleaningSlim = new (1, 1);
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

        if (dictionary.TryGetValue(key, out var entry))
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
        var addVal = dictionary.GetOrAdd(key, entry);

        if (!addVal.Equals(val) && dictionary.Count > _threshold)
            DoEviction();
    }

    private void DoEviction()
    {
        _cleaningSlim.Wait();

        try
        {
            if (dictionary.Count < _threshold) return;

            var timestamps = dictionary
                .OrderBy(p => p.Value.LastTouch)
                .ToArray();

            var countOfEntriesToRemoved = _threshold - _capacity;

            for (var i = 0; i <= countOfEntriesToRemoved; i++)
            {
                dictionary.TryRemove(timestamps[i].Key, timestamps[i].Value);
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
        dictionary.Clear();
        _cleaningSlim.Dispose();
    }
}
