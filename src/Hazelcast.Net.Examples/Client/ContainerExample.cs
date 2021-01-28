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
using Hazelcast.Configuration;
using Hazelcast.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hazelcast.Examples.Client
{
    // ReSharper disable once UnusedMember.Global
    public class ContainerExample
    {
        //
        // this is a complete example of a console application using dependency injection
        //
        // configuration (https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration)
        //
        //   environment
        //     optional configurationBuilder.AddHazelcast(...) 'environmentName' argument: Development, Staging, Production (default), ...
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
            // create a service collection
            var services = new ServiceCollection();

            // build the IConfiguration
            var configuration = new ConfigurationBuilder()
                .AddDefaults(args) // add default configuration (appsettings.json, etc)
                .AddHazelcast(args) // add Hazelcast-specific configuration
                .Build();

            // add hazelcast to services
            // this adds the options, and also wires logging to get a logger from the container
            services.AddHazelcast(configuration);

            // add logging to the container, the normal way
            services.AddLogging(builder => builder.AddConfiguration(configuration.GetSection("logging")).AddConsole());

            // add Hazelcast client custom factory to the container
            services.AddSingleton<Factory>();

            // configure hazelcast (can do it multiple times..)
            services.Configure<HazelcastOptions>(options =>
            {
                options.Networking.TimeoutMilliseconds = 45_000;
            });

            // add the worker - which uses the factory to get a client (async)
            services.AddTransient<Worker>();

            // create the service provider
            // will be disposed before the method exits
            // which will dispose (and shutdown) the Hazelcast client
            await using var serviceProvider = services.BuildServiceProvider();

            // gets the worker from the container, and run
            var a = serviceProvider.GetRequiredService<Worker>();
            await a.RunAsync();
        }

        public class Factory
        {
            // in real-life scenario we may want to cache the client
            // instead of creating a new one all the time...

            private readonly HazelcastOptions _options;

            public Factory(IOptions<HazelcastOptions> options)
            {
                _options = options.Value;
            }

            public ValueTask<IHazelcastClient> StartClientAsync()
                => HazelcastClientFactory.StartNewClientAsync(_options);
        }

        public class Worker
        {
            private readonly Factory _clientFactory;
            private readonly ILogger<Worker> _logger;

            public Worker(Factory clientFactory, ILogger<Worker> logger)
            {
                _clientFactory = clientFactory;
                _logger = logger;
            }

            public async Task RunAsync()
            {
                _logger.LogDebug("debug");
                _logger.LogInformation("debug");
                _logger.LogWarning("debug");

                await using var client = await _clientFactory.StartClientAsync();

                await using var map = await client.GetMapAsync<string, int>("test-map");

                await map.SetAsync("key", 42);
                var value = await map.GetAsync("key");
                if (value != 42) throw new Exception("Error!");

                Console.WriteLine("It worked.");

                // destroy the map
                await client.DestroyAsync(map);
            }
        }
    }
}
