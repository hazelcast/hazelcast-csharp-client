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

namespace Hazelcast.Core
{
    /// <summary>
    ///     IPartitioningStrategy allows keys to be located on the same member
    ///     This makes related data to be stored in the same location.
    /// </summary>
    /// <remarks>
    ///     IPartitioningStrategy allows keys to be located on the same member
    ///     This makes related data to be stored in the same location. (See data-affinity.)
    /// </remarks>
    public interface IPartitioningStrategy
    {
        /// <summary>Returns the key object that will be used by Hazelcast to specify the partition.</summary>
        /// <remarks>Returns the key object that will be used by Hazelcast to specify the partition.</remarks>
        /// <param name="key">actual key object</param>
        /// <returns>partition key object or null to fallback to default partition calculation</returns>
        object GetPartitionKey(object key);
    }
}