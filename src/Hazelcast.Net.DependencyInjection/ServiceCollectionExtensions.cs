// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Hazelcast.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to the <see cref="IServiceCollection"/> interface.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Hazelcast services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddHazelcast(this IServiceCollection services, IConfiguration configuration)
        {
            configuration = configuration.GetSection("hazelcast");

            // wire the Hazelcast-specific configuration
            services.AddOptions();
            services.AddSingleton<IOptionsChangeTokenSource<HazelcastOptions>>(new ConfigurationChangeTokenSource<HazelcastOptions>(string.Empty, configuration));
            services.AddSingleton<IConfigureOptions<HazelcastOptions>>(new HazelcastNamedConfigureFromConfigurationOptions(string.Empty, configuration));

            // wire the HazelcastOptions which will be injected in the HazelcastClientFactory.
            // the main library is not DI-aware and therefore does not expose a constructor
            // accepting IOptions<>, and in addition we want to inject the service provider in
            // the options so that service factory creators can use it.
            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<HazelcastOptions>>().Value;
                options.ServiceProvider = provider; // required by factories
                return options;
            });

            // wire creators
            services.Configure<HazelcastOptions>(options =>
            {
                // assumes that the ILoggerFactory has been registered in the container
                options.LoggerFactory.ServiceProvider = options.ServiceProvider;

                // we could do it for others but we cannot assume that users want all other services
                // wired through dependency injection - so... this is just an example of how we would
                // do it for the authenticator - but we are not going to do any here
                //options.Authentication.Authenticator.Creator = () => options.ServiceProvider.GetRequiredService<IAuthenticator>();
            });

            return services;
        }
    }
}
