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

using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.NearCaching;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.NearCache
{
    // this was NearCacheStalenessTest.NoStaleValue_WhenUpdatedByMultipleThreads in the original code
    //
    // but ... what it really does is ensure that if values are changed on the server
    // by some other code, the cache is properly invalidated/refreshed.

    [TestFixture]
    public class NearCacheTracksRemoteChangesTest : NearCacheTestBase
    {
        private readonly string _name = "nc-" + TestUtils.RandomString();

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

            options.NearCaches["nc*"] = new NearCacheOptions
            {
                EvictionPolicy = EvictionPolicy.None,
                InvalidateOnChange = true,
                InMemoryFormat = InMemoryFormat.Object
            };

            options.NearCache.MaxToleratedMissCount = 0;

            return options;
        }

        [Test]
        public async Task NearCacheTracksRemoteChanges()
        {
            var dictionary = await _client.GetMapAsync<int, int>(_name);
            await using var _ = new AsyncDisposable(dictionary.DestroyAsync);
            var cache = GetNearCache(dictionary);

            var tasks = new List<Task>();
            var stop = false;

            const int entryCount = 10;
            const int writerTaskCount = 1;
            const int readerTaskCount = 10;

            // start tasks that add / update (thus invalidate) values on the server
            for (var i = 0; i < writerTaskCount; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    while (!stop)
                    {
                        Assert.That((await PopulateMapWithRandomValueFromServerAsync(_name, entryCount)).Success);
                        await Task.Delay(100);
                    }
                }));
            }

            // start tasks that read values
            for (var i = 0; i < readerTaskCount; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    while (!stop)
                    {
                        for (var j = 0; j < entryCount && !stop; j++)
                        {
                            await dictionary.GetAsync(j);
                        }
                        await Task.Delay(100);
                    }
                }));
            }

            // run for some time
            await Task.Delay(8_000);

            // stop tasks
            stop = true;
            await Task.WhenAll(tasks);

            // assert that eventually all cache values will match what's on the member
            await AssertEx.SucceedsEventually(async () =>
            {
                // 1. get all values, some should come from the cache
                var cacheValues = new List<int>();
                for (var i = 0; i < entryCount; i++)
                    cacheValues.Add(await dictionary.GetAsync(i));

                // 2. get all values from server directly
                var memberValues = await GetAllValueFromMemberAsync(entryCount, _name);

                // after a while, all values in the cache will be same as server
                for (var i = 0; i < entryCount; i++)
                    Assert.AreEqual(memberValues[i], cacheValues[i]);

            }, 10000,500);
        }
    }
}
