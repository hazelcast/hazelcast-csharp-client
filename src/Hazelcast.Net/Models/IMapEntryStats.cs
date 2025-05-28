// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.DistributedObjects;

namespace Hazelcast.Models
{
    /// <summary>Represents statistics for an entry in an <see cref="IHMap{TKey,TValue}"/>.</summary>
    public interface IMapEntryStats<out TKey, out TValue>
    {
        /// <summary>Gets the key of the entry.</summary>
        /// <returns>The key of the entry.</returns>
        TKey Key { get; }

        /// <summary>Gets the value of the entry.</summary>
        /// <returns>The value of the entry.</returns>
        TValue Value { get; }

        /// <summary>Gets the cost in bytes of the entry.</summary>
        /// <returns>The cost in bytes, if statistics are enabled; otherwise <c>-1</c>.</returns>
        long Cost { get; }

        /// <summary>Gets the creation epoch time of the entry.</summary>
        /// <returns>The creation epoch time of the entry, if statistics are enabled; otherwise <c>-1</c>.</returns>
        long CreationTime { get; }

        /// <summary>Gets the expiration epoch time of the entry.</summary>
        /// <returns>The expiration epoch time of the entry, if statistics are enabled; otherwise <c>-1</c>.</returns>
        long ExpirationTime { get; }

        /// <summary>Gets number of hits of the entry.</summary>
        /// <returns>The number of hits of the entry, if statistics are enabled; otherwise <c>-1</c>.</returns>
        long Hits { get; }

        /// <summary>Gets the last access epoch time to the entry.</summary>
        /// <returns>The last access epoch time of the entry, if statistics are enabled; otherwise <c>-1</c>.</returns>
        long LastAccessTime { get; }

        /// <summary>Gets the last epoch time the value was flushed to MapStore.</summary>
        /// <returns>The last epoch time the value was flushed to MapStore, if statistics are enabled; otherwise <c>-1</c>.</returns>
        long LastStoredTime { get; }

        /// <summary>Gets the last epoch time the value was updated.</summary>
        /// <returns>The last epoch time the value was updated, if statistics are enabled; otherwise <c>-1</c>.</returns>
        long LastUpdateTime { get; }

        /// <summary>Gets the version of the entry.</summary>
        /// <returns>The version of the entry.</returns>
        long Version { get; }
    }
}
