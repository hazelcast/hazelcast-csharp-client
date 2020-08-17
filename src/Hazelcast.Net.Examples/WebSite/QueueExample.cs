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
    public class QueueExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // create an Hazelcast client and connect to a server running on localhost
            await using var client = new HazelcastClientFactory(BuildExampleOptions(args)).CreateClient();
            await client.StartAsync();

            // Get a Blocking Queue called "my-distributed-queue"
            var queue = await client.GetQueueAsync<string>("my-distributed-queue");
            // Offer a String into the Distributed Queue
            await queue.EnqueueAsync("item");
            // Poll the Distributed Queue and return the String
            await queue.TryDequeueAsync();
            //Timed blocking Operations
            await queue.TryEnqueueAsync("anotheritem", TimeSpan.FromMilliseconds(500));
            await queue.TryDequeueAsync(TimeSpan.FromSeconds(5));
            //Indefinitely blocking Operations
            await queue.EnqueueAsync("yetanotheritem");
            Console.WriteLine(await queue.DequeueAsync(true));
        }
    }
}
