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
using Hazelcast.Security;

namespace Hazelcast.Config
{
    public class SecurityConfig
    {
        private IIdentityConfig _identityConfig;

        public UsernamePasswordIdentityConfig UsernamePasswordIdentityConfig
        {
            get => _identityConfig as UsernamePasswordIdentityConfig;
            set => _identityConfig = value;
        }

        public TokenIdentityConfig TokenIdentityConfig
        {
            get => _identityConfig as TokenIdentityConfig;
            set => _identityConfig = value;
        }

        public CredentialsIdentityConfig CredentialsIdentityConfig
        {
            get => _identityConfig as CredentialsIdentityConfig;
            set => _identityConfig = value;
        }

        public CredentialsFactoryConfig CredentialsFactoryConfig
        {
            get => _identityConfig as CredentialsFactoryConfig;
            set => _identityConfig = value;
        }

        public bool HasIdentityConfig => _identityConfig != null;

        public ICredentialsFactory AsCredentialsFactory()
        {
            return _identityConfig?.AsCredentialsFactory();
        }

        public SecurityConfig ConfigureCredentialsIdentity(ICredentials credentials)
        {
            CredentialsIdentityConfig = new CredentialsIdentityConfig {Credentials = credentials};
            return this;
        }

        public SecurityConfig ConfigureUsernamePasswordIdentity(string username, string password)
        {
            UsernamePasswordIdentityConfig = new UsernamePasswordIdentityConfig {Username = username, Password = password};
            return this;
        }

        public SecurityConfig ConfigureTokenIdentity(string encodedToken, TokenEncoding encoding)
        {
            TokenIdentityConfig = new TokenIdentityConfig(encodedToken, encoding);
            return this;
        }

        public SecurityConfig ConfigureKerberosIdentity(string spn)
        {
            _identityConfig = new KerberosIdentityConfig(spn);
            return this;
        }

        internal SecurityConfig ConfigureKerberosIdentity(string spn, string username, string password, string domain)
        {
            _identityConfig = new KerberosIdentityConfig(spn, username, password, domain);
            return this;
        }

        public SecurityConfig ConfigureCredentialsFactory(string typeName, IDictionary<string, string> properties)
        {
            CredentialsFactoryConfig = new CredentialsFactoryConfig {TypeName = typeName, Properties = properties};
            return this;
        }

        public SecurityConfig ConfigureCredentialsFactory(ICredentialsFactory implementation)
        {
            CredentialsFactoryConfig = new CredentialsFactoryConfig {Implementation = implementation};
            return this;
        }
    }
}