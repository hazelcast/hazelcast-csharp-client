// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Examples.DistributedObjects
{
    // ReSharper disable once UnusedMember.Global
    public class TopicExample
    {
        public static async Task Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .Build();

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get distributed topic from cluster
            await using var topic = await client.GetTopicAsync<string>("topic-example");

            var count = 100;
            var counted = new SemaphoreSlim(0);

            // subscribe to event
            await topic.SubscribeAsync(on => on
                .Message((sender, args) =>
                {
                    Console.WriteLine($"Got message {args.Payload}");
                    if (Interlocked.Decrement(ref count) == 0)
                        counted.Release();
                }));

            // publish messages
            for (var i = 0; i < 100; i++)
            {
                await topic.PublishAsync($"Message {i}");
            }

            // start publisher task
            //var publisher = Task.Run(async () =>
            //{
            //    for (var i = 0; i < 100; i++)
            //    {
            //        await topic.PublishAsync($"Message {i}");
            //    }
            //});

            // wait for all events
            await counted.WaitAsync();

            // destroy the topic
            await client.DestroyAsync(topic);
        }
    }
}
