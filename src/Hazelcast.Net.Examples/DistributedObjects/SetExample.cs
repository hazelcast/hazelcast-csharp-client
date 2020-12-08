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
    public class SetExample : ExampleBase
    {
        public async Task Run(string[] args)
        {
            // creates the example options
            var options = BuildExampleOptions(args);

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get the distributed set from the cluster
            await using var set = await client.GetSetAsync<string>("set-example");

            await set.AddAsync("item1");
            await set.AddAsync("item2");
            await set.AddAsync("item3");
            await set.AddAsync("item3");

            Console.WriteLine("All: " + string.Join(", ", await set.GetAllAsync()));

            Console.WriteLine("Contains: " + await set.ContainsAsync("item2"));

            Console.WriteLine("Count: " + await set.GetSizeAsync());

            // destroy the set
            await client.DestroyAsync(set);
        }
    }
}
