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

namespace Hazelcast.Examples.Map
{
    // ReSharper disable once UnusedMember.Global
    public class MapListenerExample : ExampleBase
    {
        public static async Task Run(params string[] args)
        {
            var options = BuildExampleOptions(args);

            // create an Hazelcast client and connect to a server running on localhost
            var hz = new HazelcastClientFactory(options).CreateClient();
            await hz.OpenAsync();

            var map = await hz.GetMapAsync<string, string>("listener-example");

            var count = 3;
            var counted = new SemaphoreSlim(0);

            // subscribe
            var id = await map.SubscribeAsync(on => on
                .EntryAdded((map, args) =>
                {
                    Console.WriteLine("Added '{0}': '{1}'", args.Key, args.Value);
                    if (Interlocked.Decrement(ref count) == 0)
                        counted.Release();
                })
                .EntryUpdated((map, args) =>
                {
                    Console.WriteLine("Updated '{0}': '{1}' (was: '{2}')", args.Key, args.Value, args.OldValue);
                    if (Interlocked.Decrement(ref count) == 0)
                        counted.Release();
                })
                .EntryRemoved((map, args) =>
                {
                    Console.WriteLine("Removed'{0}': '{1}'", args.Key, args.OldValue);
                    if (Interlocked.Decrement(ref count) == 0)
                        counted.Release();
                }));

            await map.AddOrReplaceAsync("key", "value"); // add
            await map.AddOrReplaceAsync("key", "valueNew"); //update
            await map.RemoveAndReturnAsync("key");

            // wait for events
            await counted.WaitAsync();

            // unsubscribe
            await map.UnsubscribeAsync(id);

            // destroy the map
            map.Destroy();

            // terminate the client
            await hz.DisposeAsync();
        }
    }
}
