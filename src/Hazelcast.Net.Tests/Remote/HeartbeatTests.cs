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
using System.Threading.Tasks;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    [Explicit("Takes time")]
    public class HeartbeatTests : SingleMemberRemoteTestBase
    {
        private IDisposable HConsoleForTest()

            => HConsole.Capture(options => options
                .ClearAll()
                .Set(x => x.Verbose())
                .Set(this, x => x.SetPrefix("TEST"))
                .Set<AsyncContext>(x => x.Quiet())
                .Set<SocketConnectionBase>(x => x.SetIndent(1).SetLevel(0).SetPrefix("SOCKET")));

        protected override HazelcastOptions CreateHazelcastOptions()
        {
            var keyValues = new Dictionary<string, string>();

            static void AddIfMissing(IDictionary<string, string> d, string k, string v)
            {
                if (!d.ContainsKey(k)) d.Add(k, v);
            }

            // add Microsoft logging configuration
            AddIfMissing(keyValues, "Logging:LogLevel:Default", "Debug");
            AddIfMissing(keyValues, "Logging:LogLevel:System", "Information");
            AddIfMissing(keyValues, "Logging:LogLevel:Microsoft", "Information");

            return HazelcastOptions.Build(
                builder =>
                {
                    builder.AddDefaults(null);
                    builder.AddHazelcast(null);
                    builder.AddInMemoryCollection(keyValues);
                    builder.AddUserSecrets(GetType().Assembly, true);
                },
                (configuration, options) =>
                {
                    options.Networking.Addresses.Clear();
                    options.Networking.Addresses.Add("127.0.0.1:5701");

                    options.ClusterName = RcCluster?.Id ?? options.ClusterName;

                    // configure logging factory and add the console provider
                    options.LoggerFactory.Creator = () =>
                        Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                            builder
                                .AddConfiguration(configuration.GetSection("logging"))
                                .AddHConsole());

                }, ConfigurationSecretsKey);
        }

        [Test]
        public async Task Heartbeat()
        {
            using var _ = HConsoleForTest();

            var options = CreateHazelcastOptions();
            options.Heartbeat.TimeoutMilliseconds = 4_000; // cannot be < period!
            options.Heartbeat.PeriodMilliseconds = 3_000;

            await using var client = await HazelcastClientFactory.StartNewClientAsync(options, CreateAndStartClientTimeout);

            await Task.Delay(3_000);
            Assert.That(client.IsActive);

            await Task.Delay(3_000);
            Assert.That(client.IsActive);

            await Task.Delay(3_000);
            Assert.That(client.IsActive);
        }

        [Test]
        public async Task DemoTest()
        {
            using var _ = HConsoleForTest();

            await new DictionarySimpleExample().Run(CreateHazelcastOptions(), 1000);
        }

        public class DictionarySimpleExample
        {
            public const string CacheName = "simple-example";

            public async Task Run(HazelcastOptions options, int count)
            {
                // create an Hazelcast client and connect to a server running on localhost
                var client = await HazelcastClientFactory.StartNewClientAsync(options);

                // get the distributed map from the cluster
                var map = await client.GetDictionaryAsync<string, string>(CacheName);

                // get the logger
                var logger = options.LoggerFactory.Service.CreateLogger("Demo");

                // loop
                try
                {
                    // add values
                    for (var i = 0; i < 1000; i++)
                    {
                        await map.SetAsync("key-" + i, "value-" + i);

                    }

                    // NOTE
                    // if processing a message that is too big, takes too long, then a heartbeat 'ping'
                    // response may be waiting in some queue and not be processed = timeout! what would
                    // be the correct way to handle this? have a parallel, priority queue of some sort
                    // for these messages, or simply increase timeout?

                    // get values, count, etc...
                    logger.LogDebug("Key: " + await map.GetAsync("key"));
                    logger.LogDebug("Values: " + string.Join(", ", await map.GetValuesAsync()));
                    logger.LogDebug("Keys: " + string.Join(", ", await map.GetKeysAsync()));
                    logger.LogDebug("Count: " + await map.CountAsync());

                    logger.LogDebug("Entries: " + string.Join(", ", await map.GetEntriesAsync()));
                    logger.LogDebug("ContainsKey: " + await map.ContainsKeyAsync("key"));
                    logger.LogDebug("ContainsValue: " + await map.ContainsValueAsync("value"));

                    logger.LogDebug("Press ESC to stop");
                    var x = 0;
                    //while (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
                    while (x < count)
                    {
                        logger.LogDebug($"{x++}: Client Connected:" + client.IsConnected);
                        await map.SetAsync("key1", "Hello, world.");
                    }
                    logger.LogDebug("Exit loop.");
                }
                catch (Exception e)
                {
                    logger.LogError($"Ooops!!!!: '{e}'");
                }

                // destroy the map
                await client.DestroyAsync(map);

                // dispose & close the client
                await client.DisposeAsync();

                logger.LogDebug("Test Completed!");

                // dispose the logger factory = flush
                options.LoggerFactory.Service.Dispose();
            }
        }
    }
}
