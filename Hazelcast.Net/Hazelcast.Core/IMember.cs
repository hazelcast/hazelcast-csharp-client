/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System.Collections.Generic;
using System.Net;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

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
        Address GetAddress();

        /// <summary>Returns the socket address of this member.</summary>
        /// <remarks>Returns the socket address of this member.</remarks>
        /// <returns>socket address of this member</returns>
        IPEndPoint GetSocketAddress();

        /// <summary>Returns UUID of this member.</summary>
        /// <remarks>Returns UUID of this member.</remarks>
        /// <returns>UUID of this member.</returns>
        string GetUuid();

        /// <summary>
        /// Returns configured attributes for this member.<br/>
        /// <b>This method might not be available on all native clients.</b>
        /// </summary>
        /// <returns>Attributes for this member.</returns>
        /// <since>3.2</since>
        IDictionary<string, string> GetAttributes();

        string GetAttribute(string key);

        // <summary>Returns true if this member is a lite member.</summary>
        /// <returns>
        /// <tt>true</tt> if this member is a lite member, <tt>false</tt> otherwise.
        /// Lite members do not own any partition.
        /// </returns>
        bool IsLiteMember { get; }
    }
}