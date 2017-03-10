// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.IO;

namespace Hazelcast.Core
{
    /// <summary>Cluster member interface.</summary>
    /// <remarks>
    /// Cluster member interface. The default implementation violates the Serialization contract.
    /// It should be serialized/deserialized by Hazelcast.
    /// </remarks>
    /// <seealso cref="ICluster">Cluster</seealso>
    /// <seealso cref="IMembershipListener">MembershipListener</seealso>
    public interface IMember : IEndpoint
    {
        // <summary>Returns true if this member is a lite member.</summary>
        /// <returns>
        /// <tt>true</tt> if this member is a lite member, <tt>false</tt> otherwise.
        /// Lite members do not own any partition.
        /// </returns>
        bool IsLiteMember { get; }

        Address GetAddress();

        string GetAttribute(string key);

        /// <summary>
        /// Returns configured attributes for this member.<br/>
        /// <b>This method might not be available on all native clients.</b>
        /// </summary>
        /// <returns>Attributes for this member.</returns>
        /// <since>3.2</since>
        IDictionary<string, string> GetAttributes();
    }
}