using Hazelcast.Clustering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

            // wire the HazelcastOptions
            // should be preferred to IOptions<HazelcastOptions> because of the service provider
            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<HazelcastOptions>>().Value;
                options.ServiceProvider = provider; // required by creators
                return options;
            });

            // wire configurations
            services.AddSingleton(provider => provider.GetRequiredService<HazelcastOptions>().Security);

            // wire the client factory
            services.AddSingleton<HazelcastClientFactory>();

            // wire creators
            services.Configure<HazelcastOptions>(options =>
            {
                options.Logging.LoggerFactory.Creator = () => options.ServiceProvider.GetRequiredService<ILoggerFactory>();
                options.Authentication.Authenticator.Creator = () => options.ServiceProvider.GetRequiredService<IAuthenticator>();

                // TODO: think!
                // when running without DI, everything comes from options
                // including instances of classes such as the authenticator
                // with DI we want them to come from DI
                // but how can the *same* code support both?
                // we'd have to pass everything from HazelcastClient to-level options, down to each class?
            });

            // creators for:
            // ILoggingFactory
            // IAuthenticator
            // ICredentialsFactory
            // ILoadBalancer
            // ISocketInterceptor
            // + all serialization stuff

            return services;
        }
    }
}
