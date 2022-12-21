// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hazelcast.Examples.Client
{
    // ReSharper disable once UnusedMember.Global
    public class HostedExample
    {
        //
        // this is a complete example of a hosted service using dependency injection
        //
        // configuration (https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration)
        //
        //   environment
        //     environment variable DOTNET_ENVIRONMENT + ASPNETCORE_ENVIRONMENT: Development, Staging, Production (default), ...
        //     determines <env>
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
            // runs until stopped with Ctrl+C
            //await CreateHostBuilder(args).Build().RunAsync();

            // runs for some time
            var cancel = new CancellationTokenSource(4_000);
            await CreateHostBuilder(args).Build().RunAsync(cancel.Token);
        }

        // note: CreateDefaultBuilder registers a default logging setup
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHazelcast(args)
                .ConfigureServices((hostingContext, services) =>
                {
                    // add Hazelcast services
                    services.AddHazelcast(hostingContext.Configuration);

                    // this is how options could be altered
                    services.Configure<HazelcastOptions>(options =>
                    {
                        //options.Labels.Add("test");
                    });

                    // add a hosted service
                    services.AddHostedService<Worker>();
                });

        // worker (background) service
        public class Worker : IHostedService
        {
            private readonly ILogger<Worker> _logger;

            private Task _running;
            private CancellationTokenSource _cancel;
            private HazelcastOptions _options;

            public Worker(ILogger<Worker> logger, IOptions<HazelcastOptions> options)
            {
                _logger = logger;
                _options = options.Value;
                // just to show how to get and log options
                _logger.LogInformation($"Create worker (client name: \"{options.Value.ClientName}\", cluster name: \"{options.Value.ClusterName}\")");
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("Starting...");

                // open a client
                var client = await HazelcastClientFactory.StartNewClientAsync(_options, cancellationToken);

                // start the running task
                _cancel = new CancellationTokenSource();
                _running = RunAsync(client, _cancel.Token);

                _logger.LogInformation("Started.");
            }

            private static async Task RunAsync(IHazelcastClient client, CancellationToken cancellationToken)
            {
                // 'await using' ensure that both the client and the map will be disposed before the method returns
                await using var c = client;
                await using var map = await client.GetMapAsync<string, int>("test-map");

                // loop while not canceled
                while (!cancellationToken.IsCancellationRequested)
                {
                    // pretend to do some work
                    var i = await map.GetAsync("foo");
                    i += 1;
                    await map.SetAsync("foo", i);
                    Console.WriteLine(i);

                    try
                    {
                        await Task.Delay(1000, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // expected
                    }
                }

                await client.DestroyAsync(map);
            }

            public async Task StopAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("Stopping...");

                // cancel and await the running task
                _cancel.Cancel();
                await _running;

                _logger.LogInformation("Stopped.");
            }
        }
    }
}
