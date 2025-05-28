// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
namespace Hazelcast.Clustering
{
    internal partial class ClusterEvents // InstallResult
    {
        /// <summary>
        /// Defines the possible results of the installation of a subscription.
        /// </summary>
        private enum InstallResult
        {
            /// <summary>
            /// The subscription was successfully installed on the client.
            /// </summary>
            Success = 1, // zero is for default

            /// <summary>
            /// Could not install the subscription because it is not active (do not retry).
            /// </summary>
            SubscriptionNotActive,

            /// <summary>
            /// Could not install the subscription because the connection is not active (do not retry).
            /// </summary>
            ConnectionNotActive,

            /// <summary>
            /// Could not install the subscription on the member (may want to retry?).
            /// </summary>
            Failed
        }
    }
}
