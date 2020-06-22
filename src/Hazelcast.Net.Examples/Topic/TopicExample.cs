﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;

namespace Hazelcast.Examples.Topic
{
    // ReSharper disable once UnusedMember.Global
    public class TopicExample : ExampleBase
    {
        public static async Task Run(params string[] args)
        {
            var options = BuildExampleOptions(args);

            // create an Hazelcast client and connect to a server running on localhost
            var hz = new HazelcastClientFactory(options).CreateClient();
            await hz.OpenAsync();

            // get distributed topic from cluster
            var topic = await hz.GetTopicAsync<string>("topic-example");

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
            await hz.DestroyAsync(topic).CAF();

            // terminate the client
            await hz.DisposeAsync();
        }
    }
}
