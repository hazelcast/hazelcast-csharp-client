// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Impl
{
    internal class HTxMultiMap<TKey, TValue> : TransactionalDistributedObjectBase, IHTxMultiMap<TKey, TValue>
    {
        public HTxMultiMap(string name, DistributedObjectFactory factory, Cluster cluster, MemberConnection transactionClientConnection, Guid transactionId, SerializationService serializationService, ILoggerFactory loggerFactory)
            : base(ServiceNames.MultiMap, name, factory, cluster, transactionClientConnection, transactionId, serializationService, loggerFactory)
        { }

        public async Task<IReadOnlyCollection<TValue>> GetAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMultiMapGetCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            var response = TransactionalMultiMapGetCodec.DecodeResponse(responseMessage).Response;
            var result = new ReadOnlyLazyList<TValue>(SerializationService);
            await result.AddAsync(response).CfAwait();
            return result;
        }

        public async Task<bool> PutAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var requestMessage = TransactionalMultiMapPutCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            return TransactionalMultiMapPutCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<bool> RemoveAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var requestMessage = TransactionalMultiMapRemoveEntryCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            return TransactionalMultiMapRemoveEntryCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<IReadOnlyCollection<TValue>> RemoveAsync(TKey key)
        {
            var keyData = ToData(key);
            var requestMessage = TransactionalMultiMapRemoveCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            var response = TransactionalMultiMapRemoveCodec.DecodeResponse(responseMessage).Response;
            var result = new ReadOnlyLazyList<TValue>(SerializationService);
            await result.AddAsync(response).CfAwait();
            return result;
        }

        public async Task<int> GetSizeAsync()
        {
            var requestMessage = TransactionalMultiMapSizeCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            return TransactionalMultiMapSizeCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<int> GetValueCountAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMultiMapValueCountCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            return TransactionalMultiMapValueCountCodec.DecodeResponse(responseMessage).Response;
        }
    }
}
