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

namespace Hazelcast.Examples.WebSite
{
    // ReSharper disable once UnusedMember.Global
    public class ReplicatedDictionaryExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // create an Hazelcast client and connect to a server running on localhost
            await using var client = HazelcastClientFactory.CreateClient(BuildExampleOptions(args));
            await client.StartAsync();

            // Get a Replicated Map called "my-replicated-map"
            await using var map = await client.GetReplicatedDictionaryAsync<string, string>("my-replicated-map");
            // Put and Get a value from the Replicated Map
            var replacedValue = await map.SetAsync("key", "value"); // key/value replicated to all members
            Console.WriteLine("replacedValue = " + replacedValue); // Will be null as its first update
            var value = await map.GetAsync("key"); // the value is retrieved from a random member in the cluster
            Console.WriteLine("value for key = " + value);
        }
    }
}
