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
            CredentialsFactory = new SingletonServiceFactory<ICredentialsFactory>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationOptions"/> class.
        /// </summary>
        private AuthenticationOptions(AuthenticationOptions other)
        {
            TpcEnabled = other.TpcEnabled;
            CredentialsFactory = other.CredentialsFactory.Clone();
        }

        // internal option that carries Networking.Tpc.Enabled over for the time we have it
        [BinderIgnore]
        internal bool TpcEnabled { get; set; }

        /// <summary>
        /// Gets the <see cref="SingletonServiceFactory{TService}"/> for the <see cref="ICredentialsFactory"/>.
        /// </summary>
        /// <remarks>
        /// <para>When set in the configuration file, it is defined as an injected type, for instance:
        /// <code>
        /// "credentialsFactory":
        /// {
        ///   "typeName": "My.CredentialsFactory",
        ///   "args":
        ///   {
        ///     "foo": 42
        ///   }
        /// }
        /// </code>
        /// where <c>typeName</c> is the name of the type, and <c>args</c> is an optional dictionary
        /// of arguments for the type constructor.</para>
        /// <para>In addition, shortcuts exists for common credentials factory. The whole <c>credentialsFactory</c>
        /// block can be omitted and replace by one of the following:</para>
        /// <para>Username and password:<code>
        /// "username-password":
        /// {
        ///   "username": "someone",
        ///   "password": "secret"
        /// }
        /// </code></para>
        /// <para>Kerberos:<code>
        /// "kerberos":
        /// {
        ///   "spn": "service-provider-name"
        /// }
        /// </code></para>
        /// <para>Token:<code>
        /// "token":
        /// {
        ///   "data": "some-secret-token",
        ///   "encoding": "none"
        /// }
        /// </code>Supported encodings are: <c>none</c> and <c>base64</c>.</para>
        /// </remarks>
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

        [BinderName("username-password")] // + "username", "password"
        [BinderIgnore(false)]
#pragma warning disable IDE0051 // Remove unused private members
        // ReSharper disable once UnusedMember.Local
        private UsernamePasswordCredentialsFactoryOptions UsernamePasswordCredentialsFactoryBinder
#pragma warning restore IDE0051 // Remove unused private members
        {
            get => default;
            set => CredentialsFactory.Creator = () => new UsernamePasswordCredentialsFactory(value.Username, value.Password);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class UsernamePasswordCredentialsFactoryOptions
        {
            public string Username { get; set; }

            public string Password { get; set; }
        }

        [BinderName("token")] // + "encoding", "data"
        [BinderIgnore(false)]
#pragma warning disable IDE0051 // Remove unused private members
        // ReSharper disable once UnusedMember.Local
        private TokenCredentialsFactoryOptions TokenCredentialsFactoryBinder
#pragma warning restore IDE0051 // Remove unused private members
        {
            get => default;
            set => CredentialsFactory.Creator = () => new TokenCredentialsFactory(value.Data, value.Encoding);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class TokenCredentialsFactoryOptions
        {
            public string Data { get; set; }

            public string Encoding { get; set; }
        }

        [BinderName("kerberos")] // + "spn"
        [BinderIgnore(false)]
#pragma warning disable IDE0051 // Remove unused private members
        // ReSharper disable once UnusedMember.Local
        private KerberosCredentialsFactoryOptions KerberosCredentialsFactoryBinder
#pragma warning restore IDE0051 // Remove unused private members
        {
            get => default;
            set => CredentialsFactory.Creator = () => new KerberosCredentialsFactory(value.Spn);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class KerberosCredentialsFactoryOptions
        {
            public string Spn { get; set; }
        }

        /// <summary>
        /// Configures a user name and password as the authentication mechanism.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <returns>The security options.</returns>
        public AuthenticationOptions ConfigureUsernamePasswordCredentials(string username, string password)
        {
            CredentialsFactory.Creator = () => new UsernamePasswordCredentialsFactory(username, password);
            return this;
        }

        /// <summary>
        /// Configures static credentials as the authentication mechanism.
        /// </summary>
        /// <param name="credentials">Credentials.</param>
        /// <returns>The security options.</returns>
        public AuthenticationOptions ConfigureCredentials(ICredentials credentials)
        {
            CredentialsFactory.Creator = () => new StaticCredentialsFactory(credentials);
            return this;
        }

        /// <summary>
        /// Configures a static token as the authentication mechanism.
        /// </summary>
        /// <param name="token">A token.</param>
        /// <returns>The security configuration.</returns>
        public AuthenticationOptions ConfigureTokenCredentials(byte[] token)
        {
            return ConfigureCredentials(new TokenCredentials(token));
        }

        /// <summary>
        /// Configures Kerberos as the authentication mechanism.
        /// </summary>
        /// <param name="spn">The service principal name of the Hazelcast cluster.</param>
        /// <returns>The authentication options.</returns>
        public AuthenticationOptions ConfigureKerberosCredentials(string spn)
        {
            CredentialsFactory.Creator = () => new KerberosCredentialsFactory(spn);
            return this;
        }

        /// <summary>
        /// Clone the options.
        /// </summary>
        internal AuthenticationOptions Clone() => new AuthenticationOptions(this);
    }
}
