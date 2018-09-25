// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientSemaphoreTest : SingleMemberBaseTest
    {
        private ISemaphore semaphore;
        
        [SetUp]
        public void Init()
        {
            semaphore = Client.GetSemaphore(TestSupport.RandomString());
        }

        [TearDown]
        public void Destroy()
        {
            semaphore.Destroy();
        }

        [Test]
        public void TestAcquire()
        {
            var s = Client.GetSemaphore(TestSupport.RandomString());
            var latch = new CountdownEvent(1);
            var t = new Thread(delegate(object o)
            {
                try
                {
                    s.Acquire();
                    latch.Signal();
                }
                catch
                {
                }
            });
            t.Start();

            Thread.Sleep(100);
            s.Release(2);
            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
            Assert.AreEqual(1, s.AvailablePermits());
        }
        
        [Test]
        public void testdrainPermits() 
        {
            semaphore.Init(10);
            Assert.AreEqual(10, semaphore.DrainPermits());
        }


        [Test]
        public void TestInit()
        {
            semaphore.Init(2);
            Assert.AreEqual(2, semaphore.AvailablePermits());
        }

        [Test]
        public void TestInitNeg()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                semaphore.Init(-2);
            });
        }

        [Test]
        public void TestTryAcquire()
        {
            var latch = new CountdownEvent(1);
            var t = new Thread(delegate(object o)
            {
                try
                {
                    if (semaphore.TryAcquire(2, 5, TimeUnit.Seconds))
                    {
                        latch.Signal();
                    }
                }
                catch
                {
                }
            });
            t.Start();

            semaphore.Release(2);
            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
            Assert.AreEqual(0, semaphore.AvailablePermits());
        }
        
        [Test]
        public void TestNegativePermitsJucCompatibility()
        {
            semaphore.Init(0);
            semaphore.ReducePermits(100);
            semaphore.Release(10);
    
            Assert.AreEqual(-90, semaphore.AvailablePermits());
            Assert.AreEqual(-90, semaphore.DrainPermits());
    
            semaphore.Release(10);
    
            Assert.AreEqual(10, semaphore.AvailablePermits());
            Assert.AreEqual(10, semaphore.DrainPermits());
        }

    }
}