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

using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HList<T> // Setting
    {
        /// <inheritdoc />
        public async Task<T> Set(int index, T item)
        {
            var itemData = ToSafeData(item);
            var requestMessage = ListSetCodec.EncodeRequest(Name, index, itemData);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            var response = ListSetCodec.DecodeResponse(responseMessage).Response;
            return await ToObjectAsync<T>(response).CfAwait();
        }

        /// <inheritdoc />
        public async Task AddAsync(int index, T item)
        {
            var itemData = ToSafeData(item);
            var requestMessage = ListAddWithIndexCodec.EncodeRequest(Name, index, itemData);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            _ = ListAddWithIndexCodec.DecodeResponse(responseMessage);
        }

        /// <inheritdoc />
        public override async Task<bool> AddAsync(T item)
        {
            var itemData = ToSafeData(item);
            var requestMessage = ListAddCodec.EncodeRequest(Name, itemData);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return ListAddCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public override async Task<bool> AddAll<TItem>(ICollection<TItem> items)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = ListAddAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return ListAddAllCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<bool> AddAllAsync<TItem>(int index, ICollection<TItem> items)
            where TItem : T
        {
            var itemsData = ToSafeData(items);
            var requestMessage = ListAddAllWithIndexCodec.EncodeRequest(Name, index, itemsData);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return ListAddAllWithIndexCodec.DecodeResponse(responseMessage).Response;
        }

    }
}
