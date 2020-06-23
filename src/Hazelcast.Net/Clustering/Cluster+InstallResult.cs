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

namespace Hazelcast.Clustering
{
    internal partial class Cluster // InstallResult
    {
        /// <summary>
        /// Defines the possible results of the installation of a subscription.
        /// </summary>
        private enum InstallResult
        {
            /// <summary>
            /// Unknown (default).
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// The subscription was successfully installed on the client.
            /// </summary>
            Success,

            /// <summary>
            /// Could not install the subscription because it is not active.
            /// </summary>
            SubscriptionNotActive,

            /// <summary>
            /// Could not install the subscription because the client is not active.
            /// </summary>
            ClientNotActive,

            /// <summary>
            /// Could not install the subscription, server may be confused.
            /// </summary>
            /// <remarks>
            /// <para>A confused server can be a client on which the subscription was installed,
            /// only to realize that the subscription had been removed in the meantime, but
            /// we failed to un-install the subscription. So the remote end still has the
            /// subscription (although we will ignore all events). Probably, something is
            /// wrong with either the client or the remote end.</para>
            /// </remarks>
            ConfusedServer,

            /// <summary>
            /// Could not install the subscription on the server.
            /// </summary>
            Failed
        }
    }
}
