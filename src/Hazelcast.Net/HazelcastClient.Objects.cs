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
        public async ValueTask DestroyAsync(IDistributedObject o)
        {
            await _distributedObjectFactory.DestroyAsync(o).CAF();
        }

        /// <inheritdoc />
        public async Task<IHDictionary<TKey, TValue>> GetDictionaryAsync<TKey, TValue>(string name)
        {
            // lookup a cache configuration for the specified map
            var nearCacheOptions = _options.NearCaching.GetNearCacheOptions(name);
            var nearCache = nearCacheOptions == null
                ? null
                : await _nearCacheManager.GetOrCreateNearCacheAsync<TValue>(name, nearCacheOptions).CAF();

            HDictionary<TKey, TValue> CreateMap(string n, DistributedObjectFactory factory, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
                => nearCacheOptions == null
                    ? new HDictionary<TKey, TValue>(n, factory, cluster, serializationService, _lockReferenceIdSequence, loggerFactory)
                    : new HDictionaryWithCache<TKey, TValue>(n, factory, cluster, serializationService, _lockReferenceIdSequence, nearCache, loggerFactory);

            return await _distributedObjectFactory.GetOrCreateAsync<IHDictionary<TKey, TValue>, HDictionary<TKey, TValue>>(ServiceNames.Dictionary, name, true, CreateMap).CAF();
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<IHReplicatedDictionary<TKey, TValue>> GetReplicatedDictionaryAsync<TKey, TValue>(string name)
        {
            var partitionId = Cluster.Partitioner.GetRandomPartitionId();

            var task = _distributedObjectFactory.GetOrCreateAsync<IHReplicatedDictionary<TKey, TValue>, HReplicatedDictionary<TKey, TValue>>(ServiceNames.ReplicatedDictionary, name, true,
                (n, f, c, sr, lf)
                    => new HReplicatedDictionary<TKey,TValue>(n, f, c, sr, partitionId, lf));

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
        Task<IHMultiDictionary<TKey, TValue>> GetMultiDictionaryAsync<TKey, TValue>(string name)
        {
            var task = _distributedObjectFactory.GetOrCreateAsync<IHMultiDictionary<TKey, TValue>, HMultiDictionary<TKey, TValue>>(ServiceNames.MultiDictionary, name, true,
                (n, f, c, sr, lf)
                    => new HMultiDictionary<TKey, TValue>(n, f, c, sr, _lockReferenceIdSequence, lf));

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
        Task<IHTopic<T>> GetTopicAsync<T>(string name)
        {
            var task = _distributedObjectFactory.GetOrCreateAsync<IHTopic<T>, HTopic<T>>(ServiceNames.Topic, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HTopic<T>(n, factory, cluster, serializationService, loggerFactory));

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
        Task<IHList<T>> GetListAsync<T>(string name)
        {
            var task = _distributedObjectFactory.GetOrCreateAsync<IHList<T>, HList<T>>(ServiceNames.List, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HList<T>(n, factory, cluster, serializationService, loggerFactory));

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
        Task<IHSet<T>> GetSetAsync<T>(string name)
        {
            var task = _distributedObjectFactory.GetOrCreateAsync<IHSet<T>, HSet<T>>(ServiceNames.Set, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HSet<T>(n, factory, cluster, serializationService, loggerFactory));

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
        Task<IHQueue<T>> GetQueueAsync<T>(string name)
        {
            var task = _distributedObjectFactory.GetOrCreateAsync<IHQueue<T>, HQueue<T>>(ServiceNames.Queue, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HQueue<T>(n, factory, cluster, serializationService, loggerFactory));

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
        Task<IHRingBuffer<T>> GetRingBufferAsync<T>(string name)
        {
            var task = _distributedObjectFactory.GetOrCreateAsync<IHRingBuffer<T>, HRingBuffer<T>>(ServiceNames.RingBuffer, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HRingBuffer<T>(n, factory, cluster, serializationService, loggerFactory));

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }
    }
}
