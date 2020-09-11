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
using System.Threading.Tasks;

namespace Hazelcast.Examples.DistributedObjects
{
    // ReSharper disable once UnusedMember.Global
    public class DictionarySimpleExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // creates the example options
            var options = BuildExampleOptions(args);

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = HazelcastClientFactory.CreateClient(options);
            await client.StartAsync();

            // get the distributed map from the cluster
            await using var map = await client.GetDictionaryAsync<string, string>("simple-example");

            // add values
            await map.SetAsync("key", "value");
            await map.SetAsync("key2", "value2");

            // get values, count, etc...
            Console.WriteLine("Key: " + await map.GetAsync("key"));
            Console.WriteLine("Values: " + string.Join(", ", await map.GetValuesAsync()));
            Console.WriteLine("Keys: " + string.Join(", ", await map.GetKeysAsync()));
            Console.WriteLine("Count: " + await map.CountAsync());

            Console.WriteLine("Entries: " + string.Join(", ", await map.GetAsync()));
            Console.WriteLine("ContainsKey: " + await map.ContainsKeyAsync("key"));
            Console.WriteLine("ContainsValue: " + await map.ContainsAsync("value"));

            // destroy the map
            await client.DestroyAsync(map);
        }
    }
}
