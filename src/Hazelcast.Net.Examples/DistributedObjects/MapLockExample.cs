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
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.Examples.DistributedObjects
{
    // ReSharper disable once UnusedMember.Global
    public class MapLockExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // uncomment and enable HzConsole to see the context changes
            //HzConsole.Configure<AsyncContext>(config => { config.SetMaxLevel(0); });

            // creates the example options
            var options = BuildExampleOptions(args);

            // FIXME: this should not be required since we dispose the client?! or ?!
            // we need a better mechanism
            // in a dependency-injection app, the logger factory would be disposed when
            // the container is disposed - we *want* to dispose the logger factory to flush
            // the loggers buffers before the app stops - in a non-DI app,
            // we cannot ask the Hazelcast client, or the client factory, to dispose the
            // logger factory since it does not *own* the factory
            // unless we dispose everything returned by a Creator - but how can we tell
            // it needs to be disposed? shall the creator return an instance + ownership?
            using var ensureLoggerFactoryIsDisposed = options.Logging.LoggerFactory.Service;

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = new HazelcastClientFactory(options).CreateClient();
            await client.OpenAsync();

            // get the distributed map from the cluster
            await using var map = await client.GetMapAsync<string, string>("map-lock-example");

            // add value
            await map.AddOrUpdateAsync("key", "value");

            // locking in the current context
            await map.LockAsync("key");

            // start a task that immediately update the value, in a new context
            // because it is a new context it will wait until the lock is released!
            //
            // simply running the code here would break the example, as it would run
            // with the same context, i.e. the context that has the lock, so we need
            // to start it with a new context
            //
            var task = TaskEx.WithNewContext(async () =>
            {
                await map.AddOrUpdateAsync("key", "value1");
                Console.WriteLine("Put new value");
            });

            try
            {
                var value = await map.GetAsync("key");
                // pretend to do something with the value..
                await Task.Delay(5000);
                await map.AddOrUpdateAsync("key", "value2");
            }
            finally
            {
                await map.UnlockAsync("key");
            }

            // now wait for the background task
            await task;

            // report
            Console.WriteLine("New value (should be 'value1'): " + await map.GetAsync("key")); // should be value1

            // destroy the map
            await client.DestroyAsync(map);
        }
    }
}
