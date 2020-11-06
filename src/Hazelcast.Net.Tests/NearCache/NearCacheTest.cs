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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.NearCaching;
using Hazelcast.Predicates;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.NearCache
{
    public class NearCacheTest : NearCacheTestBase
    {
        private const int MaxSize = 1000;

        private IHazelcastClient _client;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            // add a member to the cluster
            await AddMember();
        }

        [SetUp]
        public async Task Init()
        {
            _client = await CreateAndStartClientAsync();
            var client = _client as HazelcastClient;
            Assert.That(client, Is.Not.Null);
            SerializationService = client.SerializationService;
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_client != null) await _client.DisposeAsync();
            _client = null;
        }

        protected override HazelcastOptions CreateHazelcastOptions()
        {
            var options = base.CreateHazelcastOptions();
            var nearCacheOptions = options.NearCache;

            nearCacheOptions.Caches["nc*"] = new NearCacheOptions
            {
                EvictionPolicy = EvictionPolicy.None,
                InvalidateOnChange = false,
                InMemoryFormat = InMemoryFormat.Object,
                MaxSize = MaxSize
            };

            nearCacheOptions.Caches["nc-invalidate*"] = new NearCacheOptions
            {
                InvalidateOnChange = true
            };

            nearCacheOptions.Caches["nc-lru*"] = new NearCacheOptions
            {
                EvictionPolicy = EvictionPolicy.Lru,
                InvalidateOnChange = false,
                MaxSize = MaxSize
            };

            nearCacheOptions.Caches["nc-lfu*"] = new NearCacheOptions
            {
                EvictionPolicy = EvictionPolicy.Lfu,
                InvalidateOnChange = false,
                MaxSize = MaxSize
            };

            nearCacheOptions.Caches["nc-ttl*"] = new NearCacheOptions
            {
                TimeToLiveSeconds = 1,
                InvalidateOnChange = false
            };

            nearCacheOptions.Caches["nc-idle*"] = new NearCacheOptions
            {
                MaxIdleSeconds = 1,
                CleanupPeriodSeconds = 2,
                InvalidateOnChange = false
            };

            return options;
        }

        [Test]
        public async Task TestNearCache() // CacheIsPopulatedByReads
        {
            var dictionary = await _client.GetMapAsync<object, object>("nc-" + TestUtils.RandomString());
            await using var _ = new AsyncDisposable(dictionary.DestroyAsync);
            var cache = GetNearCache(dictionary);

            // add values to the dictionary
            for (var i = 0; i < MaxSize; i++)
                await dictionary.SetAsync("key" + i, "value" + i);

            // near cache remains empty
            Assert.That(cache.Count, Is.EqualTo(0));

            // get values, this will populate near cache
            var begin = DateTime.Now;
            for (var i = 0; i < MaxSize; i++)
                await dictionary.GetAsync("key" + i);
            var firstRead = DateTime.Now - begin;

            // get values again, this should read from near cache = be way faster
            // TODO: we should have a way (instrumentation) to count server requests
            begin = DateTime.Now;
            for (var i = 0; i < MaxSize; i++)
                await dictionary.GetAsync("key" + i);
            var secondRead = DateTime.Now - begin;

            // verify it's faster the second time
            Assert.IsTrue(secondRead < firstRead);

            // verify the cache contains all values
            Assert.That(cache.Count, Is.EqualTo(MaxSize));
            Assert.That(cache.Statistics.EntryCount, Is.EqualTo(MaxSize));
        }

        [Test]
        public async Task TestNearCacheContains() // CacheIsUsedForContainsKey
        {
            var dictionary = await _client.GetMapAsync<object, object>("nc-" + TestUtils.RandomString());
            await using var _ = new AsyncDisposable(dictionary.DestroyAsync);
            //var cache = GetNearCache(dictionary);

            // add a value to the dictionary
            await dictionary.SetAsync("key", "value");

            // get two values, one existing and one non-existing
            // this will hit the server
            await dictionary.GetAsync("key");
            await dictionary.GetAsync("invalidKey");

            // get again - the first one should not hit the server
            Assert.That(await dictionary.ContainsKeyAsync("key"));
            Assert.That(await dictionary.ContainsKeyAsync("invalidKey"), Is.False);
        }

        [Test]
        public async Task TestNearCacheGet() // CacheIsPopulatedByRead
        {
            var dictionary = await _client.GetMapAsync<object, object>("nc-" + TestUtils.RandomString());
            await using var _ = new AsyncDisposable(dictionary.DestroyAsync);
            var cache = GetNearCache(dictionary);

            // add a vale to the dictionary
            await dictionary.SetAsync("key", "value");

            // get the value = populates the cache
            await dictionary.GetAsync("key");

            // validate that the value is in the cache
            var getAttempt = await cache.TryGetAsync(ToData("key"));

            Assert.That(getAttempt.Success, Is.True);
            Assert.That(getAttempt.Value, Is.EqualTo("value"));
        }

        [Test]
        public async Task TestNearCacheGetAll() // CacheIsPopulatedByReadMany
        {
            var dictionary = await _client.GetMapAsync<string, string>("nc-" + TestUtils.RandomString());
            await using var _ = new AsyncDisposable(dictionary.DestroyAsync);
            var cache = GetNearCache(dictionary);

            // add values to the dictionary
            var keys = new List<string>();
            for (var i = 0; i < 100; i++)
            {
                await dictionary.SetAsync("key" + i, "value" + i);
                keys.Add("key" + i);
            }

            // get all values
            await dictionary.GetAllAsync(keys);

            // validate that all values are in the cache
            Assert.AreEqual(100, cache.Count);
            Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);
        }

        [Test]
        public async Task TestNearCacheGetAsync() // EntryIsHit
        {
            var dictionary = await _client.GetMapAsync<object, object>("nc-" + TestUtils.RandomString());
            await using var _ = new AsyncDisposable(dictionary.DestroyAsync);
            var cache = GetNearCache(dictionary);

            // add a value to the dictionary
            await dictionary.SetAsync("key", "value");

            // get the value
            var result = await dictionary.GetAsync("key");
            Assert.That(result.Success);
            Assert.AreEqual("value", result.Value);

            // validate that the cache contains a value
            Assert.AreEqual(1, cache.Count);

            // and that the corresponding entry has zero hits
            var cacheEntries = await cache.SnapshotEntriesAsync();
            Assert.That(cacheEntries.Count, Is.EqualTo(1));
            Assert.That(cacheEntries.First().Hits, Is.EqualTo(0));

            // get the value again
            result = await dictionary.GetAsync("key");
            Assert.That(result.Success);
            Assert.AreEqual("value", result.Value);

            // validate that the entry now has one hit
            cacheEntries = await cache.SnapshotEntriesAsync();
            Assert.That(cacheEntries.Count, Is.EqualTo(1));
            Assert.That(cacheEntries.First().Hits, Is.EqualTo(1));
        }

        [Test]
        public async Task TestNearCacheIdleEviction()
        {
            var dictionary = await _client.GetMapAsync<int, int>("nc-idle-" + TestUtils.RandomString());
            await using var _ = new AsyncDisposable(dictionary.DestroyAsync);
            var cache = GetNearCache(dictionary);

            var keys = Enumerable.Range(0, 10).ToList();
            foreach (var k in keys)
                await dictionary.SetAsync(k, k);
            Assert.AreEqual(0, cache.Count); // cache is still empty
            Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);

            const int nonIdleKey = 100;
            await dictionary.SetAsync(nonIdleKey, nonIdleKey);
            Assert.AreEqual(0, cache.Count); // cache is still empty
            Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);

            await dictionary.GetAllAsync(keys); // populates the cache
            Assert.AreEqual(keys.Count, cache.Count); // which only contains the 10 keys
            Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);

            await dictionary.GetAsync(nonIdleKey); // also the non-idle key
            Assert.AreEqual(keys.Count + 1, cache.Count); // now also contains the idle key
            Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);

            var nonIdleKeyData = ToData(nonIdleKey);

            await AssertEx.SucceedsEventually(async () =>
            {
                var (succ, val) = await dictionary.GetAsync(nonIdleKey); // force ttl check
                Assert.That(succ);
                Assert.That(val, Is.EqualTo(100));

                // note: ContainsKeyAsync(, false) to make sure we don't hit the entries,
                // else they would never ever become idle of course...

                // idle keys are going away
                foreach (var k in keys)
                {
                    var keyData = ToData(k);
                    Assert.IsFalse(await cache.ContainsKeyAsync(keyData, false), "Key " + k + " should have expired.");
                }

                // non-idle key is never going away
                Assert.IsTrue(await cache.ContainsKeyAsync(nonIdleKeyData, false), "Key 100 should not have expired.");

            }, 8000, 200);
        }

        [Test]
        public async Task TestNearCacheInvalidationOnClear()
        {
            await TestInvalidateAsync((m, k) => m.ClearAsync());
        }

        [Test]
        public async Task TestNearCacheInvalidationOnEvict()
        {
            await TestInvalidateAsync((m, k) => m.EvictAllAsync());
        }

        [Test]
        public async Task TestNearCacheInvalidationOnPut()
        {
            await TestInvalidateAsync((m, k) => m.RemoveAsync(k));
        }

        [Test]
        public async Task TestNearCacheInvalidationOnRemove()
        {
            await TestInvalidateAsync((m, k) => m.RemoveAsync(k));
        }

        [Test]
        public async Task TestNearCacheInvalidationOnRemoveAllPredicate()
        {
            var dictionary = await _client.GetMapAsync<string, string>("nc-invalidate-" + TestUtils.RandomString());
            await using var _ = new AsyncDisposable(dictionary.DestroyAsync);
            var cache = GetNearCache(dictionary);

            for (var i = 0; i < 100; i++)
                await dictionary.SetAsync("key" + i, "value" + i);

            for (var i = 0; i < 100; i++)
                await dictionary.GetAsync("key" + i);

            Assert.AreEqual(100, cache.Count);
            Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);

            await dictionary.RemoveAsync(new SqlPredicate("this == 'value2'"));

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.AreEqual(0, cache.Count);
                Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);

            }, 8000, 500);
        }

        [Test]
        public async Task TestNearCacheLfuEviction()
        {
            var dictionary = await _client.GetMapAsync<int, int>("nc-lfu-" + TestUtils.RandomString());
            await using var _ = new AsyncDisposable(dictionary.DestroyAsync);
            var cache = GetNearCache(dictionary);

            var keys = new List<int>();
            for (var i = 0; i < MaxSize; i++)
            {
                await dictionary.SetAsync(i, i);
                keys.Add(i);
            }

            // make sure all keys are cached
            await dictionary.GetAllAsync(keys);

            // make keys in sublist accessed again
            var subList = keys.Take(MaxSize / 2).ToList();
            await dictionary.GetAllAsync(subList);

            // Add another item, triggering eviction
            await dictionary.SetAsync(MaxSize, MaxSize);
            await dictionary.GetAsync(MaxSize);

            // var sl = new SortedList<int, int>();
            //
            // foreach (var cacheRecord in cache.Records.Keys)
            // {
            //     sl.Add(ClientInternal.SerializationService.ToObject<int>(cacheRecord), 0);
            // }

            // foreach (var lazy in cache.Records.Values)
            // {
            //     if (lazy.Value.Hit.Get() > 0)
            //     {
            //         Console.WriteLine($"non zero hit {ClientInternal.SerializationService.ToObject<int>(lazy.Value.Key)}");
            //     }
            // }

            await AssertEx.SucceedsEventually(async () =>
            {
                Assert.IsTrue(cache.Count <= MaxSize, cache.Count + " should be less than " + MaxSize);
                Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);

                foreach (var key in subList)
                {
                    var keyData = ToData(key);
                    Assert.IsTrue(await cache.ContainsKeyAsync(keyData), "key " + key + " not found in cache");
                }
            }, 8000, 1000);
        }

        [Test]
        public async Task TestNearCacheLocalInvalidations()
        {
            var dictionary = await _client.GetMapAsync<string, string>("nc-invalidate-" + TestUtils.RandomString());
            await using var _ = new AsyncDisposable(dictionary.DestroyAsync);
            var cache = GetNearCache(dictionary);

            await dictionary.SetAsync("key", "value");

            var (success, value) = await dictionary.GetAsync("key");
            Assert.That(success);
            Assert.AreEqual("value", value);

            Assert.AreEqual(1, cache.Count);
            Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);

            await dictionary.RemoveAsync("key");
            (success, value) = await dictionary.GetAsync("key");
            Assert.That(success, Is.False);
        }

        [Test]
        public async Task TestNearCacheLruEviction()
        {
            var dictionary = await _client.GetMapAsync<int, int>("nc-lru-" + TestUtils.RandomString());
            await using var _ = new AsyncDisposable(dictionary.DestroyAsync);
            var cache = GetNearCache(dictionary);

            var keys = new List<int>();
            for (var i = 0; i < MaxSize; i++)
            {
                await dictionary.SetAsync(i, i);
                keys.Add(i);
            }

            // make sure all keys are cached
            await dictionary.GetAllAsync(keys);

            // make keys in sublist accessed again
            var subList = keys.Take(MaxSize / 2).ToList();
            await dictionary.GetAllAsync(subList);

            // Add another item, triggering eviction
            await dictionary.SetAsync(MaxSize, MaxSize);
            await dictionary.GetAsync(MaxSize);

            await AssertEx.SucceedsEventually(async () =>
            {
                Assert.IsTrue(cache.Count <= MaxSize, cache.Count + " should be less than " + MaxSize);
                Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);

                foreach (var key in subList)
                {
                    var keyData = ToData(key);
                    Assert.IsTrue(await cache.ContainsKeyAsync(keyData), "key " + key + " not found in cache");
                }
            }, 8000, 1000);
        }

        [Test]
        public async Task TestNearCacheNoEviction()
        {
            var dictionary = await _client.GetMapAsync<int, int>("nc-" + TestUtils.RandomString());
            await using var _ = new AsyncDisposable(dictionary.DestroyAsync);
            var cache = GetNearCache(dictionary);

            var keys = new List<int>();
            for (var i = 0; i < MaxSize * 2; i++)
            {
                await dictionary.SetAsync(i, i);
                keys.Add(i);
            }
            await dictionary.GetAllAsync(keys);

            Assert.AreEqual(MaxSize, cache.Count);
            Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);
        }

        [Test]
        [Timeout(20000)]
        public async Task TestNearCacheTtlEviction()
        {
            var dictionary = await _client.GetMapAsync<int, int>("nc-ttl-" + TestUtils.RandomString());
            await using var _ = new AsyncDisposable(dictionary.DestroyAsync);
            var cache = GetNearCache(dictionary);

            var keys = Enumerable.Range(0, 10).ToList();
            foreach (var k in keys)
                await dictionary.SetAsync(k, k);

            await dictionary.GetAllAsync(keys);

            Assert.AreEqual(keys.Count, cache.Count);

            await AssertEx.SucceedsEventually(async () =>
            {
                await dictionary.GetAsync(100); // force ttl check
                foreach (var k in keys)
                {
                    var keyData = ToData(k);
                    Assert.That(await cache.ContainsKeyAsync(keyData), Is.False, $"Key {k} should have expired.");
                }

            }, 8000, 1000);
        }

        private async Task TestInvalidateAsync(Func<IHMap<string, string>, string, Task> invalidatingAction)
        {
            var dictionary = await _client.GetMapAsync<string, string>("nc-invalidate-" + TestUtils.RandomString());
            await using var _ = new AsyncDisposable(dictionary.DestroyAsync);
            var cache = GetNearCache(dictionary);

            await dictionary.SetAsync("key", "value");

            var (success, value) = await dictionary.GetAsync("key");
            Assert.That(success);
            Assert.AreEqual("value", value);

            Assert.AreEqual(1, cache.Count);
            Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);

            await using (var client = await CreateAndStartClientAsync())
            {
                await invalidatingAction(await client.GetMapAsync<string, string>(dictionary.Name), "key");
            }

            var keyData = ToData("key");

            await AssertEx.SucceedsEventually(async () =>
            {

                Assert.IsFalse(await cache.ContainsKeyAsync(keyData), "Key should have been invalidated");

            }, 8000, 1000);
        }
    }
}