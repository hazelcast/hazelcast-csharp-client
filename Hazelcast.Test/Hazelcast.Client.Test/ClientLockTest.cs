using System;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[TestFixture]
	public class ClientLockTest:HazelcastBaseTest
	{
        internal const string name = "ClientLockTest";

		internal static ILock l;

        [SetUp]
        public static void Init()
        {
            InitClient();
            l = client.GetLock(name);
        }

        [TearDown]
        public static void Destroy()
        {
        }

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestLock()
		{
			l.Lock();
            CountdownEvent latch = new CountdownEvent(1);

		    var t1 = new Thread(delegate(object o)
		    {
                if (!ClientLockTest.l.TryLock())
                {
                    latch.Signal();
                }
		    });
            t1.Start();

			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(5)));
			l.ForceUnlock();
		}


		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestLockTtl()
		{
			l.Lock(3, TimeUnit.SECONDS);

            CountdownEvent latch = new CountdownEvent(2);

            var t1 = new Thread(delegate(object o)
            {
                if (!ClientLockTest.l.TryLock())
                {
                    latch.Signal();
                }
                try
                {
                    if (ClientLockTest.l.TryLock(5, TimeUnit.SECONDS))
                    {
                        latch.Signal();
                    }
                }
                catch (Exception e)
                {
                    
                }
            });
            t1.Start();
            
			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
			l.ForceUnlock();
		}



		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestTryLock()
		{
			Assert.IsTrue(l.TryLock(2, TimeUnit.SECONDS));

            CountdownEvent latch = new CountdownEvent(1);

            var t1 = new Thread(delegate(object o)
            {
                try
                {
                    if (!l.TryLock(2, TimeUnit.SECONDS))
                    {
                        latch.Signal();
                    }
                }
                catch (Exception e)
                {
                }
            });
            t1.Start();


			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(100)));
			Assert.IsTrue(l.IsLocked());


            CountdownEvent latch2 = new CountdownEvent(1);

            var t2 = new Thread(delegate(object o)
            {
                try
                {
                    if (ClientLockTest.l.TryLock(20, TimeUnit.SECONDS))
                    {
                        latch2.Signal();
                    }
                }
                catch (Exception e)
                {
                }
            });
            t2.Start();

			Thread.Sleep(1000);
			l.Unlock();

            Assert.IsTrue(latch2.Wait(TimeSpan.FromSeconds(100)));
			Assert.IsTrue(l.IsLocked());
			l.ForceUnlock();
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestForceUnlock()
		{
			l.Lock();

            CountdownEvent latch = new CountdownEvent(1);

            var t2 = new Thread(delegate(object o)
            {
                try
                {
                    l.ForceUnlock();
                    latch.Signal();
                }
                catch (Exception e)
                {
                }
            });
            t2.Start();

			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(100)));
			Assert.IsFalse(l.IsLocked());
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
			l.Lock(1, TimeUnit.MINUTES);
			Assert.IsTrue(l.IsLocked());
			Assert.IsTrue(l.IsLockedByCurrentThread());
			Assert.AreEqual(1, l.GetLockCount());
			Assert.IsTrue(l.GetRemainingLeaseTime() > 1000 * 30);

            CountdownEvent latch2 = new CountdownEvent(1);

            var t2 = new Thread(delegate(object o)
            {
                Assert.IsTrue(ClientLockTest.l.IsLocked());
                Assert.IsFalse(ClientLockTest.l.IsLockedByCurrentThread());
                Assert.AreEqual(1, ClientLockTest.l.GetLockCount());
                Assert.IsTrue(ClientLockTest.l.GetRemainingLeaseTime() > 1000 * 30);
                latch2.Signal();
            });
            t2.Start();

            Assert.IsTrue(latch2.Wait(TimeSpan.FromMinutes(1)));
		}

	}
}
