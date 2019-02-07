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

using System;
using System.Collections.Generic;
using Hazelcast.Client.Proxy;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientPNCounterTest : SingleMemberBaseTest
    {
        private ClientPNCounterProxy _pnCounter;

        [SetUp]
        public void Setup()
        {
            _pnCounter = Client.GetPNCounter(TestSupport.RandomString()) as ClientPNCounterProxy;
        }

        protected override void ConfigureGroup(ClientConfig config)
        {
            config.GetGroupConfig().SetName(HzCluster.Id).SetPassword(HzCluster.Id);
        }

        protected override void ConfigureClient(ClientConfig config)
        {
            config.GetNetworkConfig().SetConnectionAttemptLimit(1);
            config.GetNetworkConfig().SetConnectionAttemptPeriod(2000);
        }

        protected override string GetServerConfig()
        {
            return Resources.hazelcast_quick_node_switching;
        }

        [Test]
        public void Reset_Succeeded()
        {
            _pnCounter.Reset();
        }

        [Test]
        public void AddAndGet_Succeeded()
        {
            var result = _pnCounter.AddAndGet(10);
            Assert.AreEqual(10, result);
        }

        [Test]
        public void DecrementAndGet_Succeeded()
        {
            _pnCounter.AddAndGet(10);
            var result = _pnCounter.DecrementAndGet();

            Assert.AreEqual(9, result);
        }

        [Test]
        public void Get_Succeeded()
        {
            _pnCounter.AddAndGet(10);
            var result = _pnCounter.Get();

            Assert.AreEqual(10, result);
        }

        [Test]
        public void GetAndAdd_Succeeded()
        {
            _pnCounter.AddAndGet(10);

            var result1 = _pnCounter.GetAndAdd(10);
            var result2 = _pnCounter.Get();

            Assert.AreEqual(result1+10, result2);
        }

        [Test]
        public void GetAndDecrement_Succeeded()
        {
            _pnCounter.AddAndGet(10);

            var result1 = _pnCounter.GetAndDecrement();
            var result2 = _pnCounter.Get();

            Assert.AreEqual(result1, result2 + 1);
        }

        [Test]
        public void GetAndIncrement_Succeeded()
        {
            _pnCounter.AddAndGet(10);

            var result1 = _pnCounter.GetAndIncrement();
            var result2 = _pnCounter.Get();

            Assert.AreEqual(result1 + 1, result2);
        }

        [Test]
        public void GetAndSubtract_Succeeded()
        {
            _pnCounter.AddAndGet(10);

            var result1 = _pnCounter.GetAndSubtract(5);
            var result2 = _pnCounter.Get();

            Assert.AreEqual(result1, result2 + 5);
        }

        [Test]
        public void IncrementAndGet_Succeeded()
        {
            _pnCounter.AddAndGet(10);

            var result1 = _pnCounter.IncrementAndGet();
            var result2 = _pnCounter.Get();

            Assert.AreEqual(result1, result2);
            Assert.AreEqual(11, result2);
        }

        [Test]
        public void SubtractAndGet_Succeeded()
        {
            _pnCounter.AddAndGet(10);

            var result1 = _pnCounter.SubtractAndGet(5);
            var result2 = _pnCounter.Get();

            Assert.AreEqual(result1, result2);
            Assert.AreEqual(5, result2);
        }

        [Test]
        public void UpdateObservedReplicaTimestamps_Later_Succeeded()
        {
            var initList = new List<KeyValuePair<string, long>>()
            {
                new KeyValuePair<string, long>("node-1", 10),
                new KeyValuePair<string, long>("node-2", 20),
                new KeyValuePair<string, long>("node-3", 30),
                new KeyValuePair<string, long>("node-4", 40),
                new KeyValuePair<string, long>("node-5", 50)
            };

            _pnCounter.UpdateObservedReplicaTimestamps(initList);

            var testList = new List<KeyValuePair<string, long>>()
            {
                new KeyValuePair<string, long>("node-1", 10),
                new KeyValuePair<string, long>("node-2", 50),
                new KeyValuePair<string, long>("node-3", 30),
                new KeyValuePair<string, long>("node-4", 40),
                new KeyValuePair<string, long>("node-5", 50)
            };

            _pnCounter.UpdateObservedReplicaTimestamps(testList);
        }

        [Test]
        public void UpdateObservedReplicaTimestamps_Earlier_Succeeded()
        {
            var initList = new List<KeyValuePair<string, long>>()
            {
                new KeyValuePair<string, long>("node-1", 10),
                new KeyValuePair<string, long>("node-2", 20),
                new KeyValuePair<string, long>("node-3", 30),
                new KeyValuePair<string, long>("node-4", 40),
                new KeyValuePair<string, long>("node-5", 50)
            };

            _pnCounter.UpdateObservedReplicaTimestamps(initList);

            var testList = new List<KeyValuePair<string, long>>()
            {
                new KeyValuePair<string, long>("node-1", 10),
                new KeyValuePair<string, long>("node-2", 10),
                new KeyValuePair<string, long>("node-3", 30),
                new KeyValuePair<string, long>("node-4", 40),
                new KeyValuePair<string, long>("node-5", 50)
            };

            _pnCounter.UpdateObservedReplicaTimestamps(testList);
        }

        [Test]
        public void InvokeAdd_NoAddressNoLastException_ThrowsDefaultException()
        {
            var excludedAddresses=new HashSet<Address>();
            Exception lastException = null;
            Address targetAddress = null;

            var ex = Assert.Throws<NoDataMemberInClusterException>(() => _pnCounter.InvokeAddInternal(10, true, excludedAddresses, lastException, targetAddress));
        }

        [Test]
        public void InvokeAdd_NoAddressHasLastException_ThrowsLastException()
        {
            var excludedAddresses = new HashSet<Address>();
            Exception lastException = new OutOfMemoryException();
            Address targetAddress = null;

            var ex = Assert.Throws<OutOfMemoryException>(() => _pnCounter.InvokeAddInternal(10, true, excludedAddresses, lastException, targetAddress));
        }

        [Test]
        public void InvokeGet_NoAddressNoLastException_ThrowsDefaultException()
        {
            var excludedAddresses = new HashSet<Address>();
            Exception lastException = null;
            Address targetAddress = null;

            var ex = Assert.Throws<NoDataMemberInClusterException>(() => _pnCounter.InvokeGetInternal(excludedAddresses, lastException, targetAddress));
        }

        [Test]
        public void InvokeGet_NoAddressHasLastException_ThrowsLastException()
        {
            var excludedAddresses = new HashSet<Address>();
            Exception lastException = new OutOfMemoryException();
            Address targetAddress = null;

            var ex = Assert.Throws<OutOfMemoryException>(() => _pnCounter.InvokeGetInternal(excludedAddresses, lastException, targetAddress));
        }
    }
}
