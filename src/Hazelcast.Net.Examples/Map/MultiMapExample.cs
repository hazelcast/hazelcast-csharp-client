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
    public class MultiMapExample : ExampleBase
    {
        private static async Task Run(string[] args)
        {
            // creates the example options
            var options = BuildExampleOptions(args);

            // create an Hazelcast client and connect to a server running on localhost
            await using var hz = new HazelcastClientFactory(options).CreateClient();
            await hz.OpenAsync().CAF();

            // get the distributed map from the cluster
            var map = await hz.GetMultiMapAsync<string, string>("multimap-example").CAF();

            // add values
            await map.TryAddAsync("key", "value").CAF();
            await map.TryAddAsync("key", "value2").CAF();
            await map.TryAddAsync("key2", "value3").CAF();

            // report
            Console.WriteLine("Value: " + string.Join(", ", await map.GetAsync("key").CAF()));
            Console.WriteLine("Values : " + string.Join(", ", await map.GetValuesAsync().CAF()));
            Console.WriteLine("Keys: " + string.Join(", ", await map.GetKeysAsync().CAF()));
            Console.WriteLine("Count: " + await map.CountAsync().CAF());
            Console.WriteLine("Entries: " + string.Join(", ", await map.GetAllAsync().CAF()));
            Console.WriteLine("ContainsKey: " + await map.ContainsKeyAsync("key").CAF());
            Console.WriteLine("ContainsValue: " + await map.ContainsValueAsync("value").CAF());

            // destroy the map
            await hz.DestroyAsync(map).CAF();
        }
    }
}
