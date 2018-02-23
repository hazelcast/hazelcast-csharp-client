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
using Hazelcast.Transaction;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientTxnQueueTest : SingleMemberBaseTest
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
                    catch
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
                    s = q0.Poll(20, TimeUnit.Seconds);
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