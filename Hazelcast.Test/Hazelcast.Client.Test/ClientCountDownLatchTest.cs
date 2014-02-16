using System;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[TestFixture]
	public class ClientCountDownLatchTest:HazelcastBaseTest
	{
        //internal const string name = "ClientCountDownLatchTest";

		internal ICountDownLatch l;

        [SetUp]
        public void Init()
        {
            l = client.GetCountDownLatch(Name);
            
        }

        [TearDown]
        public void Destroy()
        {
            l.Destroy();
            Console.WriteLine("destroy");
        }


		[Test]
		public virtual void TestLatch()
		{
			Assert.IsTrue(l.TrySetCount(20));
			Assert.IsFalse(l.TrySetCount(10));
			Assert.AreEqual(20, l.GetCount());

		    var t1 = new Thread(delegate(object o)
		    {
                for (int i = 0; i < 20; i++)
                {
                    l.CountDown();
                    try
                    {
                        Thread.Sleep(60);
                    }
                    catch (Exception e)
                    {
                        
                    }
                }
		    });
            t1.Start();

            
            Assert.IsFalse(l.Await(1, TimeUnit.SECONDS));
			Assert.IsTrue(l.Await(5, TimeUnit.SECONDS));

		    t1.Join();
		}

	}
}
