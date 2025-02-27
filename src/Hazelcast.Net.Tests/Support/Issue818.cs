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
#define NOT_SYNC_CLIENT

// define CFAWAIT_CLIENT
#define NOT_CFAWAIT_CLIENT

// define SYNC_MAP
#define SYNC_MAP

using System;
using System.Threading.Tasks;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Support;

[TestFixture]
public class Issue818 : SingleMemberClientRemoteTestBase
{
    [Test]
    [Timeout(30_000)]
    public async Task Reproduce()
    {
        var options = new HazelcastOptionsBuilder()
            //.With((config, options) => options.Networking.Addresses.Add("127.0.0.1"))
            //.With((config, options) => options.Networking.ReconnectMode = ReconnectMode.ReconnectSync)
            //.With((config, options) => options.Networking.SmartRouting = true)
            //.With((config, options) => options.Networking.ShuffleAddresses = true)
            //.With((config, options) => options.Networking.RedoOperations = true)
            .With(o =>
            {
                o.Networking.Addresses.Add("127.0.0.1");
                o.Networking.ReconnectMode = ReconnectMode.ReconnectSync;
                o.Networking.RoutingMode.Mode = RoutingModes.AllMembers;
                o.Networking.ShuffleAddresses = true;
                o.Networking.RedoOperations = true;

                o.ClusterName = RcCluster.Id;

                o.LoggerFactory.Creator = () => Microsoft.Extensions.Logging.LoggerFactory.Create(loggingBuilder =>
                    loggingBuilder
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddConsole());
            })
            .Build();

#if SYNC_CLIENT
        var client = HazelcastClientFactory.StartNewClientAsync(options).Result;
#else
#if CFAWAIT_CLIENT
        var client = await HazelcastClientFactory.StartNewClientAsync(options).ConfigureAwait(false);
#else
        var client = await HazelcastClientFactory.StartNewClientAsync(options);
#endif
#endif

        var mapTask = client.GetMapAsync<string, string>("map-name");

        //while (!mapTask.IsCompleted)
        //{
        //    Console.WriteLine("waiting for map");
        //}

#if SYNC_MAP
        var map = mapTask.Result;
#else
        var map = await mapTask;
#endif

        await map.PutAsync("key", "value");

        Console.WriteLine("map [\"key\"] = " + await map.GetAsync("key"));
    }
}