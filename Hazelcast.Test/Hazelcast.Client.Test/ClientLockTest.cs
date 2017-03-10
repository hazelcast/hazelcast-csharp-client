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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientLockTest : SingleMemberBaseTest
    {
        [SetUp]
        public void Setup()
        {
            l = Client.GetLock(TestSupport.RandomString());
        }

        [TearDown]
        public static void Destroy()
        {
            l.Destroy();
        }

        internal static ILock l;

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestForceUnlock()
        {
            l.Lock();

            var latch = new CountdownEvent(1);

            var t2 = new Thread(delegate(object o)
            {
                try
                {
                    l.ForceUnlock();
                    latch.Signal();
                }
                catch
                {
                }
            });
            t2.Start();

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(100)));
            Assert.IsFalse(l.IsLocked());
        }

        [Test]
        public virtual void TestIsLock()
        {
            Assert.IsTrue(l.TryLock(20, TimeUnit.Seconds));

            var isLocked = l.IsLocked();
            Assert.IsTrue(isLocked);
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestLock()
        {
            l.Lock();
            var latch = new CountdownEvent(1);

            Task.Factory.StartNew(() =>
            {
                if (!l.TryLock())
                {
                    latch.Signal();
                }
            });
            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(5)));
            l.ForceUnlock();
        }


        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestLockTtl()
        {
            l.Lock(3, TimeUnit.Seconds);

            var latch = new CountdownEvent(2);

            Task.Factory.StartNew(() =>
            {
                if (!l.TryLock())
                {
                    latch.Signal();
                }
                try
                {
                    if (l.TryLock(5, TimeUnit.Seconds))
                    {
                        latch.Signal();
                    }
                }
                catch
                {
                }
            });

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
            l.ForceUnlock();
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestStats()
        {
            l.Lock();
            Assert.IsTrue(l.IsLocked());
            Assert.IsTrue(l.IsLockedByCurrentThread());
            Assert.AreEqual(1, l.GetLockCount());
            l.Unlock();
            Assert.IsFalse(l.IsLocked());
            Assert.AreEqual(0, l.GetLockCount());
            Assert.AreEqual(-1L, l.GetRemainingLeaseTime());
            l.Lock(1, TimeUnit.Minutes);
            Assert.IsTrue(l.IsLocked());
            Assert.IsTrue(l.IsLockedByCurrentThread());
            Assert.AreEqual(1, l.GetLockCount());
            Assert.IsTrue(l.GetRemainingLeaseTime() > 1000*30);

            var latch2 = new CountdownEvent(1);

            var t2 = new Thread(delegate(object o)
            {
                Assert.IsTrue(l.IsLocked());
                Assert.IsFalse(l.IsLockedByCurrentThread());
                Assert.AreEqual(1, l.GetLockCount());
                Assert.IsTrue(l.GetRemainingLeaseTime() > 1000*30);
                latch2.Signal();
            });
            t2.Start();

            Assert.IsTrue(latch2.Wait(TimeSpan.FromMinutes(1)));
        }

        [Test]
        public virtual void TestTryLock()
        {
            Assert.IsTrue(l.TryLock(2, TimeUnit.Seconds));

            var manualReset = new ManualResetEvent(false);
            //CountdownEvent latch = new CountdownEvent(1);
            var t1 = new Thread(delegate(object o)
            {
                try
                {
                    var tryLock = l.TryLock(2, TimeUnit.Seconds);
                    if (!tryLock)
                    {
                        manualReset.Set();
                        //latch.Signal();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            });
            t1.Start();

            Assert.IsTrue(manualReset.WaitOne(TimeSpan.FromSeconds(100)));
            //var isLocked = l.IsLocked();
            //Assert.IsTrue(isLocked);


            var manualReset2 = new ManualResetEvent(false);

            var t2 = new Thread(delegate(object o)
            {
                try
                {
                    if (l.TryLock(20, TimeUnit.Seconds))
                    {
                        manualReset2.Set();
                        //latch2.Signal();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            });
            t2.Start();

            Thread.Sleep(1000);

            l.Unlock();

            Assert.IsTrue(manualReset2.WaitOne(TimeSpan.FromSeconds(10)));

            //Assert.IsTrue(l.IsLocked());
            l.ForceUnlock();
        }

        [Test]
        [ExpectedException(typeof (SynchronizationLockException))]
        public void TestUnlockBeforeLock_ShouldThrowException()
        {
            l.Unlock();
        }
    }
}