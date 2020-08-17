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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hazelcast.Examples.DistributedObjects
{
    // ReSharper disable once UnusedMember.Global
    public class DictionaryAsyncExample : ExampleBase
    {
        public static async Task Run(params string[] args)
        {
            // creates the example options
            var options = BuildExampleOptions(args);

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = HazelcastClientFactory.CreateClient(options);
            await client.StartAsync();

            // get the distributed map from the cluster
            await using var map = await client.GetDictionaryAsync<string, string>("simple-example");

            // create tasks that add values to the map
            var tasks = new List<Task>();
            for (var i = 0; i < 100; i++)
            {
                var key = "key " + i;
                var task = map.AddOrUpdateAsync(key, " value " + i).ContinueWith(t => { Console.WriteLine("Added " + key); });
                tasks.Add(task);
            }

            // await all tasks
            await Task.WhenAll(tasks.ToArray());

            // destroy the map
            await client.DestroyAsync(map);
        }
    }
}
