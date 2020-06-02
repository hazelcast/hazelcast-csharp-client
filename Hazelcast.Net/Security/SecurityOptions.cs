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
using System.Collections.Generic;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Exceptions;

namespace Hazelcast.Security
{
    /// <summary>
    /// Represents the security options.
    /// </summary>
    public class SecurityOptions
    {
        private string _credentialsFactoryType;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityOptions"/> class.
        /// </summary>
        public SecurityOptions()
        {
            // FIXME these could move up to the HazelcastOptions in some sort of DI-like thing
            // but what about... do we have more?

            CredentialsFactory = new ServiceFactory<ICredentialsFactory>();
            CredentialsFactoryArgs = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets or sets the credentials factory factory.
        /// </summary>
        public ServiceFactory<ICredentialsFactory> CredentialsFactory { get; private set; }

        /// <summary>
        /// Gets or sets the type of the credentials factory.
        /// </summary>
        /// <remarks>
        /// <para>Returns the correct value only if it has been set via the same property. If the
        /// credentials factory has been configured via code and the <see cref="CredentialsFactory"/>
        /// property, the value returned by this property is unspecified.</para>
        /// </remarks>
        public string CredentialsFactoryType
        {
            get => _credentialsFactoryType;

            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(value));

                _credentialsFactoryType = value;

                CredentialsFactory.Creator = () => Services.CreateInstance<ICredentialsFactory>(value, this);
            }
        }

        /// <summary>
        /// Gets the arguments for the credentials factory.
        /// </summary>
        /// <remarks>
        /// <para>Arguments are used when creating a credentials factory from its type as set
        /// via the <see cref="CredentialsFactoryType"/> property. They are ignored if the
        /// credentials factory has been configured via code and the <see cref="CredentialsFactory"/>
        /// property.</para>
        /// </remarks>
        public Dictionary<string, object> CredentialsFactoryArgs { get; private set; }

        /// <summary>
        /// Configures Kerberos as the authentication mechanism.
        /// </summary>
        /// <param name="spn">The service principal name of the Hazelcast cluster.</param>
        /// <returns>The security options.</returns>
        public SecurityOptions ConfigureKerberosCredentials(string spn)
        {
            CredentialsFactory.Creator = () => new KerberosCredentialsFactory(spn);
            return this;
        }

        /// <summary>
        /// Configures a user name and password as the authentication mechanism.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <returns>The security options.</returns>
        private SecurityOptions ConfigurePasswordCredentials(string username, string password)
        {
            var credentials = new UsernamePasswordCredentials { Name = username, Password = password };
            CredentialsFactory.Creator = () => new StaticCredentialsFactory(credentials);
            return this;
        }

        /// <summary>
        /// Configures static credentials as the authentication mechanism.
        /// </summary>
        /// <param name="credentials">Credentials.</param>
        /// <returns>The security options.</returns>
        private SecurityOptions ConfigureCredentials(ICredentials credentials)
        {
            CredentialsFactory.Creator = () => new StaticCredentialsFactory(credentials);
            return this;
        }

        /// <summary>
        /// Configures a static token as the authentication mechanism.
        /// </summary>
        /// <param name="token">A token.</param>
        /// <returns>The security configuration.</returns>
        private SecurityOptions ConfigureTokenCredentials(byte[] token)
        {
            return ConfigureCredentials(new TokenCredentials(token));
        }

        /// <summary>
        /// Clone the options.
        /// </summary>
        public SecurityOptions Clone()
        {
            return new SecurityOptions
            {
                _credentialsFactoryType = _credentialsFactoryType,
                CredentialsFactory = CredentialsFactory.Clone(),
                CredentialsFactoryArgs = new Dictionary<string, object>(CredentialsFactoryArgs)
            };
        }
    }
}
