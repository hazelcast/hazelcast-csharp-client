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
    public class MapEntryProcessorExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // creates the example options
            var options = BuildExampleOptions(args);

            // customize options for this example
            options.Serialization.AddDataSerializableFactory(
                EntryProcessorDataSerializableFactory.FactoryId,
                new EntryProcessorDataSerializableFactory());

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = new HazelcastClientFactory(options).CreateClient();
            await client.StartAsync();

            // get the distributed map from the cluster
            await using var map = await client.GetMapAsync<int, string>("entry-processor-example");

            // add values
            Console.WriteLine("Populate map");
            for (var i = 0; i < 10; i++)
                await map.SetAsync(i, "value" + i);

            // verify
            Console.WriteLine("Count: " + await map.CountAsync());

            // process
            // note: hazelcast-test.jar has the same UpdateEntryProcessor,
            // named com.hazelcast.client.test.IdentifiedEntryProcessor, so
            // this works
            var result = await map.ExecuteAsync(
                new UpdateEntryProcessor("value-UPDATED"),
                Predicates.Predicate.Sql("this==value5"));

            Console.WriteLine("Updated value result: " + result[5]);
            Console.WriteLine("The same value from  the map: " + await map.GetAsync(5));

            // destroy the map
            await client.DestroyAsync(map);
        }
    }
}
