﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.IO;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientPNCounterTest : SingleMemberBaseTest
    {
        private static readonly Guid[] _guids = {Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()};
        private ClientPNCounterProxy _pnCounter;

        [SetUp]
        public void Setup()
        {
            _pnCounter = Client.GetPNCounter(TestSupport.RandomString()) as ClientPNCounterProxy;
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
            var initList = new List<KeyValuePair<Guid, long>>()
            {
                new KeyValuePair<Guid, long>(_guids[0], 10),
                new KeyValuePair<Guid, long>(_guids[1], 20),
                new KeyValuePair<Guid, long>(_guids[2], 30),
                new KeyValuePair<Guid, long>(_guids[3], 40),
                new KeyValuePair<Guid, long>(_guids[4], 50)
            };

            _pnCounter.UpdateObservedReplicaTimestamps(initList);

            var testList = new List<KeyValuePair<Guid, long>>()
            {
                new KeyValuePair<Guid, long>(_guids[0], 10),
                new KeyValuePair<Guid, long>(_guids[1], 50),
                new KeyValuePair<Guid, long>(_guids[2], 30),
                new KeyValuePair<Guid, long>(_guids[3], 40),
                new KeyValuePair<Guid, long>(_guids[4], 50)
            };

            _pnCounter.UpdateObservedReplicaTimestamps(testList);

            Assert.AreEqual(testList, _pnCounter._observedClock.EntrySet());
        }

        [Test]
        public void UpdateObservedReplicaTimestamps_Earlier_Succeeded()
        {
            var initList = new List<KeyValuePair<Guid, long>>()
            {
                new KeyValuePair<Guid, long>(_guids[0], 10),
                new KeyValuePair<Guid, long>(_guids[1], 20),
                new KeyValuePair<Guid, long>(_guids[2], 30),
                new KeyValuePair<Guid, long>(_guids[3], 40),
                new KeyValuePair<Guid, long>(_guids[4], 50)
            };

            _pnCounter.UpdateObservedReplicaTimestamps(initList);

            var testList = new List<KeyValuePair<Guid, long>>()
            {
                new KeyValuePair<Guid, long>(_guids[0], 10),
                new KeyValuePair<Guid, long>(_guids[1], 10),
                new KeyValuePair<Guid, long>(_guids[2], 30),
                new KeyValuePair<Guid, long>(_guids[3], 40),
                new KeyValuePair<Guid, long>(_guids[4], 50)
            };

            _pnCounter.UpdateObservedReplicaTimestamps(testList);

            Assert.AreEqual(initList, _pnCounter._observedClock.EntrySet());
        }

        [Test]
        public void InvokeAdd_NoAddressNoLastException_ThrowsDefaultException()
        {
            var excludedAddresses=new HashSet<IMember>();
            Exception lastException = null;
            IMember targetAddress = null;

            Assert.Throws<NoDataMemberInClusterException>(() => _pnCounter.InvokeAddInternal(10, true, excludedAddresses, lastException, targetAddress));
        }

        [Test]
        public void InvokeAdd_NoAddressHasLastException_ThrowsLastException()
        {
            var excludedAddresses = new HashSet<IMember>();
            Exception lastException = new OutOfMemoryException();
            IMember targetAddress = null;

            Assert.Throws<OutOfMemoryException>(() => _pnCounter.InvokeAddInternal(10, true, excludedAddresses, lastException, targetAddress));
        }

        [Test]
        public void InvokeGet_NoAddressNoLastException_ThrowsDefaultException()
        {
            var excludedAddresses = new HashSet<IMember>();
            Exception lastException = null;
            IMember targetAddress = null;

            Assert.Throws<NoDataMemberInClusterException>(() => _pnCounter.InvokeGetInternal(excludedAddresses, lastException, targetAddress));
        }

        [Test]
        public void InvokeGet_NoAddressHasLastException_ThrowsLastException()
        {
            var excludedAddresses = new HashSet<IMember>();
            Exception lastException = new OutOfMemoryException();
            IMember targetAddress = null;

            Assert.Throws<OutOfMemoryException>(() => _pnCounter.InvokeGetInternal(excludedAddresses, lastException, targetAddress));
        }
    }
}
