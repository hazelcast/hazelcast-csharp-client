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

using System.Collections.Generic;
using Hazelcast.Configuration.Binding;
using Hazelcast.Core;
using Hazelcast.Security;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents authentication options.
    /// </summary>
    public class AuthenticationOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationOptions"/> class.
        /// </summary>
        public AuthenticationOptions()
        {
            Authenticator = new SingletonServiceFactory<IAuthenticator>();
            CredentialsFactory = new SingletonServiceFactory<ICredentialsFactory>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationOptions"/> class.
        /// </summary>
        private AuthenticationOptions(AuthenticationOptions other)
        {
            Authenticator = other.Authenticator.Clone();
            CredentialsFactory = other.CredentialsFactory.Clone();
        }

        /// <summary>
        /// Gets the authenticator service factory.
        /// </summary>
        [BinderIgnore]
        public SingletonServiceFactory<IAuthenticator> Authenticator { get; }

        [BinderName("authenticator")]
        [BinderIgnore(false)]
#pragma warning disable IDE0051 // Remove unused private members
        // ReSharper disable once UnusedMember.Local
        private InjectionOptions AuthenticatorBinder
#pragma warning restore IDE0051 // Remove unused private members
        {
            get => default;
            set
            {
                Authenticator.Creator = () => ServiceFactory.CreateInstance<IAuthenticator>(value.TypeName, null, this);
            }
        }

        /// <summary>
        /// Gets the credentials factory service factory.
        /// </summary>
        [BinderIgnore]
        public SingletonServiceFactory<ICredentialsFactory> CredentialsFactory { get; }

        [BinderName("credentialsFactory")]
        [BinderIgnore(false)]
#pragma warning disable IDE0051 // Remove unused private members
        // ReSharper disable once UnusedMember.Local
        private InjectionOptions CredentialsFactoryBinder
#pragma warning restore IDE0051 // Remove unused private members
        {
            get => default;
            set => CredentialsFactory.Creator = () => ServiceFactory.CreateInstance<ICredentialsFactory>(value.TypeName, value.Args);
        }

        /// <summary>
        /// Configures Kerberos as the authentication mechanism.
        /// </summary>
        /// <param name="spn">The service principal name of the Hazelcast cluster.</param>
        /// <returns>The security options.</returns>
        public AuthenticationOptions ConfigureKerberosCredentials(string spn)
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
        private AuthenticationOptions ConfigureUsernamePasswordCredentials(string username, string password)
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
        private AuthenticationOptions ConfigureCredentials(ICredentials credentials)
        {
            CredentialsFactory.Creator = () => new StaticCredentialsFactory(credentials);
            return this;
        }

        /// <summary>
        /// Configures a static token as the authentication mechanism.
        /// </summary>
        /// <param name="token">A token.</param>
        /// <returns>The security configuration.</returns>
        private AuthenticationOptions ConfigureTokenCredentials(byte[] token)
        {
            return ConfigureCredentials(new TokenCredentials(token));
        }

        /// <summary>
        /// Clone the options.
        /// </summary>
        internal AuthenticationOptions Clone() => new AuthenticationOptions(this);
    }
}
