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

using System.Collections.Generic;
using Hazelcast.Config;

namespace Hazelcast.Security
{
    /// <summary>
    /// Simple username password credentials factory.
    /// </summary>
    /// <remarks>
    /// This factory uses the properties dictionary of <see cref="CredentialsFactoryConfig"/>. <c>username</c> and <c>password</c>
    /// properties are used for creating <see cref="UsernamePasswordCredentials"/>.
    /// if username is omitted in the properties list then <see cref="ClientConfig.GetClusterName"/> will be used instead
    /// if password is omitted in the properties list then <see cref="ClientConfig.GetClusterPassword"/> will be used instead
    /// </remarks>
    public class UsernamePasswordCredentialsFactory : ICredentialsFactory
    {
        private string _username;
        private string _password;

        public void Configure(ClientConfig groupConfig, IDictionary<string, string> properties)
        {
            if (!properties.TryGetValue("username", out _username))
            {
                _username = groupConfig.GetClusterName();
            }
            if(!properties.TryGetValue("password", out _password))
            {
                _password = groupConfig.GetClusterPassword();
            }
        }

        public ICredentials NewCredentials()
        {
            return new UsernamePasswordCredentials(_username, _password);
        }

        public void Destroy()
        {
        }
    }
}