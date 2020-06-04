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

            // wire the HazelcastOptions which will be injected in the HazelcastClientFactory.
            // the main library is not DI-aware and therefore does not expose a constructor
            // accepting IOptions<>, and in addition we want to inject the service provider in
            // the options so that service factory creators can use it.
            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<HazelcastOptions>>().Value;
                options.ServiceProvider = provider; // required by creators
                return options;
            });

            // wire the client factory
            services.AddSingleton<HazelcastClientFactory>();

            // wire creators
            services.Configure<HazelcastOptions>(options =>
            {
                // assumes that the ILoggerFactory has been registered in the container
                options.Logging.LoggerFactory.Creator = () => options.ServiceProvider.GetRequiredService<ILoggerFactory>();

                // we could do it for others but we cannot assume that users want all other services
                // wired through dependency injection - so... this is just an example of how we would
                // do it for the authenticator - but we are not going to do any here
                //options.Authentication.Authenticator.Creator = () => options.ServiceProvider.GetRequiredService<IAuthenticator>();
            });

            return services;
        }
    }
}
