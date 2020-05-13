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
    /// Represents the security configuration.
    /// </summary>
    public class SecurityConfiguration
    {
        private ICredentialsFactory _credentialsFactory;
        private IAuthenticator _authenticator;

        /// <summary>
        /// Gets or sets the credentials factory.
        /// </summary>
        public ICredentialsFactory CredentialsFactory
        {
            get => _credentialsFactory ?? (_credentialsFactory = new DefaultCredentialsFactory());
            set => _credentialsFactory = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the authenticator.
        /// </summary>
        public IAuthenticator Authenticator
        {
            get => _authenticator ?? (_authenticator = new Authenticator());
            set => _authenticator = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}