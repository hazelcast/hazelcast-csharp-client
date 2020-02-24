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
    /// <summary>
    /// Contains the configuration for Credentials Factory.
    /// </summary>
    public class CredentialsFactoryConfig
    {
        public string ClassName { get; set; }

        public ICredentialsFactory Implementation { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public ICredentialsFactory GetCredentialsFactory()
        {
            if (Implementation != null) 
                return Implementation;

            if (string.IsNullOrWhiteSpace(ClassName))
                return Implementation = new DefaultCredentialsFactory();

            try
            {
                var type = Type.GetType(ClassName, true, false);
                Implementation = Activator.CreateInstance(type) as ICredentialsFactory;
                Implementation?.Init(Properties);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Could not create instance of '{ClassName}', cause: {e.Message}", e);
            }

            return Implementation;
        }
    }
}