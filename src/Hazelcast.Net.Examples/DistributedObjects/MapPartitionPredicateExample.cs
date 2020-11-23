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
using Hazelcast.Predicates;

namespace Hazelcast.Examples.DistributedObjects
{
    // ReSharper disable once UnusedMember.Global
    public class MapPartitionPredicateExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // creates the example options
            var options = BuildExampleOptions(args);

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get the distributed map from the cluster
            await using var map = await client.GetMapAsync<int, int>("predicate-example");

            // add values
            Console.WriteLine("Populating map");
            for (var i = 0; i < 1000; i++)
                await map.SetAsync(i, i);

            // count
            Console.WriteLine("Map size: " + await map.SizeAsync());

            // report
            const int partitionKey = 10;

            // all keys on the same partition of the partitionKey will be returned
            var partitionKeys = await map.GetKeysAsync(Predicate.Partition(partitionKey, Predicate.True()));
            Console.WriteLine("Partition keys: " + string.Join(", ", partitionKeys));

            // keys less than 100 and on the same partition of the partitionKey will be returned
            var filteredKeys = await map.GetKeysAsync(Predicate.Partition(partitionKey, Predicate.IsLessThan("this",100)));
            Console.WriteLine("Filtered keys: " + string.Join(", ", filteredKeys));

            // destroy the map
            await client.DestroyAsync(map);
        }
    }
}
