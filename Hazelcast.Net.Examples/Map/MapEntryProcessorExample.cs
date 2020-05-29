﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Examples.Map
{
    // ReSharper disable once UnusedMember.Global
    public class MapEntryProcessorExample
    {
        public static async Task Run()
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            static void Configure(HazelcastConfiguration configuration)
            {
                configuration.Serialization.AddDataSerializableFactory(
                    EntryProcessorDataSerializableFactory.FactoryId,
                    new EntryProcessorDataSerializableFactory());
            }

            // create an Hazelcast client and connect to a server running on localhost
            var hz = new HazelcastClientFactory().CreateClient(Configure);
            await hz.OpenAsync();

            var map = await hz.GetMapAsync<int, string>("entry-processor-example");

            Console.WriteLine("Populating map");

            for (var i = 0; i < 10; i++)
            {
                await map.AddOrReplaceAsync(i, "value" + i);
            }

            Console.WriteLine("Map size: " + await map.CountAsync());

            var result = await map.ExecuteAsync(
                new UpdateEntryProcessor("value-UPDATED"), 
                Predicates.Predicates.Sql("this==value5"));

            Console.WriteLine("Updated value result: " + result[5]);
            Console.WriteLine("The same value from  the map: " + await map.GetAsync(5));

            // destroy the map
            map.Destroy();

            // terminate the client
            await hz.DisposeAsync();
        }
    }
}