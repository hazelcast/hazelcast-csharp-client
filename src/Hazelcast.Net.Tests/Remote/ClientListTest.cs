// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ClientListTest : SingleMemberClientRemoteTestBase
    {
        private const string ListNameBase = "List";

        [Test]
        public async Task RemoveRetainAll()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            Assert.That(await list.AddAsync("item_1"));
            Assert.That(await list.AddAsync("item_2"));
            Assert.That(await list.AddAsync("item_1"));
            Assert.That(await list.AddAsync("item_4"));

            var l = new List<string> { "item_4", "item_3" };
            Assert.That(await list.RemoveAllAsync(l));
            Assert.That(await list.CountAsync(), Is.EqualTo(3));
            Assert.That(await list.RemoveAllAsync(l), Is.False);
            Assert.That(await list.CountAsync(), Is.EqualTo(3));
            l.Clear();
            l.Add("item_1");
            l.Add("item_2");
            Assert.That(await list.RetainAllAsync(l), Is.False);
            Assert.That(await list.CountAsync(), Is.EqualTo(3));
            l.Clear();
            Assert.That(await list.RetainAllAsync(l));
            Assert.That(await list.CountAsync(), Is.EqualTo(0));
        }

        [Test]
        public async Task TestAddAll()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            var l = new List<string> { "item_1", "item_2" };
            Assert.That(await list.AddRangeAsync(l));

            Assert.That(await list.CountAsync(), Is.EqualTo(2));
            Assert.That(await list.InsertRangeAsync(1, l));
            Assert.That(await list.CountAsync(), Is.EqualTo(4));

            Assert.That(await list.GetAsync(0), Is.EqualTo("item_1"));
            Assert.That(await list.GetAsync(1), Is.EqualTo("item_1"));
            Assert.That(await list.GetAsync(2), Is.EqualTo("item_2"));
            Assert.That(await list.GetAsync(3), Is.EqualTo("item_2"));
        }

        [Test]
        public async Task TestAddSetRemove()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            Assert.That(await list.AddAsync("item_1"));
            Assert.That(await list.AddAsync("item_2"));
            await list.InsertAsync(0, "item_3");
            Assert.That(await list.CountAsync(), Is.EqualTo(3));
            var o = await list.SetAsync(2, "item_4");
            Assert.AreEqual("item_2", o);
            Assert.That(await list.CountAsync(), Is.EqualTo(3));
            Assert.That(await list.GetAsync(0), Is.EqualTo("item_3"));
            Assert.That(await list.GetAsync(1), Is.EqualTo("item_1"));
            Assert.That(await list.GetAsync(2), Is.EqualTo("item_4"));
            Assert.IsFalse(await list.RemoveAsync("item_2"));
            Assert.IsTrue(await list.RemoveAsync("item_3"));
            o = await list.RemoveAtAsync(1);
            Assert.AreEqual("item_4", o);
            Assert.That(await list.CountAsync(), Is.EqualTo(1));
            Assert.AreEqual("item_1", await list.GetAsync(0));

            await list.SetAsync(0, "itemMod");
            Assert.AreEqual("itemMod", await list.GetAsync(0));
        }

        [Test]
        public async Task TestContains()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            Assert.IsTrue(await list.AddAsync("item1"));
            Assert.IsTrue(await list.AddAsync("item2"));
            Assert.IsTrue(await list.AddAsync("item1"));
            Assert.IsTrue(await list.AddAsync("item4"));
            Assert.IsFalse(await list.ContainsAsync("item3"));
            Assert.IsTrue(await list.ContainsAsync("item2"));
            var l = new List<string> { "item4", "item3" };
            Assert.IsFalse(await list.ContainsAllAsync(l));
            Assert.IsTrue(await list.AddAsync("item3"));
            Assert.IsTrue(await list.ContainsAllAsync(l));
        }

        [Test]
        public async Task TestIndexOf()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            Assert.IsTrue(await list.AddAsync("item1"));
            Assert.IsTrue(await list.AddAsync("item2"));
            Assert.IsTrue(await list.AddAsync("item1"));
            Assert.IsTrue(await list.AddAsync("item4"));
            Assert.AreEqual(-1, await list.IndexOfAsync("item5"));
            Assert.AreEqual(0, await list.IndexOfAsync("item1"));
            Assert.AreEqual(-1, await list.LastIndexOfAsync("item6"));
            Assert.AreEqual(2, await list.LastIndexOfAsync("item1"));
        }

        [Test]
        public async Task TestInsert()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            await list.AddAsync("item0");
            await list.AddAsync("item1");
            await list.AddAsync("item2");
            await list.InsertAsync(1, "item1Mod");
            Assert.AreEqual("item1Mod", await list.GetAsync(1));
            await list.RemoveAtAsync(0);
            Assert.AreEqual("item1Mod", await list.GetAsync(0));
            Assert.AreEqual("item1", await list.GetAsync(1));
        }

        [Test]
        public async Task TestIsEmpty()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            Assert.IsTrue(await list.IsEmptyAsync());
            await list.AddAsync("item1");
            Assert.IsFalse(await list.IsEmptyAsync());
            await list.ClearAsync();
            Assert.IsTrue(await list.IsEmptyAsync());
        }

        [Test]
        public async Task TestIterator()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            Assert.IsTrue(await list.AddAsync("item1"));
            Assert.IsTrue(await list.AddAsync("item2"));
            Assert.IsTrue(await list.AddAsync("item1"));
            Assert.IsTrue(await list.AddAsync("item4"));
            var iter = list.GetAsyncEnumerator();
            Assert.IsTrue(await iter.MoveNextAsync());
            Assert.AreEqual("item1", iter.Current);
            Assert.IsTrue(await iter.MoveNextAsync());
            Assert.AreEqual("item2", iter.Current);
            Assert.IsTrue(await iter.MoveNextAsync());
            Assert.AreEqual("item1", iter.Current);
            Assert.IsTrue(await iter.MoveNextAsync());
            Assert.AreEqual("item4", iter.Current);
            Assert.IsFalse(await iter.MoveNextAsync());

            var l = await list.GetRangeAsync(1, 3);
            Assert.AreEqual(2, l.Count);
            Assert.AreEqual("item2", l[0]);
            Assert.AreEqual("item1", l[1]);
        }

        [Test]
        public async Task TestListener()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            var eventsCount = 0;
            var sid = await list.SubscribeAsync(true, handle => handle
                .ItemAdded((sender, args) =>
                {
                    Interlocked.Increment(ref eventsCount);
                }));

            for (var i = 0; i < 5; i++)
                await list.AddAsync("item" + i);
            await list.AddAsync("done");

            await AssertEx.SucceedsEventually(() => Assert.That(eventsCount, Is.EqualTo(6)), 4000, 500);

            await list.UnsubscribeAsync(sid);
        }

        [Test]
        public async Task TestRemoveListener()
        {
            var list = await Client.GetListAsync<string>(ListNameBase + CreateUniqueName());
            await using var _ = DestroyAndDispose(list);

            var eventsCount = 0;
            var sid = await list.SubscribeAsync(true, handle => handle
                .ItemAdded((sender, args) =>
                {
                    Interlocked.Increment(ref eventsCount);
                }));

            await list.UnsubscribeAsync(sid);

            await list.AddAsync("item");

            await Task.Delay(4_000);
            Assert.That(eventsCount, Is.Zero);
        }
    }
}