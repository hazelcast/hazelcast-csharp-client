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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using Hazelcast.Core;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Configuration;
using Hazelcast.Testing.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Cloud
{
    [TestFixture]
    [Explicit("Has special requirements, see comments in code.")]
    public class CloudTests : SingleMemberClientRemoteTestBase
    {
        // REQUIREMENTS
        //
        // 1. a working Cloud environment
        //    browse to https://cloud.hazelcast.com/ to create an environment
        //    (or to one of the internal Hazelcast test clouds)
        //
        // 2. parameters for this environment, configured as Visual Studio secrets,
        //    with a specific key indicated by the following constant. The secrets
        //    file would then need to contain a section looking like:
        //      {
        //          "cloud-test": {
        //              "clusterName": "<cluster-name>",
        //              "networking": {
        //                  "cloud": {
        //                      "discoveryToken": "<token>",
        //                      "url": "<cloud-url>"
        //                  }
        //              }
        //          },
        //      }
        //
        private const string SecretsKey = "cloud-test";
        //
        // 3. the number of put/get iterations + how long to wait between each iteration
        private const int IterationCount = 60;
        private const int IterationPauseMilliseconds = 100;

        [Test]
        public async Task SampleClient()
        {
            HConsole.Configure(options => options.ConfigureDefaults(this));

            HConsole.WriteLine(this, "Hazelcast Cloud Client");
            var stopwatch = Stopwatch.StartNew();

            HConsole.WriteLine(this, "Build options...");
            var options = new HazelcastOptionsBuilder()
                .WithHConsoleLogger()
                .With("Logging:LogLevel:Hazelcast", "Debug")
                .WithUserSecrets(GetType().Assembly, SecretsKey)
                .Build();

            // log level must be a valid Microsoft.Extensions.Logging.LogLevel value
            //   Trace | Debug | Information | Warning | Error | Critical | None

            // enable metrics
            options.Metrics.Enabled = true;

            // enable reconnection
            options.Networking.ReconnectMode = ReconnectMode.ReconnectSync;

            // instead of using Visual Studio secrets, configuration via code is
            // possible, by uncommenting some of the blocks below - however, this
            // is not recommended as it increases the risk of leaking private
            // infos in a Git repository.

            // uncomment to run on localhost
            /*
            options.Networking.Addresses.Clear();
            options.Networking.Addresses.Add("localhost:5701");
            options.ClusterName = "dev";
            */

            // uncomment to run on cloud
            /*
            options.ClusterName = "...";
            options.Networking.Cloud.DiscoveryToken = "...";
            options.Networking.Cloud.Url = new Uri("https://...");
            */

            HConsole.WriteLine(this, "Get and connect client...");
            HConsole.WriteLine(this, $"Connect to cluster \"{options.ClusterName}\"{(options.Networking.Cloud.Enabled ? " (cloud)" : "")}");
            if (options.Networking.Cloud.Enabled) HConsole.WriteLine(this, $"Cloud Discovery Url: {options.Networking.Cloud.Url}");
            var client = await HazelcastClientFactory.StartNewClientAsync(options).ConfigureAwait(false);

            HConsole.WriteLine(this, "Get map...");
            var map = await client.GetMapAsync<string, string>("map").ConfigureAwait(false);

            HConsole.WriteLine(this, "Put value into map...");
            await map.PutAsync("key", "value").ConfigureAwait(false);

            HConsole.WriteLine(this, "Get value from map...");
            var value = await map.GetAsync("key").ConfigureAwait(false);

            HConsole.WriteLine(this, "Validate value...");
            if (!value.Equals("value"))
            {
                HConsole.WriteLine(this, "Error: check your configuration.");
                return;
            }

            HConsole.WriteLine(this, "Put/Get values in/from map with random values...");
            var random = new Random();
            var step = IterationCount / 10;
            for (var i = 0; i < IterationCount; i++)
            {
                var randomValue = random.Next(100_000);
                await map.PutAsync("key_" + randomValue, "value_" + randomValue).ConfigureAwait(false);

                randomValue = random.Next(100_000);
                await map.GetAsync("key" + randomValue).ConfigureAwait(false);

                if (i % step == 0)
                {
                    HConsole.WriteLine(this, $"[{i:D3}] map size: {await map.GetSizeAsync().ConfigureAwait(false)}");
                }

                if (IterationPauseMilliseconds > 0)
                    await Task.Delay(IterationPauseMilliseconds).ConfigureAwait(false);
            }

            HConsole.WriteLine(this, "Destroy the map...");
            await map.DestroyAsync().ConfigureAwait(false);

            HConsole.WriteLine(this, "Dispose map...");
            await map.DisposeAsync().ConfigureAwait(false);

            HConsole.WriteLine(this, "Dispose client...");
            await client.DisposeAsync().ConfigureAwait(false);

            HConsole.WriteLine(this, $"Done (elapsed: {stopwatch.Elapsed.ToString("hhmmss\\.fff\\ ", CultureInfo.InvariantCulture)}).");
        }
    }
}
