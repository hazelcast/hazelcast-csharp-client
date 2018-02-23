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

namespace Hazelcast.Core
{
    /// <summary>
    ///     IPartitionAware allows implementing keys to be located on the same member
    ///     or implementing tasks to be executed on
    ///     <see cref="IPartitionAware{T}.GetPartitionKey()">IPartitionAware&lt;T&gt;.GetPartitionKey()</see>
    ///     's owner member.
    ///     This makes related data to be stored in the same location. (See data-affinity.)
    /// </summary>
    public interface IPartitionAware<T>
    {
        /// <summary>The key object that will be used by Hazelcast to specify the partition.</summary>
        /// <remarks>
        ///     The key object that will be used by Hazelcast to specify the partition.
        ///     You should give the same key for objects that you want them to locate in the same partition.
        /// </remarks>
        /// <returns>partition key</returns>
        T GetPartitionKey();
    }
}