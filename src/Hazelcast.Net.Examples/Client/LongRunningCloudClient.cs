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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.NearCaching;
using Hazelcast.Networking;

namespace Hazelcast.Examples.Client
{
    /*
        This example will run forever until stopped with ^C
        unless an iteration count or iteration duration is provided via options
        
        this is to test that a client simply pinging a cluster can stay connected
        
        go to https://viridian.hazelcast.com/ and start a (new) cluster, get its name, token and
        ssl certificate from the UI, and update the code accordingly (see commented-out options) 
        or pass these parameters to the example via the command line:
        
        hz run-example ~LongRunningCloudClient --- \
          --hazelcast.clusterName="YOUR_CLUSTER_NAME" \
          --hazelcast.networking.cloud.discoveryToken="YOUR_CLUSTER_DISCOVERY_TOKEN" \
          --hazelcast.metrics.enabled=true \
          --hazelcast.networking.ssl.enabled=true \
          --hazelcast.networking.ssl.validateCertificateChain=false \
          --hazelcast.networking.ssl.protocol=TLS12 \
          --hazelcast.networking.ssl.certificatePath="path/to/client.pfx" \
          --hazelcast.networking.ssl.certificatePassword="YOUR_SSL_PASSWORD"
        
        then example behavior can be controlled with example options:
          --hazelcast.example.iterationCount=10
          --hazelcast.example.iterationDuration=00:10:00
          --hazelcast.example.iterationPauseMilliseconds=50
        
        the example will run IterationCount times, for IterationDuration, whichever limit
        is reached first. if both are set to infinite, the example will run until interrupted
        via Ctrl-C. a pause of IterationPauseMilliseconds is observed between each iteration.
    */

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
            var optionsBuilder = new HazelcastOptionsBuilder()
                .With(args)
                /*.With(config =>
                {
                    // Your Viridian cluster name.
                    config.ClusterName = "YOUR_CLUSTER_NAME";
                    // Your discovery token and url to connect Viridian cluster.
                    config.Networking.Cloud.DiscoveryToken = "YOUR_CLUSTER_DISCOVERY_TOKEN";    
                    // Enable metrics to see on Management Center.
                    config.Metrics.Enabled = true;
                    // Configure SSL.
                    config.Networking.Ssl.Enabled = true;
                    config.Networking.Ssl.ValidateCertificateChain = false;
                    config.Networking.Ssl.Protocol = SslProtocols.Tls12;
                    config.Networking.Ssl.CertificatePath = "client.pfx";
                    config.Networking.Ssl.CertificatePassword = "YOUR_SSL_PASSWORD";
                })*/
                .WithConsoleLogger();

            var exampleOptions = new ExampleOptions();
            optionsBuilder = optionsBuilder.Bind("hazelcast:example", exampleOptions)
                .WithDefault("Logging:LogLevel:Hazelcast", "Debug")
                .WithDefault(o => { o.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 4000; });
            
            if (args.All(x => x != "Logging:LogLevel:Hazelcast"))
                optionsBuilder = optionsBuilder.With("Logging:LogLevel:Hazelcast", "Debug");

            var options = optionsBuilder.Build();

            // log level must be a valid Microsoft.Extensions.Logging.LogLevel value
            //   Trace | Debug | Information | Warning | Error | Critical | None

            // the name of the map (needed for NearCache options)
            var mapName = "map_" + Guid.NewGuid().ToString("N").Substring(0, 7);

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
                Console.WriteLine($"Error: check your configuration ('{value}' != 'value').");
                return;
            }

            Console.WriteLine("Put/Get values in/from map with random values...");
            if (exampleOptions.IterationCount < 0) Console.WriteLine("(press Ctrl-C to stop)");
            var random = new Random();
            const int step = 40;
            var i = 0;
            var loop = true;
            const int maxKeys = 20;
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

                // get value for a known key
                randomValue = keys[random.Next(keys.Count)];
                value = await map.GetAsync("key_" + randomValue).ConfigureAwait(false);
                if (!value.Equals("value_" + randomValue))
                {
                    Console.WriteLine($"Error: check your configuration ('{value}' != '{"value_" + randomValue}').");
                    loop = false;
                }

                if (i % step == 0 || stopwatch.Elapsed - previousElapsed > consolePeriod)
                {
                    Console.WriteLine($"  [{i:D6}] map [{"key_" + randomValue}] = {value}");
                    Console.WriteLine($"  [{i:D6}] map size: {await map.GetSizeAsync().ConfigureAwait(false)}");
                    previousElapsed = stopwatch.Elapsed;
                }

                if (exampleOptions.IterationPauseMilliseconds > 0)
                    await Task.Delay(exampleOptions.IterationPauseMilliseconds).ConfigureAwait(false);

                i++;
            }

            Console.WriteLine("Stopping...");
            Console.WriteLine($"  [{i:D6}] map size: {await map.GetSizeAsync().ConfigureAwait(false)}");

            Console.WriteLine("Destroy the map...");
            await map.DestroyAsync().ConfigureAwait(false);

            Console.WriteLine("Dispose the map...");
            await map.DisposeAsync().ConfigureAwait(false);

            Console.WriteLine("Dispose the client...");
            await client.DisposeAsync().ConfigureAwait(false);

            Console.WriteLine($"Done (elapsed: {stopwatch.Elapsed.ToString("hhmmss\\.fff\\ ", CultureInfo.InvariantCulture)}).");
        }
    }
}
