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

namespace Hazelcast.Core
{
    /// <summary>Virtual partition instance.</summary>
    /// <remarks>
    ///     Virtual partition instance.
    ///     Each partition belongs to a member and this ownership may change when a member joins to or leaves the cluster.
    /// </remarks>
    public interface IPartition
    {
        /// <summary>Returns owner member of the partition.</summary>
        /// <returns>owner member of the partition</returns>
        IMember PartitionOwner { get; }

        /// <summary>Returns id of the partition.</summary>
        /// <returns>id of the partition</returns>
        int PartitionId { get; }

    }
}