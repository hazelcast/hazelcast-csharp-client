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
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Logging;
using Hazelcast.Networking;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests
{
    [TestFixture]
    public class MapTests : HazelcastTestBase
    {
        public void TestSetUp()
        {
            XConsole.Configure(this, config => config.SetIndent(0).SetPrefix("TEST"));
            XConsole.Configure<SocketConnectionBase>(config => config.SetMaxLevel(0)); // 1: logs bytes
            XConsole.Configure<Client>(config => config.SetMaxLevel(1)); // 1: logs message & frames
        }

        private async ValueTask<IHazelcastClient> CreateOpenClientAsync()
        {
            var client = new HazelcastClientFactory().CreateClient(configuration =>
            {
                configuration.Networking.Addresses.Add("sgay-l4");
            });
            _asyncDisposables.Add(client);
            await client.OpenAsync();
            return client;
        }

        private async ValueTask<IHazelcastClient> CreateOpenClientAsync(Action<HazelcastConfiguration> configure)
        {
            var client = new HazelcastClientFactory().CreateClient(configure);
            _asyncDisposables.Add(client);
            await client.OpenAsync();
            return client;
        }

        [TearDown]
        public async Task TearDown()
        {
            var success = true;
            foreach (var asyncDisposable in _asyncDisposables)
            {
                try
                {
                    await asyncDisposable.DisposeAsync();
                }
                catch (Exception e)
                {
                    // FIXME LOG + FAIL!
                    success = false;
                }
            }

            _asyncDisposables.Clear();

            // FIXME: better throw an aggregate exception
            if (!success) throw new Exception("Failed to dispose IAsyncDisposable items.");
        }

        private readonly List<IAsyncDisposable> _asyncDisposables = new List<IAsyncDisposable>();

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task AddOrReplace()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName());

            // AddOrReplace adds a new value, or replaces an existing value,
            // and does not return anything
            // NOTE: no way to know whether it added or replaced?

            await map.AddOrReplaceAsync("key", 42);

            await map.AddOrReplaceAsync("key", 43);

            var value = await map.GetAsync("key");
            NUnit.Framework.Assert.AreEqual(43, value);

            var count = await map.CountAsync();
            NUnit.Framework.Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task AddOrReplaceWithValue()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName());

            // AddOrReplace adds a new value, or replaces an existing value,
            // and returns the existing value, or the default value
            // NOTE: no way to know if the default value existed (eg zero)?

            var result1 = await map.AddOrReplaceWithValueAsync("key", 42);
            NUnit.Framework.Assert.AreEqual(0, result1);

            var result2 = await map.AddOrReplaceWithValueAsync("key", 43);
            NUnit.Framework.Assert.AreEqual(42, result2);

            var value = await map.GetAsync("key");
            NUnit.Framework.Assert.AreEqual(43, value);

            var count = await map.CountAsync();
            NUnit.Framework.Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task AddIfMissing()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName());

            // TryAdd adds a new value if no value exists already,
            // and returns the existing value, or the default value
            // NOTE: no way to know if the default value existed (eg zero)?

            await map.AddOrReplaceWithValueAsync("key1", 42);

            var result1 = await map.AddIfMissingAsync("key1", 43);
            NUnit.Framework.Assert.AreEqual(42, result1);

            var result2 = await map.AddIfMissingAsync("key2", 43);
            NUnit.Framework.Assert.AreEqual(0, result2);

            var value1 = await map.GetAsync("key1");
            NUnit.Framework.Assert.AreEqual(42, value1);

            var value2 = await map.GetAsync("key2");
            NUnit.Framework.Assert.AreEqual(43, value2);

            var count = await map.CountAsync();
            NUnit.Framework.Assert.AreEqual(2, count);
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task AddOrReplaceMany()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName());

            // AddOrReplace adds new values, or replaces existing values
            // NOTE: no way to know what happened

            await map.AddOrReplaceWithValueAsync("key1", 42);

            var value1 = await map.GetAsync("key1");
            NUnit.Framework.Assert.AreEqual(42, value1);

            await map.AddOrReplaceAsync(new Dictionary<string, int>
            {
                ["key1"] = 43,
                ["key2"] = 44
            });

            value1 = await map.GetAsync("key1");
            NUnit.Framework.Assert.AreEqual(43, value1);

            var value2 = await map.GetAsync("key2");
            NUnit.Framework.Assert.AreEqual(44, value2);

            var count = await map.CountAsync();
            NUnit.Framework.Assert.AreEqual(2, count);
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task ReplaceByKey()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName());

            // Replace replaces an existing value, and returns the existing value,
            // else does nothing if no value exists already (does not add)

            await map.AddOrReplaceAsync("key1", 42);

            var result1 = await map.ReplaceAsync("key1", 43);
            NUnit.Framework.Assert.AreEqual(42, result1);

            var result2 = await map.ReplaceAsync("key2", 43);
            NUnit.Framework.Assert.AreEqual(0, result2);

            var count = await map.CountAsync();
            NUnit.Framework.Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task ReplaceByKeyAndValue()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName());

            // Replace replaces an existing value, and returns the existing value,
            // else does nothing if no value exists already (does not add)

            await map.AddOrReplaceAsync("key1", 42);

            var result1 = await map.ReplaceAsync("key1", 43, 44);
            NUnit.Framework.Assert.IsFalse(result1);

            var result2 = await map.ReplaceAsync("key1", 42, 44);
            NUnit.Framework.Assert.IsTrue(result2);

            var count = await map.CountAsync();
            NUnit.Framework.Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task AddOrReplaceWithTimeToLive()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName());

            // AddOrReplace adds a new value, or replaces an existing value,
            // and does not return anything
            // NOTE: no way to know whether it added or replaced?

            await map.AddOrReplaceAsync("key", 42, TimeSpan.FromSeconds(1));
            var value = await map.GetAsync("key");
            NUnit.Framework.Assert.AreEqual(42, value);

            await Task.Delay(1000); // wait for 1 second

            value = await map.GetAsync("key");
            NUnit.Framework.Assert.AreEqual(0, value); // zero vs missing?

            var count = await map.CountAsync();
            NUnit.Framework.Assert.AreEqual(0, count);
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task AddOrReplaceTransient()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName());

            // AddTransient adds a new value, or replaces an existing value,
            // and does not return anything
            // (so it's "add or replace" really)
            // NOTE: no way to know whether it added or replaced?

            await map.AddOrReplaceTransientAsync("key", 42, Timeout.InfiniteTimeSpan);

            await map.AddOrReplaceTransientAsync("key", 43, Timeout.InfiniteTimeSpan);

            await map.AddOrReplaceTransientAsync("key1", 43, Timeout.InfiniteTimeSpan);

            var value = await map.GetAsync("key");
            NUnit.Framework.Assert.AreEqual(43, value);

            var count = await map.CountAsync();
            NUnit.Framework.Assert.AreEqual(2, count);
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task TryAddOrReplace()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName());

            // TryAddOrReplace is like AddOrReplace but with a timeout

            await map.TryAddOrReplaceAsync("key", 42, TimeSpan.FromSeconds(1));
            var value = await map.GetAsync("key");
            NUnit.Framework.Assert.AreEqual(42, value);

            await map.TryAddOrReplaceAsync("key", 43, TimeSpan.FromSeconds(1));
            value = await map.GetAsync("key");
            NUnit.Framework.Assert.AreEqual(43, value);

            var count = await map.CountAsync();
            NUnit.Framework.Assert.AreEqual(1, count);

        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task Clear()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName());

            var entries = new Dictionary<string, int>();
            for (var i = 0; i < 100; i++)
                entries["key" + i] = i;

            await map.AddOrReplaceAsync(entries);

            var count = await map.CountAsync();
            NUnit.Framework.Assert.AreEqual(100, count);

            await map.ClearAsync();

            count = await map.CountAsync();
            NUnit.Framework.Assert.AreEqual(0, count);
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task GetAll()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName());

            var entries = new Dictionary<string, int>();
            for (var i = 0; i < 100; i++)
                entries["key" + i] = i;

            await map.AddOrReplaceAsync(entries);

            var count = await map.CountAsync();
            NUnit.Framework.Assert.AreEqual(100, count);

            var keys = await map.GetKeysAsync();
            NUnit.Framework.Assert.AreEqual(100, keys.Count);

            var s = new HashSet<int>();
            for (var i = 0; i < 100; i++)
                s.Add(i);

            foreach (var key in keys)
                NUnit.Framework.Assert.IsTrue(s.Remove(int.Parse(key.Substring(3))));

            NUnit.Framework.Assert.AreEqual(0, s.Count);

            var values = await map.GetValuesAsync();
            NUnit.Framework.Assert.AreEqual(100, values.Count);

            s = new HashSet<int>();
            for (var i = 0; i < 100; i++)
                s.Add(i);

            foreach (var value in values)
                NUnit.Framework.Assert.IsTrue(s.Remove(value));

            NUnit.Framework.Assert.AreEqual(0, s.Count);

            // FIXME how can we get everything?
            //var all = await map.GetAsync();
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task Events()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName());

            var eventsCount = 0;
            var id = await map.SubscribeAsync(on => on
                .EntryAdded((sender, args) =>
                {
                    XConsole.WriteLine(this, $"! added: {args.Key} {args.Value}");
                    Interlocked.Increment(ref eventsCount);
                }));

            await map.AddOrReplaceWithValueAsync("a", 1);
            await map.AddOrReplaceWithValueAsync("b", 2);

            await map.UnsubscribeAsync(id);

            while (eventsCount < 2)
                await Task.Delay(500);

            await map.AddOrReplaceWithValueAsync("c", 3);
            await Task.Delay(500);

            NUnit.Framework.Assert.AreEqual(2, eventsCount);

            // FIXME how are we supposed to release it all?
            //client.Close();
            //client.Dispose();
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task ConfiguredEvents()
        {
            TestSetUp();

            var eventsCount = 0;

            void ConfigureClient(HazelcastConfiguration config)
            {
                config.Networking.Addresses.Add("sgay-l4");

                config.Cluster.AddEventSubscriber(async cluster =>
                {
                    await cluster.SubscribeAsync(on => on.ObjectCreated((sender, args) =>
                    {
                        XConsole.WriteLine(this, $"! created: {args.ServiceName}/{args.Name}");
                        Interlocked.Increment(ref eventsCount);
                    }));
                });
            }

            var client = await CreateOpenClientAsync(ConfigureClient);

            _ = await client.GetMapAsync<string, int>("map_" + CreateUniqueName());

            while (eventsCount < 1)
                await Task.Delay(500);
        }
    }
}