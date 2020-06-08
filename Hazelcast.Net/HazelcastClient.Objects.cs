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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.DistributedObjects.Implementation.List;
using Hazelcast.DistributedObjects.Implementation.Map;
using Hazelcast.DistributedObjects.Implementation.Topic;
using Hazelcast.DistributedObjects.Implementation.Set;

namespace Hazelcast
{
    internal partial class HazelcastClient // Distributed Objects
    {
        // TODO: implement HazelcastClient access to other Distributed Objects
        
        private readonly ISequence<long> _lockReferenceIdSequence = new Int64Sequence();

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<IMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name, TimeSpan timeout = default)
        {
            var task = TaskEx.WithTimeout(GetMapAsync<TKey, TValue>, name, timeout, _options.Messaging.DefaultOperationTimeoutMilliseconds);

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
        Task<IMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name, CancellationToken cancellationToken)
        {
            var task = _distributedObjectFactory.GetOrCreateAsync(Map.ServiceName, name, true,
                (n, cluster, serializationService, loggerFactory)
                    => new Map<TKey, TValue>(n, cluster, serializationService, _lockReferenceIdSequence, loggerFactory),
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
        Task<ITopic<T>> GetTopicAsync<T>(string name, TimeSpan timeout = default)
        {
            var task = TaskEx.WithTimeout(GetTopicAsync<T>, name, timeout, _options.Messaging.DefaultOperationTimeoutMilliseconds);

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
        Task<ITopic<T>> GetTopicAsync<T>(string name, CancellationToken cancellationToken)
        {
            var task = _distributedObjectFactory.GetOrCreateAsync(Topic.ServiceName, name, true,
                (n, cluster, serializationService, loggerFactory)
                    => new Topic<T>(n, cluster, serializationService, loggerFactory),
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
            Task<IHList<T>> GetListAsync<T>(string name, TimeSpan timeout = default)
        {
            var task = TaskEx.WithTimeout(GetListAsync<T>, name, timeout, _options.Messaging.DefaultOperationTimeoutMilliseconds);

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
            var task = _distributedObjectFactory.GetOrCreateAsync(HList.ServiceName, name, true,
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
            Task<IHSet<T>> GetSetAsync<T>(string name, TimeSpan timeout = default)
        {
            var task = TaskEx.WithTimeout(GetSetAsync<T>, name, timeout, _options.Messaging.DefaultOperationTimeoutMilliseconds);

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
            var task = _distributedObjectFactory.GetOrCreateAsync(Set.ServiceName, name, true,
                (n, cluster, serializationService, loggerFactory)
                    => new HSet<T>(n, cluster, serializationService, loggerFactory),
                cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }
    }
}
