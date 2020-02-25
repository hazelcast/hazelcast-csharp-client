using System;
using Hazelcast.Security;

namespace Hazelcast.Config
{
    /// <summary>
    /// Provides extension methods for configuring Kerberos.
    /// </summary>
    public static class KerberosExtensions
    {
        /// <summary>
        /// Configures Kerberos as the authentication mechanism.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="spn">The service principal name of the Hazelcast cluster.</param>
        /// <param name="timeout">The optional timeout for getting tickets from the KDC.</param>
        /// <returns>The configuration.</returns>
        public static ClientConfig ConfigureKerberosCredentials(this ClientConfig config, string spn, int timeout = 0)
        {
            return config.ConfigureSecurity(security => security.ConfigureKerberosCredentials(spn, timeout));
        }

        /// <summary>
        /// Configures Kerberos as the authentication mechanism.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="spn">The service principal name of the Hazelcast cluster.</param>
        /// <param name="timeout">The optional timeout for getting tickets from the KDC.</param>
        /// <returns>The configuration.</returns>
        public static ClientSecurityConfig ConfigureKerberosCredentials(this ClientSecurityConfig config, string spn, int timeout = 0)
        {
            return config.ConfigureCredentialsFactory(x =>
            {
                x.Implementation = new KerberosCredentialsFactory();
                x.Properties["spn"] = spn;
                x.Properties["timeout"] = timeout.ToString();
            });
        }

        // FIXME code below should be replaced by Asim's revised configuration mechanisms

        private static ClientConfig ConfigureSecurity(this ClientConfig config, Action<ClientSecurityConfig> configure)
        {
            var clientSecurityConfig = config.GetSecurityConfig();
            configure(clientSecurityConfig);
            return config;
        }

        private static ClientSecurityConfig ConfigureCredentialsFactory(this ClientSecurityConfig config, Action<CredentialsFactoryConfig> configure)
        {
            // FIXME should be lazy in config
            var credentialsFactoryConfig = config.CredentialsFactoryConfig 
                                           ?? (config.CredentialsFactoryConfig = new CredentialsFactoryConfig());
            configure(credentialsFactoryConfig);
            return config;
        }
    }
}
