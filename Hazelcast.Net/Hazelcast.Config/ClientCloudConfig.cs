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

namespace Hazelcast.Config
{
    /// <summary>
    /// hazelcast.cloud configuration to let the client connect the cluster via hazelcast.cloud
    /// </summary>
    public class ClientCloudConfig 
    {

        private string discoveryToken;
        private bool enabled;

        /// <summary>
        /// hazelcast.cloud discoveryToken of your cluster
        /// </summary>
        /// <returns>discoveryToken</returns>
        public string GetDiscoveryToken() 
        {
            return discoveryToken;
        }

        /// <summary>
        /// Set the discoveryToken hazelcast.cloud discoveryToken of your cluster
        /// </summary>
        /// <param name="discoveryToken">discoveryToken for hazelcast.cloud</param>
        /// <returns>configured <see cref="ClientCloudConfig"/> for chaining</returns>
        public ClientCloudConfig SetDiscoveryToken(string discoveryToken) 
        {
            this.discoveryToken = discoveryToken;
            return this;
        }

        /// <summary>
        /// return true if enabled, false otherwise
        /// </summary>
        /// <returns>is enabled or not</returns>
        public bool IsEnabled() {
            return enabled;
        }

        /// <summary>
        /// enabled true to use hazelcast.cloud
        /// </summary>
        /// <param name="enabled">is enabled or not</param>
        /// <returns>configured <see cref="ClientCloudConfig"/> for chaining</returns>
        public ClientCloudConfig SetEnabled(bool enabled) {
            this.enabled = enabled;
            return this;
        }
    }
}