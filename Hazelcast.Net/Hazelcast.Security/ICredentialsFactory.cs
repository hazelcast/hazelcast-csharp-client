// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
    /// <see cref="ICredentialsFactory"/> is used to create <see cref="ICredentials"/> objects to be used
    /// during node authentication before connection is accepted by the master node.
    /// </summary>
    public interface ICredentialsFactory
    {
        
        /// <summary>
        /// Configures <see cref="ICredentialsFactory"/>
        /// </summary>
        /// <param name="groupConfig"><see cref="GroupConfig"/></param>
        /// <param name="properties">properties that will be used to pass custom configurations by user</param>
        void Configure(GroupConfig groupConfig, IDictionary<string, string> properties);

        /// <summary>
        /// Creates new <see cref="ICredentials"/> object.
        /// </summary>
        /// <remarks>
        /// This method will be called on every connection authentication tries.
        /// </remarks>
        /// <returns>the new Credentials object</returns>
        ICredentials NewCredentials();

        /// <summary>
        /// Destroys <see cref="ICredentialsFactory"/>
        /// </summary>
        void Destroy();
    }
}