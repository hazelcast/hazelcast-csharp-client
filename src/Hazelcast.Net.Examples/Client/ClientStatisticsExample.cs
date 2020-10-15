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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.NearCaching;

namespace Hazelcast.Examples.Client
{
    // ReSharper disable once UnusedMember.Global
    public class ClientStatisticsExample
    {
        public static async Task Run()
        {
            // enable client statistics
            // set the statistics send period, default value is 3 seconds
            Environment.SetEnvironmentVariable("hazelcast.client.statistics.enabled", "true");
            Environment.SetEnvironmentVariable("hazelcast.client.statistics.period.seconds", "3");

            // create an Hazelcast client and connect to a server running on localhost
            var options = HazelcastOptions.Build();
            options.NearCache.Configurations["myMap"] = new NearCacheNamedOptions
            {
                MaxSize = 1000,
                InvalidateOnChange = true,
                EvictionPolicy = EvictionPolicy.Lru,
                InMemoryFormat = InMemoryFormat.Binary
            };
            var hz = await HazelcastClientFactory.StartNewClientAsync(options);

            // get a map
            var map = await hz.GetDictionaryAsync<string, string>("myMap");

            // generate stats
            for (var i = 0; i < 100000; i++)
            {
                await map.SetAsync("key-" + i, "value-" + i);
                Thread.Sleep(500);
            }

            // is that all?
            // "after client connected you can use Management Center to visualize client statistics"

            // terminate the client
            await hz.DisposeAsync();
        }
    }
}
