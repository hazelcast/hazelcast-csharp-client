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

using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.DistributedObjects.Impl;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast
{
    internal partial class HazelcastClient // Distributed Objects
    {
        private readonly ISequence<long> _lockReferenceIdSequence = new Int64Sequence();

        /// <inheritdoc />
        public async ValueTask DestroyAsync(IDistributedObject o, CancellationToken cancellationToken)
        {
            await _distributedObjectFactory.DestroyAsync(o.ServiceName, o.Name, cancellationToken).CAF();
        }

        /// <inheritdoc />
        public async Task<IHMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name, CancellationToken cancellationToken)
        {
            // FIXME canecllation?!

            // lookup a cache configuration for the specified map
            var nearCacheOptions = _options.NearCache.GetConfig(name);
            var nearCache = nearCacheOptions == null
                ? null
                : await _nearCacheManager.GetOrCreateNearCacheAsync(name, nearCacheOptions, cancellationToken).CAF();

            HMap<TKey, TValue> CreateMap(string n, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
                => nearCacheOptions == null
                    ? new HMap<TKey, TValue>(n, cluster, serializationService, _lockReferenceIdSequence, loggerFactory)
                    : new HMapWithCache<TKey, TValue>(n, cluster, serializationService, _lockReferenceIdSequence, nearCache, loggerFactory);

            return await _distributedObjectFactory.GetOrCreateAsync<IHMap<TKey, TValue>, HMap<TKey, TValue>>(HMap.ServiceName, name, true, CreateMap, cancellationToken).CAF();
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<IHReplicatedMap<TKey, TValue>> GetReplicatedMapAsync<TKey, TValue>(string name, CancellationToken cancellationToken)
        {
            var partitionId = Cluster.Partitioner.GetRandomPartitionId();

            var task = _distributedObjectFactory.GetOrCreateAsync<IHReplicatedMap<TKey, TValue>, HReplicatedMap<TKey, TValue>>(HReplicatedMap.ServiceName, name, true,
                (n, c, sr, lf) => new HReplicatedMap<TKey,TValue>(n, c, sr, partitionId, lf), cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<IHMultiMap<TKey, TValue>> GetMultiMapAsync<TKey, TValue>(string name, CancellationToken cancellationToken)
        {
            var task = _distributedObjectFactory.GetOrCreateAsync<IHMultiMap<TKey, TValue>, HMultiMap<TKey, TValue>>(HMultiMap.ServiceName, name, true,
                (n, c, sr, lf) => new HMultiMap<TKey, TValue>(n, c, sr, _lockReferenceIdSequence, lf), cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<IHTopic<T>> GetTopicAsync<T>(string name, CancellationToken cancellationToken)
        {
            var task = _distributedObjectFactory.GetOrCreateAsync<IHTopic<T>, HTopic<T>>(HTopic.ServiceName, name, true,
                (n, cluster, serializationService, loggerFactory)
                    => new HTopic<T>(n, cluster, serializationService, loggerFactory),
                cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<IHList<T>> GetListAsync<T>(string name, CancellationToken cancellationToken)
        {
            var task = _distributedObjectFactory.GetOrCreateAsync<IHList<T>, HList<T>>(HList.ServiceName, name, true,
                (n, cluster, serializationService, loggerFactory)
                    => new HList<T>(n, cluster, serializationService, loggerFactory),
                cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<IHSet<T>> GetSetAsync<T>(string name, CancellationToken cancellationToken)
        {
            var task = _distributedObjectFactory.GetOrCreateAsync<IHSet<T>, HSet<T>>(HSet.ServiceName, name, true,
                (n, cluster, serializationService, loggerFactory)
                    => new HSet<T>(n, cluster, serializationService, loggerFactory),
                cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<IHQueue<T>> GetQueueAsync<T>(string name, CancellationToken cancellationToken)
        {
            var task = _distributedObjectFactory.GetOrCreateAsync<IHQueue<T>, HQueue<T>>(HQueue.ServiceName, name, true,
                (n, cluster, serializationService, loggerFactory)
                    => new HQueue<T>(n, cluster, serializationService, loggerFactory),
                cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<IHRingBuffer<T>> GetRingBufferAsync<T>(string name, CancellationToken cancellationToken)
        {
            const int maxBatchSize = 1000; // TODO: should become an option
            var task = _distributedObjectFactory.GetOrCreateAsync<IHRingBuffer<T>, HRingBuffer<T>>(HRingBuffer.ServiceName, name, true,
                (n, cluster, serializationService, loggerFactory)
                    => new HRingBuffer<T>(n, cluster, maxBatchSize, serializationService, loggerFactory),
                cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }
    }
}
