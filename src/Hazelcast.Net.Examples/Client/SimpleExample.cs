using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Examples.Client
{
    // ReSharper disable once UnusedMember.Global
    public class SimpleExample : ExampleBase
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

        public static async Task Run(string[] args)
        {
            // build options
            var options = HazelcastOptions.Build(args);

            // build a console logger factory from scratch
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            // and register the logger factory in the options
            options.Logging.LoggerFactory.Creator = () => loggerFactory;

            // create a logger, a client factory and a client
            var logger = loggerFactory.CreateLogger<Worker>();
            var factory = new HazelcastClientFactory(options);
            await using var client = factory.CreateClient(); // disposed when method exits

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

                // this is just an example - practically, connecting the client
                // would be managed elsewhere - and the class would expect to
                // receive a connected client - and, that 'elsewhere' would also
                // dispose the client, etc.
                await _client.StartAsync();

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
