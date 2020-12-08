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
    public class QueueExample : ExampleBase
    {
        public async Task Run(string[] args)
        {
            // creates the example options
            var options = BuildExampleOptions(args);

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get the distributed queue from the cluster
            await using var queue = await client.GetQueueAsync<string>("queue-example");

            var producer = Task.Run(async () =>
            {
                for (var i = 0; i < 100; i++)
                {
                    await queue.OfferAsync("value " + i);
                }
                Console.WriteLine("produced");
            });

            var consumer = Task.Run(async () =>
            {
                var nConsumed = 0;
                string e;
                while (nConsumed++ < 100 && (e = await queue.PollAsync()) != null)
                {
                    Console.WriteLine("Consuming " + e);
                }
                Console.WriteLine("consumed");
            });

            await Task.WhenAll(producer, consumer);

            // destroy the queue
            await client.DestroyAsync(queue);
        }
    }
}
