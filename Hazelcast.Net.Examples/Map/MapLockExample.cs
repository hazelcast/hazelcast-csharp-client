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
using Hazelcast.Core;
using Hazelcast.Logging;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Examples.Map
{
    // ReSharper disable once UnusedMember.Global
    public class MapLockExample
    {
        public static async Task Run()
        {
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "info");
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");

            // uncomment and enable HzConsole to see the context changes
            //HzConsole.Configure<AsyncContext>(config => { config.SetMaxLevel(0); });

            // create an Hazelcast client and connect to a server running on localhost
            var hz = new HazelcastClientFactory().CreateClient(configuration =>
            {
                // configure server address
                //configuration.Networking.Addresses.Add("sgay-l4");

                // configure logging
                configuration.Logging.LoggerFactory.Creator = () =>
                    LoggerFactory.Create(builder => builder.AddConsole());
            });
            await hz.OpenAsync();

            var map = await hz.GetMapAsync<string, string>("map-lock-example");

            await map.AddOrReplaceAsync("key", "value");

            // locking in the current context
            await map.LockAsync("key");

            // start a task that immediately update the value, in a new context
            // because it is a new context it will wait until the lock is released!
            //
            // simply using Task.Run here would break the example, as Task.Run would
            // queue work with the same context, i.e. the context that has the lock
            //
            var task = AsyncContext.RunDetached(async () =>
            {
                await map.AddOrReplaceAsync("key", "value1");
                Console.WriteLine("Put new value");
            });

            try
            {
                var value = await map.GetAsync("key");
                // pretend to do something with the value..
                Thread.Sleep(5000);
                await map.AddOrReplaceAsync("key", "value2");
            }
            finally
            {
                await map.UnlockAsync("key");
            }

            task.Wait();

            Console.WriteLine("New value (should be 'value1'): " + await map.GetAsync("key")); // should be value1

            // destroy the map
            map.Destroy();

            // terminate the client
            await hz.DisposeAsync();
        }
    }
}