using System;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Configuration;
using Hazelcast.Configuration.Binding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Hazelcast.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Examples.DependencyInjection
{
    public class SimpleMapExample : ExampleBase
    {
        public static async Task Run(params string[] args)
        {
            var services = new ServiceCollection();

            // build the configuration
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddHazelcast(args);
            IConfiguration configuration = configurationBuilder.Build();

            // add hazelcast to services
            // this adds the configuration + the client factory
            services.AddHazelcast(configuration);

            // wire a client
            services.AddSingleton(provider => provider.GetService<HazelcastClientFactory>().CreateClient());

            // configure (can do it multiple times..)
            services.Configure<HazelcastOptions>(options =>
            {
                options.Network.ConnectionTimeoutMilliseconds = 2_000;
            });

            services.AddTransient<A>();

            services.AddLogging(builder => builder.AddConfiguration(configuration.GetSection("logging")).AddConsole());

            services.AddSingleton<IAuthenticator, Authenticator>();

            var serviceProvider = services.BuildServiceProvider();

            var a = serviceProvider.GetService<A>();
            await a.RunAsync();
        }

        public class A
        {
            private readonly IHazelcastClient _client;
            private readonly ILogger _logger;

            public A(IHazelcastClient client, ILoggerFactory loggerFactory)
            {
                _client = client;
                _logger = loggerFactory.CreateLogger<A>();
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
                await _client.OpenAsync();

                // get distributed map from cluster
                var map = await _client.GetMapAsync<string, string>("simple-example");

                await map.AddOrReplaceAsync("key", "value");
                var value = await map.GetAsync("key");
                if (value != "value") throw new Exception("Error!");

                // destroy the map
                map.Destroy();
            }
        }
    }
}
