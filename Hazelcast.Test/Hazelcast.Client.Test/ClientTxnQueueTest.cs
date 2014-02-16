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
        //internal const string name = "ClientTxnQueueTest";

        [SetUp]
        public void Init()
        {
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
            string name = Name;//"testTransactionalOfferPoll1";
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
		    ITransactionContext context=null;
            try
            {
                //string name0 = "qqq";
                string name0 = Name;//"defQueue0";
                string name1 = Name;//"defQueue1";
                
                var latch = new ManualResetEvent(false);
                var queue = client.GetQueue<string>(name0);
                var t = new Thread(delegate()
                {
                    try
                    {
                        latch.WaitOne();
                        //Thread.Sleep(3000);
                        queue.Offer("item0");
                        
                    }
                    catch (Exception e)
                    {
                        Assert.Fail();
                    }
                });
                t.Start();

                context = client.NewTransactionContext();
                context.BeginTransaction();
                ITransactionalQueue<string> q0 = context.GetQueue<string>(name0);
                string s = null;
                latch.Set();
                t.Join();
                try
                {
                    s = q0.Poll(20, TimeUnit.SECONDS);
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
                Assert.AreEqual("item0", s);
                context.CommitTransaction();
                context = null;
                Assert.AreEqual(0, queue.Count);
            }
            finally
            {
                if (context != null)
                {
                    context.RollbackTransaction();
                }

            }
		}

		/// <exception cref="System.Exception"></exception>
		[Test]
		public virtual void TestTransactionalPeek()
		{
            string name = Name;//"defQueue";
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
