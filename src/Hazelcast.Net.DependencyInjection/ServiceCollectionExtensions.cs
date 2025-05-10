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
        /// Add <see cref="HazelcastOptions"/> to the service provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="configure">An <see cref="Action{HazelcastOptionsBuilder}"/> to configure the provided <see cref="HazelcastOptionsBuilder"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddHazelcastOptions(this IServiceCollection services, Action<HazelcastOptionsBuilder> configure)
        {
            services.AddOptions();

            // register the factory that will instantiate & configure the options instance
            services.AddSingleton<IOptionsFactory<HazelcastOptions>>(serviceProvider => new HazelcastOptionsFactory(serviceProvider, configure));
            return services;
        }

        /// <summary>
        /// Add <see cref="HazelcastFailoverOptions"/> to the service provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="configure">An <see cref="Action{HazelcastFailoverOptionsBuilder}"/> to configure the provided <see cref="HazelcastFailoverOptionsBuilder"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddHazelcastFailoverOptions(this IServiceCollection services, Action<HazelcastFailoverOptionsBuilder> configure)
        {
            services.AddOptions();

            // register the factory that will instantiate & configure the options instance
            services.AddSingleton<IOptionsFactory<HazelcastFailoverOptions>>(serviceProvider => new HazelcastFailoverOptionsFactory(serviceProvider, configure));
            return services;
        }

        /// <summary>
        /// Adds <see cref="IHazelcastClient"/> to the service provider.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddHazelcast(this IServiceCollection services, IConfiguration configuration)
        {
            // wire the Hazelcast-specific configuration
            services.AddOptions();
            services.AddHazelcastOptions(builder => builder.AddConfiguration(configuration));

            // wire creators
            services.Configure<HazelcastOptions>(options =>
            {
                // assumes that the ILoggerFactory has been registered in the container
                options.ObtainLoggerFactoryFromServiceProvider();

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
            // wire the Hazelcast-specific configuration
            services.AddOptions();
            services.AddHazelcastFailoverOptions(builder => builder.AddConfiguration(configuration));

            // wire creators
            services.Configure<HazelcastFailoverOptions>(options => options.ObtainLoggerFactoryFromServiceProvider());

            return services;
        }
    }
}
