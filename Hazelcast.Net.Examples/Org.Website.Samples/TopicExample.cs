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
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;

namespace Hazelcast.Examples.Org.Website.Samples
{
    // ReSharper disable once UnusedMember.Global
    public class TopicExample
    {
        private static void OnMessage(ITopic<string> sender, TopicMessageEventArgs<string> args)
        {
            Console.WriteLine($"Got message " + args.Payload);
        }

        public static async Task Run()
        {
            // create an Hazelcast client and connect to a server running on localhost
            var hz = new HazelcastClientFactory().CreateClient();
            await hz.OpenAsync();

            // get distributed topic from cluster
            var topic = await hz.GetTopicAsync<string>("my-distributed-topic");

            // subscribe to event
            await topic.SubscribeAsync(on => on.Message(OnMessage));

            // publish a message to the Topic
            await topic.PublishAsync("Hello to distributed world");

            // allow event to trigger
            await Task.Delay(1_000);

            // destroy the topic
            topic.Destroy();

            // terminate the client
            await hz.DisposeAsync();
        }
    }
}
