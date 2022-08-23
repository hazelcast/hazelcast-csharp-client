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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Serialization;
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
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            // Set adds a new value, or replaces an existing value,
            // and does not return anything
            // NOTE: no way to know whether it added or replaced?

            await map.SetAsync("key", 42).CfAwait();
            await map.SetAsync("key", 43).CfAwait();

            var value = await map.GetAsync("key").CfAwait();
            Assert.AreEqual(43, value);

            var count = await map.GetSizeAsync().CfAwait();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task SetWithTimeout()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            // AddOrReplace adds a new value, or replaces an existing value,
            // and does not return anything
            // NOTE: no way to know whether it added or replaced?

            await map.SetAsync("key", 42).CfAwait();

            // add-or-replace with a 3 seconds timeout and a potential TaskTimeoutException
            // in case of a TaskTimeoutException - map.SetAsync() will keep running in the
            // background, its exception will be observed, but the end result (whether the
            // value was actually set or not) is unspecified.
            await map.SetAsync("key", 43).CfAwait(TimeSpan.FromSeconds(12));

            var value = await map.GetAsync("key").CfAwait();
            Assert.AreEqual(43, value);

            var count = await map.GetSizeAsync().CfAwait();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task GetMissingValValue()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            Assert.That(await map.GetAsync("key"), Is.Zero);

            await map.SetAsync("key", default);

            Assert.That(await map.GetAsync("key"), Is.Zero);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task GetMissingNullableValue()
        {
            var map = await Client.GetMapAsync<string, int?>("map_" + CreateUniqueName()).CfAwait();
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
            var map = await Client.GetMapAsync<string, string>("map_" + CreateUniqueName()).CfAwait();
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
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            // AddOrReplace adds a new value, or replaces an existing value,
            // and returns the existing value, or the default value
            // NOTE: no way to know if the default value existed (eg zero)?

            Assert.That(await map.PutAsync("key", 42), Is.Zero);

            Assert.That(await map.PutAsync("key", 43), Is.EqualTo(42));

            Assert.That(await map.GetAsync("key"), Is.EqualTo(43));

            var count = await map.GetSizeAsync().CfAwait();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task GetAndSetNullable()
        {
            var map = await Client.GetMapAsync<string, int?>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            // AddOrReplace adds a new value, or replaces an existing value,
            // and returns the existing value, or the default value
            // NOTE: no way to know if the default value existed (eg zero)?

            Assert.That(await map.PutAsync("key", 42), Is.Null);

            Assert.That(await map.PutAsync("key", 43), Is.EqualTo(42));

            Assert.That(await map.GetAsync("key"), Is.EqualTo(43));

            var count = await map.GetSizeAsync().CfAwait();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task Add()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            // TryAdd adds a new value if no value exists already,
            // and returns the existing value, or the default value
            // NOTE: no way to know if the default value existed (eg zero)?

            await map.SetAsync("key1", 42).CfAwait();

            var result1 = await map.PutIfAbsentAsync("key1", 43).CfAwait();
            Assert.That(result1, Is.EqualTo(42));

            var result2 = await map.PutIfAbsentAsync("key2", 43).CfAwait();
            Assert.That(result2, Is.EqualTo(0));

            var value1 = await map.GetAsync("key1").CfAwait();
            Assert.AreEqual(42, value1);

            var value2 = await map.GetAsync("key2").CfAwait();
            Assert.AreEqual(43, value2);

            var count = await map.GetSizeAsync().CfAwait();
            Assert.AreEqual(2, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task SetMany()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            // AddOrReplace adds new values, or replaces existing values
            // NOTE: no way to know what happened

            await map.SetAsync("key1", 42).CfAwait();

            var value1 = await map.GetAsync("key1").CfAwait();
            Assert.AreEqual(42, value1);

            await map.SetAllAsync(new Dictionary<string, int>
            {
                ["key1"] = 43,
                ["key2"] = 44
            });

            value1 = await map.GetAsync("key1").CfAwait();
            Assert.AreEqual(43, value1);

            var value2 = await map.GetAsync("key2").CfAwait();
            Assert.AreEqual(44, value2);

            var count = await map.GetSizeAsync().CfAwait();
            Assert.AreEqual(2, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task ReplaceByKey()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            // Replace replaces an existing value, and returns the existing value,
            // else does nothing if no value exists already (does not add)

            await map.SetAsync("key1", 42).CfAwait();

            var result = await map.ReplaceAsync("key1", 43).CfAwait();
            Assert.That(result, Is.EqualTo(42));

            Assert.That(await map.ReplaceAsync("key2", 43), Is.Zero);

            var count = await map.GetSizeAsync().CfAwait();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task ReplaceNullableByKey()
        {
            var map = await Client.GetMapAsync<string, int?>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            // Replace replaces an existing value, and returns the existing value,
            // else does nothing if no value exists already (does not add)

            await map.SetAsync("key1", 42).CfAwait();

            var result = await map.ReplaceAsync("key1", 43).CfAwait();
            Assert.That(result, Is.EqualTo(42));

            Assert.That(await map.ReplaceAsync("key2", 43), Is.Null);

            var count = await map.GetSizeAsync().CfAwait();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task ReplaceByKeyAndValue()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            // Replace replaces an existing value, and returns the existing value,
            // else does nothing if no value exists already (does not add)

            await map.SetAsync("key1", 42).CfAwait();

            var result1 = await map.ReplaceAsync("key1", 43, 44).CfAwait();
            Assert.IsFalse(result1);

            var result2 = await map.ReplaceAsync("key1", 42, 44).CfAwait();
            Assert.IsTrue(result2);

            var count = await map.GetSizeAsync().CfAwait();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task SetWithTimeToLive()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            // AddOrReplace adds a new value, or replaces an existing value,
            // and does not return anything
            // NOTE: no way to know whether it added or replaced?

            await map.SetAsync("key", 42, TimeSpan.FromSeconds(2)).CfAwait();
            var value = await map.GetAsync("key").CfAwait();
            Assert.AreEqual(42, value);

            await Task.Delay(3000); // wait for 3 second

            Assert.That(await map.GetAsync("key"), Is.Zero);

            var count = await map.GetSizeAsync().CfAwait();
            Assert.AreEqual(0, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task SetTransient()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            // AddTransient adds a new value, or replaces an existing value,
            // and does not return anything
            // (so it's "add or replace" really)
            // NOTE: no way to know whether it added or replaced?

            await map.PutTransientAsync("key", 42, TimeSpan.Zero).CfAwait();

            await map.PutTransientAsync("key", 43, TimeSpan.Zero).CfAwait();

            await map.PutTransientAsync("key1", 43, TimeSpan.Zero).CfAwait();

            var value = await map.GetAsync("key").CfAwait();
            Assert.AreEqual(43, value);

            var count = await map.GetSizeAsync().CfAwait();
            Assert.AreEqual(2, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task TrySet()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            // TryAddOrReplace is like AddOrReplace but with a timeout

            await map.TryPutAsync("key", 42, TimeSpan.FromSeconds(1)).CfAwait();
            var value = await map.GetAsync("key").CfAwait();
            Assert.AreEqual(42, value);

            await map.TryPutAsync("key", 43, TimeSpan.FromSeconds(1)).CfAwait();
            value = await map.GetAsync("key").CfAwait();
            Assert.AreEqual(43, value);

            var count = await map.GetSizeAsync().CfAwait();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task Clear()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            var entries = new Dictionary<string, int>();
            for (var i = 0; i < 100; i++)
                entries["key" + i] = i;

            await map.SetAllAsync(entries).CfAwait();

            var count = await map.GetSizeAsync().CfAwait();
            Assert.AreEqual(100, count);

            await map.ClearAsync().CfAwait();

            count = await map.GetSizeAsync().CfAwait();
            Assert.AreEqual(0, count);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task GetAll()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            var entries = new Dictionary<string, int>();
            for (var i = 0; i < 100; i++)
                entries["key" + i] = i;

            await map.SetAllAsync(entries).CfAwait();

            var count = await map.GetSizeAsync().CfAwait();
            Assert.AreEqual(100, count);

            var keys = await map.GetKeysAsync().CfAwait();
            Assert.AreEqual(100, keys.Count);

            var s = new HashSet<int>();
            for (var i = 0; i < 100; i++)
                s.Add(i);

            foreach (var key in keys)
                Assert.IsTrue(s.Remove(int.Parse(key.Substring(3))));

            Assert.AreEqual(0, s.Count);

            var values = await map.GetValuesAsync().CfAwait();
            Assert.AreEqual(100, values.Count);

            s = new HashSet<int>();
            for (var i = 0; i < 100; i++)
                s.Add(i);

            foreach (var value in values)
                Assert.IsTrue(s.Remove(value));

            Assert.AreEqual(0, s.Count);

            var allEntries = await map.GetEntriesAsync().CfAwait();
            Assert.That(allEntries.Count, Is.EqualTo(100));
            foreach (var (key, value) in allEntries)
                Assert.That(key, Is.EqualTo($"key{value}"));
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task Events()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            var eventsCount = 0;
            var id = await map.SubscribeAsync(on => on
                .EntryAdded((sender, args) =>
                {
                    HConsole.WriteLine(this, $"! added: {args.Key} {args.Value}");
                    Interlocked.Increment(ref eventsCount);
                }));

            await map.SetAsync("a", 1).CfAwait();
            await map.SetAsync("b", 2).CfAwait();

            await AssertEx.SucceedsEventually(() =>
                    Assert.That(eventsCount, Is.EqualTo(2)),
                4000, 500);

            await map.UnsubscribeAsync(id).CfAwait();

            await map.SetAsync("c", 3).CfAwait();
            await Task.Delay(500).CfAwait();

            Assert.AreEqual(2, eventsCount);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task AsyncEvents()
        {
            var map = await Client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            var eventsCount = 0;
            var id = await map.SubscribeAsync(on => on
                .EntryAdded(async (sender, args) =>
                {
                    await Task.Yield();
                    HConsole.WriteLine(this, $"! added: {args.Key} {args.Value}");
                    Interlocked.Increment(ref eventsCount);
                }));

            await map.SetAsync("a", 1).CfAwait();
            await map.SetAsync("b", 2).CfAwait();

            while (eventsCount < 2)
                await Task.Delay(500).CfAwait();

            await map.UnsubscribeAsync(id).CfAwait();

            await map.SetAsync("c", 3).CfAwait();
            await Task.Delay(500).CfAwait();

            Assert.AreEqual(2, eventsCount);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task ConfiguredEvents()
        {
            var eventHandled = new SemaphoreSlim(0);

            void ConfigureClient(HazelcastOptions options)
            {
                options.AddSubscriber(events => events
                    .ObjectCreated((sender, args) =>
                    {
                        HConsole.WriteLine(this, $"! created: {args.ServiceName}/{args.Name}");
                        eventHandled.Release();
                    }));
            }

            // here we have to create our own client with a different configuration
            await using var client = await CreateAndStartClientAsync(ConfigureClient).CfAwait();
            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CfAwait();

            await eventHandled.WaitAsync(2000);
            await client.DestroyAsync(map);
        }

        [Test]
        [Timeout(TestTimeoutMilliseconds)]
        public async Task CustomTypes()
        {
            await using var client = await CreateAndStartClientAsync(options =>
            {
                options.Serialization.Serializers.Add(new SerializerOptions
                {
                    SerializedType = typeof(MyClass),
                    Creator = () => new MyClassSerializer()
                });
            }).CfAwait();

            var map = await client.GetMapAsync<string, MyClass>("map_" + CreateUniqueName()).CfAwait();
            await using var _ = DestroyAndDispose(map);

            await map.SetAsync("key", new MyClass { Name = "value1" }).CfAwait();
            await map.SetAsync("key", new MyClass { Name = "value2" }).CfAwait();

            var value = await map.GetAsync("key").CfAwait();
            Assert.That(value, Is.Not.Null);
            Assert.That(value.Name, Is.EqualTo("value2"));

            var map2 = await client.GetMapAsync<MyClass, string>("map2_" + CreateUniqueName()).CfAwait();
            await using var _2 = DestroyAndDispose(map2);

            await map2.SetAsync(new MyClass { Name = "name1" }, "value1").CfAwait();
            await map2.SetAsync(new MyClass { Name = "name2" }, "value2").CfAwait();
            Assert.That(await map2.GetSizeAsync().CfAwait(), Is.EqualTo(2));

            await map2.SetAsync(new MyClass { Name = "name2" }, "value3").CfAwait();
            Assert.That(await map2.GetSizeAsync().CfAwait(), Is.EqualTo(2));

            var value2 = await map2.GetAsync(new MyClass { Name = "name2" }).CfAwait();
            Assert.That(value2, Is.EqualTo("value3"));
        }

        public class MyClassSerializer : IByteArraySerializer<MyClass>
        {
            public void Dispose()
            { }

            public int TypeId => 665142;

            public MyClass Read(byte[] buffer)
            {
                return new MyClass { Name = Encoding.UTF8.GetString(buffer) };
            }

            public byte[] Write(MyClass obj)
            {
                return Encoding.UTF8.GetBytes(obj.Name);
            }
        }

        public class MyClass
        {
            public string Name { get; set; }
        }
    }
}
