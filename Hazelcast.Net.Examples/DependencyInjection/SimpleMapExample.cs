﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Examples.DependencyInjection
{
    // ReSharper disable once UnusedMember.Global
    public class SimpleMapExample : ExampleBase
    {
        public static async Task Run(params string[] args)
        {
            var services = new ServiceCollection();

            // build the configuration
            // alternatively, configuration may from from a 'host'
            // but even then, it is important to AddHazelcast()
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddHazelcast(args);
            IConfiguration configuration = configurationBuilder.Build();

            // add hazelcast to services
            // this adds the options + the client factory
            // this also wires logging to get
            services.AddHazelcast(configuration);

            // add logging the normal way
            services.AddLogging(builder => builder.AddConfiguration(configuration.GetSection("logging")).AddConsole());

            // wire a client
            services.AddSingleton(provider => provider.GetService<HazelcastClientFactory>().CreateClient());

            // configure (can do it multiple times..)
            services.Configure<HazelcastOptions>(options =>
            {
                options.Networking.ConnectionTimeoutMilliseconds = 2_000;
            });

            services.AddTransient<A>();

            // create the service provider, get the object and run
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
