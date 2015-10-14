using System;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Transaction;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientTxnQueueTest : HazelcastBaseTest
    {
        [SetUp]
        public void Init()
        {
            _name = TestSupport.RandomString();
        }

        [TearDown]
        public static void Destroy()
        {
        }

        private string _name;

        [Test]
        public void TestQueueSizeAfterTxnOfferPoll()
        {
            var item = "offered";
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txnQueue = context.GetQueue<object>(_name);
            Assert.IsTrue(txnQueue.Offer(item));
            Assert.AreEqual(1, txnQueue.Size());
            Assert.AreEqual(item, txnQueue.Take());
            context.CommitTransaction();
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestTransactionalOfferPoll1()
        {
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var q = context.GetQueue<string>(_name);
            Assert.IsTrue(q.Offer("ali"));
            var s = q.Poll();
            Assert.AreEqual("ali", s);
            context.CommitTransaction();
            Assert.AreEqual(0, Client.GetQueue<string>(_name).Count);
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestTransactionalOfferPoll2()
        {
            ITransactionContext context = null;
            try
            {
                var latch = new ManualResetEvent(false);
                var queue = Client.GetQueue<string>(_name);
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

                context = Client.NewTransactionContext();
                context.BeginTransaction();
                var q0 = context.GetQueue<string>(_name);
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

        [Test]
        public void TestTransactionalOfferTake()
        {
            var item = "offered";
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var txnQueue = context.GetQueue<object>(_name);
            Assert.IsTrue(txnQueue.Offer(item));
            Assert.AreEqual(1, txnQueue.Size());
            Assert.AreEqual(item, txnQueue.Take());
            context.CommitTransaction();
        }

        /// <exception cref="System.Exception"></exception>
        [Test]
        public virtual void TestTransactionalPeek()
        {
            var context = Client.NewTransactionContext();
            context.BeginTransaction();
            var q = context.GetQueue<string>(_name);
            Assert.IsTrue(q.Offer("ali"));
            var s = q.Peek();
            Assert.AreEqual("ali", s);
            s = q.Peek();
            Assert.AreEqual("ali", s);
            context.CommitTransaction();
            Assert.AreEqual(1, Client.GetQueue<string>(_name).Count);
        }
    }
}