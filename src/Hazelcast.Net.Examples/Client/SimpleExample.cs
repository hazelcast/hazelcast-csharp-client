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
using System;
using System.Threading.Tasks;
using Hazelcast.Exceptions;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Examples.Client
{
    // ReSharper disable once UnusedMember.Global
    public class SimpleExample
    {
        
    /*
    This is a complete example of a simple console application where
    every component is configured and created explicitly.
    
    configuration (https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration)
    
      environment
        optional Hazelcast.Build(...) 'environmentName' argument: Development, Staging, Production (default), ...
        falls back to DOTNET_ENVIRONMENT + ASPNETCORE_ENVIRONMENT environment variables
        determines <env>, default is Production
    
      configuration file
        appsettings.json
        appsettings.<env>.json
        hazelcast.json
        hazelcast.<env>.json
    
        {
          "hazelcast": {
            "networking": {
              "addresses": [ "server:port" ]
            }
          }
        }
    
      environment variables
        hazelcast__networking__addresses__0=server:port (standard .NET)
        hazelcast.networking.addresses.0=server:port (hazelcast-specific)
    
      command line
        hazelcast:networking:addresses:0=server:port (standard .NET)
        hazelcast.networking.addresses.0=server:port (hazelcast-specific)
    
    the simplest way to run this example is to build the code:
     ./hz.ps1 build
    
    then to execute the example:
     ./hz.ps1 run-example Client.SimpleExample --- --hazelcast.networking.addresses.0=server.port
    
    it is possible to run more than once with --hazelcast.example.runCount=2
    the pause between the runs can be configured to 10s with --hazelcast.example.pauseDuration=00:00:10
    you may want to enable re-connection with --hazelcast.networking.reconnectMode=reconnectAsync
    it is possible to change the Hazelcast logging level from Information to anything else,
    with --Logging:LogLevel:Hazelcast=Debug (note that for this non-Hazelcast property, the dot-separator
    is not supported and a true .NET supported separator must be used - here, a colon)
    */

        public static async Task Main(string[] args)
        {
            // build options
            var exampleOptions = new ExampleOptions();

            var options = new HazelcastOptionsBuilder()
                .Bind("hazelcast:example", exampleOptions)
                .With(args)
                .WithConsoleLogger()
                .Build();

            // create a client
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options); // disposed when method exits

            // obtain a logger
            var logger = client.Options.LoggerFactory.CreateLogger<SimpleExample>();
            logger.LogDebug("Example of a debug Message");
            logger.LogInformation("Example of an info Message");
            logger.LogWarning("Example of a warning Message");

            // create a worker
            var worker = new Worker(client, client.Options.LoggerFactory);

            // end
            logger.LogInformation("Begin.");

            // run
            for (var i = 0; i < exampleOptions.RunCount; i++)
            {
                // pause?
                if (i > 0 && exampleOptions.PauseDuration > TimeSpan.Zero)
                {
                    logger.LogInformation("Wait...");
                    await Task.Delay(exampleOptions.PauseDuration).ConfigureAwait(false);
                }

                logger.LogInformation($"Run {i+1}...");
                try
                {
                    await worker.RunAsync().ConfigureAwait(false);
                }
                catch (ClientOfflineException)
                {
                    logger.LogWarning("Worker has failed, client is offline.");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Worker has failed.");
                }
            }

            // end
            logger.LogInformation("End.");
        }

        public class ExampleOptions
        {
            public TimeSpan PauseDuration { get; set; } = TimeSpan.Zero;

            public int RunCount { get; set; } = 1;
        }

        public class Worker
        {
            private readonly IHazelcastClient _client;
            private readonly ILogger _logger;

            public Worker(IHazelcastClient client, ILoggerFactory loggerFactory)
            {
                _client = client;
                _logger = loggerFactory.CreateLogger<Worker>();
            }

            public async Task RunAsync()
            {
                _logger.LogInformation("Begin run.");

                _logger.LogInformation("Get the map.");
                await using var map = await _client.GetMapAsync<string, int>("test-map").ConfigureAwait(false);

                // NOTE that regardless of ConfigureAwait(false) the map operations below may seem to
                // hand if the cluster is currently managing the loss of a member (i.e. if a member pod
                // was just deleted) because the *member* does not respond because it's presumably
                // dealing with the situation - nothing we can do nor need to fix at .NET level

                _logger.LogInformation("Set the value.");
                await map.SetAsync("key", 42).ConfigureAwait(false);

                _logger.LogInformation("Get the value.");
                var value = await map.GetAsync("key").ConfigureAwait(false);

                if (value != 42) throw new Exception("Error!");
                _logger.LogInformation("Got the value: it works!");

                // destroy the map
                _logger.LogInformation("Destroy the map.");
                await _client.DestroyAsync(map).ConfigureAwait(false);
                _logger.LogInformation("Destroyed the map.");

                _logger.LogInformation("End run.");
            }
        }
    }
}
