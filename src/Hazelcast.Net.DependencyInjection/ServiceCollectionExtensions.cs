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
            configuration = configuration.GetSection(HazelcastOptions.SectionNameConstant);

            // wire the Hazelcast-specific configuration
            services.AddOptions();
            services.AddSingleton<IOptionsChangeTokenSource<HazelcastOptions>>(new ConfigurationChangeTokenSource<HazelcastOptions>(string.Empty, configuration));

            // register the HazelcastOptions, making sure that (1) HzBind is used to bind them, and (2) the
            // service provider is assigned so that service factories that require it (see logging below) can
            // use it
            services.AddSingleton<IConfigureOptions<HazelcastOptions>>(provider =>
                new HazelcastNamedConfigureFromConfigurationOptions<HazelcastOptions>(string.Empty, configuration, provider));

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

        /// <summary>
        /// Adds Hazelcast services for the failover mode.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddHazelcastFailover(this IServiceCollection services, IConfiguration configuration)
        {
            configuration = configuration.GetSection(HazelcastOptions.SectionNameConstant);

            // wire the Hazelcast-specific configuration
            services.AddOptions();
            services.AddSingleton<IOptionsChangeTokenSource<HazelcastFailoverOptions>>(new ConfigurationChangeTokenSource<HazelcastFailoverOptions>(string.Empty, configuration));

            // register the HazelcastOptions, making sure that (1) HzBind is used to bind them, and (2) the
            // service provider is assigned so that service factories that require it (see logging below) can
            // use it
            services.AddSingleton<IConfigureOptions<HazelcastFailoverOptions>>(provider =>
                new HazelcastNamedConfigureFromConfigurationOptions<HazelcastFailoverOptions>(string.Empty, configuration, provider));

            // wire creators
            services.Configure<HazelcastFailoverOptions>(options =>
            {
                // propagates the service provide + initialize the logger factory
                // assumes that the ILoggerFactory has been registered in the container
                foreach (var clusterOptions in options.Clients)
                {
                    clusterOptions.ServiceProvider = options.ServiceProvider;
                    clusterOptions.LoggerFactory.ServiceProvider = clusterOptions.ServiceProvider;
                }
            });

            return services;
        }
    }
}
