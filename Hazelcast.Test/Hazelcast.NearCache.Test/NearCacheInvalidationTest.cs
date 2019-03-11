﻿// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Client.Test;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.NearCache.Test
{
    [TestFixture]
    public class NearCacheInvalidationTest : NearcacheTestSupport
    {
        protected override void InitMembers()
        {
            //Init 2 members
            MemberList.Add(RemoteController.startMember(HzCluster.Id));
            MemberList.Add(RemoteController.startMember(HzCluster.Id));
        }

        protected override string GetServerConfig()
        {
            return Resources.hazelcast;
        }

        [SetUp]
        public void Setup()
        {
            _map = Client.GetMap<object, object>("nearCachedMap-" + TestSupport.RandomString());
            var nc = GetNearCache(_map);
            Assert.AreEqual(typeof(NearCache), nc.GetType());
        }

        [TearDown]
        public void Destroy()
        {
            _map.Destroy();
        }

        private IMap<object, object> _map;

        [OneTimeTearDown]
        public void RestoreEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("hazelcast.invalidation.max.tolerated.miss.count", null);
            Environment.SetEnvironmentVariable("hazelcast.invalidation.reconciliation.interval.seconds", null);
            Environment.SetEnvironmentVariable("hazelcast.invalidation.min.reconciliation.interval.seconds", null);
        }

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            Environment.SetEnvironmentVariable("hazelcast.invalidation.max.tolerated.miss.count", "0");
            Environment.SetEnvironmentVariable("hazelcast.invalidation.reconciliation.interval.seconds", "10");
            Environment.SetEnvironmentVariable("hazelcast.invalidation.min.reconciliation.interval.seconds", "10");
            var defaultConfig = new NearCacheConfig().SetInvalidateOnChange(true).SetEvictionPolicy("None")
                .SetInMemoryFormat(InMemoryFormat.Binary);
            config.AddNearCacheConfig("nearCachedMap*", defaultConfig);
        }

        [Test]
        public void TestSequenceFixIfKeyRemoveAtServer()
        {
            const string theKey = "key";
            _map.Put(theKey, "value1");

            var partitionId = ClientInternal.GetClientPartitionService().GetPartitionId(theKey);
            var nc = GetNearCache(_map) as NearCache;
            var metaDataContainer = nc.RepairingHandler.GetMetaDataContainer(partitionId);

            TestSupport.AssertTrueEventually(() =>
            {
                var initialSequence = metaDataContainer.Sequence;
                Assert.AreEqual(1, initialSequence);
            });
            metaDataContainer.Sequence -= 2; //distort the sequence

            RemoveKeyAtServer(_map.GetName(), theKey);

            TestSupport.AssertTrueEventually(() =>
            {
                var latestSequence = metaDataContainer.Sequence;
                Assert.AreEqual(2, latestSequence);
                Assert.IsTrue(nc.Records.IsEmpty);
            });
        }

        [Test]
        public void TestSequenceUpdateIfKeyRemoveAtServer()
        {
            const string theKey = "key";
            _map.Put(theKey, "value");

            var partitionId = ClientInternal.GetClientPartitionService().GetPartitionId(theKey);
            var nc = GetNearCache(_map) as NearCache;
            var metaDataContainer = nc.RepairingHandler.GetMetaDataContainer(partitionId);
            var initialSequence = metaDataContainer.Sequence;

            RemoveKeyAtServer(_map.GetName(), theKey);

            TestSupport.AssertTrueEventually(() =>
            {
                var latestSequence = metaDataContainer.Sequence;
                Assert.Greater(latestSequence, initialSequence);
            });
        }
    }
}