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

using System;
using Hazelcast.Security;

#if !NETSTANDARD

namespace Hazelcast.Config
{ 
    /// <summary>
    /// Provides methods for configuring Kerberos based security.
    /// </summary>
    public static class KerberosCredentialsExtensions
    {
        /// <summary>
        /// Configures using Kerberos as the authentication mechanism.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="spn">The Service Principal Name, <see cref="https://docs.microsoft.com/en-us/windows/win32/ad/service-principal-names"/>.</param>
        /// <param name="tokenTimeout">The Kerberos token timeout.</param>
        /// <returns>The client configuration.</returns>
        public static ClientConfig UseKerberosCredentials(this ClientConfig config, string spn, TimeSpan tokenTimeout)
        {
            config.SetCredentials(new KerberosCredentials(spn, tokenTimeout));
            return config;
        }

        /// <summary>
        /// Configures using Kerberos as the authentication mechanism.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="spn">The Service Principal Name, <see cref="https://docs.microsoft.com/en-us/windows/win32/ad/service-principal-names"/>.</param>
        /// <param name="tokenTimeout">The Kerberos token timeout.</param>
        /// <returns>The security configuration.</returns>
        public static ClientSecurityConfig UseKerberosCredentials(this ClientSecurityConfig config, string spn, TimeSpan tokenTimeout)
        {
            config.SetCredentials(new KerberosCredentials(spn, tokenTimeout));
            return config;
        }
    }
}

#endif