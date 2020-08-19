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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Impl
{
    internal class HTxQueue<TItem> : TransactionalDistributedObjectBase, IHTxQueue<TItem>
    {
        public HTxQueue(string name, DistributedObjectFactory factory, Cluster cluster, MemberConnection transactionClientConnection, Guid transactionId, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : base(HQueue.ServiceName, name, factory, cluster, transactionClientConnection, transactionId, serializationService, loggerFactory)
        { }

        public Task<bool> TryEnqueueAsync(TItem item)
            => TryEnqueueAsync(item, TimeToWait.Zero);

        public async Task<bool> TryEnqueueAsync(TItem item, TimeSpan timeToWait)
        {
            var itemData = ToSafeData(item);
            var timeToWaitMilliseconds = timeToWait.TimeoutMilliseconds(0);
            var requestMessage = TransactionalQueueOfferCodec.EncodeRequest(Name, TransactionId, ContextId, itemData, timeToWaitMilliseconds);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CAF();
            return TransactionalQueueOfferCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<TItem> PeekAsync()
            => await TryPeekAsync(TimeToWait.Zero).CAF() ??
               throw new InvalidOperationException("The queue is empty.");

        public async Task<TItem> TryPeekAsync(TimeSpan timeToWait)
        {
            var timeToWaitMilliseconds = timeToWait.TimeoutMilliseconds(0);
            var requestMessage = TransactionalQueuePeekCodec.EncodeRequest(Name, TransactionId, ContextId, timeToWaitMilliseconds);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CAF();
            var response = TransactionalQueuePeekCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TItem>(response);
        }

        public Task<TItem> TryDequeueAsync()
            => TryDequeueAsync(TimeToWait.Zero);

        public async Task<TItem> TryDequeueAsync(TimeSpan timeToWait)
        {
            var timeToWaitMilliseconds = timeToWait.TimeoutMilliseconds(0);
            var requestMessage = TransactionalQueuePollCodec.EncodeRequest(Name, TransactionId, ContextId, timeToWaitMilliseconds);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CAF();
            var response = TransactionalQueuePollCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TItem>(response);
        }

        public async Task<int> CountAsync()
        {
            var requestMessage = TransactionalQueueSizeCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CAF();
            return TransactionalQueueSizeCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<TItem> DequeueAsync()
        {
            var requestMessage = TransactionalQueueTakeCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CAF();
            var response = TransactionalQueueTakeCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TItem>(response);
        }
    }
}
