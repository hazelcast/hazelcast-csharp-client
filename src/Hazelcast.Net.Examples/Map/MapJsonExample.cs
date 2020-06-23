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
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.Examples.Map
{
    // ReSharper disable once UnusedMember.Global
    public class MapJsonExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            //HConsole.Configure<object>(config => config.SetMaxLevel(2));
            HConsole.Configure<AsyncContext>(config => config.SetMaxLevel(-1));

            // creates the example options
            var options = BuildExampleOptions(args);

            // create an Hazelcast client and connect to a server running on localhost
            await using var hz = new HazelcastClientFactory(options).CreateClient();
            await hz.OpenAsync().CAF();

            // get the distributed map from the cluster
            var map = await hz.GetMapAsync<string, HazelcastJsonValue>("json-example").CAF();

            // add values
            Console.WriteLine("Populate map");
            await map.AddOrReplaceAsync("item1", new HazelcastJsonValue("{ \"age\": 4 }")).CAF();
            await map.AddOrReplaceAsync("item2", new HazelcastJsonValue("{ \"age\": 20 }")).CAF();

            // count
            Console.WriteLine("Count");
            Console.WriteLine($"{await map.CountAsync().CAF()} entries");

            // get all
            Console.WriteLine("List");
            var entries = await map.GetAsync().CAF();
            foreach (var (key, value) in entries)
                Console.WriteLine($"[{key}]: {value}");

            // read
            Console.WriteLine("Query");
            var values = await map.GetValuesAsync(Predicates.Predicate.IsLessThan("age", 6)).CAF();
            Console.WriteLine($"Retrieved {values.Count} entries with 'age < 6'.");
            foreach (var value in values)
                Console.WriteLine($"Entry value: {value}");

            // destroy the map
            await hz.DestroyAsync(map).CAF();
        }
    }
}
