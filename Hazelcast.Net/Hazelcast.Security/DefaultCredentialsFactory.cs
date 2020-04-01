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
using System.Configuration;
using Hazelcast.Config;
using Hazelcast.Util;
using ConfigurationException = Hazelcast.Config.ConfigurationException;

namespace Hazelcast.Security
{
    public class DefaultCredentialsFactory : ICredentialsFactory
    {
        private readonly ICredentials credentials;

        public DefaultCredentialsFactory(ClientSecurityConfig securityConfig, GroupConfig groupConfig)
        {
            credentials = InitCredentials(securityConfig, groupConfig);
        }

        private ICredentials InitCredentials(ClientSecurityConfig securityConfig, GroupConfig groupConfig)
        {
            var credentials = securityConfig.GetCredentials();
            if (credentials == null)
            {
                var credentialsClassname = securityConfig.GetCredentialsClassName();
                if (credentialsClassname != null)
                {
                    try
                    {
                        var type = Type.GetType(credentialsClassname, true, false);
                        if (type != null)
                        {
                            credentials = Activator.CreateInstance(type) as ICredentials;
                        }
                    }
                    catch (Exception e)
                    {
                        throw ExceptionUtil.Rethrow(e);
                    }
                }
            }
            if (credentials == null)
            {
                credentials = new UsernamePasswordCredentials(groupConfig.GetName(), groupConfig.GetPassword());
            }
            return credentials;
        }

        public void Configure(GroupConfig groupConfig, IDictionary<string, string> properties)
        {
        }

        public ICredentials NewCredentials()
        {
            return credentials;
        }

        public void Destroy()
        {
        }
    }
}