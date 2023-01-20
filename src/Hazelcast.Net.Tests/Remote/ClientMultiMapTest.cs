// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Partitioning;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class ClientMultiMapTest : SingleMemberClientRemoteTestBase
    {
        [Test]
        public async Task TestClear()
        {
            var dictionary = await Client.GetMultiMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.PutAsync("a", "b");
            await dictionary.PutAsync("a", "c");
            await dictionary.ClearAsync();
            Assert.AreEqual(0, await dictionary.GetSizeAsync());
        }

        [Test]
        public async Task TestContains()
        {
            var dictionary = await Client.GetMultiMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            Assert.IsTrue(await dictionary.PutAsync("key1", "value1"));
            Assert.IsTrue(await dictionary.PutAsync("key1", "value2"));
            Assert.IsTrue(await dictionary.PutAsync("key1", "value3"));
            Assert.IsTrue(await dictionary.PutAsync("key2", "value4"));
            Assert.IsTrue(await dictionary.PutAsync("key2", "value5"));
            Assert.IsFalse(await dictionary.ContainsKeyAsync("key3"));
            Assert.IsTrue(await dictionary.ContainsKeyAsync("key1"));
            Assert.IsFalse(await dictionary.ContainsValueAsync("value6"));
            Assert.IsTrue(await dictionary.ContainsValueAsync("value4"));
            Assert.IsFalse(await dictionary.ContainsEntryAsync("key1", "value4"));
            Assert.IsFalse(await dictionary.ContainsEntryAsync("key2", "value3"));
            Assert.IsTrue(await dictionary.ContainsEntryAsync("key1", "value1"));
            Assert.IsTrue(await dictionary.ContainsEntryAsync("key2", "value5"));
        }

        [Test]
        public async Task TestForceUnlock()
        {
            var dictionary = await Client.GetMultiMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.LockAsync("key1");

            using (AsyncContext.New())
            {
                await dictionary.ForceUnlockAsync("key1");
            };

            Assert.IsFalse(await dictionary.IsLockedAsync("key1"));
        }

        [Test]
        public async Task TestKeySetEntrySetAndValues()
        {
            var dictionary = await Client.GetMultiMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            Assert.IsTrue(await dictionary.PutAsync("key1", "value1"));
            Assert.IsTrue(await dictionary.PutAsync("key1", "value2"));
            Assert.IsTrue(await dictionary.PutAsync("key1", "value3"));
            Assert.IsTrue(await dictionary.PutAsync("key2", "value1"));
            Assert.IsTrue(await dictionary.PutAsync("key2", "value2"));
            Assert.AreEqual(2, (await dictionary.GetKeysAsync()).Count); // distinct keys
            Assert.AreEqual(5, (await dictionary.GetValuesAsync()).Count); // all values
            Assert.AreEqual(5, (await dictionary.GetEntriesAsync()).Count); // all key-value pairs
        }

        private IDisposable HConsoleForTest()

            => HConsole.Capture(options => options
                .Configure().SetMaxLevel()
                .Configure(this).SetPrefix("TEST")
                .Configure<SocketConnectionBase>().SetIndent(8).SetPrefix("SOCKET").SetLevel(0)
                .Configure<ClientMessageConnection>().SetMinLevel()
                .Configure<AsyncContext>().SetMinLevel()
                .Configure<Partitioner>().SetLevel(1));

        [Test]
        public async Task TestListener()
        {
            using var __ = HConsoleForTest();

            var dictionary = await Client.GetMultiMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            var added1 = 0;
            var removed1 = 0;
            var added2 = 0;
            var removed2 = 0;
            var added3 = 0;
            var removed3 = 0;

            var sid1 = await dictionary.SubscribeAsync(events => events
                .EntryAdded((sender, args) => Interlocked.Increment(ref added1))
                .EntryRemoved((sender, args) => Interlocked.Increment(ref removed1)),
                true
            );

            var sid2 = await dictionary.SubscribeAsync(events => events
                .EntryAdded((sender, args) => Interlocked.Increment(ref added2))
                .EntryRemoved((sender, args) => Interlocked.Increment(ref removed2)),
                "key3", true
            );

            var sid3 = await dictionary.SubscribeAsync(events => events
                    .EntryAdded((sender, args) => Interlocked.Increment(ref added3))
                    .EntryRemoved((sender, args) => Interlocked.Increment(ref removed3)),
                "key4", true
            );

            await dictionary.PutAsync("key1", "value1");
            await dictionary.PutAsync("key1", "value2");
            await dictionary.PutAsync("key1", "value3");
            await dictionary.PutAsync("key2", "value4");
            await dictionary.PutAsync("key2", "value5");
            await dictionary.RemoveAsync("key1", "value2");
            await dictionary.PutAsync("key3", "value6");
            await dictionary.PutAsync("key3", "value7");
            await dictionary.PutAsync("key3", "value8");
            await dictionary.PutAsync("key4", "value9");

            // NOTE
            // plain RemoveAsync (MultiMapDeleteCodec) does not trigger any event for the 'key3' subscription,
            // but GetAndRemoveAsync (MultiMapRemoveCodec) does - and also return values - even though we are
            // not filtering on predicate nor on value here - only on key
            // but, same thing happens on Java too so it is a server-side thing, not our code - just test
            // accordingly
            await dictionary.RemoveAsync("key3");
            await dictionary.DeleteAsync("key4");

            await AssertEx.SucceedsEventually(() =>
            {
                Assert.That(added1, Is.EqualTo(9));
                Assert.That(removed1, Is.EqualTo(4));
                Assert.That(added2, Is.EqualTo(3));
                Assert.That(removed2, Is.EqualTo(3));
                Assert.That(added3, Is.EqualTo(1));
            }, 10000, 500);

            await Task.Delay(1000);
            Assert.That(removed3, Is.EqualTo(0)); // event did not trigger
        }

        [Test]
        public async Task TestLock()
        {
            var dictionary = await Client.GetMultiMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.LockAsync("key1");

            bool locked;
            using (AsyncContext.New())
            {
                locked = await dictionary.TryLockAsync("key1");
            };

            Assert.That(locked, Is.False);

            await dictionary.ForceUnlockAsync("key1");
        }

        [Test]
        public async Task TestLockTtl()
        {
            var dictionary = await Client.GetMultiMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await dictionary.LockAsync("key1", TimeSpan.FromSeconds(1));

            var count = 0;
            using (AsyncContext.New())
            {
                if (!await dictionary.TryLockAsync("key1"))
                    Interlocked.Increment(ref count);

                if (await dictionary.TryLockAsync("key1", TimeSpan.FromSeconds(2)))
                    Interlocked.Increment(ref count);
            };

            Assert.That(count, Is.EqualTo(2));
            await dictionary.ForceUnlockAsync("key1");
        }

        [Test]
        public async Task TestPutGetRemove()
        {
            var dictionary = await Client.GetMultiMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            Assert.IsTrue(await dictionary.PutAsync("key1", "value1"));
            Assert.IsTrue(await dictionary.PutAsync("key1", "value2"));
            Assert.IsTrue(await dictionary.PutAsync("key1", "value3"));
            Assert.IsTrue(await dictionary.PutAsync("key2", "value4"));
            Assert.IsTrue(await dictionary.PutAsync("key2", "value5"));
            Assert.AreEqual(3, await dictionary.GetValueCountAsync("key1"));
            Assert.AreEqual(2, await dictionary.GetValueCountAsync("key2"));
            Assert.AreEqual(5, await dictionary.GetSizeAsync());
            var coll = await dictionary.GetAsync("key1");
            Assert.AreEqual(3, coll.Count);
            coll = await dictionary.RemoveAsync("key2");
            Assert.AreEqual(2, coll.Count);
            Assert.AreEqual(0, await dictionary.GetValueCountAsync("key2"));
            Assert.AreEqual(0, (await dictionary.GetAsync("key2")).Count);
            Assert.IsFalse(await dictionary.RemoveAsync("key1", "value4"));
            Assert.AreEqual(3, await dictionary.GetSizeAsync());
            Assert.IsTrue(await dictionary.RemoveAsync("key1", "value2"));
            Assert.AreEqual(2, await dictionary.GetSizeAsync());
            Assert.IsTrue(await dictionary.RemoveAsync("key1", "value1"));
            Assert.AreEqual(1, await dictionary.GetSizeAsync());
            using var enumerator = (await dictionary.GetAsync("key1")).GetEnumerator();
            enumerator.MoveNext();
            Assert.AreEqual("value3", enumerator.Current);
        }

        [Test]
        public async Task TestRemoveListener()
        {
            var dictionary = await Client.GetMultiMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            var count = 0;
            var sid = await dictionary.SubscribeAsync(events => events
                .EntryAdded((sender, args) => Interlocked.Increment(ref count))
                .EntryRemoved((sender, args) => Interlocked.Increment(ref count))
            );

            await dictionary.PutAsync("key1", "value1");
            await AssertEx.SucceedsEventually(() => Assert.That(count, Is.EqualTo(1)), 10000, 500);

            Assert.IsTrue(await dictionary.UnsubscribeAsync(sid));
            await dictionary.DeleteAsync("key1");
            await Task.Delay(1000);
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public async Task TestTryLock()
        {
            var dictionary = await Client.GetMultiMapAsync<string, string>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            Assert.IsTrue(await dictionary.TryLockAsync("key1", TimeSpan.FromMilliseconds(200)));
            Assert.IsTrue(await dictionary.IsLockedAsync("key1"));

            var contextId = AsyncContext.Current.Id;
            var otherContextId = contextId;

            bool wasLocked, locked;
            using (AsyncContext.New())
            {
                otherContextId = AsyncContext.Current.Id;
                wasLocked = await dictionary.IsLockedAsync("key1");
                locked = await dictionary.TryLockAsync("key1", TimeSpan.FromMilliseconds(200));
            }

            Assert.That(contextId, Is.Not.EqualTo(otherContextId));
            Assert.That(wasLocked);
            Assert.That(locked, Is.False);

            Assert.IsTrue(await dictionary.IsLockedAsync("key1"));

            Task<bool> locking;
            using (AsyncContext.New())
            {
                locking = Task.Run(async () =>
                {
                    otherContextId = AsyncContext.Current.Id;
                    return await dictionary.TryLockAsync("key1", TimeSpan.FromSeconds(20));
                });
            }
            Assert.That(contextId, Is.Not.EqualTo(otherContextId));

            await Task.Delay(100);
            await dictionary.UnlockAsync("key1");

            locked = await locking;
            Assert.That(locked, Is.True);
            Assert.IsTrue(await dictionary.IsLockedAsync("key1"));
            await dictionary.ForceUnlockAsync("key1");
        }
    }
}
