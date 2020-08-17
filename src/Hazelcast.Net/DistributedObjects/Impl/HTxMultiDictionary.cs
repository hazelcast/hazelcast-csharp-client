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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Impl
{
    internal class HTxMultiDictionary<TKey, TValue> : TransactionalDistributedObjectBase, IHTxMultiDictionary<TKey, TValue>
    {
        public HTxMultiDictionary(string name, DistributedObjectFactory factory, Cluster cluster, ClientConnection transactionClientConnection, Guid transactionId, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : base(HMultiDictionary.ServiceName, name, factory, cluster, transactionClientConnection, transactionId, serializationService, loggerFactory)
        { }

        public async Task<IReadOnlyList<TValue>> GetAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMultiMapGetCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            var response = TransactionalMultiMapGetCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TValue>(response, SerializationService);
        }

        public async Task<bool> TryAddAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var requestMessage = TransactionalMultiMapPutCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            return TransactionalMultiMapPutCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<bool> RemoveAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var requestMessage = TransactionalMultiMapRemoveEntryCodec.EncodeRequest(Name, TransactionId, ContextId, keyData, valueData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            return TransactionalMultiMapRemoveEntryCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<IReadOnlyList<TValue>> RemoveAsync(TKey key)
        {
            var keyData = ToData(key);
            var requestMessage = TransactionalMultiMapRemoveCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            var response = TransactionalMultiMapRemoveCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<TValue>(response, SerializationService);
        }

        public async Task<int> CountAsync()
        {
            var requestMessage = TransactionalMultiMapSizeCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            return TransactionalMultiMapSizeCodec.DecodeResponse(responseMessage).Response;
        }

        public async Task<int> ValueCountAsync(TKey key)
        {
            var keyData = ToSafeData(key);
            var requestMessage = TransactionalMultiMapValueCountCodec.EncodeRequest(Name, TransactionId, ContextId, keyData);
            var responseMessage = await Cluster.SendToClientAsync(requestMessage, TransactionClientConnection).CAF();
            return TransactionalMultiMapValueCountCodec.DecodeResponse(responseMessage).Response;
        }
    }
}
