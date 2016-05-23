// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientRingbufferTest : SingleMemberBaseTest
    {
        [SetUp]
        public void Init()
        {
            _ringBuffer = Client.GetRingbuffer<string>("ClientRingbufferTest" + TestSupport.RandomString());
        }

        [TearDown]
        public void Teardown()
        {
            _ringBuffer.Destroy();
        }

        private const int Capacity = 10; //should be set to same as in the server.xml file
        private IRingbuffer<string> _ringBuffer;

        [Test]
        public void TestAddAll()
        {
            var task = _ringBuffer.AddAllAsync(new List<string> {"foo", "bar"}, OverflowPolicy.Overwrite);

            Assert.IsTrue(task.ContinueWith(t =>
            {
                var seq = t.Result;
                Assert.AreEqual(seq, _ringBuffer.TailSequence());
                Assert.AreEqual("foo", _ringBuffer.ReadOne(0));
                Assert.AreEqual("bar", _ringBuffer.ReadOne(1));
                Assert.AreEqual(0, _ringBuffer.HeadSequence());
                Assert.AreEqual(1, _ringBuffer.TailSequence());
            }).Wait(5000));
        }

        [Test]
        public void TestAddAndReadOne()
        {
            var sequence = _ringBuffer.Add("foo");

            Assert.AreEqual("foo", _ringBuffer.ReadOne(sequence));
        }

        [Test]
        public void TestAddAsync()
        {
            var sequenceFuture = _ringBuffer.AddAsync("foo", OverflowPolicy.Overwrite);

            Assert.IsTrue(
                sequenceFuture.ContinueWith(f => { Assert.AreEqual("foo", _ringBuffer.ReadOne(f.Result)); }).Wait(5000));
        }

        [Test]
        public void TestCapacity()
        {
            Assert.AreEqual(Capacity, _ringBuffer.Capacity());
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void TestExcessiveMaxCount()
        {
            _ringBuffer.ReadManyAsync(0, 0, ClientRingbufferProxy<string>.MaxBatchSize + 1);
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void TestExcessiveMinCount()
        {
            _ringBuffer.ReadManyAsync(0, Capacity + 1, Capacity + 1);
        }

        [Test]
        public void TestHeadSequence()
        {
            for (var k = 0; k < 2*Capacity; k++)
            {
                _ringBuffer.Add("foo");
            }

            Assert.AreEqual(Capacity, _ringBuffer.HeadSequence());
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void TestInvalidReadCount()
        {
            _ringBuffer.ReadManyAsync(0, 2, 1);
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void TestInvalidSequence()
        {
            _ringBuffer.ReadOne(-1);
        }

        [Test]
        public void TestReadManyAsync()
        {
            _ringBuffer.Add("1");
            _ringBuffer.Add("2");
            _ringBuffer.Add("3");


            var task = _ringBuffer.ReadManyAsync(0, 3, 3);

            Assert.IsTrue(task.ContinueWith(t =>
            {
                var list = t.Result;
                Assert.That(list, Is.EquivalentTo(new[] {"1", "2", "3"}));
            }).Wait(5000));
        }

        [Test]
        public void TestReadManyAsyncWithMaxCount()
        {
            _ringBuffer.Add("1");
            _ringBuffer.Add("2");
            _ringBuffer.Add("3");
            _ringBuffer.Add("4");
            _ringBuffer.Add("5");
            _ringBuffer.Add("6");

            var task = _ringBuffer.ReadManyAsync(0, 3, 3);

            //surplus results should not be read
            Assert.IsTrue(task.ContinueWith(t =>
            {
                var list = t.Result;
                Assert.That(list, Is.EquivalentTo(new[] {"1", "2", "3"}));
            }).Wait(5000));
        }

        [Test]
        public void TestRemainingCapacity()
        {
            _ringBuffer = Client.GetRingbuffer<string>("ClientRingbufferTestWithTTL" + TestSupport.RandomString());

            _ringBuffer.Add("foo");

            Assert.AreEqual(Capacity - 1, _ringBuffer.RemainingCapacity());
        }

        [Test]
        public void TestSize()
        {
            _ringBuffer.Add("foo");

            Assert.AreEqual(1, _ringBuffer.Size());
        }

        [Test, ExpectedException(typeof (StaleSequenceException))]
        public void TestStaleSequence()
        {
            for (var k = 0; k < Capacity*2; k++)
            {
                _ringBuffer.Add("foo");
            }
            _ringBuffer.ReadOne(_ringBuffer.HeadSequence() - 1);
        }

        [Test]
        public void TestTailSequence()
        {
            for (var k = 0; k < 2*Capacity; k++)
            {
                _ringBuffer.Add("foo");
            }

            Assert.AreEqual(Capacity*2 - 1, _ringBuffer.TailSequence());
        }
    }
}