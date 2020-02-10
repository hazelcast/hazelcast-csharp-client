// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.IO;

namespace Hazelcast.Core
{
    /// <summary>Cluster member interface.</summary>
    /// <seealso cref="ICluster">ICluster</seealso>
    /// <seealso cref="IMembershipListener">IMembershipListener</seealso>
    public interface IMember
    {
        /// <summary>Returns true if this member is a lite member.</summary>
        /// <returns>
        /// <c>true</c> if this member is a lite member, <c>false</c> otherwise.
        /// Lite members do not own any partition.
        /// </returns>
        bool IsLiteMember { get; }

        Address Address { get; }

        /// <summary>Returns configured attributes for this member</summary>
        /// <value>Attributes for this member.</value>
        IDictionary<string, string> Attributes { get; }
        
        /// <summary>
        /// Returns the Hazelcast codebase version of this member
        /// </summary>
        MemberVersion Version { get; }


        /// <summary>Returns unique uuid for this endpoint</summary>
        /// <value>unique uuid for this endpoint</value>
        Guid Uuid { get; }
    }
}