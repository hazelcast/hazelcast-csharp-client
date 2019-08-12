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
using System.Linq;
using System.Threading.Tasks;
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

        const int Capacity = 10; //should be set to same as in the server.xml file
        IRingbuffer<string> _ringBuffer;

        [Test]
        public void AddAll()
        {
            var task = _ringBuffer.AddAllAsync(new List<string> { "foo", "bar" }, OverflowPolicy.Overwrite);

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
        public void AddAndReadOne()
        {
            var sequence = _ringBuffer.Add("foo");

            Assert.AreEqual("foo", _ringBuffer.ReadOne(sequence));
        }

        [Test]
        public void AddAsync()
        {
            var sequenceFuture = _ringBuffer.AddAsync("foo");

            Assert.IsTrue(
                sequenceFuture.ContinueWith(f => { Assert.AreEqual("foo", _ringBuffer.ReadOne(f.Result)); }).Wait(5000));
        }

        [Test]
        public void Capacity_IsEqual()
        {
            Assert.AreEqual(Capacity, _ringBuffer.Capacity());
        }

        [Test]
        public void ExcessiveMaxCount()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ringBuffer.ReadManyAsync(0, 0, ClientRingbufferProxy<string>.MaxBatchSize + 1);
            });
        }

        [Test]
        public void ExcessiveMinCount()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ringBuffer.ReadManyAsync(0, Capacity + 1, Capacity + 1);
            });
        }

        [Test]
        public void HeadSequence()
        {
            for (var k = 0; k < 2 * Capacity; k++)
            {
                _ringBuffer.Add("foo");
            }

            Assert.AreEqual(Capacity, _ringBuffer.HeadSequence());
        }

        [Test]
        public void InvalidReadCount()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ringBuffer.ReadManyAsync(0, 2, 1);
            });
        }

        [Test]
        public void InvalidSequence()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _ringBuffer.ReadOne(-1);
            });
        }

        [Test]
        public async Task ReadManyAsync()
        {
            var values = new[] { "1", "2", "3" };

            foreach (var value in values)
            {
                await _ringBuffer.AddAsync(value);
            }

            var read = await _ringBuffer.ReadManyAsync(0, 3, 3);
            
            CollectionAssert.AreEqual(values, read);
        }

        [Test]
        public async Task ReadManyAsyncWithMaxCount()
        {
            var values = new[] { "1", "2", "3", "4", "5", "6" };

            foreach (var value in values)
            {
                await _ringBuffer.AddAsync(value);
            }

            var read = await _ringBuffer.ReadManyAsync(0, 3, 3);

            //surplus results should not be read
            CollectionAssert.AreEqual(values.Take(3), read);
        }

        [Test]
        public void RemainingCapacity()
        {
            _ringBuffer = Client.GetRingbuffer<string>("ClientRingbufferTestWithTTL" + TestSupport.RandomString());

            _ringBuffer.Add("foo");

            Assert.AreEqual(Capacity - 1, _ringBuffer.RemainingCapacity());
        }

        [Test]
        public void Size()
        {
            _ringBuffer.Add("foo");

            Assert.AreEqual(1, _ringBuffer.Size());
        }

        [Test]
        public void StaleSequence()
        {
            Assert.Throws<StaleSequenceException>(() =>
            {
                for (var k = 0; k < Capacity * 2; k++)
                {
                    _ringBuffer.Add("foo");
                }

                _ringBuffer.ReadOne(_ringBuffer.HeadSequence() - 1);
            });
        }

        [Test]
        public void TailSequence()
        {
            for (var k = 0; k < 2 * Capacity; k++)
            {
                _ringBuffer.Add("foo");
            }

            Assert.AreEqual(Capacity * 2 - 1, _ringBuffer.TailSequence());
        }
    }
}