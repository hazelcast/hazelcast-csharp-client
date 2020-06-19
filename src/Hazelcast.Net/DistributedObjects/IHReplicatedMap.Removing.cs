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

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    public partial interface IHReplicatedMap<TKey, TValue> // Removing
    {
        /// <summary>Removes the mapping for a key from this map if it is present.</summary>
        /// <remarks>
        ///     Removes the mapping for a key from this map if it is present.
        ///     <p>
        ///         The map will not contain a mapping for the specified key once the call returns.
        ///     </p>
        /// </remarks>
        /// <param name="key">key</param>
        /// <returns>
        ///     previous value associated with <c>key</c> or <c>null</c>
        ///     if there was no mapping for <c>key</c> .
        /// </returns>
        Task<TValue> RemoveAsync(TKey key, TimeSpan timeout = default);
        Task<TValue> RemoveAsync(TKey key, CancellationToken cancellationToken);

        /// <summary>
        /// The clear operation wipes data out of the replicated maps.
        /// </summary>
        /// <remarks>
        /// If some node fails on executing the operation, it is retried for at most
        /// 5 times (on the failing nodes only).
        ///</remarks>
        Task ClearAsync(TimeSpan timeout = default);
        Task ClearAsync(CancellationToken cancellationToken);
    }
}
