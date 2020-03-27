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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Config;

namespace Hazelcast.Examples.Client
{
    internal class ClientStatisticsExample
    {
        private static void Run(string[] args)
        {
            //This will enable client statistics 
            Environment.SetEnvironmentVariable("hazelcast.client.statistics.enabled", "true");

            //set the statistics send period, default value is 3 seconds
            Environment.SetEnvironmentVariable("hazelcast.client.statistics.period.seconds", "3");

            var config = new Configuration();
            config.NetworkConfig.AddAddress("127.0.0.1");

            var nearCacheConfig = new NearCacheConfig
            {
                MaxSize = 1000, InvalidateOnChange = true, EvictionPolicy = EvictionPolicy.Lru, InMemoryFormat = InMemoryFormat.Binary
            };

            config.NearCacheConfigs.Add("myMap", nearCacheConfig);

            var client = HazelcastClient.NewHazelcastClient(config);

            //Let's generate some statistics 
            var map = client.GetMap<string, string>("myMap");
            var task = Task.Factory.StartNew(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    map.Put("key-" + i, "value-" + i);
                    Thread.Sleep(500);
                }
            });

            //After client connected you can use Management Center to visualize client statistics
            task.Wait();
            client.Shutdown();
        }
    }
}