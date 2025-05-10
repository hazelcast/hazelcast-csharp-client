// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Models;

namespace Hazelcast.Examples.DistributedObjects
{
    // ReSharper disable once UnusedMember.Global
    public class MapDynamicConfigurationExample
    {
        public static async Task Main(params string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .Build();

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // You can create and configure a map on runtime.
            await client.DynamicOptions.ConfigureMapAsync("my-stream-map", options =>
            {
                // In this example, event journaling is enabled for the map.
                // For other configurations, please see https://docs.hazelcast.com/hazelcast/latest/data-structures/map-config
                options.EventJournal.Enabled = EventJournalOptions.Defaults.Enabled;
                options.EventJournal.Capacity = EventJournalOptions.Defaults.Capacity;
                options.EventJournal.TimeToLiveSeconds = 123; 
                
            });
            
            // get the distributed map from the cluster
            await using var map = await client.GetMapAsync<string, string>("my-stream-map");

            // destroy the map
            await client.DestroyAsync(map);
        }
    }
}
