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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientSemaphoreTest : SingleMemberBaseTest
    {
        private ISemaphore _semaphore;

        [SetUp]
        public void Init()
        {
            _semaphore = Client.GetSemaphore(TestSupport.RandomString());
        }

        [TearDown]
        public void Destroy()
        {
            _semaphore.Destroy();
        }

        [Test]
        public void Acquire()
        {
            var s = Client.GetSemaphore(TestSupport.RandomString());
            var latch = new CountdownEvent(1);
            TestSupport.Run(() =>
            {
                s.Acquire();
                latch.Signal();
            });

            Thread.Sleep(100);
            s.Release(2);
            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
            Assert.AreEqual(1, s.AvailablePermits());
        }

        [Test]
        public void DrainPermits()
        {
            const int value = 10;

            _semaphore.Init(value);
            Assert.AreEqual(value, _semaphore.DrainPermits());
        }

        [Test]
        public void AvailablePermits()
        {
            _semaphore.Init(2);
            Assert.AreEqual(2, _semaphore.AvailablePermits());
        }

        [Test]
        public void InitNeg()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                _semaphore.Init(-2);
            });
        }

        [Test]
        public void TryAcquire()
        {
            var latch = new CountdownEvent(1);
            TestSupport.Run(() =>
            {
                try
                {
                    if (_semaphore.TryAcquire(2, 5, TimeUnit.Seconds))
                    {
                        latch.Signal();
                    }
                }
                catch
                {
                }
            });

            _semaphore.Release(2);
            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
            Assert.AreEqual(0, _semaphore.AvailablePermits());
        }

        [Test]
        [Category("3.10")]
        public void NegativePermitsJucCompatibility()
        {
            _semaphore.Init(0);
            _semaphore.ReducePermits(100);
            _semaphore.Release(10);

            Assert.AreEqual(-90, _semaphore.AvailablePermits());
            Assert.AreEqual(-90, _semaphore.DrainPermits());

            _semaphore.Release(10);

            Assert.AreEqual(10, _semaphore.AvailablePermits());
            Assert.AreEqual(10, _semaphore.DrainPermits());
        }
    }
}