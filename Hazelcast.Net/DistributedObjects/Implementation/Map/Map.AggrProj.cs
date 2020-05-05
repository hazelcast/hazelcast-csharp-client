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

using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Aggregators;
using Hazelcast.Core.Collections;
using Hazelcast.Predicates;
using Hazelcast.Projections;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Implementation.Map
{
    internal partial class Map<TKey, TValue>
    {
        /// <inheritdoc />
        public async Task<TResult> AggregateAsync<TResult>(IAggregator<TResult> aggregator, IPredicate predicate = null)
        {
            var aggregatorData = ToSafeData(aggregator);

            if (predicate == null)
            {
                var requestMessage = MapAggregateCodec.EncodeRequest(Name, aggregatorData);
                var responseMessage = await Cluster.SendAsync(requestMessage);
                var response = MapAggregateCodec.DecodeResponse(responseMessage).Response;
                return ToObject<TResult>(response);
            }

            {
                var predicateData = ToData(predicate);

                var requestMessage = MapAggregateWithPredicateCodec.EncodeRequest(Name, aggregatorData, predicateData);
                var responseMessage = await Cluster.SendAsync(requestMessage);
                var response = MapAggregateWithPredicateCodec.DecodeResponse(responseMessage).Response;
                return ToObject<TResult>(response);
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TResult>> ProjectAsync<TResult>(IProjection projection, IPredicate predicate = null)
        {
            var projectionData = ToSafeData(projection);

            if (predicate == null)
            {
                var requestMessage = MapProjectCodec.EncodeRequest(Name, projectionData);
                var responseMessage = await Cluster.SendAsync(requestMessage);
                var response = MapProjectCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyList<TResult>(response, SerializationService);
            }

            {
                var predicateData = ToData(predicate);

                var requestMessage = MapProjectWithPredicateCodec.EncodeRequest(Name, projectionData, predicateData);
                var responseMessage = await Cluster.SendAsync(requestMessage);
                var response = MapProjectWithPredicateCodec.DecodeResponse(responseMessage).Response;
                return new ReadOnlyLazyList<TResult>(response, SerializationService);
            }
        }
    }
}