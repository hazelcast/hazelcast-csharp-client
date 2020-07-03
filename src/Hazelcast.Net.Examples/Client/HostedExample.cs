using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Configuration;
using Hazelcast.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hazelcast.Examples.Client
{
    // ReSharper disable once UnusedMember.Global
    public class HostedExample : ExampleBase
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

        public static async Task Run(string[] args)
        {
            // runs until stopped with Ctrl+C
            //await CreateHostBuilder(args).Build().RunAsync();

            // runs for some time
            var cancel = new CancellationTokenSource(4_000);
            await CreateHostBuilder(args).Build().RunAsync(cancel.Token);
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, builder) =>
                {
                    // add Hazelcast-specific configuration
                    // (default configuration has been added by the host)
                    builder.AddHazelcast(args);

                    // example: change the hazelcast options file name
                    //builder.AddHazelcast(args, optionsFileName: "special.json");
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    // add Hazelcast services
                    services.AddHazelcast(hostingContext.Configuration);

                    // this is how options could be altered
                    services.Configure<HazelcastOptions>(options =>
                    {
                        options.ClientNamePrefix = "test.client_";
                    });

                    // add a hosted service
                    services.AddHostedService<Worker>();
                });

        // worker (background) service
        public class Worker : IHostedService
        {
            private readonly HazelcastClientFactory _factory;
            private readonly ILogger<Worker> _logger;

            private Task _running;
            private CancellationTokenSource _cancel;

            public Worker(HazelcastClientFactory factory, ILogger<Worker> logger, IOptions<HazelcastOptions> options)
            {
                _factory = factory;
                _logger = logger;

                // just to show how to get and log options
                _logger.LogInformation($"Create worker (client name: \"{options.Value.ClientName}\", cluster name: \"{options.Value.ClusterName}\")");
            }

            public async Task StartAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("Starting...");

                // open a client
                var client = _factory.CreateClient();
                await client.OpenAsync(cancellationToken);

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
                    await map.AddOrUpdateAsync("foo", i);
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
