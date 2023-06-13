// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Exceptions;
using Hazelcast.Models;

namespace Hazelcast.Examples.DistributedObjects
{
    // ReSharper disable once UnusedMember.Global
    public class ReliableTopicExample
    {
        public static async Task Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .With((conf, opt) =>
                {
                    // Publish messages when there is a space and read messages as batches size of 5. 
                    opt.ReliableTopics["reliable-topic-example"] = new ReliableTopicOptions(TopicOverloadPolicy.Block, 5);
                })
                .WithConsoleLogger()
                .Build();

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get distributed reliable topic from cluster
            await using var topic = await client.GetReliableTopicAsync<string>("reliable-topic-example");

            var count = 100;
            var counted = new SemaphoreSlim(0);

            // subscribe to event
            await topic.SubscribeAsync(on => on
                    .Message((sender, args) =>
                    {
                        Console.WriteLine($"Got message {args.Payload}");
                        if (Interlocked.Decrement(ref count) == 0)
                            counted.Release();
                    })
                    .Terminated((sender, args) =>
                    {
                        Console.WriteLine("The listener is disposed, and the task at the background is canceled.");
                        
                    }).Exception((sender, args) =>
                    {
                        // Terminate the subscription if client goes offline.
                        if (args.Exception is ClientOfflineException)
                            args.Cancel = true;
                    }),
                // Setting StoreSequence=true and IsLossTolerant=false means listener is durable.
                new ReliableTopicEventHandlerOptions() {InitialSequence = -1, StoreSequence = true, IsLossTolerant = false});

            // publish messages
            for (var i = 0; i < 100; i++)
            {
                await topic.PublishAsync($"Message {i}");
            }

            // wait for all events
            await counted.WaitAsync();

            // destroy the topic
            await client.DestroyAsync(topic);
        }
    }
}
