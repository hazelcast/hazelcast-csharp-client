﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
    internal partial class HDictionaryWithCache<TKey, TValue> // Setting
    {
        /// <inheritdoc />
        protected override async Task<TValue> AddOrUpdateAsync(IData keyData, IData valueData, TimeSpan timeToLive, bool returnValue, CancellationToken cancellationToken)
        {
            // if we Remove before AddOrUpdate then we could get a read after Remove and before AddOrUpdate,
            // which would populate the cache with the wrong value - so we clear *after* the value has effectively
            // changed on the server - so a read between AddOrUpdate and Remove would get the old value, but
            // eventually all reads will get the correct value
            var value = await base.AddOrUpdateAsync(keyData, valueData, timeToLive, returnValue, cancellationToken).CAF();
            _cache.Remove(keyData);
            return value;
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
                    _cache.Remove(key);
            }

            var tasks = new List<Task>();
            foreach (var (ownerId, part) in ownerEntries)
            {
                foreach (var (partitionId, list) in part)
                {
                    if (list.Count == 0) continue;

                    var requestMessage = MapPutAllCodec.EncodeRequest(Name, list);
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
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        protected override async Task<bool> TryAddOrUpdateAsync(IData keyData, IData valueData, TimeSpan serverTimeout, CancellationToken cancellationToken)
        {
            var added = await base.TryAddOrUpdateAsync(keyData, valueData, serverTimeout, cancellationToken).CAF();
            if (added) _cache.Remove(keyData);
            return added;
        }

        /// <inheritdoc />
        protected override
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<TValue> GetOrAdd(IData keyData, IData valueData, TimeSpan timeToLive, CancellationToken cancellationToken)
        {
            _cache.Remove(keyData);
            var task = base.GetOrAdd(keyData, valueData, timeToLive, cancellationToken);

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
            _cache.Remove(keyData);
            var task = base.AddOrUpdateTransientAsync(keyData, valueData, timeToLive, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }
    }
}
