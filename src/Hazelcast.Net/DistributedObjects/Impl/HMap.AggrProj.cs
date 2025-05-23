﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Aggregation;
using Hazelcast.Core;
using Hazelcast.Projection;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Query;
using Hazelcast.Serialization.Collections;

namespace Hazelcast.DistributedObjects.Impl
{
    // ReSharper disable UnusedTypeParameter
    internal partial class HMap<TKey, TValue> // Aggregations and Projections
    // ReSharper restore NonReadonlyMemberInGetHashCode
    {
        /// <inheritdoc />
        public Task<TResult> AggregateAsync<TResult>(IAggregator<TResult> aggregator)
            => AggregateAsync(aggregator, CancellationToken.None);

        private async Task<TResult> AggregateAsync<TResult>(IAggregator<TResult> aggregator, CancellationToken cancellationToken)
        {
            var aggregatorData = ToSafeData(aggregator);

            var requestMessage = MapAggregateCodec.EncodeRequest(Name, aggregatorData);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
            var response = MapAggregateCodec.DecodeResponse(responseMessage).Response;
            return await ToObjectAsync<TResult>(response).CfAwait();
        }

        /// <inheritdoc />
        public Task<TResult> AggregateAsync<TResult>(IAggregator<TResult> aggregator, IPredicate predicate)
            => AggregateAsync(aggregator, predicate, CancellationToken.None);

        private async Task<TResult> AggregateAsync<TResult>(IAggregator<TResult> aggregator, IPredicate predicate, CancellationToken cancellationToken)
        {
            var (aggregatorData, predicateData) = ToSafeData(aggregator, predicate);

            var requestMessage = MapAggregateWithPredicateCodec.EncodeRequest(Name, aggregatorData, predicateData);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
            var response = MapAggregateWithPredicateCodec.DecodeResponse(responseMessage).Response;
            return await ToObjectAsync<TResult>(response).CfAwait();
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<TResult>> ProjectAsync<TResult>(IProjection projection)
            => ProjectAsync<TResult>(projection, CancellationToken.None);

        private async Task<IReadOnlyCollection<TResult>> ProjectAsync<TResult>(IProjection projection, CancellationToken cancellationToken)
        {
            var projectionData = ToSafeData(projection);

            var requestMessage = MapProjectCodec.EncodeRequest(Name, projectionData);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
            var response = MapProjectCodec.DecodeResponse(responseMessage).Response;
            var result = new ReadOnlyLazyList<TResult>(SerializationService);
            await result.AddAsync(response).CfAwait();
            return result;
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<TResult>> ProjectAsync<TResult>(IProjection projection, IPredicate predicate)
            => ProjectAsync<TResult>(projection, predicate, CancellationToken.None);

        private async Task<IReadOnlyCollection<TResult>> ProjectAsync<TResult>(IProjection projection, IPredicate predicate, CancellationToken cancellationToken)
        {
            var (projectionData, predicateData) = ToSafeData(projection, predicate);

            var requestMessage = MapProjectWithPredicateCodec.EncodeRequest(Name, projectionData, predicateData);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
            var response = MapProjectWithPredicateCodec.DecodeResponse(responseMessage).Response;
            var result = new ReadOnlyLazyList<TResult>(SerializationService);
            await result.AddAsync(response).CfAwait();
            return result;
        }
    }
}
