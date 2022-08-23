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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HMapWithCache<TKey, TValue> // Setting
    {
        /// <inheritdoc />
        protected override async Task SetAsync(IData keyData, IData valueData, TimeSpan timeToLive, TimeSpan maxIdle)
        {
            // if we Remove before AddOrUpdate then we could get a read after Remove and before AddOrUpdate,
            // which would populate the cache with the wrong value - so we clear *after* the value has effectively
            // changed on the server - so a read between AddOrUpdate and Remove would get the old value, but
            // eventually all reads will get the correct value
            await base.SetAsync(keyData, valueData, timeToLive, maxIdle).CfAwait();
            _cache.Remove(keyData);
        }

        /// <inheritdoc />
        protected override async Task<TValue> GetAndSetAsync(IData keyData, IData valueData, TimeSpan timeToLive, TimeSpan maxIdle)
        {
            // if we Remove before AddOrUpdate then we could get a read after Remove and before AddOrUpdate,
            // which would populate the cache with the wrong value - so we clear *after* the value has effectively
            // changed on the server - so a read between AddOrUpdate and Remove would get the old value, but
            // eventually all reads will get the correct value
            var value = await base.GetAndSetAsync(keyData, valueData, timeToLive, maxIdle).CfAwait();
            _cache.Remove(keyData);
            return value;
        }

        /// <inheritdoc />
        protected override
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task SetAsync(Dictionary<Guid, Dictionary<int, List<KeyValuePair<IData, IData>>>> ownerEntries, CancellationToken cancellationToken)
        {
            // see comments on the base Map class
            // this should be no different except for this entry invalidation method,
            // added as a continuation to each task that is being created

            void InvalidateEntries(IEnumerable<KeyValuePair<IData, IData>> list)
            {
                foreach (var (key, _) in list)
                    _cache.Remove(key);
            }

            var tasks = new List<Task>();
            foreach (var (ownerId, part) in ownerEntries)
            {
                foreach (var (partitionId, list) in part)
                {
                    if (list.Count == 0) continue;

                    var requestMessage = MapPutAllCodec.EncodeRequest(Name, list, false);
                    requestMessage.PartitionId = partitionId;
                    var ownerTask = Cluster.Messaging.SendToMemberAsync(requestMessage, ownerId, cancellationToken)
                        .ContinueWith(_ => InvalidateEntries(list), default, default, TaskScheduler.Current);
                    tasks.Add(ownerTask);
                }
            }

            var task = Task.WhenAll(tasks);


#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CfAwait();
#endif
        }

        /// <inheritdoc />
        protected override async Task<bool> TrySetAsync(IData keyData, IData valueData, TimeSpan serverTimeout, CancellationToken cancellationToken)
        {
            var added = await base.TrySetAsync(keyData, valueData, serverTimeout, cancellationToken).CfAwait();
            if (added) _cache.Remove(keyData);
            return added;
        }

        /// <inheritdoc />
        protected override
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<TValue> GetOrAdd(IData keyData, IData valueData, TimeSpan timeToLive, TimeSpan maxIdle, CancellationToken cancellationToken)
        {
            _cache.Remove(keyData);
            var task = base.GetOrAdd(keyData, valueData, timeToLive, maxIdle, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <inheritdoc />
        protected override
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task SetTransientAsync(IData keyData, IData valueData, TimeSpan timeToLive, TimeSpan maxIdle, CancellationToken cancellationToken)
        {
            _cache.Remove(keyData);
            var task = base.SetTransientAsync(keyData, valueData, timeToLive, maxIdle, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CfAwait();
#endif
        }
    }
}
