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

using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.NearCaching;
using Hazelcast.Testing;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.NearCache
{
    [TestFixture]
    public class NearCacheRecoversFromDistortionsTest : NearCacheTestBase
    {
        private IHazelcastClient _client;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            // add members to the cluster
            await AddMember();
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
            await _client.DisposeAsync();
            _client = null;
        }

        protected override HazelcastOptions CreateHazelcastOptions()
        {
            var options = base.CreateHazelcastOptions();

            var nearCacheOptions = options.NearCache;

            nearCacheOptions.Configurations["nc*"] = new NearCacheNamedOptions
            {
                MaxSize = int.MaxValue,
                InvalidateOnChange = true,
                EvictionPolicy = EvictionPolicy.None,
                InMemoryFormat = InMemoryFormat.Binary
            };

            nearCacheOptions.MaxToleratedMissCount = 0;

            return options;
        }

        protected override string RcClusterConfiguration => Resources.Cluster_NearCache;

        [Test]
        [Timeout(120_000)]
        public void CanConnectToCluster()
        {
            var client = (HazelcastClient) _client;
            var cluster = client.Cluster;

            var x = cluster.Connections;
        }

        [Test]
        [Timeout(240_000)]
        public async Task NearCacheRecoversFromDistortions()
        {
            const string name = "nc-distortion";
            const int size = 100000;
            var stop = false;

            // pre-populate the dictionary with (k,k) pairs for all keys
            Logger.LogInformation("Populate...");
            Assert.True((await PopulateMapFromServerAsync(name, size)).Success);

            var dictionary = await _client.GetDictionaryAsync<int, int>(name);
            await using var _ = new AsyncDisposable(() => dictionary?.DestroyAsync() ?? default);
            var cache = GetNearCache(dictionary);

            Logger.LogInformation("Start tasks...");

            var caching = Task.Run(async () =>
            {
                while (!stop)
                {
                    // reads values, thus populating the cache
                    for (var i = 0; i < size && !stop; i++)
                    {
                        await dictionary.GetAsync(i);
                    }
                }
                Logger.LogInformation("Caching stopped.");
            });

            var distortingSequence = Task.Run(async () =>
            {
                while (!stop)
                {
                    // randomly changes the sequence of a partition on the server
                    var response = await DistortRandomPartitionSequenceAsync(name);
                    Assert.True(response.Success, response.Message);
                    await Task.Delay(1000);
                }
                Logger.LogInformation("Distorting sequence stopped.");
            });

            var distortingUuid = Task.Run(async () =>
            {
                while (!stop)
                {
                    // randomly changes the uuid of a partition on the server
                    var response = await DistortRandomPartitionUuidAsync();
                    Assert.That(response.Success, response.Message);
                    await Task.Delay(5000);
                }
                Logger.LogInformation("Distorting uuid stopped.");
            });

            var addingOnMember = Task.Run(async () =>
            {
                while (!stop)
                {
                    // adds or update on the server
                    var key = RandomProvider.Random.Next(size);
                    var value = RandomProvider.Random.Next(int.MaxValue);
                    var response = await PutOnMemberAsync(key, value, name);
                    Assert.That(response.Success);
                    await Task.Delay(100);
                }
                Logger.LogInformation("Adding stopped.");
            });

            // let it run for 60 seconds
            Logger.LogInformation("Run...");
            await Task.Delay(60_000);

            // stop
            Logger.LogInformation("Stop tasks...");
            stop = true;
            await Task.WhenAll(caching, distortingSequence, distortingUuid, addingOnMember);

            Logger.LogInformation("Assert...");
            await AssertEx.SucceedsEventually(async () =>
            {
                // compare the values directly obtained from the server,
                // with the values obtained from the client via the cache,
                // everything should match
                var memberValues = await GetAllValueFromMemberAsync(size, name);
                for (var i = 0; i < size; i++)
                {
                    var memberValue = memberValues[i] as int?;
                    var clientValue = await dictionary.GetAsync(i);
                    Assert.AreEqual(memberValue, clientValue, $"Bad value (i={i}, server={memberValue}, client={clientValue})");
                }
            }, 40_000, 500);
        }
    }
}