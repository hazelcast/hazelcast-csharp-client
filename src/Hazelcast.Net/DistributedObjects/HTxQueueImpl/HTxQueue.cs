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
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.HTxQueueImpl
{
    internal class HTxQueue<TItem> : TransactionalDistributedObjectBase, IHTxQueue<TItem>
    {
        public HTxQueue(string name, Cluster cluster, ClientConnection transactionClient, Guid transactionId, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : base(HQueue.ServiceName, name, cluster, transactionClient, transactionId, serializationService, loggerFactory)
        { }

        public Task<bool> TryEnqueueAsync(TItem item)
            => TryEnqueueAsync(item, TimeToWait.Zero, default);

        public Task<bool> TryEnqueueAsync(TItem item, CancellationToken cancellationToken)
            => TryEnqueueAsync(item, TimeToWait.Zero, cancellationToken);

        public Task<bool> TryEnqueueAsync(TItem item, TimeSpan timeToWait)
            => TryEnqueueAsync(item, timeToWait, default);

        public async Task<bool> TryEnqueueAsync(TItem item, TimeSpan timeToWait, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);
            var timeToWaitMilliseconds = timeToWait.TimeoutMilliseconds(0);
            var requestMessage = TransactionalQueueOfferCodec.EncodeRequest(Name, TransactionId, ContextId, itemData, timeToWaitMilliseconds);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalQueueOfferCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<TItem> PeekAsync(TimeSpan timeout = default)
            => await TaskEx.WithTimeout(TryPeekAsync, TimeToWait.Zero, timeout, DefaultOperationTimeoutMilliseconds).CAF() ??
               throw new InvalidOperationException("The queue is empty.");

        public async Task<TItem> PeekAsync(CancellationToken cancellationToken)
            => await TryPeekAsync(TimeToWait.Zero, cancellationToken).CAF() ??
               throw new InvalidOperationException("The queue is empty.");

        public Task<TItem> TryPeekAsync(TimeSpan timeToWait, TimeSpan timeout = default)
            => TaskEx.WithTimeout(TryPeekAsync, timeToWait, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<TItem> TryPeekAsync(TimeSpan timeToWait, CancellationToken cancellationToken)
        {
            var timeToWaitMilliseconds = timeToWait.TimeoutMilliseconds(0);
            var requestMessage = TransactionalQueuePeekCodec.EncodeRequest(Name, TransactionId, ContextId, timeToWaitMilliseconds);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            var response = TransactionalQueuePeekCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TItem>(response);
        }

        public Task<TItem> TryDequeueAsync()
            => TryDequeueAsync(TimeToWait.Zero, default);

        public Task<TItem> TryDequeueAsync(CancellationToken cancellationToken)
            => TryDequeueAsync(TimeToWait.Zero, cancellationToken);

        public Task<TItem> TryDequeueAsync(TimeSpan timeToWait)
            => TryDequeueAsync(timeToWait, default);

        public async Task<TItem> TryDequeueAsync(TimeSpan timeToWait, CancellationToken cancellationToken)
        {
            var timeToWaitMilliseconds = timeToWait.TimeoutMilliseconds(0);
            var requestMessage = TransactionalQueuePollCodec.EncodeRequest(Name, TransactionId, ContextId, timeToWaitMilliseconds);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            var response = TransactionalQueuePollCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TItem>(response);
        }

        public Task<int> CountAsync(TimeSpan timeout = default)
            => TaskEx.WithTimeout(CountAsync, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            var requestMessage = TransactionalQueueSizeCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalQueueSizeCodec.DecodeResponse(responseMessage).Response;
        }

        public Task<TItem> DequeueAsync(TimeSpan timeout = default)
            => TaskEx.WithTimeout(DequeueAsync, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<TItem> DequeueAsync(CancellationToken cancellationToken)
        {
            var requestMessage = TransactionalQueueTakeCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            var response = TransactionalQueueTakeCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TItem>(response);
        }
    }
}
