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
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Examples.Client
{
    // ReSharper disable once UnusedMember.Global
    public class SimpleExample
    {
        //
        // this is a complete example of a simple console application where
        // every component is configured and created explicitly.
        //
        // configuration (https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration)
        //
        //   environment
        //     optional Hazelcast.Build(...) 'environmentName' argument: Development, Staging, Production (default), ...
        //     falls back to DOTNET_ENVIRONMENT + ASPNETCORE_ENVIRONMENT environment variables
        //     determines <env>, default is Production
        //
        //   configuration file
        //     appsettings.json
        //     appsettings.<env>.json
        //     hazelcast.json
        //     hazelcast.<env>.json
        //
        //     {
        //       "hazelcast": {
        //         "networking": {
        //           "addresses": [ "server:port" ]
        //         }
        //       }
        //     }
        //
        //   environment variables
        //     hazelcast__networking__addresses__0=server:port (standard .NET)
        //     hazelcast.networking.addresses.0=server:port (hazelcast-specific)
        //
        //   command line
        //     hazelcast:networking:addresses:0=server:port (standard .NET)
        //     hazelcast.networking.addresses.0=server:port (hazelcast-specific)
        //

        public static async Task Main(string[] args)
        {
            // build options
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .Build();

            // create a logger, a client factory and a client
            var logger = options.LoggerFactory.Service.CreateLogger<Worker>();
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options); // disposed when method exits

            // create the worker, and run
            var worker = new Worker(client, logger);
            await worker.RunAsync();
        }

        public class Worker
        {
            private readonly IHazelcastClient _client;
            private readonly ILogger<Worker> _logger;

            public Worker(IHazelcastClient client, ILogger<Worker> logger)
            {
                _client = client;
                _logger = logger;
            }

            public async Task RunAsync()
            {
                _logger.LogDebug("debug");
                _logger.LogInformation("debug");
                _logger.LogWarning("debug");

                await using var map = await _client.GetMapAsync<string, int>("test-map");

                await map.SetAsync("key", 42);
                var value = await map.GetAsync("key");
                if (value != 42) throw new Exception("Error!");

                Console.WriteLine("It worked.");

                // destroy the map
                await _client.DestroyAsync(map);
            }
        }
    }
}
