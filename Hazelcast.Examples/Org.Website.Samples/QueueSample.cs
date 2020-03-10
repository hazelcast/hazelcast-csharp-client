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
using Hazelcast.Client;
using Hazelcast.Core;

namespace Hazelcast.Examples.Org.Website.Samples
{
    public class QueueSample
    {
        public static void Run(string[] args)
        {
            // Start the Hazelcast Client and connect to an already running Hazelcast Cluster on 127.0.0.1
            var hz = HazelcastClient.NewHazelcastClient();
            // Get a Blocking Queue called "my-distributed-queue"
            var queue = hz.GetQueue<string>("my-distributed-queue");
            // Offer a String into the Distributed Queue
            queue.Offer("item");
            // Poll the Distributed Queue and return the String
            queue.Poll();
            //Timed blocking Operations
            queue.Offer("anotheritem", 500, TimeUnit.Milliseconds);
            queue.Poll(5, TimeUnit.Seconds);
            //Indefinitely blocking Operations
            queue.Put("yetanotheritem");
            Console.WriteLine(queue.Take());
            // Shutdown this Hazelcast Client
            hz.Shutdown();
        }
    }
}