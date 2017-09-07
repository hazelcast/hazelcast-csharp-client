// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Client.Model;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class PartitionPredicateTest : SingleMemberBaseTest
    {
        private const int ItemsPerPartition = 10;
        private object partitionKey;
        private int partitionId;
        private IPredicate predicate;
        
        internal static IMap<object, object> map;
        
        [SetUp]
        public void Init()
        {
            var partitionService = ((HazelcastClientProxy) Client).GetClient().GetClientPartitionService();
            map = Client.GetMap<object, object>(TestSupport.RandomString());
            
            map.Clear();
            var partitionCount = partitionService.GetPartitionCount();
            for (var p = 0; p < partitionCount; p++)
            {
                for (var k = 0; k < ItemsPerPartition; k++)
                {
                    map.Put(GenerateKeyForPartition(Client, p), p);
                }
            }
            
            partitionKey = TestSupport.RandomString();
            partitionId = partitionService.GetPartitionId(partitionKey);
            predicate = new PartitionPredicate(partitionKey, Predicates.True());
        }

        [TearDown]
        public static void Destroy()
        {
            map.Clear();
        }

        [Test]
        public void TestValues()
        {
            var values = map.Values(predicate);
            Assert.AreEqual(ItemsPerPartition, values.Count);
            foreach (var value in values)
            {
                Assert.AreEqual(partitionId, value);
            }
        }

        
        [Test]
        public void TestKeySet() 
        {
            var partitionService = ((HazelcastClientProxy) Client).GetClient().GetClientPartitionService();
            var keys = map.KeySet(predicate);
            Assert.AreEqual(ItemsPerPartition, keys.Count);
            foreach (var key in keys)
            {
                Assert.AreEqual(partitionId, partitionService.GetPartitionId(key));
            }
        }

        [Test]
        public void TestEntries() 
        {
            var partitionService = ((HazelcastClientProxy) Client).GetClient().GetClientPartitionService();
            var entries = map.EntrySet(predicate);
            Assert.AreEqual(ItemsPerPartition, entries.Count);
            foreach (var entry in entries)
            {
                Assert.AreEqual(partitionId, partitionService.GetPartitionId(entry.Key));
                Assert.AreEqual(partitionId, entry.Value);
            }
        }

        [Test]
        public void TestSerialization()
        {
            var ss = new SerializationServiceBuilder().Build();
            var data = ss.ToData(predicate);
            var partitionPredicate = ss.ToObject<PartitionPredicate>(data);
            Assert.AreEqual(partitionKey, partitionPredicate.GetPartitionKey());
            Assert.AreEqual(Predicates.True(), partitionPredicate.GetTarget());
        }

    }
}