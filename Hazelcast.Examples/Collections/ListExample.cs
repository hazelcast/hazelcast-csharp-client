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

namespace Hazelcast.Examples.Collections
{
    internal class ListExample
    {
        private static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var list = client.GetList<string>("list-example");

            list.Add("item1");
            list.Add("item2");
            list.Add("item3");

            Console.WriteLine("Get: " + list[0]);

            Console.WriteLine("Enumerator : " + string.Join(", ", list));

            Console.WriteLine("Contains: " + string.Join(", ", list.Contains("item2")));

            Console.WriteLine("Count: " + string.Join(", ", list.Count));

            Console.WriteLine("Sublist: " + string.Join(", ", list.SubList(0, 2)));

            list.Destroy();
            client.Shutdown();
        }
    }
}