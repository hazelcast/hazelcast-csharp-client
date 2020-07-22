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
    public class ListExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // create an Hazelcast client and connect to a server running on localhost
            await using var client = new HazelcastClientFactory(BuildExampleOptions(args)).CreateClient();
            await client.ConnectAsync();

            // Get the Distributed List from Cluster.
            await using var list = await client.GetListAsync<string>("my-distributed-list");

            // Add elements to the list
            await list.AddAsync("item1");
            await list.AddAsync("item2");

            // Remove the first element
            Console.WriteLine("Removed: " + await list.RemoveAtAsync(0));
            // There is only one element left
            Console.WriteLine("Current size is " + await list.CountAsync());
            // Clear the list
            await list.ClearAsync();

            await client.DestroyAsync(list);
        }
    }
}
