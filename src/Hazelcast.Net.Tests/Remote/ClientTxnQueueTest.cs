// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Testing;
using Hazelcast.Transactions;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ClientTxnQueueTest : SingleMemberClientRemoteTestBase
    {
        [Test]
        public async Task TestQueueSizeAfterTxnOfferPoll()
        {
            const string item = "offered";
            await using var context = await Client.BeginTransactionAsync();
            var txQueue = await context.GetQueueAsync<string>(CreateUniqueName());
            Assert.IsTrue(await txQueue.OfferAsync(item));
            Assert.AreEqual(1, await txQueue.GetSizeAsync());
            Assert.AreEqual(item, await txQueue.TakeAsync());
            await context.CommitAsync();
        }

        [Test]
        public async Task TestTransactionalOfferPoll1()
        {
            await using var context = await Client.BeginTransactionAsync();
            var txQueue = await context.GetQueueAsync<string>(CreateUniqueName());
            Assert.IsTrue(await txQueue.OfferAsync("ali"));
            var s = await txQueue.PollAsync();
            Assert.AreEqual("ali", s);
            await context.CommitAsync();
            var queue = await Client.GetQueueAsync<string>(txQueue.Name);
            Assert.AreEqual(0, await queue.GetSizeAsync());
        }

        [Test]
        public async Task TestTransactionalOfferPoll2()
        {
            ITransactionContext context = null;
            try
            {
                var latch = new ManualResetEvent(false);
                var queue = await Client.GetQueueAsync<string>(CreateUniqueName());

                var t = Task.Run(async () =>
                {
                    try
                    {
                        latch.WaitOne();
                        //Thread.Sleep(3000);
                        await queue.OfferAsync("item0");
                    }
                    catch
                    {
                        Assert.Fail();
                    }
                });

                context = await Client.BeginTransactionAsync();
                var txQueue = await context.GetQueueAsync<string>(queue.Name);
                string s = null;
                latch.Set();
                await t;
                try
                {
                    s = await txQueue.PollAsync(TimeSpan.FromSeconds(20));
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
                Assert.AreEqual("item0", s);
                await context.CommitAsync();
                context = null;
                Assert.AreEqual(0, await queue.GetSizeAsync());
            }
            finally
            {
                if (context != null)
                {
                    await context.RollbackAsync();
                    await context.DisposeAsync();
                }
            }
        }

        [Test]
        public async Task TestTransactionalOfferTake()
        {
            var item = "offered";
            await using var context = await Client.BeginTransactionAsync();
            var txQueue = await context.GetQueueAsync<string>(CreateUniqueName());
            Assert.IsTrue(await txQueue.OfferAsync(item));
            Assert.AreEqual(1, await txQueue.GetSizeAsync());
            Assert.AreEqual(item, await txQueue.TakeAsync());
            await context.CommitAsync();
        }

        [Test]
        public async Task TestTransactionalPeek()
        {
            await using var context = await Client.BeginTransactionAsync();
            var txQueue = await context.GetQueueAsync<string>(CreateUniqueName());
            Assert.IsTrue(await txQueue.OfferAsync("ali"));
            var s = await txQueue.PeekAsync();
            Assert.AreEqual("ali", s);
            s = await txQueue.PeekAsync();
            Assert.AreEqual("ali", s);
            await context.CommitAsync();
            var queue = await Client.GetQueueAsync<string>(txQueue.Name);
            Assert.AreEqual(1, await queue.GetSizeAsync());
        }
    }
}
