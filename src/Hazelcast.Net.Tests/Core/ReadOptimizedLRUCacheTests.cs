// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Hazelcast.Core;
using Hazelcast.Models;

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
        cache.Cache.TryGetValue(1, out var entry);
        var firstTouch = entry.LastTouch;
        await Task.Delay(100);
        var keyExist = cache.TryGetValue(1, out var _);

        Assert.True(keyExist);

        var secondTouch = entry.LastTouch;

        Assert.Greater(secondTouch, firstTouch);
    }

    [Test]
    public async Task TestEviction()
    {
        var capacity = 10;
        var threshold = 15;
        var cache = new ReadOptimizedLruCache<int, int>(capacity, threshold);

        for (var i = 0; i < threshold; i++)
        {
            cache.Add(i, i);
            await Task.Delay(10);
        }
            

        Assert.AreEqual(cache.Cache.Count, threshold);
        // Last stroke to break camel's back
        cache.Add(15, 15);
        // The cache size exceeds the threshold, so remove the least recent used # of (threshold - capacity) items
        Assert.AreEqual(capacity, cache.Cache.Count);
    }

    [Test]
    public void TestOperationsThrowAfterDispose()
    {
        var cache = new ReadOptimizedLruCache<int, int>(5, 10);
        cache.Dispose();

        Assert.Throws<ObjectDisposedException>(() => cache.Add(1, 1));
        Assert.Throws<ObjectDisposedException>(() => cache.TryGetValue(1, out _));
        //doesn't throw.
        cache.Dispose();
    }

    [Test]
    public async Task TestEvictionHappensWithSynchronization()
    {
        var capacity = 10;
        var threshold = 15;
        var cache = new ReadOptimizedLruCache<int, int>(capacity, threshold);
        var slim = new SemaphoreSlim(0, 2);

        for (var i = 0; i < threshold; i++)
        {
            cache.Add(i, i);
            await Task.Delay(10);
        }
            

        Assert.AreEqual(cache.Cache.Count, threshold);

        var taskAddAndEvict = Task.Run(() =>
        {
            slim.Wait();
            cache.Add(15, 15);
            slim.Release();
        });

        Assert.AreNotEqual(TaskStatus.RanToCompletion, taskAddAndEvict.Status);

        // Add one more item, expect that first task `taskAddAndEvict` will remove the old ones,
        // and `taskAddAndEvict2` won't do eviction since it won't be necessary.
        var taskAddAndEvict2 = Task.Run(() =>
        {
            slim.Wait();
            cache.Add(16, 16);
            slim.Release();
        });

        Assert.AreNotEqual(TaskStatus.RanToCompletion, taskAddAndEvict2.Status);

        Thread.Sleep(500);
        slim.Release(2);
        await Task.WhenAll(taskAddAndEvict, taskAddAndEvict2);

        // Cache size will shrink eventually. 
        Assert.LessOrEqual(cache.Cache.Count, threshold);
    }

    [Test]
    public async Task TestEvictionIsCorrect()
    {
        var capacity = 10;
        var threshold = 15;
        var cache = new ReadOptimizedLruCache<int, int>(capacity, threshold);

        // +1 to escape from default value -0- of int.
        for (var i = 1; i < threshold + 1; i++)
            cache.Add(i, i);

        Assert.AreEqual(cache.Cache.Count, threshold);

        // Refresh some of the entries.
        for (var i = 1; i < capacity + 1; i++)
        {
            await Task.Delay(10);
            cache.TryGetValue(i, out _);
        }

        await Task.Delay(10);
        //Note: Here some delays happened to assert exact evicted items, specially in the head and tail.
        //In real world, it doesn't matter since we already know that eviction behaves correct.
        
        // Keys between [2,10] and 16 will stay, rest will be evicted.
        cache.Add(16, 16);

        Assert.LessOrEqual(cache.Cache.Count, threshold);

        // Notice that although key 1 refreshed key 16 is more recent than key 1. 
        for (var i = 2; i < capacity + 1; i++)
        {
            var result = cache.TryGetValue(i, out var val);
            Assert.AreEqual(i, val);
            Assert.True(result);
        }

        Assert.True(cache.TryGetValue(16, out var val15));
        Assert.AreEqual(16, val15);
        Assert.False(cache.TryGetValue(13, out _));
    }

    [Test]
    public void TestCacheGetCorrect()
    {
        var capacity = 10;
        var threshold = 15;
        var cache = new ReadOptimizedLruCache<int, int>(capacity, threshold);

        cache.Add(1, 1);
        Assert.True(cache.TryGetValue(1, out var val));
        Assert.AreEqual(1, val);

        Assert.False(cache.TryGetValue(2, out var defaultVal));
        Assert.AreEqual(default(int), defaultVal);
    }

    [Test]
    public async Task TestTimeBasedEntryWorks()
    {
        var entry = new TimeBasedEntry<int>(1);
        await Task.Delay(10);
        var firstTouch = entry.LastTouch;
        Assert.Greater(Clock.Milliseconds, firstTouch);
        entry.Touch();
        Assert.Greater(entry.LastTouch, firstTouch);
    }

    [Test]
    public void TestTryRemove()
    {
        var cache = new ReadOptimizedLruCache<string, string>(1, 2);

        Assert.Throws<ArgumentNullException>(() => cache.TryRemove(null, out _));
        Assert.True(!cache.TryRemove("1", out var val) && val == default);
        cache.Add("1", "1");
        Assert.True(cache.TryRemove("1", out var val2) && val2 == "1");
        cache.Dispose();
        Assert.Throws<ObjectDisposedException>(() => cache.TryRemove("1", out _));
    }
}
