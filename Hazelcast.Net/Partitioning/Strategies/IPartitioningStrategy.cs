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

namespace Hazelcast.Partitioning.Strategies
{
    /// <summary>
    /// Defines ways to get the partition key of objects.
    /// </summary>
    public interface IPartitioningStrategy
    {
        /// <summary>
        /// Gets the partition key of an object.
        /// </summary>
        /// <param name="o">The object.</param>
        /// <returns>The partition key of the object, or null to fallback to the default strategy.</returns>
        object GetPartitionKey(object o);
    }
}