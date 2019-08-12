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
    public class ClientLockTest : SingleMemberBaseTest
    {
        [SetUp]
        public void Setup()
        {
            _l = Client.GetLock(TestSupport.RandomString());
        }

        [TearDown]
        public void Destroy()
        {
            _l.Destroy();
        }

        ILock _l;

        [Test]
        public void ForceUnlock()
        {
            _l.Lock();

            var latch = new CountdownEvent(1);

            var t = Task.Run(() =>
            {
                _l.ForceUnlock();
                latch.Signal();
            });

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(100)));
            Assert.IsFalse(_l.IsLocked());
        }

        [Test]
        public void IsLock()
        {
            Assert.IsTrue(_l.TryLock(20, TimeUnit.Seconds));

            var isLocked = _l.IsLocked();
            Assert.IsTrue(isLocked);
        }

        [Test]
        public void Lock()
        {
            _l.Lock();
            var latch = new CountdownEvent(1);

            Task.Run(() =>
            {
                if (!_l.TryLock())
                {
                    latch.Signal();
                }
            });

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(5)));
            _l.ForceUnlock();
        }

        [Test]
        public void LockTtl()
        {
            _l.Lock(3, TimeUnit.Seconds);

            var latch = new CountdownEvent(2);

            Task.Run(() =>
            {
                if (!_l.TryLock())
                {
                    latch.Signal();
                }
                try
                {
                    if (_l.TryLock(5, TimeUnit.Seconds))
                    {
                        latch.Signal();
                    }
                }
                catch
                {
                    // TODO: what and why can throw here
                }
            });

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
            _l.ForceUnlock();
        }

        [Test]
        public void Stats()
        {
            _l.Lock();
            Assert.IsTrue(_l.IsLocked());
            Assert.IsTrue(_l.IsLockedByCurrentThread());
            Assert.AreEqual(1, _l.GetLockCount());

            _l.Unlock();
            Assert.IsFalse(_l.IsLocked());
            Assert.AreEqual(0, _l.GetLockCount());
            Assert.AreEqual(-1L, _l.GetRemainingLeaseTime());

            _l.Lock(1, TimeUnit.Minutes);
            Assert.IsTrue(_l.IsLocked());
            Assert.IsTrue(_l.IsLockedByCurrentThread());
            Assert.AreEqual(1, _l.GetLockCount());
            Assert.IsTrue(_l.GetRemainingLeaseTime() > 1000 * 30);

            var latch2 = new CountdownEvent(1);

            var t = Task.Run(() =>
            {
                Assert.IsTrue(_l.IsLocked());
                Assert.IsFalse(_l.IsLockedByCurrentThread());
                Assert.AreEqual(1, _l.GetLockCount());
                Assert.IsTrue(_l.GetRemainingLeaseTime() > 1000 * 30);
                latch2.Signal();
            });

            Assert.IsTrue(latch2.Wait(TimeSpan.FromMinutes(1)));
        }

        [Test]
        public void TryLock()
        {
            Assert.IsTrue(_l.TryLock(2, TimeUnit.Seconds));

            var manualReset = new ManualResetEvent(false);
            //CountdownEvent latch = new CountdownEvent(1);
            var t1 = new Thread(delegate(object o)
            {
                try
                {
                    var tryLock = _l.TryLock(2, TimeUnit.Seconds);
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
                    if (_l.TryLock(20, TimeUnit.Seconds))
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

            _l.Unlock();

            Assert.IsTrue(manualReset2.WaitOne(TimeSpan.FromSeconds(10)));

            //Assert.IsTrue(l.IsLocked());
            _l.ForceUnlock();
        }

        [Test]
        public void UnlockBeforeLock_ShouldThrowException()
        {
            Assert.Throws<SynchronizationLockException>(() => { _l.Unlock(); });
        }
    }
}