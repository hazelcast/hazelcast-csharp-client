// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.Examples.DistributedObjects
{
    // ReSharper disable once UnusedMember.Global
    public class MapJsonExample : ExampleBase
    {
        public async Task Run(string[] args)
        {
            // creates the example options
            var options = BuildExampleOptions(args);

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get the distributed map from the cluster
            await using var map = await client.GetMapAsync<string, HazelcastJsonValue>("json-example");

            // add values
            Console.WriteLine("Populate map");
            await map.SetAsync("item1", new HazelcastJsonValue("{ \"age\": 4 }"));
            await map.SetAsync("item2", new HazelcastJsonValue("{ \"age\": 20 }"));

            // count
            Console.WriteLine("Count");
            Console.WriteLine($"{await map.GetSizeAsync()} entries");

            // get all
            Console.WriteLine("List");
            var entries = await map.GetEntriesAsync();
#if NETCOREAPP
            foreach (var (key, value) in entries)
                Console.WriteLine($"[{key}]: {value}");
#else
            foreach (var kvp in entries)
                Console.WriteLine($"[{kvp.Key}]: {kvp.Value}");
#endif

            // read
            Console.WriteLine("Query");
            var values = await map.GetValuesAsync(Query.Predicates.LessThan("age", 6));
            Console.WriteLine($"Retrieved {values.Count} entries with 'age < 6'.");
            foreach (var value in values)
                Console.WriteLine($"Entry value: {value}");

            // destroy the map
            await client.DestroyAsync(map);
        }
    }
}
