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
    public class MapTests : SingleMemberClientRemoteTestBase
    {
        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task Set()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            // Set adds a new value, or replaces an existing value,
            // and does not return anything
            // NOTE: no way to know whether it added or replaced?

            await map.SetAsync("key", 42).CAF();
            await map.SetAsync("key", 43).CAF();

            var value = await map.GetAsync("key").CAF();
            Assert.AreEqual(43, value);

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(1, count);

            value = await TaskEx.RunWithTimeout(t=> map.GetAsync("key"), TimeSpan.FromSeconds(30)).CAF();
            Assert.AreEqual(43, value);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task SetWithTimeout()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            // AddOrReplace adds a new value, or replaces an existing value,
            // and does not return anything
            // NOTE: no way to know whether it added or replaced?

            await map.SetAsync("key", 42).CAF();

            // add-or-replace with a 3 seconds timeout and a potential TaskTimeoutException
            // in case of a TaskTimeoutException - map.SetAsync() will keep running in the
            // background, its exception will be observed, but the end result (whether the
            // value was actually set or not) is unspecified.
            await TaskEx.RunWithTimeout(t => map.SetAsync("key", 43), TimeSpan.FromSeconds(12)).CAF();

            var value = await map.GetAsync("key").CAF();
            Assert.AreEqual(43, value);

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task GetMissingValValue()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            Assert.That(await map.GetAsync("key"), Is.Zero);

            await map.SetAsync("key", default);

            Assert.That(await map.GetAsync("key"), Is.Zero);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task GetMissingNullableValue()
        {
            var map = await Client.GetMapAsync<string, int?>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            Assert.That(await map.GetAsync("key"), Is.Null);

            await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await map.SetAsync("key", default));
            await map.SetAsync("key", 0);

            Assert.That(await map.GetAsync("key"), Is.Zero);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task GetMissingRefValue()
        {
            var map = await Client.GetMapAsync<string, string>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            var value = await map.GetAsync("key");
            Assert.That(value, Is.Null);

            await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await map.SetAsync("key", default));
            await map.SetAsync("key", string.Empty);

            value = await map.GetAsync("key");
            Assert.That(value, Is.EqualTo(string.Empty));
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task GetAndSet()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            // AddOrReplace adds a new value, or replaces an existing value,
            // and returns the existing value, or the default value
            // NOTE: no way to know if the default value existed (eg zero)?

            Assert.That(await map.GetAndSetAsync("key", 42), Is.Zero);

            Assert.That(await map.GetAndSetAsync("key", 43), Is.EqualTo(42));

            Assert.That(await map.GetAsync("key"), Is.EqualTo(43));

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task GetAndSetNullable()
        {
            var map = await Client.GetMapAsync<string, int?>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            // AddOrReplace adds a new value, or replaces an existing value,
            // and returns the existing value, or the default value
            // NOTE: no way to know if the default value existed (eg zero)?

            Assert.That(await map.GetAndSetAsync("key", 42), Is.Null);

            Assert.That(await map.GetAndSetAsync("key", 43), Is.EqualTo(42));

            Assert.That(await map.GetAsync("key"), Is.EqualTo(43));

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task Add()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            // TryAdd adds a new value if no value exists already,
            // and returns the existing value, or the default value
            // NOTE: no way to know if the default value existed (eg zero)?

            await map.SetAsync("key1", 42).CAF();

            var result1 = await map.GetOrAddAsync("key1", 43).CAF();
            Assert.That(result1, Is.EqualTo(42));

            var result2 = await map.GetOrAddAsync("key2", 43).CAF();
            Assert.That(result2, Is.EqualTo(0));

            var value1 = await map.GetAsync("key1").CAF();
            Assert.AreEqual(42, value1);

            var value2 = await map.GetAsync("key2").CAF();
            Assert.AreEqual(43, value2);

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(2, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task SetMany()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            // AddOrReplace adds new values, or replaces existing values
            // NOTE: no way to know what happened

            await map.SetAsync("key1", 42).CAF();

            var value1 = await map.GetAsync("key1").CAF();
            Assert.AreEqual(42, value1);

            await map.SetAllAsync(new Dictionary<string, int>
            {
                ["key1"] = 43,
                ["key2"] = 44
            });

            value1 = await map.GetAsync("key1").CAF();
            Assert.AreEqual(43, value1);

            var value2 = await map.GetAsync("key2").CAF();
            Assert.AreEqual(44, value2);

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(2, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task ReplaceByKey()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            // Replace replaces an existing value, and returns the existing value,
            // else does nothing if no value exists already (does not add)

            await map.SetAsync("key1", 42).CAF();

            var result1 = await map.TryUpdateAsync("key1", 43).CAF();
            Assert.AreEqual(42, result1);

            Assert.That(await map.TryUpdateAsync("key2", 43), Is.Zero);

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task ReplaceNullableByKey()
        {
            var map = await Client.GetMapAsync<string, int?>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            // Replace replaces an existing value, and returns the existing value,
            // else does nothing if no value exists already (does not add)

            await map.SetAsync("key1", 42).CAF();

            var result1 = await map.TryUpdateAsync("key1", 43).CAF();
            Assert.AreEqual(42, result1);

            Assert.That(await map.TryUpdateAsync("key2", 43), Is.Null);

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task ReplaceByKeyAndValue()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            // Replace replaces an existing value, and returns the existing value,
            // else does nothing if no value exists already (does not add)

            await map.SetAsync("key1", 42).CAF();

            var result1 = await map.TryUpdateAsync("key1", 43, 44).CAF();
            Assert.IsFalse(result1);

            var result2 = await map.TryUpdateAsync("key1", 42, 44).CAF();
            Assert.IsTrue(result2);

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task SetWithTimeToLive()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            // AddOrReplace adds a new value, or replaces an existing value,
            // and does not return anything
            // NOTE: no way to know whether it added or replaced?

            await map.SetAsync("key", 42, TimeSpan.FromSeconds(1)).CAF();
            var value = await map.GetAsync("key").CAF();
            Assert.AreEqual(42, value);

            await Task.Delay(1000); // wait for 1 second

            Assert.That(await map.GetAsync("key"), Is.Zero);

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(0, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task SetTransient()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            // AddTransient adds a new value, or replaces an existing value,
            // and does not return anything
            // (so it's "add or replace" really)
            // NOTE: no way to know whether it added or replaced?

            await map.SetTransientAsync("key", 42, TimeToLive.InfiniteTimeSpan).CAF();

            await map.SetTransientAsync("key", 43, TimeToLive.InfiniteTimeSpan).CAF();

            await map.SetTransientAsync("key1", 43, TimeToLive.InfiniteTimeSpan).CAF();

            var value = await map.GetAsync("key").CAF();
            Assert.AreEqual(43, value);

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(2, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task TrySet()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            // TryAddOrReplace is like AddOrReplace but with a timeout

            await map.TrySetAsync("key", 42, TimeSpan.FromSeconds(1)).CAF();
            var value = await map.GetAsync("key").CAF();
            Assert.AreEqual(42, value);

            await map.TrySetAsync("key", 43, TimeSpan.FromSeconds(1)).CAF();
            value = await map.GetAsync("key").CAF();
            Assert.AreEqual(43, value);

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task Clear()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            var entries = new Dictionary<string, int>();
            for (var i = 0; i < 100; i++)
                entries["key" + i] = i;

            await map.SetAllAsync(entries).CAF();

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(100, count);

            await map.ClearAsync().CAF();

            count = await map.CountAsync().CAF();
            Assert.AreEqual(0, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task GetAll()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            var entries = new Dictionary<string, int>();
            for (var i = 0; i < 100; i++)
                entries["key" + i] = i;

            await map.SetAllAsync(entries).CAF();

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(100, count);

            var keys = await map.GetKeysAsync().CAF();
            Assert.AreEqual(100, keys.Count);

            var s = new HashSet<int>();
            for (var i = 0; i < 100; i++)
                s.Add(i);

            foreach (var key in keys)
                Assert.IsTrue(s.Remove(int.Parse(key.Substring(3))));

            Assert.AreEqual(0, s.Count);

            var values = await map.GetValuesAsync().CAF();
            Assert.AreEqual(100, values.Count);

            s = new HashSet<int>();
            for (var i = 0; i < 100; i++)
                s.Add(i);

            foreach (var value in values)
                Assert.IsTrue(s.Remove(value));

            Assert.AreEqual(0, s.Count);

            // TODO: is there a way to get 'all'?
            //var all = await map.GetAsync().CAF();
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task Events()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            var eventsCount = 0;
            var id = await map.SubscribeAsync(on => on
                .EntryAdded((sender, args) =>
                {
                    HConsole.WriteLine(this, $"! added: {args.Key} {args.Value}");
                    Interlocked.Increment(ref eventsCount);
                }));

            await map.SetAsync("a", 1).CAF();
            await map.SetAsync("b", 2).CAF();

            await AssertEx.SucceedsEventually(() =>
                    Assert.That(eventsCount, Is.EqualTo(2)),
                4000, 500);

            await map.UnsubscribeAsync(id).CAF();

            await map.SetAsync("c", 3).CAF();
            await Task.Delay(500).CAF();

            Assert.AreEqual(2, eventsCount);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task AsyncEvents()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();
            await using var _ = DestroyAndDispose(map);

            var eventsCount = 0;
            var id = await map.SubscribeAsync(on => on
                .EntryAdded(async (sender, args) =>
                {
                    await Task.Yield();
                    HConsole.WriteLine(this, $"! added: {args.Key} {args.Value}");
                    Interlocked.Increment(ref eventsCount);
                }));

            await map.SetAsync("a", 1).CAF();
            await map.SetAsync("b", 2).CAF();

            while (eventsCount < 2)
                await Task.Delay(500).CAF();

            await map.UnsubscribeAsync(id).CAF();

            await map.SetAsync("c", 3).CAF();
            await Task.Delay(500).CAF();

            Assert.AreEqual(2, eventsCount);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task ConfiguredEvents()
        {
            var eventHandled = new SemaphoreSlim(0);

            void ConfigureClient(HazelcastOptions options)
            {
                options.AddSubscriber(handle => handle.ObjectCreated((sender, args) =>
                {
                    HConsole.WriteLine(this, $"! created: {args.ServiceName}/{args.Name}");
                    eventHandled.Release();
                }));
            }

            // here we have to create our own client with a different configuration
            await using var client = await CreateAndStartClientAsync(ConfigureClient).CAF();
            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();

            await eventHandled.WaitAsync();
            await client.DestroyAsync(map);
        }
    }
}