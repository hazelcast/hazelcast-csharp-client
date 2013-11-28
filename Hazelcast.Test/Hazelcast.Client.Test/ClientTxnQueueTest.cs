using System;
using System.Threading;
using Hazelcast.Client.Test;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using Hazelcast.Transaction;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
	[TestFixture]
	public class ClientTxnQueueTest:HazelcastBaseTest
	{
        internal const string name = "ClientTxnQueueTest";

        [SetUp]
        public static void Init()
        {
            InitClient();
            //map = client.GetMap<object, object>(name);
        }

        [TearDown]
        public static void Destroy()
        {
            //map.Clear();
            //client.GetLifecycleService().Shutdown();
        }

        /// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestTransactionalOfferPoll1()
		{
			string name = "testTransactionalOfferPoll1";
			ITransactionContext context = client.NewTransactionContext();
			context.BeginTransaction();
            ITransactionalQueue<string> q = context.GetQueue<string>(name);
			Assert.IsTrue(q.Offer("ali"));
			string s = q.Poll();
			Assert.AreEqual("ali", s);
			context.CommitTransaction();
            Assert.AreEqual(0, client.GetQueue<string>(name).Count);
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestTransactionalOfferPoll2()
		{
			string name0 = "defQueue0";
			string name1 = "defQueue1";
            CountdownEvent latch = new CountdownEvent(1);

		    var t = new Thread(delegate(object o)
		    {
                try
                {
                    latch.Wait(TimeSpan.FromSeconds(5));
                    Thread.Sleep(3000);
                    client.GetQueue<string>(name0).Offer("item0");
                }
                catch (Exception)
                {
                }
		    });
            t.Start();

			ITransactionContext context = client.NewTransactionContext();
			context.BeginTransaction();
            ITransactionalQueue<string> q0 = context.GetQueue<string>(name0);
            ITransactionalQueue<string> q1 = context.GetQueue<string>(name1);
			string s = null;
			latch.Signal();
			try
			{
				s = q0.Poll(10, TimeUnit.SECONDS);
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message);
			}
			Assert.AreEqual("item0", s);
			q1.Offer(s);
			context.CommitTransaction();
            Assert.AreEqual(0, client.GetQueue<string>(name0).Count);
            Assert.AreEqual("item0", client.GetQueue<string>(name1).Poll());
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestTransactionalPeek()
		{
			string name = "defQueue";
			ITransactionContext context = client.NewTransactionContext();
			context.BeginTransaction();
            ITransactionalQueue<string> q = context.GetQueue<string>(name);
			Assert.IsTrue(q.Offer("ali"));
			string s = q.Peek();
			Assert.AreEqual("ali", s);
			s = q.Peek();
			Assert.AreEqual("ali", s);
			context.CommitTransaction();
            Assert.AreEqual(1, client.GetQueue<string>(name).Count);
		}
	}
}
