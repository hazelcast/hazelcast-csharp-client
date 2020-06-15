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
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.HQueueImpl
{
    internal partial class HQueue<T> // Setting (Enqueue)
    {
        // <inheritdoc />
        // need that one because we are an HCollection - weird?
        // tries to enqueue immediately, does not wait & does not throw
        public override
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<bool> AddAsync(T item, CancellationToken cancellationToken)
        {
            var task = EnqueueAsync(item, TimeToWait.Zero, false, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        // <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<bool> TryEnqueueAsync(T item)
        {
            var task = EnqueueAsync(item, TimeToWait.Zero, false, CancellationToken.None);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        // <inheritdoc />
        // was: Offer with no timeout - tries to enqueue immediately, does not wait & does not throw
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<bool> TryEnqueueAsync(T item, CancellationToken cancellationToken)
        {
            var task = EnqueueAsync(item, TimeToWait.Zero, false, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        // <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<bool> TryEnqueueAsync(T item, TimeSpan timeToWait)
        {
            var task = EnqueueAsync(item, timeToWait, false, CancellationToken.None);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        // <inheritdoc />
        // was: Offer with timeout - tries to enqueue within timeToWait, does not throw
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<bool> TryEnqueueAsync(T item, TimeSpan timeToWait, CancellationToken cancellationToken)
        {
            var task = EnqueueAsync(item, timeToWait, false, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        // <inheritdoc />
        // was: Put - enqueue, wait indefinitely, may throw
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task EnqueueAsync(T item, TimeSpan timeout = default)
        {
            var task = TaskEx.WithTimeout(EnqueueAsync, item, TimeToWait.InfiniteTimeSpan, true, timeout, DefaultOperationTimeoutMilliseconds);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        // <inheritdoc />
        // was: Put - enqueue, wait indefinitely, may throw
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task EnqueueAsync(T item, CancellationToken cancellationToken)
        {
            var task = EnqueueAsync(item, TimeToWait.InfiniteTimeSpan, true, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        private async Task<bool> EnqueueAsync(T item, TimeSpan timeToWait, bool doThrow, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);

            var timeToWaitMilliseconds = timeToWait.TimeoutMilliseconds(0);
            var requestMessage = timeToWaitMilliseconds < 0
                ? QueuePutCodec.EncodeRequest(Name, itemData)
                : QueueOfferCodec.EncodeRequest(Name, itemData, timeToWaitMilliseconds);

            ClientMessage responseMessage;
            try
            {
                responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            }
            catch
            {
                if (doThrow) throw;
                return false;
            }

            bool queued;
            if (timeToWaitMilliseconds < 0)
            {
                _ = QueuePutCodec.DecodeResponse(responseMessage);
                queued = true;
            }
            else
            {
                queued = QueueOfferCodec.DecodeResponse(responseMessage).Response;
            }

            if (queued) return true;
            if (doThrow) throw new InvalidOperationException("Queue is full.");
            return false;
        }

        // <inheritdoc />
        // need to have it because HCollection but feels weird
        // maybe we need to have something above HCollection?
        public override async Task<bool> AddRangeAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = QueueAddAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return QueueAddAllCodec.DecodeResponse(responseMessage).Response;
        }
    }
}