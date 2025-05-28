// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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

// this is an example of debugging a client by enabling verbose logging & output
// (was initially created to troubleshoot duplicate connections to docker servers)
//
// docker: docker run -p 5701:5701 hazelcast/hazelcast:latest-snapshot
// build: hz -d HZ_CONSOLE,HZ_CONSOLE_PUBLIC -c Debug build
// run: src/Hazelcast.Net.Examples/bin/Debug/netcoreapp3.1/hx Client.VerboseClient
//
// in order to build, the HZ_CONSOLE and HZ_CONSOLE_PUBLIC symbol must be defined,
// in Directory.Build.props, for the proper build target.

#if HZ_CONSOLE_PUBLIC

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Hazelcast.Core;
using Hazelcast.Testing.Logging;

namespace Hazelcast.Examples.Client
{
    public static class VerboseClientExample
    {
        public static async Task Main(string[] args)
        {
            var h = new object();
            using var _ = HConsole.Capture(options => options
                .ClearAll()
                .Configure().SetLevel(1)
                .Configure(h).SetPrefix("PROGRAM")
                .Configure<AsyncContext>().SetMinLevel()
                .Configure("Hazelcast.Networking.SocketConnectionBase").SetIndent(1).SetLevel(0).SetPrefix("SOCKET")
                .Configure("Hazelcast.Clustering.MemberConnection").SetLevel(1)
            );

            var options = new HazelcastOptionsBuilder()
                .With(args)
                .With("Logging:LogLevel:Hazelcast", "Debug")
                .WithHConsoleLogger()
                .Build();

            options.Networking.Addresses.Clear();
            options.Networking.Addresses.Add("127.0.0.1:5701");
            options.ClusterName = "dev";

            var logger = options.LoggerFactory.Service.CreateLogger<Program>();
            logger.LogInformation("Begin.");

            logger.LogInformation("Start client...");
            var hz = await HazelcastClientFactory.StartNewClientAsync(options);

            logger.LogWarning("Wait...");
            await Task.Delay(1000);

            logger.LogWarning("Dispose client...");
            await hz.DisposeAsync();

            logger.LogWarning("End.");
            options.LoggerFactory.Service.Dispose();
        }

        // configure logging with an HConsole logger (internal for troubleshooting purposes)
        private static HazelcastOptionsBuilder WithHConsoleLogger(this HazelcastOptionsBuilder builder)
        {
            return builder
                .With("Logging:LogLevel:Default", "Debug")
                .With("Logging:LogLevel:System", "Information")
                .With("Logging:LogLevel:System", "Information")
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

#endif
