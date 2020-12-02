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
    public class ListEventsExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // creates the example options
            var options = BuildExampleOptions(args);

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get a distributed list from the cluster
            await using var list = await client.GetListAsync<string>("collection-listener-example");

            var count = 0;

            // subscribe to some events
            await list.SubscribeAsync(handle => handle
                .ItemAdded((sender, args) =>
                {
                    Console.WriteLine("Item added: " + args.Item);
                    count++;
                })
                .ItemRemoved((sender, args) =>
                {
                    Console.WriteLine("Item removed: " + args.Item);
                    count++;
                }));

            await list.AddAsync("item1");
            await list.AddAsync("item2");
            await list.RemoveAsync("item1");

            while (count < 3) await Task.Delay(100);

            // destroy the list
            await client.DestroyAsync(list);
        }
    }
}
