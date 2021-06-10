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
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Hazelcast.Networking;

namespace Hazelcast.Examples.Client
{
    // this example will run forever until stopped with ^C
    // this is to test that a client simply pinging a cluster can stay connected

    public static class LongRunningCloudClient
    {
        private const int IterationCount = 60;
        private const int IterationPauseMilliseconds = 100;

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hazelcast Cloud Client");
            var stopwatch = Stopwatch.StartNew();

            Console.WriteLine("Build options...");
            var options = new HazelcastOptionsBuilder()
                //.With(args)
                .WithConsoleLogger()
                .With("Logging:LogLevel:Hazelcast", "Debug")
                .Build();

            // log level must be a valid Microsoft.Extensions.Logging.LogLevel value
            //   Trace | Debug | Information | Warning | Error | Critical | None

            // enable metrics
            options.Metrics.Enabled = true;

            // configure cloud
            options.ClusterName = "***";
            options.Networking.Cloud.DiscoveryToken = "***";
            //options.Networking.Cloud.Url = new Uri("https://...");

            options.Networking.ReconnectMode = ReconnectMode.ReconnectAsync;

            Console.WriteLine("Get and connect client...");
            Console.WriteLine($"Connect to cluster \"{options.ClusterName}\"{(options.Networking.Cloud.Enabled ? " (cloud)" : "")}");
            if (options.Networking.Cloud.Enabled) Console.WriteLine($"Cloud Discovery Url: {options.Networking.Cloud.Url}");
            var client = await HazelcastClientFactory.StartNewClientAsync(options).ConfigureAwait(false);

            Console.WriteLine("Get map...");
            var map = await client.GetMapAsync<string, string>("map").ConfigureAwait(false);

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

            Console.WriteLine("Put/Get values in/from map with random values...");
            var random = new Random();
            var step = 100;
            var i = 0;
            var loop = true;

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                loop = false;
                eventArgs.Cancel = true;
            };

            while (loop)
            {
                var randomValue = random.Next(100_000);
                await map.PutAsync("key_" + randomValue, "value_" + randomValue).ConfigureAwait(false);

                randomValue = random.Next(100_000);
                await map.GetAsync("key" + randomValue).ConfigureAwait(false);

                if (i % step == 0)
                {
                    Console.WriteLine($"[{i:D3}] map size: {await map.GetSizeAsync().ConfigureAwait(false)}");
                }

                if (IterationPauseMilliseconds > 0)
                    await Task.Delay(IterationPauseMilliseconds).ConfigureAwait(false);

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
