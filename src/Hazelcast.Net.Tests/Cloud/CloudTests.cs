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
using Hazelcast.Core;
using Hazelcast.Testing.Logging;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Cloud
{
    [TestFixture]
    public class CloudTests
    {
        private const int IterationCount = 600;
        private const int IterationPauseMilliseconds = 10;

        [Test]
        public async Task SampleClient()
        {
            using var _ = HConsoleForTest();

            HConsole.WriteLine(this, "Hazelcast Cloud Client");
            var stopwatch = Stopwatch.StartNew();

            HConsole.WriteLine(this, "Build options...");
            var options = new HazelcastOptionsBuilder()
                //.With(args)
                .WithHConsoleLogger()
                .With("Logging:LogLevel:Hazelcast", "Debug")
                .Build();

            // log level must be a valid Microsoft.Extensions.Logging.LogLevel value
            //   Trace | Debug | Information | Warning | Error | Critical | None

            // enable metrics
            options.Metrics.Enabled = true;

            // set the cluster name
            options.ClusterName = "stephan-cloud-test-2";

            // set the cloud discovery token and url
            options.Networking.Cloud.DiscoveryToken = "T2dJx1RJzpdW1jyYIJVQjt8RgkA8tk8I981utdZiFooWT9sKKC";
            options.Networking.Cloud.Url = new Uri("https://uat.hazelcast.cloud");

            HConsole.WriteLine(this, "Get and connect client...");
            var client = await HazelcastClientFactory.StartNewClientAsync(options);

            HConsole.WriteLine(this, "Get map...");
            var map = await client.GetMapAsync<string, string>("map");

            HConsole.WriteLine(this, "Put value into map...");
            await map.PutAsync("key", "value");

            HConsole.WriteLine(this, "Get value from map...");
            var value = await map.GetAsync("key");

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
                await map.PutAsync("key_" + randomValue, "value_" + randomValue);

                randomValue = random.Next(100_000);
                await map.GetAsync("key" + randomValue);

                if (i % step == 0)
                {
                    HConsole.WriteLine(this, $"[{i:D3}] map size: {await map.GetSizeAsync()}");
                }

                if (IterationPauseMilliseconds > 0)
                    await Task.Delay(IterationPauseMilliseconds);
            }

            HConsole.WriteLine(this, "Destroy the map...");
            await map.DestroyAsync();

            HConsole.WriteLine(this, "Dispose map...");
            await map.DisposeAsync();

            HConsole.WriteLine(this, "Dispose client...");
            await client.DisposeAsync();

            HConsole.WriteLine(this, $"Done (elapsed: {stopwatch.Elapsed.ToString("hhmmss\\.fff\\ ", CultureInfo.InvariantCulture)}).");
        }

        private IDisposable HConsoleForTest()

            => HConsole.Capture(options => options
                .ClearAll()

                //.Set<AsyncContext>(x => x.Quiet())
                //.Set<SocketConnectionBase>(x => x.SetIndent(1).SetLevel(0).SetPrefix("SOCKET"))
                
                .Set<HConsoleLoggerProvider>(x => x.SetPrefix("LOG").Verbose())
                .Set(x => x.Quiet().EnableTimeStamp(origin: DateTime.Now))
                .Set(this, x => x.Verbose().SetPrefix("TEST"))
            );
    }

    public static class CloudTestsExtensions
    {
        public static HazelcastOptionsBuilder WithHConsoleLogger(this HazelcastOptionsBuilder builder)
        {
            return builder
                .With("Logging:LogLevel:Default", "None")
                .With("Logging:LogLevel:System", "None")
                .With("Logging:LogLevel:Microsoft", "None")
                .With((configuration, options) =>
                {
                    // configure logging factory and add the console provider
                    options.LoggerFactory.Creator = () => LoggerFactory.Create(loggingBuilder =>
                        loggingBuilder
                            .AddConfiguration(configuration.GetSection("logging"))
                            .AddHConsole());
                });
        }
    }
}
