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
using System.Linq;
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.Core;

namespace Hazelcast.Examples.Map
{
    internal class MapJsonExample
    {
        public static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");

            var client = HazelcastClient.NewHazelcastClient(config);
            var map = client.GetMap<string, HazelcastJsonValue>("json-example");

            map.Put("item1", new HazelcastJsonValue("{ \"age\": 4 }"));
            map.Put("item2", new HazelcastJsonValue("{ \"age\": 20 }"));

            var result = map.Values(Predicates.IsLessThan("age", 6));

            Console.WriteLine("Retrieved " + result.Count + " values whose age is less than 6.");
            Console.WriteLine("Entry is: " + result.First().ToString());

            Console.WriteLine("Finished");

            client.Shutdown();
        }
    }
}