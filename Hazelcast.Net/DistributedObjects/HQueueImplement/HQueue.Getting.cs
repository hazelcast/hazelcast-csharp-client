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
using Hazelcast.Serialization.Collections;

namespace Hazelcast.DistributedObjects.HQueueImplement
{
    internal partial class HQueue<T> // Getting
    {
        // <inheritdoc />
        public override async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            var requestMessage = QueueSizeCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return QueueSizeCodec.DecodeResponse(responseMessage).Response;
        }

        // <inheritdoc />
        public override async Task<bool> ContainsAsync(T item, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);
            var requestMessage = QueueContainsCodec.EncodeRequest(Name, itemData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return QueueContainsCodec.DecodeResponse(responseMessage).Response;
        }

        // <inheritdoc />
        public override async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken)
        {
            var requestMessage = QueueIteratorCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            var response = QueueIteratorCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<T>(response, SerializationService);
        }

        // <inheritdoc />
        public override async Task<bool> ContainsAllAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = QueueContainsAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return QueueContainsAllCodec.DecodeResponse(responseMessage).Response;
        }

        // <inheritdoc />
        public override async Task<bool> IsEmptyAsync(CancellationToken cancellationToken)
        {
            var requestMessage = QueueIsEmptyCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return QueueIsEmptyCodec.DecodeResponse(responseMessage).Response;
        }

        // <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<int> RemainingCapacityAsync(TimeSpan timeout = default)
        {
            var task = TaskEx.WithTimeout(RemainingCapacityAsync, timeout, DefaultOperationTimeoutMilliseconds);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        // <inheritdoc />
        public async Task<int> RemainingCapacityAsync(CancellationToken cancellationToken)
        {
            var requestMessage = QueueRemainingCapacityCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
            return QueueRemainingCapacityCodec.DecodeResponse(responseMessage).Response;
        }
    }
}