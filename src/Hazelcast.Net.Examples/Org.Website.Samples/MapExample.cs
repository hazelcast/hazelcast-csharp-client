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

using System.Threading.Tasks;

namespace Hazelcast.Examples.Org.Website.Samples
{
    // ReSharper disable once UnusedMember.Global
    public class MapExample
    {
        public static async Task Run()
        {
            // create an Hazelcast client and connect to a server running on localhost
            var hz = new HazelcastClientFactory(HazelcastOptions.Build()).CreateClient();
            await hz.OpenAsync();

            // get distributed map from cluster
            var map = await hz.GetMapAsync<string, string>("my-distributed-map");

            // set/get
            await map.AddOrReplaceAsync("key", "value");
            await map.GetAsync("key");

            // concurrent methods, optimistic updating
            await map.AddIfMissingAsync("somekey", "somevalue");
            await map.ReplaceAsync("key", "value", "newvalue");

            // destroy the map
            map.Destroy();

            // terminate the client
            await hz.DisposeAsync();
        }
    }
}
