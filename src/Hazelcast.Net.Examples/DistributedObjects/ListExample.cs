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

namespace Hazelcast.Examples.DistributedObjects
{
    // ReSharper disable once UnusedMember.Global
    public class ListExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // creates the example options
            var options = BuildExampleOptions(args);

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = new HazelcastClientFactory(options).CreateClient();
            await client.StartAsync();

            // get the distributed map from the cluster
            await using var list = await client.GetListAsync<string>("list-example");

            await list.AddAsync("item1");
            await list.AddAsync("item2");
            await list.AddAsync("item3");

            Console.WriteLine("Get: " + await list.GetAsync(0));

            Console.WriteLine("All: " + string.Join(", ", await list.GetAllAsync()));

            Console.WriteLine("Contains: " + await list.ContainsAsync("item2"));

            Console.WriteLine("Count: " + await list.CountAsync());

            Console.WriteLine("Sublist: " + string.Join(", ", await list.GetRangeAsync(0, 2)));

            // destroy the list
            await client.DestroyAsync(list);
        }
    }
}
