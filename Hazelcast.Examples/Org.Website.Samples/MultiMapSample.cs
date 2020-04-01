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

namespace Hazelcast.Examples.Org.Website.Samples
{
    public class DistributedMultiMapSample
    {
        public static void Run(string[] args)
        {
            // Start the Hazelcast Client and connect to an already running Hazelcast Cluster on 127.0.0.1
            var hz = HazelcastClient.NewHazelcastClient();
            // Get the Distributed MultiMap from Cluster.
            var multiMap = hz.GetMultiMap<string, string>("my-distributed-multimap");
            // Put values in the map against the same key
            multiMap.Put("my-key", "value1");
            multiMap.Put("my-key", "value2");
            multiMap.Put("my-key", "value3");
            // Print out all the values for associated with key called "my-key"
            var values = multiMap.Get("my-key");
            foreach (var item in values)
            {
                Console.WriteLine(item);
            }

            // remove specific key/value pair
            multiMap.Remove("my-key", "value2");
            // Shutdown this Hazelcast Client
            hz.Shutdown();
        }
    }
}