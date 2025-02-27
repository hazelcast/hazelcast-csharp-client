// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.DistributedObjects.Impl;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Support
{
    // https://github.com/hazelcast/hazelcast-csharp-client/issues/501

    [Support]
    public class Issue501 : SingleMemberRemoteTestBase
    {
        // log to HConsole
        protected override ILoggerFactory CreateLoggerFactory() =>
            Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddHConsole());

        protected override string RcClusterConfiguration => Resources.Cluster_JetEnabled;

        [TestCase(true)]
        [TestCase(false)]
        [Timeout(20_000)]
        public async Task Reproduce(bool includeValues)
        {
            var mapName = "map-" + CreateUniqueName();
            var mapSubscribed = 0;
            var mapEventsCount = 0;
            var mapEventsData = new Dictionary<string, string>();

            HConsole.Configure(options => options
                .ConfigureDefaults(this)
                .Configure(typeof(HMap<,>)).SetMaxLevel().SetPrefix("MAP")
            );

            // this can go in HMap.HandleEntryEvents for troubleshooting
            // HConsole.WriteLine(this, $"!! key:{key?.Value.ToString()} value:{value?.Value.ToString()} old:{oldValue?.Value.ToString()} merge:{mergingValue?.Value.ToString()}");

            var options = new HazelcastOptionsBuilder()
                .With((c, o) =>
                {
                    // our test environment provides a cluster, and we need to configure the client accordingly
                    o.ClusterName = RcCluster.Id;

                    // our cluster lives on localhost
                    o.Networking.Addresses.Clear();
                    o.Networking.Addresses.Add("127.0.0.1:5701");

                    // fail fast, default timeout is infinite
                    o.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 4000;

                    // our test environment provides a logger factory
                    o.LoggerFactory.Creator = () => LoggerFactory;

                    // subscribe
                    o.AddSubscriber(events => events
                        .StateChanged(async (c, a) =>
                        {
                            if (a.State == ClientState.Connected)
                            {
                                if (Interlocked.CompareExchange(ref mapSubscribed, 1, 0) == 1) return; // only once!
                                HConsole.WriteLine(this, "State == Connected");

                                var m = await c.GetMapAsync<string, string>(mapName).ConfigureAwait(false);
                                await m.SubscribeAsync(mapEvents => mapEvents
                                    .EntryRemoved((xm, xa) =>
                                    {
                                        HConsole.WriteLine(this, $"Removed entry key:\"{xa.Key}\" value:\"{xa.OldValue}\"");
                                        mapEventsData.Add(xa.Key, xa.OldValue);
                                        Interlocked.Increment(ref mapEventsCount);
                                    }), includeValues: includeValues).ConfigureAwait(false);
                            }
                        }));
                })
                .Build();

            HConsole.WriteLine(this, "Start new client...");
            var client = await HazelcastClientFactory.StartNewClientAsync(options).ConfigureAwait(false);

            HConsole.WriteLine(this, "Get map...");
            var map = await client.GetMapAsync<string, string>(mapName).ConfigureAwait(false);

            HConsole.WriteLine(this, "Add entries...");
            const int entriesCount = 12;
            for (var i = 0; i < entriesCount; i++)
            {
                await map.SetAsync($"key-{i}", $"value-{i}").ConfigureAwait(false);
            }

            HConsole.WriteLine(this, "Remove entries...");
            for (var i = 0; i < entriesCount; i++)
            {
                var value = await map.RemoveAsync($"key-{i}").ConfigureAwait(false);
                Assert.That(value, Is.EqualTo($"value-{i}"));
            }

            // eventually, the event count will match
            HConsole.WriteLine(this, "Count events...");
            await AssertEx.SucceedsEventually(() => Assert.That(mapEventsCount, Is.EqualTo(entriesCount)), 4000, 200).ConfigureAwait(false);

            // validate event args
            for (var i = 0; i < entriesCount; i++)
            {
                Assert.That(mapEventsData.TryGetValue($"key-{i}", out var value), Is.True);
                if (includeValues)
                    Assert.That(value, Is.EqualTo($"value-{i}")); // <-- this! failed before fix in MapEntryRemovedEventHandler
                else
                    Assert.That(value, Is.Null);
            }

            HConsole.WriteLine(this, "Success!");
        }
    }
}
