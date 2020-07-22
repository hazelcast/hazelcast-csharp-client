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
using System.Threading.Tasks;
using Hazelcast.Examples.Models;

namespace Hazelcast.Examples.DistributedObjects
{
    // ReSharper disable once UnusedMember.Global
    public class MapIdentifiedDataSerializableExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // creates the example options
            var options = BuildExampleOptions(args);

            // customize options for this example
            options.Serialization.AddDataSerializableFactory(
                ExampleDataSerializableFactory.FactoryId,
                new ExampleDataSerializableFactory());

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = new HazelcastClientFactory(options).CreateClient();
            await client.ConnectAsync();

            // get the distributed map from the cluster
            await using var map = await client.GetMapAsync<int, Employee>("identified-data-serializable-example");

            // create and add an employee
            Console.WriteLine("Adding employee 'the employee'.");
            var employee = new Employee { Id = 1, Name = "the employee" };
            await map.SetAsync(employee.Id, employee);

            // retrieve employee
            var e = await map.GetAsync(employee.Id);
            Console.WriteLine($"Gotten employee '{e.Name}'.");

            // destroy the map
            await client.DestroyAsync(map);
        }
    }
}
