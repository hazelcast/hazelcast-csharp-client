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
    public class MapEntryProcessorExample
    {
        public static void Run(string[] args)
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            var config = new Configuration();

            config.NetworkConfig.AddAddress("127.0.0.1");
            config.SerializationConfig
                .DataSerializableFactories.Add(EntryProcessorDataSerializableFactory.FactoryId,
                    new EntryProcessorDataSerializableFactory());

            var client = HazelcastClient.NewHazelcastClient(config);

            var map = client.GetMap<int, string>("entry-processor-example");

            Console.WriteLine("Populating map");

            for (int i = 0; i < 10; i++)
            {
                map.Put(i, "value" + i);
            }

            Console.WriteLine("Map size: " + map.Size());

            var result = map.ExecuteOnEntries(new UpdateEntryProcessor("value-UPDATED"), Predicates.Sql("this==value5"));

            Console.WriteLine("Updated value result: " +result[5]);

            Console.WriteLine("The same value from  the map: " + map.Get(5));

            client.Shutdown();
        }
    }
}