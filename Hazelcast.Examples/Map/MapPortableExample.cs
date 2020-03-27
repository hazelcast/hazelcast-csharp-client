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
using Hazelcast.Examples.Models;

namespace Hazelcast.Examples.Map
{
    internal class MapPortableExample
    {
        public static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new Configuration();
            config.SerializationConfig.PortableFactories.Add(1, new ExamplePortableFactory());
            config.NetworkConfig.AddAddress("127.0.0.1");
            var client = HazelcastClient.NewHazelcastClient(config);

            var map = client.GetMap<int, Customer>("portable-example");

            var customer = new Customer {Id = 1, LastOrder = DateTime.UtcNow, Name = "first-customer"};

            Console.WriteLine("Adding customer: " + customer);
            map.Put(customer.Id, customer);

            var c = map.Get(customer.Id);

            Console.WriteLine("Gotten customer: " + c);

            client.Shutdown();
        }
    }
}