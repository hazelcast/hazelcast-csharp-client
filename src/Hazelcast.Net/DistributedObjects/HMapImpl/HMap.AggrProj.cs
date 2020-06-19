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
using Hazelcast.Core;
using Hazelcast.Predicates;
using Hazelcast.Projections;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization.Collections;

namespace Hazelcast.DistributedObjects.HMapImpl
{
    // ReSharper disable UnusedTypeParameter
    internal partial class HMap<TKey, TValue> // Aggregations and Projections
    // ReSharper restore NonReadonlyMemberInGetHashCode
    {
        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<TResult> AggregateAsync<TResult>(IAggregator<TResult> aggregator, TimeSpan timeout = default)
        {
            var task = TaskEx.WithTimeout(AggregateAsync, aggregator, timeout, DefaultOperationTimeoutMilliseconds);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<TResult> AggregateAsync<TResult>(IAggregator<TResult> aggregator, CancellationToken cancellationToken)
        {
            var aggregatorData = ToSafeData(aggregator);

            var requestMessage = MapAggregateCodec.EncodeRequest(Name, aggregatorData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            var response = MapAggregateCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TResult>(response);
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<TResult> AggregateAsync<TResult>(IAggregator<TResult> aggregator, IPredicate predicate, TimeSpan timeout = default)
        {
            var task = TaskEx.WithTimeout(AggregateAsync, aggregator, predicate, timeout, DefaultOperationTimeoutMilliseconds);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<TResult> AggregateAsync<TResult>(IAggregator<TResult> aggregator, IPredicate predicate, CancellationToken cancellationToken)
        {
            var (aggregatorData, predicateData) = ToSafeData(aggregator, predicate);

            var requestMessage = MapAggregateWithPredicateCodec.EncodeRequest(Name, aggregatorData, predicateData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            var response = MapAggregateWithPredicateCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TResult>(response);
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<IReadOnlyList<TResult>> ProjectAsync<TResult>(IProjection projection, TimeSpan timeout)
        {
            var task = TaskEx.WithTimeout(ProjectAsync<TResult>, projection, timeout, DefaultOperationTimeoutMilliseconds);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TResult>> ProjectAsync<TResult>(IProjection projection, CancellationToken cancellationToken)
        {
            var projectionData = ToSafeData(projection);

            var requestMessage = MapProjectCodec.EncodeRequest(Name, projectionData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            var response = MapProjectCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TResult>(response, SerializationService);
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<IReadOnlyList<TResult>> ProjectAsync<TResult>(IProjection projection, IPredicate predicate, TimeSpan timeout)
        {
            var task = TaskEx.WithTimeout(ProjectAsync<TResult>, projection, predicate, timeout, DefaultOperationTimeoutMilliseconds);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TResult>> ProjectAsync<TResult>(IProjection projection, IPredicate predicate, CancellationToken cancellationToken)
        {
            var (projectionData, predicateData) = ToSafeData(projection, predicate);

            var requestMessage = MapProjectWithPredicateCodec.EncodeRequest(Name, projectionData, predicateData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            var response = MapProjectWithPredicateCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TResult>(response, SerializationService);
        }
    }
}
