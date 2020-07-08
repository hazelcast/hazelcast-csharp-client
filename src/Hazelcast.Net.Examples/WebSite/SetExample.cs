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
    public class SetExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // create an Hazelcast client and connect to a server running on localhost
            await using var client = new HazelcastClientFactory(BuildExampleOptions(args)).CreateClient();
            await client.OpenAsync();

            // Get the Distributed Set from Cluster.
            await using var set = await client.GetSetAsync<string>("my-distributed-set");
            
            // Add items to the set with duplicates
            await set.AddAsync("item1");
            await set.AddAsync("item1");
            await set.AddAsync("item2");
            await set.AddAsync("item2");
            await set.AddAsync("item2");
            await set.AddAsync("item3");

            // Get the items. Note that there are no duplicates.
            foreach (var item in await set.GetAllAsync())
            {
                Console.WriteLine(item);
            }

            await client.DestroyAsync(set);
        }
    }
}
