﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

        private ValueTask<IHazelcastClient> CreateOpenClientAsync()
        {
            return CreateOpenClientAsync(configuration =>
            {
                configuration.Networking.Addresses.Add("sgay-l4");
            });
        }

        private async ValueTask<IHazelcastClient> CreateOpenClientAsync(Action<HazelcastConfiguration> configure)
        {
            var client = new HazelcastClientFactory().CreateClient(configure);
            try
            {
                AddDisposable(client);
                await client.OpenAsync().CAF();
            }
            catch
            {
                await client.DisposeAsync().CAF();
                RemoveDisposable(client);
                throw;
            }
            return client;
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task AddOrReplace()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync().CAF();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();

            // AddOrReplace adds a new value, or replaces an existing value,
            // and does not return anything
            // NOTE: no way to know whether it added or replaced?

            await map.AddOrReplaceAsync("key", 42).CAF();

            await map.AddOrReplaceAsync("key", 43).CAF();

            var value = await map.GetAsync("key").CAF();
            Assert.AreEqual(43, value);

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task AddOrReplaceWithValue()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync().CAF();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();

            // AddOrReplace adds a new value, or replaces an existing value,
            // and returns the existing value, or the default value
            // NOTE: no way to know if the default value existed (eg zero)?

            var result1 = await map.AddOrReplaceWithValueAsync("key", 42).CAF();
            Assert.AreEqual(0, result1);

            var result2 = await map.AddOrReplaceWithValueAsync("key", 43).CAF();
            Assert.AreEqual(42, result2);

            var value = await map.GetAsync("key").CAF();
            Assert.AreEqual(43, value);

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task AddIfMissing()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync().CAF();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();

            // TryAdd adds a new value if no value exists already,
            // and returns the existing value, or the default value
            // NOTE: no way to know if the default value existed (eg zero)?

            await map.AddOrReplaceWithValueAsync("key1", 42).CAF();

            var result1 = await map.AddIfMissingAsync("key1", 43).CAF();
            Assert.AreEqual(42, result1);

            var result2 = await map.AddIfMissingAsync("key2", 43).CAF();
            Assert.AreEqual(0, result2);

            var value1 = await map.GetAsync("key1").CAF();
            Assert.AreEqual(42, value1);

            var value2 = await map.GetAsync("key2").CAF();
            Assert.AreEqual(43, value2);

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(2, count);
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task AddOrReplaceMany()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync().CAF();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();

            // AddOrReplace adds new values, or replaces existing values
            // NOTE: no way to know what happened

            await map.AddOrReplaceWithValueAsync("key1", 42).CAF();

            var value1 = await map.GetAsync("key1").CAF();
            Assert.AreEqual(42, value1);

            await map.AddOrReplaceAsync(new Dictionary<string, int>
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
        [Timeout(TimeoutMilliseconds)]
        public async Task ReplaceByKey()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync().CAF();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();

            // Replace replaces an existing value, and returns the existing value,
            // else does nothing if no value exists already (does not add)

            await map.AddOrReplaceAsync("key1", 42).CAF();

            var result1 = await map.ReplaceAsync("key1", 43).CAF();
            Assert.AreEqual(42, result1);

            var result2 = await map.ReplaceAsync("key2", 43).CAF();
            Assert.AreEqual(0, result2);

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task ReplaceByKeyAndValue()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync().CAF();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();

            // Replace replaces an existing value, and returns the existing value,
            // else does nothing if no value exists already (does not add)

            await map.AddOrReplaceAsync("key1", 42).CAF();

            var result1 = await map.ReplaceAsync("key1", 43, 44).CAF();
            Assert.IsFalse(result1);

            var result2 = await map.ReplaceAsync("key1", 42, 44).CAF();
            Assert.IsTrue(result2);

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(1, count);
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task AddOrReplaceWithTimeToLive()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync().CAF();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();

            // AddOrReplace adds a new value, or replaces an existing value,
            // and does not return anything
            // NOTE: no way to know whether it added or replaced?

            await map.AddOrReplaceTtlAsync("key", 42, TimeSpan.FromSeconds(1)).CAF();
            var value = await map.GetAsync("key").CAF();
            Assert.AreEqual(42, value);

            await Task.Delay(1000); // wait for 1 second

            value = await map.GetAsync("key").CAF();
            Assert.AreEqual(0, value); // zero vs missing?

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(0, count);
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task AddOrReplaceTransient()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync().CAF();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();

            // AddTransient adds a new value, or replaces an existing value,
            // and does not return anything
            // (so it's "add or replace" really)
            // NOTE: no way to know whether it added or replaced?

            await map.AddOrReplaceTransientAsync("key", 42, TimeToLive.InfiniteTimeSpan).CAF();

            await map.AddOrReplaceTransientAsync("key", 43, TimeToLive.InfiniteTimeSpan).CAF();

            await map.AddOrReplaceTransientAsync("key1", 43, TimeToLive.InfiniteTimeSpan).CAF();

            var value = await map.GetAsync("key").CAF();
            Assert.AreEqual(43, value);

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(2, count);
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task TryAddOrReplace()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync().CAF();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();

            // TryAddOrReplace is like AddOrReplace but with a timeout

            await map.TryAddOrReplaceAsync("key", 42, TimeSpan.FromSeconds(1)).CAF();
            var value = await map.GetAsync("key").CAF();
            Assert.AreEqual(42, value);

            await map.TryAddOrReplaceAsync("key", 43, TimeSpan.FromSeconds(1)).CAF();
            value = await map.GetAsync("key").CAF();
            Assert.AreEqual(43, value);

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(1, count);

        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task Clear()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync().CAF();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();

            var entries = new Dictionary<string, int>();
            for (var i = 0; i < 100; i++)
                entries["key" + i] = i;

            await map.AddOrReplaceAsync(entries).CAF();

            var count = await map.CountAsync().CAF();
            Assert.AreEqual(100, count);

            await map.ClearAsync().CAF();

            count = await map.CountAsync().CAF();
            Assert.AreEqual(0, count);
        }

        [Test]
        [Timeout(TimeoutMilliseconds)]
        public async Task GetAll()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync().CAF();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();

            var entries = new Dictionary<string, int>();
            for (var i = 0; i < 100; i++)
                entries["key" + i] = i;

            await map.AddOrReplaceAsync(entries).CAF();

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
        [Timeout(TimeoutMilliseconds)]
        public async Task Events()
        {
            TestSetUp();

            var client = await CreateOpenClientAsync().CAF();

            var map = await client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();

            var eventsCount = 0;
            var id = await map.SubscribeAsync(on => on
                .EntryAdded((sender, args) =>
                {
                    XConsole.WriteLine(this, $"! added: {args.Key} {args.Value}");
                    Interlocked.Increment(ref eventsCount);
                }));

            await map.AddOrReplaceWithValueAsync("a", 1).CAF();
            await map.AddOrReplaceWithValueAsync("b", 2).CAF();

            while (eventsCount < 2)
                await Task.Delay(500).CAF();

            await map.UnsubscribeAsync(id).CAF();

            await map.AddOrReplaceWithValueAsync("c", 3).CAF();
            await Task.Delay(500).CAF();

            Assert.AreEqual(2, eventsCount);
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
                    }), CancellationToken.None);
                });
            }

            var client = await CreateOpenClientAsync(ConfigureClient).CAF();

            _ = await client.GetMapAsync<string, int>("map_" + CreateUniqueName()).CAF();

            while (eventsCount < 1)
                await Task.Delay(500).CAF();
        }
    }
}