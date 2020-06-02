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
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Security;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents the security options.
    /// </summary>
    public class SecurityOptions
    {
        private string _credentialsFactoryType;
        private string _authenticatorType;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityOptions"/> class.
        /// </summary>
        public SecurityOptions()
        {
            CredentialsFactory = new ServiceFactory<ICredentialsFactory>(() => new DefaultCredentialsFactory(this));
            CredentialsFactoryArgs = new Dictionary<string, object>();
            Authenticator = new ServiceFactory<IAuthenticator>(() => new Authenticator(this));
            AuthenticatorArgs = new Dictionary<string, object>();
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
        /// Gets or sets the authenticator factory.
        /// </summary>
        public ServiceFactory<IAuthenticator> Authenticator { get; private set; }

        /// <summary>
        /// Gets or sets the type of the authenticator.
        /// </summary>
        /// <remarks>
        /// <para>Returns the correct value only if it has been set via the same property. If the
        /// authenticator has been configured via code and the <see cref="Authenticator"/>
        /// property, the value returned by this property is unspecified.</para>
        /// </remarks>
        public string AuthenticatorType
        {
            get => _authenticatorType;

            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(ExceptionMessages.NullOrEmpty, nameof(value));

                _authenticatorType = value;

                Authenticator.Creator = () => Services.CreateInstance<IAuthenticator>(value, this);
            }
        }

        /// <summary>
        /// Gets the arguments for the authenticator.
        /// </summary>
        /// <remarks>
        /// <para>Arguments are used when creating an authenticator from its type as set
        /// via the <see cref="AuthenticatorType"/> property. They are ignored if the
        /// authenticator has been configured via code and the <see cref="Authenticator"/>
        /// property.</para>
        /// </remarks>
        public Dictionary<string, object> AuthenticatorArgs { get; private set; }

        /// <summary>
        /// Clone the options.
        /// </summary>
        public SecurityOptions Clone()
        {
            return new SecurityOptions
            {
                _authenticatorType = _authenticatorType,
                _credentialsFactoryType = _credentialsFactoryType,
                Authenticator = Authenticator.Clone(),
                AuthenticatorArgs = new Dictionary<string, object>(AuthenticatorArgs),
                CredentialsFactory = CredentialsFactory.Clone(),
                CredentialsFactoryArgs = new Dictionary<string, object>(CredentialsFactoryArgs)
            };
        }
    }
}
