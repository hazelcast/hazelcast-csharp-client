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
using System.Threading.Tasks;
using NUnit.Framework;
using Hazelcast.Core;

namespace Hazelcast.Tests.Core;

public class ReadOptimizedLruCacheTests
{
    [Test]
    public void TestWrongCapacityThrows()
    {
        Assert.Throws<ArgumentException>(() => new ReadOptimizedLruCache<int, int>(-1, 0));
        Assert.Throws<ArgumentException>(() => new ReadOptimizedLruCache<int, int>(1, 0));
        Assert.Throws<ArgumentException>(() => new ReadOptimizedLruCache<int, int>(0, -1));
        Assert.Throws<ArgumentException>(() => new ReadOptimizedLruCache<int, int>(10, 9));
        var correct = new ReadOptimizedLruCache<int, int>(10, 15);
    }

    [Test]
    public void TestAdd()
    {
        var cache = new ReadOptimizedLruCache<string, string>(10, 15);

        cache.Add("1", "1");
        cache.Add("2", null);

        Assert.Throws<ArgumentNullException>(() => cache.Add(null, "1"));
    }

    [Test]
    public async Task TestAddAndGetRefreshEntry()
    {
        var cache = new ReadOptimizedLruCache<int, int>(10, 15);

        cache.Add(1, 1);
        cache.dictionary.TryGetValue(1, out var entry);
        var firstTouch = entry.LastTouch;
        await Task.Delay(100);
        var keyExist = cache.TryGetValue(1, out var _);

        Assert.True(keyExist);

        var secondTouch = entry.LastTouch;

        Assert.Greater(secondTouch, firstTouch);
    }

    [Test]
    public void TestEviction()
    {
        var capacity = 10;
        var threshold = 15;
        var cache = new ReadOptimizedLruCache<int, int>(capacity, threshold);

        for (var i = 0; i < threshold; i++)
            cache.Add(i, i);

        Assert.AreEqual(cache.dictionary.Count, threshold);
        // Last stroke to break camel's back
        cache.Add(15, 15);
        // The cache size exceeds the threshold, so remove the least recent used # of (threshold - capacity) items
        Assert.AreEqual(capacity, cache.dictionary.Count);
    }

    [Test]
    public void TestNoOperationAfterDispose()
    {
        var cache = new ReadOptimizedLruCache<int, int>(5, 10);
        cache.Dispose();

        Assert.Throws<ObjectDisposedException>(() => cache.Add(1, 1));
        Assert.Throws<ObjectDisposedException>(() => cache.TryGetValue(1, out _));
    }
}
