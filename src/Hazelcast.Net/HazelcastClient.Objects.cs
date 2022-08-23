// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.CP;
using Hazelcast.DistributedObjects;
using Hazelcast.DistributedObjects.Impl;
using Hazelcast.Serialization;
using Hazelcast.Sql;
using Microsoft.Extensions.Logging;

namespace Hazelcast
{
    internal partial class HazelcastClient // Distributed Objects
    {
        private readonly ISequence<long> _lockReferenceIdSequence = new Int64Sequence();

        /// <inheritdoc />
        public ICPSubsystem CPSubsystem { get; }

        /// <inheritdoc />
        public ISqlService Sql { get; }

        /// <inheritdoc />
        public async ValueTask DestroyAsync(IDistributedObject o)
        {
            await _distributedOjects.DestroyAsync(o).CfAwait();
        }

        // (internal for tests only)
        internal async ValueTask DestroyAsync(string serviceName, string name)
        {
            await _distributedOjects.DestroyAsync(serviceName, name).CfAwait();
        }

        /// <inheritdoc />
        public async Task<IHMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name)
        {
            // lookup a cache configuration for the specified map
            var nearCacheOptions = _options.GetNearCacheOptions(name);
            var nearCache = nearCacheOptions == null
                ? null
                : await _nearCacheManager.GetOrCreateNearCacheAsync<TValue>(name, nearCacheOptions).CfAwait();

            HMap<TKey, TValue> CreateMap(string n, DistributedObjectFactory factory, Cluster cluster, SerializationService serializationService, ILoggerFactory loggerFactory)
                => nearCacheOptions == null
                    ? new HMap<TKey, TValue>(n, factory, cluster, serializationService, _lockReferenceIdSequence, loggerFactory)
                    : new HMapWithCache<TKey, TValue>(n, factory, cluster, serializationService, _lockReferenceIdSequence, nearCache, loggerFactory);

            return await _distributedOjects.GetOrCreateAsync<IHMap<TKey, TValue>, HMap<TKey, TValue>>(ServiceNames.Map, name, true, CreateMap).CfAwait();
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<IHReplicatedMap<TKey, TValue>> GetReplicatedMapAsync<TKey, TValue>(string name)
        {
            var partitionId = Cluster.Partitioner.GetRandomPartitionId();

            var task = _distributedOjects.GetOrCreateAsync<IHReplicatedMap<TKey, TValue>, HReplicatedMap<TKey, TValue>>(ServiceNames.ReplicatedMap, name, true,
                (n, f, c, sr, lf)
                    => new HReplicatedMap<TKey,TValue>(n, f, c, sr, partitionId, lf));

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<IHMultiMap<TKey, TValue>> GetMultiMapAsync<TKey, TValue>(string name)
        {
            var task = _distributedOjects.GetOrCreateAsync<IHMultiMap<TKey, TValue>, HMultiMap<TKey, TValue>>(ServiceNames.MultiMap, name, true,
                (n, f, c, sr, lf)
                    => new HMultiMap<TKey, TValue>(n, f, c, sr, _lockReferenceIdSequence, lf));

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<IHTopic<T>> GetTopicAsync<T>(string name)
        {
            var task = _distributedOjects.GetOrCreateAsync<IHTopic<T>, HTopic<T>>(ServiceNames.Topic, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HTopic<T>(n, factory, cluster, serializationService, loggerFactory));

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<IHList<T>> GetListAsync<T>(string name)
        {
            var task = _distributedOjects.GetOrCreateAsync<IHList<T>, HList<T>>(ServiceNames.List, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HList<T>(n, factory, cluster, serializationService, loggerFactory));

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<IHSet<T>> GetSetAsync<T>(string name)
        {
            var task = _distributedOjects.GetOrCreateAsync<IHSet<T>, HSet<T>>(ServiceNames.Set, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HSet<T>(n, factory, cluster, serializationService, loggerFactory));

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<IHQueue<T>> GetQueueAsync<T>(string name)
        {
            var task = _distributedOjects.GetOrCreateAsync<IHQueue<T>, HQueue<T>>(ServiceNames.Queue, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HQueue<T>(n, factory, cluster, serializationService, loggerFactory));

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<IHRingBuffer<T>> GetRingBufferAsync<T>(string name)
        {
            var task = _distributedOjects.GetOrCreateAsync<IHRingBuffer<T>, HRingBuffer<T>>(ServiceNames.RingBuffer, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HRingBuffer<T>(n, factory, cluster, serializationService, loggerFactory));

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<IFlakeIdGenerator> GetFlakeIdGeneratorAsync(string name)
        {
            var task = _distributedOjects.GetOrCreateAsync<IFlakeIdGenerator, FlakeIdGenerator>(ServiceNames.FlakeIdGenerator, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new FlakeIdGenerator(n, factory, cluster, serializationService, loggerFactory, _options.GetFlakeIdGeneratorOptions(n)));

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }
    }
}
