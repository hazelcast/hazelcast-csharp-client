// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    [Category("3.9")]
    public class PartitionPredicateTest : SingleMemberBaseTest
    {
        const int ItemsPerPartition = 10;
        object _partitionKey;
        int _partitionId;
        IPredicate _predicate;
        
        IMap<object, object> _map;
        
        [SetUp]
        public void Init()
        {
            var partitionService = ((HazelcastClientProxy) Client).GetClient().GetClientPartitionService();
            _map = Client.GetMap<object, object>(TestSupport.RandomString());
            
            _map.Clear();
            var partitionCount = partitionService.GetPartitionCount();
            for (var p = 0; p < partitionCount; p++)
            {
                for (var k = 0; k < ItemsPerPartition; k++)
                {
                    _map.Put(GenerateKeyForPartition(Client, p), p);
                }
            }
            
            _partitionKey = TestSupport.RandomString();
            _partitionId = partitionService.GetPartitionId(_partitionKey);
            _predicate = new PartitionPredicate(_partitionKey, Predicates.True());
        }

        [TearDown]
        public void Destroy()
        {
            _map.Clear();
        }

        [Test]
        public void Values()
        {
            var values = _map.Values(_predicate);
            Assert.AreEqual(ItemsPerPartition, values.Count);
            foreach (var value in values)
            {
                Assert.AreEqual(_partitionId, value);
            }
        }
        
        [Test]
        public void KeySet() 
        {
            var partitionService = ((HazelcastClientProxy) Client).GetClient().GetClientPartitionService();
            var keys = _map.KeySet(_predicate);
            Assert.AreEqual(ItemsPerPartition, keys.Count);
            foreach (var key in keys)
            {
                Assert.AreEqual(_partitionId, partitionService.GetPartitionId(key));
            }
        }

        [Test]
        public void Entries() 
        {
            var partitionService = ((HazelcastClientProxy) Client).GetClient().GetClientPartitionService();
            var entries = _map.EntrySet(_predicate);
            Assert.AreEqual(ItemsPerPartition, entries.Count);
            foreach (var entry in entries)
            {
                Assert.AreEqual(_partitionId, partitionService.GetPartitionId(entry.Key));
                Assert.AreEqual(_partitionId, entry.Value);
            }
        }

        [Test]
        public void Serialization()
        {
            var ss = new SerializationServiceBuilder().Build();
            var data = ss.ToData(_predicate);
            var partitionPredicate = ss.ToObject<PartitionPredicate>(data);
            Assert.AreEqual(_partitionKey, partitionPredicate.GetPartitionKey());
            Assert.AreEqual(Predicates.True(), partitionPredicate.GetTarget());
        }
    }
}