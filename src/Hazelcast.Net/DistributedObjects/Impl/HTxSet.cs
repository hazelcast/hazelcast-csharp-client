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
    internal class HTxSet<TItem> : TransactionalDistributedObjectBase, IHTxSet<TItem>
    {
        public HTxSet(string name, DistributedObjectFactory factory, Cluster cluster, MemberConnection transactionClientConnection, Guid transactionId, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : base(ServiceNames.Set, name, factory, cluster, transactionClientConnection, transactionId, serializationService, loggerFactory)
        { }

        public async Task<bool> AddAsync(TItem item)
        {
            var itemData = ToSafeData(item);
            var requestMessage = TransactionalSetAddCodec.EncodeRequest(Name, TransactionId, ContextId, itemData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CAF();
            return TransactionalSetAddCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<bool> RemoveAsync(TItem item)
        {
            var itemData = ToSafeData(item);
            var requestMessage = TransactionalSetRemoveCodec.EncodeRequest(Name, TransactionId, ContextId, itemData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CAF();
            return TransactionalSetRemoveCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<int> CountAsync()
        {
            var requestMessage = TransactionalSetSizeCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CAF();
            return TransactionalSetSizeCodec.DecodeResponse(responseMessage).Response;
        }
    }
}
