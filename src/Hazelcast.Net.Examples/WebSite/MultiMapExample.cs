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

namespace Hazelcast.Examples.WebSite
{
    // ReSharper disable once UnusedMember.Global
    public class DistributedMultiMapExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // create an Hazelcast client and connect to a server running on localhost
            await using var client = new HazelcastClientFactory(BuildExampleOptions(args)).CreateClient();
            await client.StartAsync();

            // Get the Distributed MultiMap from Cluster.
            await using var multiMap = await client.GetMultiMapAsync<string, string>("my-distributed-multimap");
            // Put values in the map against the same key
            await multiMap.TryAddAsync("my-key", "value1");
            await multiMap.TryAddAsync("my-key", "value2");
            await multiMap.TryAddAsync("my-key", "value3");
            // Print out all the values for associated with key called "my-key"
            var values = await multiMap.GetAsync("my-key");
            foreach (var item in values)
            {
                Console.WriteLine(item);
            }

            // remove specific key/value pair
            await multiMap.RemoveAsync("my-key", "value2");
        }
    }
}
