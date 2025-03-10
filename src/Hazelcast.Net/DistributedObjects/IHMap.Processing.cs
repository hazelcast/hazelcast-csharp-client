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
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Query;

namespace Hazelcast.DistributedObjects
{
    // ReSharper disable once UnusedTypeParameter
    public partial interface IHMap<TKey, TValue> // Processing
    {
        /// <summary>
        /// Processes all entries.
        /// </summary>
        /// <typeparam name="TResult">The type of the results produced by the processor.</typeparam>
        /// <param name="processor">An entry processor.</param>
        /// <returns>The result of the processing of all entries.</returns>
        /// <remarks>
        /// <para>The operation is not lock-aware. The <paramref name="processor"/> will process the entries
        /// no matter if the keys are locked or not.</para>
        /// <para>The <paramref name="processor"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// <para>
        /// <p>
        /// <b>Interactions with the map store:</b>
        /// </p>
        /// <p>For each entry not found in memory <c>MapLoader.load(Object)</c> is invoked to load the value from
        /// the <c>MapStore</c> backing the map.
        /// </p>
        /// <p>If the entryProcessor updates the entry and write-through
        /// persistence mode is configured, before the value is stored
        /// in memory, <c>MapStore.store(Object, Object)</c> is called to
        /// write the value into the map store.
        /// </p>
        /// <p>If the entryProcessor updates the entry's value to null value and
        /// write-through persistence mode is configured, before the value is
        /// removed from the memory, <c>MapStore.delete(Object)</c> is
        /// called to delete the value from the <c>MapStore</c>.
        /// </p>
        /// <p>Any exceptions thrown by the <c>MapStore</c> fail the operation and are
        /// propagated to the caller. If an exception happened, the operation might
        /// already succeeded on some of the keys.</p>
        /// <p>
        /// If write-behind persistence mode is configured with
        /// write-coalescing turned off, <c>ReachedMaxSizeException</c> may be thrown
        /// if the write-behind queue has reached its per-node maximum
        /// capacity.
        /// </p>
        /// </para>
        /// </remarks>
        Task<IDictionary<TKey, TResult>> ExecuteAsync<TResult>(IEntryProcessor<TResult> processor);

        /// <summary>
        /// Processes an entry.
        /// </summary>
        /// <typeparam name="TResult">The type of the results produced by the processor.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="processor">An entry processor.</param>
        /// <returns>The result of the process.</returns>
        /// <remarks>
        /// <para>The <paramref name="processor"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<TResult> ExecuteAsync<TResult>(IEntryProcessor<TResult> processor, TKey key);

        /// <summary>
        /// Processes entries.
        /// </summary>
        /// <typeparam name="TResult">The type of the results produced by the processor.</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="processor">An entry processor.</param>
        /// <returns>The result of the processing of each entry.</returns>
        /// <remarks>
        /// <para>The <paramref name="processor"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<IDictionary<TKey, TResult>> ExecuteAsync<TResult>(IEntryProcessor<TResult> processor, IEnumerable<TKey> keys);

        /// <summary>
        /// Process entries.
        /// </summary>
        /// <typeparam name="TResult">The type of the results produced by the processor.</typeparam>
        /// <param name="processor">An entry processor.</param>
        /// <param name="predicate">A predicate to select entries.</param>
        /// <returns>The result of the processing of selected entries.</returns>
        /// <remarks>
        /// <para>The <paramref name="processor"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<IDictionary<TKey, TResult>> ExecuteAsync<TResult>(IEntryProcessor<TResult> processor, IPredicate predicate);
    }
}
