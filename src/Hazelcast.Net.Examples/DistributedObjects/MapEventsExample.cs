// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Examples.DistributedObjects
{
    // ReSharper disable once UnusedMember.Global
    public class MapEventsExample
    {
        public static async Task Main(params string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .Build();

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get the distributed map from the cluster
            await using var map = await client.GetMapAsync<string, string>("listener-example");

            var count = 3;
            var counted = new SemaphoreSlim(0);

            // subscribe to events
            var id = await map.SubscribeAsync(handle => handle
                .EntryAdded((m, a) =>
                {
                    Console.WriteLine("Added '{0}': '{1}'", a.Key, a.Value);
                    if (Interlocked.Decrement(ref count) == 0)
                        counted.Release();
                })
                .EntryUpdated((m, a) =>
                {
                    Console.WriteLine("Updated '{0}': '{1}' (was: '{2}')", a.Key, a.Value, a.OldValue);
                    if (Interlocked.Decrement(ref count) == 0)
                        counted.Release();
                })
                .EntryRemoved((m, a) =>
                {
                    Console.WriteLine("Removed '{0}': '{1}'", a.Key, a.OldValue);
                    if (Interlocked.Decrement(ref count) == 0)
                        counted.Release();
                }));

            // trigger events
            await map.SetAsync("key", "value"); // add
            await map.SetAsync("key", "valueNew"); //update
            await map.RemoveAsync("key");

            // wait for events
            await counted.WaitAsync();

            // unsubscribe
            await map.UnsubscribeAsync(id);
        }
    }
}
