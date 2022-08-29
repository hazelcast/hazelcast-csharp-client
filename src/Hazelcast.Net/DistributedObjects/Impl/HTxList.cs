// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
    /// <summary>
    /// Implements <see cref="IHTxList{TItem}"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    internal class HTxList<TItem> : TransactionalDistributedObjectBase, IHTxList<TItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HTxList{TItem}"/> class.
        /// </summary>
        /// <param name="name">The unique name of the list.</param>
        /// <param name="factory">The factory that owns this object.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="transactionClientConnection">The client supporting the transaction.</param>
        /// <param name="transactionId">The unique identifier of the transaction.</param>
        /// <param name="serializationService">The serialization service.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public HTxList(string name, DistributedObjectFactory factory, Cluster cluster, MemberConnection transactionClientConnection, Guid transactionId, SerializationService serializationService, ILoggerFactory loggerFactory)
            : base(ServiceNames.List, name, factory, cluster, transactionClientConnection, transactionId, serializationService, loggerFactory)
        { }

        /// <inheritoc />
        public async Task<bool> AddAsync(TItem item)
        {
            var itemData = ToData(item);
            var requestMessage = TransactionalListAddCodec.EncodeRequest(Name, TransactionId, ContextId, itemData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            return TransactionalListAddCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritoc />
        public async Task<bool> RemoveAsync(TItem item)
        {
            var itemData = ToData(item);
            var requestMessage = TransactionalListRemoveCodec.EncodeRequest(Name, TransactionId, ContextId, itemData);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            return TransactionalListRemoveCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritoc />
        public async Task<int> GetSizeAsync()
        {
            var requestMessage = TransactionalListSizeCodec.EncodeRequest(Name, TransactionId, ContextId);
            var responseMessage = await Cluster.Messaging.SendToMemberAsync(requestMessage, TransactionClientConnection).CfAwait();
            return TransactionalListSizeCodec.DecodeResponse(responseMessage).Response;
        }
    }
}
