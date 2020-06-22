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
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.Examples.Map
{
    // ReSharper disable once UnusedMember.Global
    public class MapJsonExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // creates the example options
            var options = BuildExampleOptions(args);

            // create an Hazelcast client and connect to a server running on localhost
            await using var hz = new HazelcastClientFactory(options).CreateClient();
            await hz.OpenAsync().CAF();

            // get the distributed map from the cluster
            var map = await hz.GetMapAsync<string, HazelcastJsonValue>("json-example").CAF();

            // add values
            await map.AddOrReplaceAsync("item1", new HazelcastJsonValue("{ \"age\": 4 }")).CAF();
            await map.AddOrReplaceAsync("item2", new HazelcastJsonValue("{ \"age\": 20 }")).CAF();

            // read
            var result = await map.GetValuesAsync(Predicates.Predicate.IsLessThan("age", 6)).CAF();
            Console.WriteLine("Retrieved " + result.Count + " values whose age is less than 6.");
            Console.WriteLine("Entry is: " + result.First());

            // destroy the map
            await hz.DestroyAsync(map).CAF();
        }
    }
}
