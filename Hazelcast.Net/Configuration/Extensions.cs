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

using System;
using Hazelcast.Clustering;
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
        /// <param name="configuration">The client configuration.</param>
        /// <param name="configure">The delegate for configuring the <see cref="SecurityConfiguration"/>.</param>
        /// <returns>The client configuration.</returns>
        public static HazelcastConfiguration ConfigureSecurity(this HazelcastConfiguration configuration, Action<SecurityConfiguration> configure)
        {
            configure(configuration.Security);
            return configuration;
        }

        /// <summary>
        /// Configures Kerberos as the authentication mechanism.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="spn">The service principal name of the Hazelcast cluster.</param>
        /// <returns>The configuration.</returns>
        public static SecurityConfiguration ConfigureKerberosCredentials(this SecurityConfiguration configuration, string spn)
        {
            configuration.CredentialsFactory.Creator = () => new KerberosCredentialsFactory(spn);
            return configuration;
        }

        /// <summary>
        /// Configures a user name and password as the authentication mechanism.
        /// </summary>
        /// <param name="configuration">The security configuration.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <returns>The security configuration.</returns>
        private static SecurityConfiguration ConfigurePasswordCredentials(this SecurityConfiguration configuration, string username, string password)
        {
            var credentials = new UsernamePasswordCredentials { Name = username, Password = password };
            configuration.CredentialsFactory.Creator = () => new StaticCredentialsFactory(credentials);
            return configuration;
        }

        /// <summary>
        /// Configures static credentials as the authentication mechanism.
        /// </summary>
        /// <param name="config">The security configuration.</param>
        /// <param name="credentials">Credentials.</param>
        /// <returns>The security configuration.</returns>
        private static SecurityConfiguration ConfigureCredentials(this SecurityConfiguration configuration, ICredentials credentials)
        {
            configuration.CredentialsFactory.Creator = () => new StaticCredentialsFactory(credentials);
            return configuration;
        }

        /// <summary>
        /// Configures a static token as the authentication mechanism.
        /// </summary>
        /// <param name="config">The security configuration.</param>
        /// <param name="token">A token.</param>
        /// <returns>The security configuration.</returns>
        private static SecurityConfiguration ConfigureTokenCredentials(this SecurityConfiguration config, byte[] token)
        {
            return config.ConfigureCredentials(new TokenCredentials(token));
        }
    }
}
