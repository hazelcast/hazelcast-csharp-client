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
using Hazelcast.Core;
using Hazelcast.Examples.Models;

namespace Hazelcast.Examples.Map
{
    public class MapPartitionPredicateExample
    {
        public static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new Configuration();

            config.NetworkConfig.AddAddress("127.0.0.1");

            var client = HazelcastClient.NewHazelcastClient(config);

            var map = client.GetMap<int, int>("predicate-example");

            Console.WriteLine("Populating map");

            for (int i = 0; i < 1000; i++)
            {
                map.Put(i, i);
            }

            Console.WriteLine("Map size: " + map.Size());

            var partitionKey = 10;

            //all keys on the same partition of the partitionKey will be returned
            var allPartitionKeys = map.KeySet(new PartitionPredicate(partitionKey, Predicates.True()));
            
            //keys less than 100 and on the same partition of the partitionKey will be returned
            var result = map.KeySet(new PartitionPredicate(partitionKey, Predicates.IsLessThan("this",100)));

            Console.Write("\nKey set: ");
            foreach (var key in result)
            {
                Console.Write(key + ", ");
            }

            client.Shutdown();
        }
    }
}