using System;
using Hazelcast.Security;

namespace Hazelcast.Configuration
{
    /// <summary>
    /// Provides extension methods for configuring Kerberos.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Configures security.
        /// </summary>
        /// <param name="config">The client configuration.</param>
        /// <param name="configure">The delegate for configuring the <see cref="ClientSecurityConfig"/>.</param>
        /// <returns>The client configuration.</returns>
        public static ClientConfig ConfigureSecurity(this ClientConfig config, Action<ClientSecurityConfig> configure)
        {
            configure(config.GetSecurityConfig());
            return config;
        }

        /// <summary>
        /// Configures the credentials factory.
        /// </summary>
        /// <param name="config">The security configuration.</param>
        /// <param name="configure">The delegate for configuring the <see cref="CredentialsFactoryConfig"/>.</param>
        /// <returns>The security configuration.</returns>
        public static ClientSecurityConfig ConfigureCredentialsFactory(this ClientSecurityConfig config, Action<CredentialsFactoryConfig> configure)
        {
            configure(config.CredentialsFactoryConfig);
            return config;
        }

        /// <summary>
        /// Configures Kerberos as the authentication mechanism.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="spn">The service principal name of the Hazelcast cluster.</param>
        /// <returns>The configuration.</returns>
        public static ClientSecurityConfig ConfigureKerberosCredentials(this ClientSecurityConfig config, string spn)
        {
            return config.ConfigureCredentialsFactory(x =>
                x.Implementation = new KerberosCredentialsFactory(spn));
        }

        /// <summary>
        /// Configures a static credentials factory with a username and a password.
        /// </summary>
        /// <param name="config">The security configuration.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <returns>The security configuration.</returns>
        private static ClientSecurityConfig ConfigurePasswordCredentials(this ClientSecurityConfig config, string username, string password)
        {
            return config.ConfigureCredentials(new UsernamePasswordCredentials { Name = username, Password = password });
        }

        /// <summary>
        /// Configures a static credentials factory with supplied credentials.
        /// </summary>
        /// <param name="config">The security configuration.</param>
        /// <param name="credentials">Credentials.</param>
        /// <returns>The security configuration.</returns>
        private static ClientSecurityConfig ConfigureCredentials(this ClientSecurityConfig config, ICredentials credentials)
        {
            return config.ConfigureCredentialsFactory(x
                => x.Implementation = new StaticCredentialsFactory(credentials));
        }

        /// <summary>
        /// Configures a static token credentials factory with a supplied token.
        /// </summary>
        /// <param name="config">The security configuration.</param>
        /// <param name="token">A token.</param>
        /// <returns>The security configuration.</returns>
        private static ClientSecurityConfig ConfigureTokenCredentials(this ClientSecurityConfig config, byte[] token)
        {
            return config.ConfigureCredentials(new TokenCredentials(token));
        }
    }
}
