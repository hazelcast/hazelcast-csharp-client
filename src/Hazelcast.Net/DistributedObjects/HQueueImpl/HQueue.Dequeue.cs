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

namespace Hazelcast.DistributedObjects.HQueueImpl
{
    internal partial class HQueue<T> // Dequeue
    {
        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<T> PeekAsync(TimeSpan timeout = default) // peek but throw - was Element
        {
            var task = TaskEx.WithTimeout(PeekAsync, timeout, DefaultOperationTimeoutMilliseconds);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<T> PeekAsync(CancellationToken cancellationToken) // peek but throw - was Element
            => await TryPeekAsync(cancellationToken).CAF() ??
               throw new InvalidOperationException("The queue is empty.");

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<T> TryPeekAsync(TimeSpan timeout = default) // peek, or null
        {
            var task = TaskEx.WithTimeout(TryPeekAsync, timeout, DefaultOperationTimeoutMilliseconds);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<T> TryPeekAsync(CancellationToken cancellationToken) // peek, or null
        {
            var requestMessage = QueuePeekCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
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
            var task = TryDequeueAsync(TimeToWait.Zero, CancellationToken.None);

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
        Task<T> TryDequeueAsync(CancellationToken cancellationToken) // was poll = take immediately with zero timeout = infinite? default?
        {
            var task = TryDequeueAsync(TimeToWait.Zero, cancellationToken);

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
        Task<T> TryDequeueAsync(TimeSpan timeToWait) // was poll, take with timeout
        {
            var task = TryDequeueAsync(timeToWait, CancellationToken.None);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<T> TryDequeueAsync(TimeSpan timeToWait, CancellationToken cancellationToken) // was poll, take with timeout
        {
            var timeToWaitMilliseconds = timeToWait.TimeoutMilliseconds(0);
            var requestMessage = QueuePollCodec.EncodeRequest(Name, timeToWaitMilliseconds);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            var response = QueuePollCodec.DecodeResponse(responseMessage).Response;
            return ToObject<T>(response);
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<T> DequeueAsync(bool waitForItem, TimeSpan timeout = default) // was take, wail until an element is avail
        {
            var task = TaskEx.WithTimeout(DequeueAsync, waitForItem, timeout, DefaultOperationTimeoutMilliseconds);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<T> DequeueAsync(bool waitForItem, CancellationToken cancellationToken) // was take, wail until an element is avail
        {
            if (!waitForItem)
                return await TryDequeueAsync(TimeToWait.Zero, cancellationToken).CAF() ??
                       throw new InvalidOperationException("The queue is empty.");

            var requestMessage = QueueTakeCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            var response = QueueTakeCodec.DecodeResponse(responseMessage).Response;
            return ToObject<T>(response);
        }

        // FIXME: Queue.Drain has issues
        // may throw if the object is T but not TItem, need to review all these weird overloads
        // bit silly, deserializing immediately instead of returning a lazy thing?

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<int> DrainToAsync<TItem>(ICollection<TItem> items, TimeSpan timeout = default)
            where TItem : T
        {
            var task = TaskEx.WithTimeout(DrainToAsync, items, timeout, DefaultOperationTimeoutMilliseconds);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<int> DrainToAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken)
            where TItem : T
        {
            var requestMessage = QueueDrainToCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            var response = QueueDrainToCodec.DecodeResponse(responseMessage).Response;

            foreach (var itemData in response) items.Add((TItem)ToObject<T>(itemData));
            return response.Count;
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<int> DrainToAsync<TItem>(ICollection<TItem> items, int count, TimeSpan timeout = default)
            where TItem : T
        {
            var task = TaskEx.WithTimeout(DrainToAsync, items, count, timeout, DefaultOperationTimeoutMilliseconds);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<int> DrainToAsync<TItem>(ICollection<TItem> items, int count, CancellationToken cancellationToken)
            where TItem : T
        {
            var requestMessage = QueueDrainToMaxSizeCodec.EncodeRequest(Name, count);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            var response = QueueDrainToMaxSizeCodec.DecodeResponse(responseMessage).Response;

            foreach (var itemData in response) items.Add((TItem)ToObject<T>(itemData));
            return response.Count;
        }
    }
}
