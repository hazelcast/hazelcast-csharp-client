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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ClientQueueTest : SingleMemberClientRemoteTestBase
    {
        // important to stick with this name as it is configured in hazelcast.xml
        // with a corresponding queue max size of 6 items
        private const string QueueNameBase = "ClientQueueTest";

        [Test]
        public async Task TestAdd()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            for (var i = 0; i < 5; i++)
            {
                Assert.IsTrue(await queue.AddAsync("item_" + i));
            }

            Assert.AreEqual(5, await queue.CountAsync());
        }

        [Test]
        public async Task TestAddAll()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            var coll = new List<string> { "item1", "item2", "item3", "item4" };
            Assert.IsTrue(await queue.AddRangeAsync(coll));
            Assert.That(await queue.CountAsync(), Is.EqualTo(coll.Count));
        }

        [Test]
        public async Task TestClear()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            for (var i = 0; i < 5; i++)
            {
                Assert.IsTrue(await queue.TryEnqueueAsync("item_" + i));
            }

            await queue.ClearAsync();

            Assert.That(await queue.CountAsync(), Is.EqualTo(0));
            Assert.That(await queue.TryDequeueAsync(), Is.Null);
        }

        [Test]
        public async Task TestContains()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            for (var i = 0; i < 5; i++)
            {
                Assert.IsTrue(await queue.TryEnqueueAsync("item_" + i));
            }

            Assert.That(await queue.ContainsAsync("item_3"));
        }

        [Test]
        public async Task TestContainsAll()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            for (var i = 0; i < 5; i++)
            {
                Assert.IsTrue(await queue.TryEnqueueAsync("item_" + i));
            }

            Assert.That(await queue.ContainsAsync("item_3"));
            Assert.That(await queue.ContainsAsync("item_9"), Is.False);

            var list = new List<string>(2) { "item_4", "item_2" };
            Assert.That(await queue.ContainsAllAsync(list));
            list.Add("item");
            Assert.That(await queue.ContainsAllAsync(list), Is.False);
        }

        [Test]
        public async Task TestCopyto()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            for (var i = 0; i < 5; i++)
            {
                Assert.IsTrue(await queue.TryEnqueueAsync("item_" + i));
            }

            var objects = new string[await queue.CountAsync()];
            await queue.CopyToAsync(objects, 0);

            Assert.That(await queue.CountAsync(), Is.EqualTo(objects.Length));
        }

        [Test]
        public async Task TestDrain()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            for (var i = 0; i < 5; i++)
            {
                Assert.IsTrue(await queue.TryEnqueueAsync("item_" + i));
            }

            var list = new List<string>();
            Assert.That(await queue.DrainToAsync(list, 2), Is.EqualTo(2));
            for (var i = 0; i < 2; i++)
            {
                Assert.That(list[i], Is.EqualTo("item_" + i));
            }

            list.Clear();
            Assert.That(await queue.DrainToAsync(list), Is.EqualTo(3));
            for (var i = 0; i < 3; i++)
            {
                Assert.That(list[i], Is.EqualTo("item_" + (i+2)));
            }
        }

        [Test]
        public async Task TestElement()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            Assert.That(await queue.TryEnqueueAsync("item_0"));
            Assert.That(await queue.PeekAsync(), Is.EqualTo("item_0"));
        }

        [Test]
        public async Task TestEnumeration()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            Assert.That(await queue.TryEnqueueAsync("item_0"));

            var e = queue.GetAsyncEnumerator();
            Assert.That(await e.MoveNextAsync());
            Assert.That(e.Current, Is.EqualTo("item_0"));
        }

        [Test]
        public async Task TestIsEmpty()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            Assert.That(await queue.IsEmptyAsync());
        }

        // was old code, we do not support readonly anymore
        // TODO: remove this code
        /*
        [Test]
        public async Task TestIsReadOnly()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            Assert.That(await queue.IsReadOnlyAsync());
        }
        */

        [Test]
        public async Task TestIterator()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            for (var i = 0; i < 5; i++)
            {
                Assert.IsTrue(await queue.TryEnqueueAsync("item_" + i));
            }

            var j = 0;
            await foreach (var item in queue)
            {
                Assert.That(item, Is.EqualTo("item_" + j++));
            }
        }

        [Test]
        public async Task TestListener()
        {
            HConsole.Configure<object>(c => c.SetVerbose());
            HConsole.Configure<AsyncContext>(c => c.SetQuiet());

            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            Assert.That(await queue.CountAsync(), Is.EqualTo(0));

            var eventsCount = 0;
            var sid = await queue.SubscribeAsync(true, handle => handle
                .ItemAdded((sender, args) =>
                {
                    HConsole.WriteLine(this, $"! added: {args.Item}");
                    Interlocked.Increment(ref eventsCount);
                }));

            var adding = Task.Run(async () =>
            {
                for (var i = 0; i < 5; i++)
                {
                    HConsole.WriteLine(this, "Enqueue item...");
                    Assert.IsTrue(await queue.TryEnqueueAsync("item_" + i));
                }

            });

            await AssertEx.SucceedsEventually(() =>
                    Assert.That(eventsCount, Is.EqualTo(5)),
                4000, 500);

            await adding;
            Assert.That(await queue.CountAsync(), Is.EqualTo(5));

            // TODO
            // at the moment disposing the IDistributedObject does not remove
            // all the event listeners, but really it would make sense to do
            // so since disposing also removes the cached object from the dist.
            // obj. factory => look into this
            await queue.UnsubscribeAsync(sid);
        }

        [Test]
        public async Task TestListenerExtreme()
        {
            var queue = await Client.GetQueueAsync<string>("AnotherQueue" + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            Assert.That(await queue.CountAsync(), Is.EqualTo(0));

            const int testItemCount = 40;

            for (var i = 0; i < testItemCount; i++)
            {
                Assert.IsTrue(await queue.TryEnqueueAsync("item_" + i));
            }

            Assert.That(await queue.CountAsync(), Is.EqualTo(testItemCount));

            var eventsCount = 0;
            var sids = new List<Guid>();
            for (var i = 0; i < testItemCount; i++)
            {
                var sid =await queue.SubscribeAsync(true, handle => handle
                    .ItemRemoved((sender, args) =>
                    {
                        Interlocked.Increment(ref eventsCount);
                    }));
                sids.Add(sid);
            }

            await queue.ClearAsync();

            Assert.That(await queue.CountAsync(), Is.EqualTo(0));

            await AssertEx.SucceedsEventually(() =>
                    Assert.That(eventsCount, Is.EqualTo(testItemCount * testItemCount)),
                4000, 500);

            foreach (var sid in sids)
                await queue.UnsubscribeAsync(sid);
        }

        [Test]
        public async Task TestOfferPoll()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            for (var i = 0; i < 10; i++)
            {
                var result = await queue.TryEnqueueAsync("item");
                if (i < 6)
                    Assert.IsTrue(result);
                else
                    Assert.IsFalse(result);
            }

            Assert.That(await queue.CountAsync(), Is.EqualTo(6));

            var task = Task.Run(async () =>
            {
                await Task.Delay(100);
                await queue.TryDequeueAsync();
            });

            Assert.That(await queue.TryEnqueueAsync("item", TimeSpan.FromMilliseconds(200)));

            await task;

            for (var i = 0; i < 10; i++)
            {
                var result = await queue.TryDequeueAsync();
                if (i < 6)
                    Assert.IsNotNull(result);
                else
                    Assert.IsNull(result);
            }

            Assert.That(await queue.CountAsync(), Is.Zero);

            task = Task.Run(async () =>
            {
                await Task.Delay(200);
                await queue.TryEnqueueAsync("item1");
            });

            Assert.That(await queue.TryDequeueAsync(TimeSpan.FromMilliseconds(300)), Is.EqualTo("item1"));

            await task;
        }

        [Test]
        public async Task TestPeek()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            Assert.That(await queue.TryEnqueueAsync("item_0"));
            Assert.That(await queue.PeekAsync(), Is.EqualTo("item_0"));
            Assert.That(await queue.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task TestPut()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            await queue.EnqueueAsync("item_0");

            Assert.That(await queue.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task TestRemainingCapacity()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            Assert.That(await queue.GetRemainingCapacityAsync(), Is.EqualTo(6));
            Assert.That(await queue.TryEnqueueAsync("item_0"));
            Assert.That(await queue.GetRemainingCapacityAsync(), Is.EqualTo(5));
        }

        [Test]
        public async Task TestRemove()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            Assert.That(await queue.TryEnqueueAsync("item1"));
            Assert.That(await queue.TryEnqueueAsync("item2"));
            Assert.That(await queue.TryEnqueueAsync("item3"));
            Assert.That(await queue.RemoveAsync("item4"), Is.False);
            Assert.That(await queue.CountAsync(), Is.EqualTo(3));
            Assert.That(await queue.RemoveAsync("item2"));
            Assert.That(await queue.CountAsync(), Is.EqualTo(2));
            Assert.That(await queue.DequeueAsync(false), Is.EqualTo("item1"));
            Assert.That(await queue.DequeueAsync(false), Is.EqualTo("item3"));

            Assert.That(await queue.RemoveAsync("itemX"), Is.False);
            Assert.That(await queue.TryEnqueueAsync("itemX"));

            // old code from when queue was ICollection
            // TODO: remove this code
            /*
            Assert.IsTrue(((ICollection<object>) queue).Remove("itemX")); //??
            */

            Assert.That(await queue.TryEnqueueAsync("itemY"));

            // FIXME what's queue.Remove() ??
            //Assert.That(await queue.RemoveAsync(), Is.EqualTo("itemY")); // ???
            //Assert.AreEqual("itemY", queue.Remove());
        }

        [Test]
        public async Task TestRemoveRetain()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            for (var i = 0; i < 5; i++)
            {
                Assert.That(await queue.TryEnqueueAsync("item" + (i+1)));
            }

            var list = new List<string> { "item8", "item9" };

            Assert.That(await queue.RemoveAllAsync(list), Is.False);
            Assert.That(await queue.CountAsync(), Is.EqualTo(5));
            list.Add("item3");
            list.Add("item4");
            list.Add("item1");
            Assert.That(await queue.RemoveAllAsync(list));
            Assert.That(await queue.CountAsync(), Is.EqualTo(2));
            list.Clear();
            list.Add("item2");
            list.Add("item5");
            Assert.That(await queue.RetainAllAsync(list), Is.False);
            Assert.That(await queue.CountAsync(), Is.EqualTo(2));
            list.Clear();
            Assert.That(await queue.RetainAllAsync(list));
            Assert.That(await queue.CountAsync(), Is.EqualTo(0));
        }


        [Test]
        public async Task TestTake()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            Assert.That(await queue.TryEnqueueAsync("item1"));
            Assert.That(await queue.DequeueAsync(false), Is.EqualTo("item1"));
        }

        [Test]
        public async Task TestToArray()
        {
            var queue = await Client.GetQueueAsync<string>(QueueNameBase + CreateUniqueName());
            await using var _ = DestroyOnDispose(queue);

            for (var i = 0; i < 5; i++)
            {
                Assert.IsTrue(await queue.TryEnqueueAsync("item_" + i));
            }

            var array = await queue.ToArrayAsync();

            var j = 0;
            foreach (var item in array)
            {
                Assert.That(item, Is.EqualTo("item_" + j++));
            }

            array = await queue.ToArrayAsync(new string[2]);
            Assert.That(array.Length, Is.EqualTo(5)); // array was too small

            j = 0;
            foreach (var item in array)
            {
                Assert.That(item, Is.EqualTo("item_" + j++));
            }
        }

        // that was an old tests when queues where ICollection
        // TODO: remove this code
        /*
        [Test]
        public void TestWrapperMethods()
        {
            var qc = (ICollection<object>) queue;

            qc.Add("asd");
            Assert.IsTrue(qc.Contains("asd"));

            var enumerator = qc.GetEnumerator();

            enumerator.MoveNext();
            Assert.AreEqual("asd", enumerator.Current);

            var enuma = ((IEnumerable) qc).GetEnumerator();
            enuma.MoveNext();
            Assert.AreEqual("asd", enuma.Current);
        }
        */
    }
}