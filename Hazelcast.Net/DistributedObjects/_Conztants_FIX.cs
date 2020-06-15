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

namespace Hazelcast.DistributedObjects
{
    internal static class Conztants // FIXME CLEAR THIS
    {
        /// <summary>
        /// Defines service name constants.
        /// </summary>
        internal class ServiceNames
        {

            /// <summary>
            /// Gets the PN-counter service name.
            /// </summary>
            public const string PNCounter = "hz:impl:PNCounterService";

            /// <summary>
            /// Gets the cluster service name.
            /// </summary>
            public const string Cluster = "hz:impl:clusterService";

            /// <summary>
            /// Gets the partition service name.
            /// </summary>
            public const string Partition = "hz:impl:partitionService";

            /// <summary>
            /// Gets the client engine service name.
            /// </summary>
            public const string ClientEngine = "hz:impl:clientEngineService";
        }
    }
}
