// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Security;

namespace Hazelcast.Config
{
    /// <summary>
    /// Contains the security configuration for a client.
    /// </summary>
    public class ClientSecurityConfig
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

        public ICredentials Credentials
        {
            set => _identityConfig = new CredentialsIdentityConfig{Credentials = value};
        }
    }
}