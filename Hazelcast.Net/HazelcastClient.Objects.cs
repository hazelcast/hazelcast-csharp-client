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

using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.DistributedObjects.Implementation.Map;
using Hazelcast.DistributedObjects.Implementation.Topic;

namespace Hazelcast
{
    // partial: distributed objects
    internal partial class HazelcastClient
    {
        // TODO: implement HazelcastClient access to other Distributed Objects

        private readonly ISequence<long> _lockReferenceIdSequence = new Int64Sequence();

        /// <inheritdoc />

#if DEBUG // maintain full stack traces
        public async Task<IMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name)
            => await GetMapAsyncTask<TKey, TValue>(name);
#else
        public Task<IMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name)
            => GetMapAsyncTask<TKey, TValue>(name);
#endif

        private ValueTask<Map<TKey, TValue>> GetMapAsyncTask<TKey, TValue>(string name)
            => _distributedObjectFactory.GetOrCreateAsync(Map.ServiceName, name, true,
                (n, cluster, serializationService, loggerFactory)
                    => new Map<TKey, TValue>(n, cluster, serializationService, _lockReferenceIdSequence, loggerFactory));

        /// <inheritdoc />

#if DEBUG // maintain full stack tracers
        public async Task<ITopic<T>> GetTopicAsync<T>(string name)
            => await GetTopicAsyncTask<T>(name);
#else
        public Task<ITopic<T>> GetTopicAsync<T>(string name)
            => GetTopicAsyncTask<T>(name);
#endif

        private ValueTask<Topic<T>> GetTopicAsyncTask<T>(string name)
            => _distributedObjectFactory.GetOrCreateAsync(Topic.ServiceName, name, true,
                (n, cluster, serializationService, loggerFactory)
                    => new Topic<T>(n, cluster, serializationService, loggerFactory));
    }
}