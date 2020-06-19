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
using Hazelcast.Aggregating;
using Hazelcast.Predicates;
using Hazelcast.Projections;

namespace Hazelcast.DistributedObjects
{
    // ReSharper disable UnusedTypeParameter
    public partial interface IHMap<TKey, TValue> // Aggregating and projecting
    // ReSharper restore UnusedTypeParameter
    {
        /// <summary>
        /// Aggregates values.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregator">The aggregator.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The result of the aggregation.</returns>
        /// <remarks>
        /// <para>The <paramref name="aggregator"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<TResult> AggregateAsync<TResult>(IAggregator<TResult> aggregator, TimeSpan timeout = default);

        /// <summary>
        /// Aggregates values.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregator">The aggregator.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The result of the aggregation.</returns>
        /// <remarks>
        /// <para>The <paramref name="aggregator"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<TResult> AggregateAsync<TResult>(IAggregator<TResult> aggregator, CancellationToken cancellationToken);

        /// <summary>
        /// Aggregates values.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregator">The aggregator.</param>
        /// <param name="predicate">An optional predicate to filter the entries with.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The result of the aggregation.</returns>
        /// <remarks>
        /// <para>The <paramref name="aggregator"/> and <paramref name="predicate"/> must be
        /// serializable via Hazelcast serialization, and have a counterpart on the server.</para>
        /// </remarks>
        Task<TResult> AggregateAsync<TResult>(IAggregator<TResult> aggregator, IPredicate predicate, TimeSpan timeout = default);

        /// <summary>
        /// Aggregates values.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregator">The aggregator.</param>
        /// <param name="predicate">An optional predicate to filter the entries with.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The result of the aggregation.</returns>
        /// <remarks>
        /// <para>The <paramref name="aggregator"/> and <paramref name="predicate"/> must be
        /// serializable via Hazelcast serialization, and have a counterpart on the server.</para>
        /// </remarks>
        Task<TResult> AggregateAsync<TResult>(IAggregator<TResult> aggregator, IPredicate predicate, CancellationToken cancellationToken);

        /// <summary>
        /// Projects values.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The projected values.</returns>
        /// <remarks>
        /// <para>The <paramref name="projection"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<IReadOnlyList<TResult>> ProjectAsync<TResult>(IProjection projection, TimeSpan timeout = default);

        /// <summary>
        /// Projects values.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The projected values.</returns>
        /// <remarks>
        /// <para>The <paramref name="projection"/> must be serializable via Hazelcast serialization,
        /// and have a counterpart on the server.</para>
        /// </remarks>
        Task<IReadOnlyList<TResult>> ProjectAsync<TResult>(IProjection projection, CancellationToken cancellationToken);

        /// <summary>
        /// Projects values.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <param name="predicate">An optional predicate to filter the entries with.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>The projected values.</returns>
        /// <remarks>
        /// <para>The <paramref name="projection"/> and <paramref name="predicate"/> must be
        /// serializable via Hazelcast serialization, and have a counterpart on the server.</para>
        /// </remarks>
        Task<IReadOnlyList<TResult>> ProjectAsync<TResult>(IProjection projection, IPredicate predicate, TimeSpan timeout = default);

        /// <summary>
        /// Projects values.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <param name="predicate">An optional predicate to filter the entries with.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The projected values.</returns>
        /// <remarks>
        /// <para>The <paramref name="projection"/> and <paramref name="predicate"/> must be
        /// serializable via Hazelcast serialization, and have a counterpart on the server.</para>
        /// </remarks>
        Task<IReadOnlyList<TResult>> ProjectAsync<TResult>(IProjection projection, IPredicate predicate, CancellationToken cancellationToken);
    }
}
