// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Examples.Models;

namespace Hazelcast.Examples.Map
{
    internal class MapIdentifiedDataSerializableExample
    {
        public static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new ClientConfig();
            
            config.GetNetworkConfig().AddAddress("127.0.0.1");
            config.GetSerializationConfig()
                .AddDataSerializableFactory(ExampleDataSerializableFactory.FactoryId,
                    new ExampleDataSerializableFactory());

            var client = HazelcastClient.NewHazelcastClient(config);

            var map = client.GetMap<int, Employee>("identified-data-serializable-example");

            var employee = new Employee { Id = 1, Name = "the employee"};

            Console.WriteLine("Adding employee: " + employee);

            map.Put(employee.Id, employee);

            var e = map.Get(employee.Id);

            Console.WriteLine("Gotten employee: " + e);

            client.Shutdown();
        }
    }
}