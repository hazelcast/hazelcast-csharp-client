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
        private IHDictionary<object, object> _dictionary;

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

            _dictionary = await _client.GetDictionaryAsync<object, object>("nc-" + TestUtils.RandomString());

            var nearCache = GetNearCache(_dictionary);
            Assert.That(nearCache, Is.InstanceOf<NearCaching.NearCache>());
        }

        [TearDown]
        public async Task TearDown()
        {
            await _dictionary.DestroyAsync();
            _dictionary = null;

            await _client.DisposeAsync();
            _client = null;
        }

        protected override HazelcastOptions CreateHazelcastOptions()
        {
            var options = base.CreateHazelcastOptions();

            var nearCacheOptions = options.NearCache;

            nearCacheOptions.Configurations["nc*"] = new NearCacheNamedOptions
            {
                EvictionPolicy = EvictionPolicy.None,
                InvalidateOnChange = false,
                InMemoryFormat = InMemoryFormat.Object,
                MaxSize = MaxSize
            };

            nearCacheOptions.Configurations["nc-invalidate*"] = new NearCacheNamedOptions
            {
                InvalidateOnChange = true
            };

            nearCacheOptions.Configurations["nc-lru*"] = new NearCacheNamedOptions
            {
                EvictionPolicy = EvictionPolicy.Lru,
                InvalidateOnChange = false,
                MaxSize = MaxSize
            };

            nearCacheOptions.Configurations["nc-lfu*"] = new NearCacheNamedOptions
            {
                EvictionPolicy = EvictionPolicy.Lfu,
                InvalidateOnChange = false,
                MaxSize = MaxSize
            };

            nearCacheOptions.Configurations["nc-ttl*"] = new NearCacheNamedOptions
            {
                TimeToLiveSeconds = 1,
                InvalidateOnChange = false
            };

            nearCacheOptions.Configurations["nc-idle*"] = new NearCacheNamedOptions
            {
                MaxIdleSeconds = 1,
                CleanupPeriodSeconds = 2,
                InvalidateOnChange = false
            };

            return options;
        }

        [Test]
        public async Task TestNearCache()
        {
            for (var i = 0; i < MaxSize; i++)
                await _dictionary.AddOrUpdateAsync("key" + i, "value" + i);

            var begin = DateTime.Now;
            for (var i = 0; i < MaxSize; i++)
                await _dictionary.GetAsync("key" + i);

            var firstRead = DateTime.Now - begin;
            begin = DateTime.Now;
            for (var i = 0; i < MaxSize; i++)
                await _dictionary.GetAsync("key" + i);

            var secondRead = DateTime.Now - begin;

            // it's faster the second time
            Assert.IsTrue(secondRead < firstRead);

            var cache = GetNearCache(_dictionary);
            Assert.That(cache.Count, Is.EqualTo(MaxSize));
            Assert.That(cache.Statistics.EntryCount, Is.EqualTo(MaxSize));
        }

        [Test]
        public async Task TestNearCacheContains()
        {
            await _dictionary.AddOrUpdateAsync("key", "value");

            await _dictionary.GetAsync("key");
            await _dictionary.GetAsync("invalidKey");

            Assert.That(await _dictionary.ContainsKeyAsync("key"));
            Assert.That(await _dictionary.ContainsKeyAsync("invalidKey"), Is.False);
        }

        [Test]
        public async Task TestNearCacheGet()
        {
            await _dictionary.AddOrUpdateAsync("key", "value");
            await _dictionary.GetAsync("key");

            var cache = GetNearCache(_dictionary);

            var getAttempt = await cache.TryGetAsync(ToData("key"));
            Assert.That(getAttempt.Success, Is.True);
            Assert.That(getAttempt.Value, Is.EqualTo("value"));
        }

        [Test]
        public async Task TestNearCacheGetAll()
        {
            var keys = new List<object>();
            for (var i = 0; i < 100; i++)
            {
                await _dictionary.AddOrUpdateAsync("key" + i, "value" + i);
                keys.Add("key" + i);
            }

            await _dictionary.GetAsync(keys);

            var cache = GetNearCache(_dictionary);

            Assert.AreEqual(100, cache.Count);
            Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);
        }

        [Test]
        public async Task TestNearCacheGetAsync()
        {
            await _dictionary.AddOrUpdateAsync("key", "value");

            var result = await _dictionary.GetAsync("key");
            Assert.AreEqual("value", result);

            var cache = GetNearCache(_dictionary);

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.AreEqual(1, cache.Count);

            }, 8000, 1000);

            result = await _dictionary.GetAsync("key");
            Assert.AreEqual("value", result);

            var cacheEntries = await cache.SnapshotEntriesAsync();
            Assert.That(cacheEntries.Count, Is.EqualTo(1));
            Assert.That(cacheEntries.First().Hits, Is.EqualTo(1));
        }

        [Test]
        public async Task TestNearCacheIdleEviction()
        {
            var dictionary = await _client.GetDictionaryAsync<int, int>("nc-idle-" + TestUtils.RandomString());
            var cache = GetNearCache(dictionary);

            var keys = Enumerable.Range(0, 10).ToList();
            foreach (var k in keys)
                await dictionary.AddOrUpdateAsync(k, k);
            Assert.AreEqual(0, cache.Count); // cache is still empty
            Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);

            const int nonIdleKey = 100;
            await dictionary.AddOrUpdateAsync(nonIdleKey, nonIdleKey);
            Assert.AreEqual(0, cache.Count); // cache is still empty
            Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);

            await dictionary.GetAsync(keys); // populates the cache
            Assert.AreEqual(keys.Count, cache.Count); // which only contains the 10 keys
            Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);

            await dictionary.GetAsync(nonIdleKey); // also the non-idle key
            Assert.AreEqual(keys.Count + 1, cache.Count); // now also contains the idle key
            Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);

            var nonIdleKeyData = ToData(nonIdleKey);

            await AssertEx.SucceedsEventually(async () =>
            {
                var val = await dictionary.GetAsync(nonIdleKey); // force ttl check
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
            var dictionary = await _client.GetDictionaryAsync<string, string>("nc-invalidate-" + TestUtils.RandomString());
            for (var i = 0; i < 100; i++)
                await dictionary.AddOrUpdateAsync("key" + i, "value" + i);

            for (var i = 0; i < 100; i++)
                await dictionary.GetAsync("key" + i);

            var cache = GetNearCache(dictionary);

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
            var dictionary = await _client.GetDictionaryAsync<object, object>("nc-lfu-" + TestUtils.RandomString());
            var keys = new List<object>();
            for (var i = 0; i < MaxSize; i++)
            {
                await dictionary.AddOrUpdateAsync(i, i);
                keys.Add(i);
            }

            // make sure all keys are cached
            await dictionary.GetAsync(keys);

            // make keys in sublist accessed again
            var subList = keys.Take(MaxSize / 2).ToList();
            await dictionary.GetAsync(subList);

            // Add another item, triggering eviction
            await dictionary.AddOrUpdateAsync(MaxSize, MaxSize);
            await dictionary.GetAsync(MaxSize);

            var cache = GetNearCache(dictionary);

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
            var dictionary = await _client.GetDictionaryAsync<string, string>("nc-invalidate-" + TestUtils.RandomString());
            await dictionary.AddOrUpdateAsync("key", "value");

            var value = await dictionary.GetAsync("key");
            Assert.AreEqual("value", value);

            var cache = GetNearCache(dictionary);

            Assert.AreEqual(1, cache.Count);
            Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);

            await dictionary.RemoveAsync("key");
            Assert.Null(await dictionary.GetAsync("key"));
        }

        [Test]
        public async Task TestNearCacheLruEviction()
        {
            var dictionary = await _client.GetDictionaryAsync<object, object>("nc-lru-" + TestUtils.RandomString());
            var keys = new List<object>();
            for (var i = 0; i < MaxSize; i++)
            {
                await dictionary.AddOrUpdateAsync(i, i);
                keys.Add(i);
            }

            // make sure all keys are cached
            await dictionary.GetAsync(keys);

            // make keys in sublist accessed again
            var subList = keys.Take(MaxSize / 2).ToList();
            await dictionary.GetAsync(subList);

            // Add another item, triggering eviction
            await dictionary.AddOrUpdateAsync(MaxSize, MaxSize);
            await dictionary.GetAsync(MaxSize);

            var cache = GetNearCache(dictionary);

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
            var keys = new List<object>();
            for (var i = 0; i < MaxSize * 2; i++)
            {
                await _dictionary.AddOrUpdateAsync(i, i);
                keys.Add(i);
            }
            await _dictionary.GetAsync(keys);
            var cache = GetNearCache(_dictionary);
            Assert.AreEqual(MaxSize, cache.Count);
            Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);
        }

        [Test]
        [Timeout(20000)]
        public async Task TestNearCacheTtlEviction()
        {
            var dictionary = await _client.GetDictionaryAsync<int, int>("nc-ttl-" + TestUtils.RandomString());

            try
            {
                var keys = Enumerable.Range(0, 10).ToList();
                foreach (var k in keys)
                    await dictionary.AddOrUpdateAsync(k, k);

                await dictionary.GetAsync(keys);
                var cache = GetNearCache(dictionary);

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
            finally
            {
                await _dictionary.DestroyAsync();
            }
        }

        private async Task TestInvalidateAsync(Func<IHDictionary<string, string>, string, Task> invalidatingAction)
        {
            var dictionary = await _client.GetDictionaryAsync<string, string>("nc-invalidate-" + TestUtils.RandomString());

            try
            {
                await dictionary.AddOrUpdateAsync("key", "value");

                var value = await dictionary.GetAsync("key");
                Assert.AreEqual("value", value);

                var cache = GetNearCache(dictionary);

                Assert.AreEqual(1, cache.Count);
                Assert.AreEqual(cache.Count, cache.Statistics.EntryCount);

                await using (var client = await CreateAndStartClientAsync())
                {
                    await invalidatingAction(await client.GetDictionaryAsync<string, string>(dictionary.Name), "key");
                }

                var keyData = ToData("key");

                await AssertEx.SucceedsEventually(async () =>
                {

                    Assert.IsFalse(await cache.ContainsKeyAsync(keyData), "Key should have been invalidated");

                }, 8000, 1000);
            }
            finally
            {
                await _dictionary.DestroyAsync();
            }
        }
    }
}