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
using Hazelcast.Config;

namespace Hazelcast.Examples.Collections
{
    internal class SetExample
    {
        private static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new Configuration();
            config.NetworkConfig.AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var set = client.GetSet<string>("set-example");

            set.Add("item1");
            set.Add("item2");
            set.Add("item3");
            set.Add("item3");

            Console.WriteLine("Enumerator : " + string.Join(", ", set));

            Console.WriteLine("Contains: " + string.Join(", ", set.Contains("item2")));

            Console.WriteLine("Count: " + string.Join(", ", set.Count));

            set.Destroy();
            client.Shutdown();
        }
    }
}