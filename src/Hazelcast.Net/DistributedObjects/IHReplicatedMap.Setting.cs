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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.DistributedObjects
{
    public partial interface IHReplicatedMap<TKey, TValue> // Setting
    {
        /// <summary>
        ///     Associates the specified value with the specified key in this map
        ///     If the map previously contained a mapping for
        ///     the key, the old value is replaced by the specified value.
        /// </summary>
        /// <remarks>
        ///     Associates the specified value with the specified key in this map
        ///     If the map previously contained a mapping for
        ///     the key, the old value is replaced by the specified value.
        ///     <p />
        ///     <p>
        ///         <b>Warning:</b>
        ///     </p>
        ///     <p>
        ///         This method returns a clone of previous value, not the original (identically equal) value
        ///         previously put into map.
        ///     </p>
        ///     <p>
        ///         <b>Warning-2:</b>
        ///     </p>
        ///     This method uses <c>GetHashCode</c> and <c>Equals</c> of binary form of
        ///     the <c>key</c>, not the actual implementations of <c>GetHashCode</c> and <c>Equals</c>
        ///     defined in <c>key</c>'s class.
        /// </remarks>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns>
        ///     previous value associated with
        ///     <c>key</c>
        ///     or
        ///     <c>null</c>
        ///     if there was no mapping for
        ///     <c>key</c>
        ///     .
        /// </returns>
        Task<TValue> AddOrUpdateAsync(TKey key, TValue value, CancellationToken cancellationToken = default);

        /// <summary>
        /// Associates a given value to the specified key and replicates it to the
        /// cluster. If there is an old value, it will be replaced by the specified
        /// one and returned from the call.
        /// </summary>
        /// <remarks>
        /// In addition, you have to specify a ttl and its <see cref="TimeUnit" />
        /// to define when the value is outdated and thus should be removed from the
        /// replicated map.
        /// </remarks>
        /// <param name="key">key with which the specified value is to be associated.</param>
        /// <param name="value">value to be associated with the specified key.</param>
        /// <param name="ttl">ttl to be associated with the specified key-value pair.</param>
        /// <param name="timeunit"><see cref="TimeUnit" /> to be used for the ttl value.</param>
        /// <returns>old value of the entry</returns>
        Task<TValue> AddOrUpdateTtlAsync(TKey key, TValue value, TimeSpan timeToLive, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Copies all of the mappings from the specified map to this map
        /// </summary>
        /// <param name="m">mappings to be stored in this map</param>
        Task AddOrUpdateAsync(IDictionary<TKey, TValue> entries, CancellationToken cancellationToken = default);
    }
}
