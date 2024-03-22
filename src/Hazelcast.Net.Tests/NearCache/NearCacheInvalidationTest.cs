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

using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.NearCaching;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.NearCache
{
    // this was NearCacheMetaDataDistortionTest.TestInvalidationDistortionSequenceAndGuid in the original code
    //
    // but ... what it really does is ensure that if uuids and sequences are randomly
    // modified (distorted) on the server, the cache properly recovers.


    [TestFixture]
    public class NearCacheInvalidationTest : NearCacheTestBase
    {
        private IHazelcastClient _client;
        private IHMap<object, object> _map;

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

            _map = await _client.GetMapAsync<object, object>("nc-" + TestUtils.RandomString());

            var nearCache = GetNearCache(_map);
            Assert.That(nearCache, Is.InstanceOf<NearCaching.NearCache>());
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_map != null) await _map.DestroyAsync();
            _map = null;

            if (_client != null) await _client.DisposeAsync();
            _client = null;
        }

        protected override HazelcastOptions CreateHazelcastOptions()
        {
            var options = base.CreateHazelcastOptions();

            options.NearCaches["nc*"] = new NearCacheOptions
            {
                InvalidateOnChange = true,
                EvictionPolicy = EvictionPolicy.None,
                InMemoryFormat = InMemoryFormat.Binary
            };

            options.NearCache.MaxToleratedMissCount = 0;
            options.NearCache.ReconciliationIntervalSeconds = 10;
            options.NearCache.MinReconciliationIntervalSeconds = 10;

            return options;
        }

        [Test]
        public async Task TestSequenceFixIfKeyRemoveAtServer()
        {
            const string theKey = "key";

            await _map.SetAsync(theKey, "value1");

            var partitioner = ((HazelcastClient)_client).Cluster.Partitioner;
            var keyData = ((HazelcastClient) _client).SerializationService.ToData(theKey);
            var partitionId = partitioner.GetPartitionId(keyData.PartitionHash);

            var cache = GetNearCache(_map) as NearCaching.NearCache;
            var metadata = cache.RepairingHandler.GetMetadata(partitionId);

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(metadata.Sequence, Is.EqualTo(1));
            }, 2000, 200);

            metadata.Sequence -= 2; //distort the sequence

            await RemoveKeyAtServerAsync(_map.Name, theKey);

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(metadata.Sequence, Is.EqualTo(2));
                Assert.That(cache.Count, Is.EqualTo(0));
            }, 2000, 200);
        }

        [Test]
        public async Task TestSequenceUpdateIfKeyRemoveAtServer()
        {
            const string theKey = "key";

            await _map.SetAsync(theKey, "value1");

            var partitioner = ((HazelcastClient) _client).Cluster.Partitioner;
            var keyData = ((HazelcastClient)_client).SerializationService.ToData(theKey);
            var partitionId = partitioner.GetPartitionId(keyData.PartitionHash);

            var cache = GetNearCache(_map) as NearCaching.NearCache;
            var metadata = cache.RepairingHandler.GetMetadata(partitionId);
            var initialSequence = metadata.Sequence;

            await RemoveKeyAtServerAsync(_map.Name, theKey);

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(metadata.Sequence, Is.GreaterThan(initialSequence));
            }, 2000, 200);
        }
    }
}
