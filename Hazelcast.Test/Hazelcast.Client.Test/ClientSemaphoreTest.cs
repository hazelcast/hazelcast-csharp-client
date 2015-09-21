using System;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[TestFixture]
	public class ClientSemaphoreTest:HazelcastBaseTest
	{
        internal const string name = "ClientSemaphoreTest";


		internal static ISemaphore s;
        [SetUp]
        public void Init()
        {
            s = Client.GetSemaphore(Name);

            s.ReducePermits(100);
            s.Release(9);
            s.Release();

        }

        [TearDown]
        public static void Destroy()
        {
        }


		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestAcquire()
		{
			Assert.AreEqual(10, s.DrainPermits());
            CountdownEvent latch = new CountdownEvent(1);

		    var t = new Thread(delegate(object o)
		    {
                try
                {
                    s.Acquire();
                    latch.Signal();
                }
                catch (Exception e)
                {
                }
		    });
            t.Start();

            Thread.Sleep(100);
			s.Release(2);
			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
			Assert.AreEqual(1, s.AvailablePermits());
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TryAcquire()
		{
			Assert.IsTrue(s.TryAcquire());
			Assert.IsTrue(s.TryAcquire(9));
			Assert.AreEqual(0, s.AvailablePermits());
			Assert.IsFalse(s.TryAcquire(1, TimeUnit.SECONDS));
			Assert.IsFalse(s.TryAcquire(2, 1, TimeUnit.SECONDS));

            CountdownEvent latch = new CountdownEvent(1);

            var t = new Thread(delegate(object o)
            {
                try
                {
                    if (s.TryAcquire(2, 5, TimeUnit.SECONDS))
                    {
                        latch.Signal();
                    }
                }
                catch (Exception e)
                {
                }
            });
            t.Start();

			s.Release(2);
			Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)));
			Assert.AreEqual(0, s.AvailablePermits());
		}

	    [Test]
	    public void TestInit()
	    {
	        var semInit = Client.GetSemaphore(Name);
	        semInit.Init(2);
	        Assert.AreEqual(2, semInit.AvailablePermits());
	        semInit.Destroy();
	    }

	    [Test]
        [ExpectedException(typeof(ArgumentException))]
	    public void TestInitNeg()
        {
            var semInit = Client.GetSemaphore(Name);
            semInit.Init(-2);
            semInit.Destroy();
        }

	}
}
