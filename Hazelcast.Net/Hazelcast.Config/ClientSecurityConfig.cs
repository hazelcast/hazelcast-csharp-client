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

using Hazelcast.Security;

namespace Hazelcast.Config
{
    /// <summary>
    /// Contains the security configuration for a client.
    /// </summary>
    public class ClientSecurityConfig
    {

        private ICredentials _credentials;
        private string _credentialsClassName;
        private CredentialsFactoryConfig credentialsFactoryConfig = new CredentialsFactoryConfig();

        /// <summary>
        /// The configured <see cref="ICredentials"/> implementation
        /// </summary>
        /// <returns>credentials</returns>
        public ICredentials GetCredentials() {
            return _credentials;
        }

        /// <summary>
        /// Sets the <see cref="ICredentials"/> implementation
        /// </summary>
        /// <param name="credentials">credentials implementation</param>
        /// <returns>configured <see cref="ClientSecurityConfig"/> for chaining</returns>
        public ClientSecurityConfig SetCredentials(ICredentials credentials) {
            _credentials = credentials;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetCredentialsClassName() {
            return _credentialsClassName;
        }

        /// <summary>
        /// Credentials class will be instantiated from class name when setCredentialsFactoryConfig and  setCredentials
        /// are not used. The class will be instantiated with empty constructor.
        /// </summary>
        /// <param name="credentialsClassname">class name for credentials</param>
        /// <returns>configured <see cref="ClientSecurityConfig"/> for chaining</returns>
        public ClientSecurityConfig SetCredentialsClassName(string credentialsClassname) {
            _credentialsClassName = credentialsClassname;
            return this;
        }
        
        /// <summary>
        /// Returns the CredentialsFactory Config
        /// </summary>
        /// <returns><see cref="GetCredentialsFactoryConfig"/></returns>
        public CredentialsFactoryConfig GetCredentialsFactoryConfig() {
            return credentialsFactoryConfig;
        }

        /// <summary>
        /// Credentials Factory Config allows user to pass custom properties and use group config when instantiating a credentials object.
        /// </summary>
        /// <param name="credentialsFactoryConfig">the config that will be used to create credentials factory</param>
        /// <returns>configured <see cref="ClientSecurityConfig"/> for chaining</returns>
        public ClientSecurityConfig SetCredentialsFactoryConfig(CredentialsFactoryConfig credentialsFactoryConfig) {
            this.credentialsFactoryConfig = credentialsFactoryConfig;
            return this;
        }

    }
}