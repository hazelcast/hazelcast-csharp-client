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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.HTxSetImpl
{
    internal class HTxSet<TItem> : TransactionalDistributedObjectBase, IHTxSet<TItem>
    {
        public HTxSet(string name, Cluster cluster, Client transactionClient, Guid transactionId, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : base(HSet.ServiceName, name, cluster, transactionClient, transactionId, serializationService, loggerFactory)
        { }

        public Task<bool> AddAsync(TItem item, TimeSpan timeout = default)
            => TaskEx.WithTimeout(AddAsync, item, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<bool> AddAsync(TItem item, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);
            var requestMessage = TransactionalSetAddCodec.EncodeRequest(Name, TransactionId, ContextId, itemData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalSetAddCodec.DecodeResponse(responseMessage).Response;
        }

        public Task<bool> RemoveAsync(TItem item, TimeSpan timeout = default)
            => TaskEx.WithTimeout(RemoveAsync, item, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<bool> RemoveAsync(TItem item, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);
            var requestMessage = TransactionalSetRemoveCodec.EncodeRequest(Name, TransactionId, ContextId, itemData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalSetRemoveCodec.DecodeResponse(responseMessage).Response;
        }

        public Task<int> CountAsync(TimeSpan timeout = default)
            => TaskEx.WithTimeout(CountAsync, timeout, DefaultOperationTimeoutMilliseconds);

        public async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            var requestMessage = TransactionalSetSizeCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClient, cancellationToken).CAF();
            return TransactionalSetSizeCodec.DecodeResponse(responseMessage).Response;
        }
    }
}
