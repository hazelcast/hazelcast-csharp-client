// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Config;

namespace Hazelcast.Examples.Map
{
    internal class ReplicatedMapExample
    {
        private static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var map = client.GetReplicatedMap<string, string>("replicatedMap-example");

            map.Put("key", "value");
            map.Put("key2", "value2");

            Console.WriteLine("Key: " + map.Get("key"));

            Console.WriteLine("Values : " + string.Join(", ", map.Values()));

            Console.WriteLine("KeySet: " + string.Join(", ", map.KeySet()));

            Console.WriteLine("Size: " + string.Join(", ", map.Size()));

            Console.WriteLine("EntrySet: " + string.Join(", ", map.EntrySet()));

            Console.WriteLine("ContainsKey: " + string.Join(", ", map.ContainsKey("key")));

            Console.WriteLine("ContainsValue: " + string.Join(", ", map.ContainsValue("value")));

            map.Destroy();
            client.Shutdown();
        }
    }
}