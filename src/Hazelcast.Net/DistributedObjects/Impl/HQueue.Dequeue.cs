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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HQueue<T> // Dequeue
    {
        /// <inheritdoc />
        public async Task<T> PeekAsync() // peek but throw - was Element
            => await TryPeekAsync().CAF() ??
               throw new InvalidOperationException("The queue is empty.");

        /// <inheritdoc />
        public async Task<T> TryPeekAsync() // peek, or null
        {
            var requestMessage = QueuePeekCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData).CAF();
            var response = QueuePeekCodec.DecodeResponse(responseMessage).Response;
            return ToObject<T>(response);
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<T> TryDequeueAsync() // was poll = take immediately with zero timeout = infinite? default?
        {
            var task = TryDequeueAsync(TimeToWait.Zero);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<T> TryDequeueAsync(TimeSpan timeToWait) // was poll, take with timeout
        {
            var timeToWaitMilliseconds = timeToWait.TimeoutMilliseconds(0);
            var requestMessage = QueuePollCodec.EncodeRequest(Name, timeToWaitMilliseconds);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData).CAF();
            var response = QueuePollCodec.DecodeResponse(responseMessage).Response;
            return ToObject<T>(response);
        }

        /// <inheritdoc />
        public async Task<T> DequeueAsync(bool waitForItem) // was take, wail until an element is avail
        {
            if (!waitForItem)
                return await TryDequeueAsync(TimeToWait.Zero).CAF() ??
                       throw new InvalidOperationException("The queue is empty.");

            var requestMessage = QueueTakeCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData).CAF();
            var response = QueueTakeCodec.DecodeResponse(responseMessage).Response;
            return ToObject<T>(response);
        }

        // TODO: Queue.Drain has issues
        // it may throw if the object is T but not TItem, need to review all these weird overloads
        // also deserializing immediately instead of returning a lazy thing?

        /// <inheritdoc />
        public async Task<int> DrainToAsync<TItem>(ICollection<TItem> items)
            where TItem : T
        {
            var requestMessage = QueueDrainToCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData).CAF();
            var response = QueueDrainToCodec.DecodeResponse(responseMessage).Response;

            foreach (var itemData in response) items.Add((TItem)ToObject<T>(itemData));
            return response.Count;
        }

        /// <inheritdoc />
        public async Task<int> DrainToAsync<TItem>(ICollection<TItem> items, int count)
            where TItem : T
        {
            var requestMessage = QueueDrainToMaxSizeCodec.EncodeRequest(Name, count);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData).CAF();
            var response = QueueDrainToMaxSizeCodec.DecodeResponse(responseMessage).Response;

            foreach (var itemData in response) items.Add((TItem)ToObject<T>(itemData));
            return response.Count;
        }
    }
}
