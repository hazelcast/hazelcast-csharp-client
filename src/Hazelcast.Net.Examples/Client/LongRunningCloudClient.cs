// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.NearCaching;
using Hazelcast.Networking;

namespace Hazelcast.Examples.Client
{
    // this example will run forever until stopped with ^C
    // unless an iteration count or iteration duration is provided via options
    //
    // this is to test that a client simply pinging a cluster can stay connected
    //
    // go to https://cloud.hazelcast.com and start a (new) cluster, get its name and token from the
    // UI, and update the code accordingly (see commented-out options) or pass these parameters
    // to the example via the command line:
    //
    // hz run-example ~LongRunningCloudClient --- \
    //   --hazelcast.clusterName="***" \
    //   --hazelcast.networking.cloud.discoveryToken="***

    public static class LongRunningCloudClient
    {
        public class ExampleOptions
        {
            public int IterationCount { get; set; } = -1; // default is infinite
            public TimeSpan IterationDuration { get; set; } = TimeSpan.Zero; // default is infinite
            public int IterationPauseMilliseconds { get; set; } = 100;
        }

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hazelcast Cloud Client");

            Console.WriteLine("Build options...");
            var exampleOptions = new ExampleOptions();
            var options = new HazelcastOptionsBuilder()
                .Bind("hazelcast:example", exampleOptions)
                .With(args)
                .WithConsoleLogger()
                .WithDefault("Logging:LogLevel:Hazelcast", "Debug")
                .WithDefault(o =>
                {
                    // make sure we don't try to connect forever
                    // but - make sure it can be overriden by the command-line options
                    o.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 4000;
                })
                .Build();

            // log level must be a valid Microsoft.Extensions.Logging.LogLevel value
            //   Trace | Debug | Information | Warning | Error | Critical | None

            // the name of the map (needed for NearCache options)
            var mapName = "map_" + Guid.NewGuid().ToString("N").Substring(0, 7);

            // enable metrics
            options.Metrics.Enabled = true;

            // configure cloud
            //options.ClusterName = "***";
            //options.Networking.Cloud.DiscoveryToken = "***";

            // configure cloud url (if not running on default Cloud)
            //options.Networking.Cloud.Url = new Uri("...");

            // make sure we reconnect
            //
            // note: this can also be achieved with preview options
            // options.Preview.EnableNewReconnectOptions = true;
            //
            options.Networking.ReconnectMode = ReconnectMode.ReconnectAsync;

            // enable NearCache for our map so that we send NearCache metrics
            options.NearCaches[mapName] = new NearCacheOptions
            {
                TimeToLiveSeconds = 60,
                EvictionPolicy = EvictionPolicy.Lru,
                MaxSize = 10000, // entries
                InMemoryFormat = InMemoryFormat.Binary,
                MaxIdleSeconds = 3600,
                InvalidateOnChange = true
            };

            Console.WriteLine("Get and connect client...");
            Console.WriteLine($"Connect to cluster \"{options.ClusterName}\"{(options.Networking.Cloud.Enabled ? " (cloud)" : "")}");
            if (options.Networking.Cloud.Enabled) Console.WriteLine($"Cloud Discovery Url: {options.Networking.Cloud.Url}");
            var client = await HazelcastClientFactory.StartNewClientAsync(options).ConfigureAwait(false);

            Console.WriteLine("Get map...");
            Console.WriteLine($"Map name: {mapName}");
            var map = await client.GetMapAsync<string, string>(mapName).ConfigureAwait(false);

            Console.WriteLine("Put value into map...");
            await map.PutAsync("key", "value").ConfigureAwait(false);

            Console.WriteLine("Get value from map...");
            var value = await map.GetAsync("key").ConfigureAwait(false);

            Console.WriteLine("Validate value...");
            if (!value.Equals("value"))
            {
                Console.WriteLine("Error: check your configuration.");
                return;
            }

            Console.WriteLine("Put/Get values in/from map with random values (^C to stop)...");
            var random = new Random();
            const int step = 100;
            var i = 0;
            var loop = true;
            const int maxKeys = 10;
            var keys = new List<int>();
            var consolePeriod = TimeSpan.FromSeconds(4);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Console.WriteLine("Received ^C, stopping...");
                loop = false;
                eventArgs.Cancel = true;
            };

            var stopwatch = Stopwatch.StartNew();
            var previousElapsed = stopwatch.Elapsed;
            while (loop &&
                   (exampleOptions.IterationCount < 0 || i < exampleOptions.IterationCount) &&
                   (exampleOptions.IterationDuration.TotalMilliseconds <= 0 || stopwatch.Elapsed < exampleOptions.IterationDuration))
            {
                // add a random key/value pair
                var randomValue = random.Next(100_000);
                await map.PutAsync("key_" + randomValue, "value_" + randomValue).ConfigureAwait(false);
                if (keys.Count < maxKeys) keys.Add(randomValue);

                // get value for a totally random key (will quite probably miss)
                randomValue = random.Next(100_000);
                await map.GetAsync("key_" + randomValue).ConfigureAwait(false);

                // get value for a known key (should cache)
                randomValue = keys[random.Next(keys.Count)];
                await map.GetAsync("key_" + randomValue).ConfigureAwait(false);

                if (i % step == 0 || stopwatch.Elapsed - previousElapsed > consolePeriod)
                {
                    Console.WriteLine($"[{i:D3}] map size: {await map.GetSizeAsync().ConfigureAwait(false)}");
                    previousElapsed = stopwatch.Elapsed;
                }

                if (exampleOptions.IterationPauseMilliseconds > 0)
                    await Task.Delay(exampleOptions.IterationPauseMilliseconds).ConfigureAwait(false);

                i++;
            }

            Console.WriteLine("Destroy the map...");
            await map.DestroyAsync().ConfigureAwait(false);

            Console.WriteLine("Dispose map...");
            await map.DisposeAsync().ConfigureAwait(false);

            Console.WriteLine("Dispose client...");
            await client.DisposeAsync().ConfigureAwait(false);

            Console.WriteLine($"Done (elapsed: {stopwatch.Elapsed.ToString("hhmmss\\.fff\\ ", CultureInfo.InvariantCulture)}).");
        }
    }
}
