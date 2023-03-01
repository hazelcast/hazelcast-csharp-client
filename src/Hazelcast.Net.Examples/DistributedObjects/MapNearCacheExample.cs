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
using System.Diagnostics;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.NearCaching;

namespace Hazelcast.Examples.DistributedObjects
{
    // ReSharper disable once UnusedMember.Global
    internal class MapNearCacheExample
    {
        public static async Task Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .Build();

            // configure NearCache
            options.NearCaches["nearcache-map-*"] = new NearCacheOptions
            {
                MaxSize = 1000,
                InvalidateOnChange = true,
                EvictionPolicy = EvictionPolicy.Lru,
                InMemoryFormat = InMemoryFormat.Binary
            };

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get the distributed map from the cluster
            await using var map = await client.GetMapAsync<string, string>("nearcache-map-1");

            // add values
            for (var i = 0; i < 1000; i++)
                await map.SetAsync("key" + i, "value" + i);

            // get values, first pass
            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < 1000; i++)
                await map.GetAsync("key" + i);
            Console.WriteLine("Got values in " + sw.ElapsedMilliseconds + " millis");

            // get values, second pass
            sw.Restart();
            for (var i = 0; i < 1000; i++)
                await map.GetAsync("key" + i);
            Console.WriteLine("Got cached values in " + sw.ElapsedMilliseconds + " millis");

            // destroy the map
            await client.DestroyAsync(map);
        }
    }
}
