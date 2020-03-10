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

using Hazelcast.Config;
using Hazelcast.Core;

namespace Hazelcast.Client
{
    /// <summary>ILoadBalancer allows you to send operations to one of a number of Members.</summary>
    /// <remarks>
    /// It is up to the implementation to use different load balancing policies.
    /// If IClient is in smart routing mode, only the operations that are not key based will be router to the member returned by
    /// the Load Balancer.
    /// If it is not in smart routing mode ILoadBalancer will not be used.
    /// </remarks>
    public interface ILoadBalancer
    {
        /// <summary>
        /// Initializes the LoadBalancer.
        /// </summary>
        /// <param name="cluster">the Cluster this LoadBalancer uses to select members from</param>
        /// <param name="config">the ClientConfig</param>
        void Init(ICluster cluster, ClientConfig config);

        /// <summary>Returns the next member to route to</summary>
        /// <returns>Returns the next member or null if no member is available</returns>
        IMember Next();
    }
}