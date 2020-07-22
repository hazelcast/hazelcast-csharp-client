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
using Hazelcast.Predicates;

namespace Hazelcast.DistributedObjects
{
    // ReSharper disable once UnusedTypeParameter
    public partial interface IHMap<TKey, TValue> // Processing
    {
        /// <summary>
        /// Processes an entry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="processor">An entry processor.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The result of the process.</returns>
        /// <remarks>
        /// <para>The <paramref name="processor"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<object> ExecuteAsync(IEntryProcessor processor, TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes entries.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <param name="processor">An entry processor.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The result of the processing of each entry.</returns>
        /// <remarks>
        /// <para>The <paramref name="processor"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<IDictionary<TKey, object>> ExecuteAsync(IEntryProcessor processor, IEnumerable<TKey> keys, CancellationToken cancellationToken = default);

        /// <summary>
        /// Process all entries.
        /// </summary>
        /// <param name="processor">An entry processor.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The result of the processing of all entries.</returns>
        /// <remarks>
        /// <para>The <paramref name="processor"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<IDictionary<TKey, object>> ExecuteAsync(IEntryProcessor processor, CancellationToken cancellationToken = default);

        /// <summary>
        /// Process entries.
        /// </summary>
        /// <param name="processor">An entry processor.</param>
        /// <param name="predicate">A predicate to select entries.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The result of the processing of selected entries.</returns>
        /// <remarks>
        /// <para>The <paramref name="processor"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<IDictionary<TKey, object>> ExecuteAsync(IEntryProcessor processor, IPredicate predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// TODO: kill that one, it seems to be ExecuteOnKeyAsync? what is this? updating the value?
        /// </summary>
        /// <param name="key"></param>
        /// <param name="processor"></param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns></returns>
        Task<object> ApplyAsync(IEntryProcessor processor, TKey key, CancellationToken cancellationToken = default);
    }
}
