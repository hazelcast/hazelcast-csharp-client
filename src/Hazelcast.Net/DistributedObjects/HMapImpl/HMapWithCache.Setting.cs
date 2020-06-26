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
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.HMapImpl
{
    internal partial class HMapWithCache<TKey, TValue> // Setting
    {
        /// <inheritdoc />
        protected override
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<TValue> AddOrUpdateWithValueAsync(IData keyData, IData valueData, TimeSpan timeToLive, CancellationToken cancellationToken)
        {
            _cache.Invalidate(keyData);
            var task = base.AddOrUpdateWithValueAsync(keyData, valueData, timeToLive, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        protected override
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task AddOrUpdateAsync(Dictionary<Guid, Dictionary<int, List<KeyValuePair<IData, IData>>>> ownerEntries, CancellationToken cancellationToken)
        {
            // see comments on the base Map class
            // this should be no different except for this entry invalidation method,
            // added as a continuation to each task that is being created

            void InvalidateEntries(IEnumerable<KeyValuePair<IData, IData>> list)
            {
                foreach (var (key, _) in list)
                    _cache.Invalidate(key);
            }

            var tasks = new List<Task>();
            foreach (var (ownerId, part) in ownerEntries)
            {
                foreach (var (partitionId, list) in part)
                {
                    if (list.Count == 0) continue;

                    var requestMessage = MapPutAllCodec.EncodeRequest(Name, list);
                    requestMessage.PartitionId = partitionId;
                    var ownerTask = Cluster.SendToMemberAsync(requestMessage, ownerId, cancellationToken)
                        .ContinueWith(_ => InvalidateEntries(list), default, default, TaskScheduler.Current);
                    tasks.Add(ownerTask);
                }
            }

            var task = Task.WhenAll(tasks);


#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        protected override async Task<bool> TryAddOrUpdateAsync(IData keyData, IData valueData, TimeSpan serverTimeout, CancellationToken cancellationToken)
        {
            var added = await base.TryAddOrUpdateAsync(keyData, valueData, serverTimeout, cancellationToken).CAF();
            if (added) _cache.Invalidate(keyData);
            return added;
        }

        /// <inheritdoc />
        protected override
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<TValue> AddAsync(IData keyData, IData valueData, TimeSpan timeToLive, CancellationToken cancellationToken)
        {
            _cache.Invalidate(keyData);
            var task = base.AddAsync(keyData, valueData, timeToLive, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        protected override
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task AddOrUpdateTransientAsync(IData keyData, IData valueData, TimeSpan timeToLive, CancellationToken cancellationToken)
        {
            _cache.Invalidate(keyData);
            var task = base.AddOrUpdateTransientAsync(keyData, valueData, timeToLive, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }
    }
}
