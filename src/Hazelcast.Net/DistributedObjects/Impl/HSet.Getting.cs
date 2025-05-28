// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Serialization.Collections;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HSet<T> // Getting
    {
        public override async Task<IReadOnlyList<T>> GetAllAsync()
        {
            var requestMessage = SetGetAllCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            var response = SetGetAllCodec.DecodeResponse(responseMessage).Response;
            var result = new ReadOnlyLazyList<T>(SerializationService);
            await result.AddAsync(response).CfAwait();
            return result;
        }

        public override async Task<bool> ContainsAsync(T item)
        {
            var itemData = ToSafeData(item);
            var requestMessage = SetContainsCodec.EncodeRequest(Name, itemData);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return SetContainsCodec.DecodeResponse(responseMessage).Response;
        }

        public override async Task<int> GetSizeAsync()
        {
            var requestMessage = SetSizeCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return SetSizeCodec.DecodeResponse(responseMessage).Response;
        }

        public override async Task<bool> ContainsAllAsync<TItem>(ICollection<TItem> items)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = SetContainsAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return SetContainsAllCodec.DecodeResponse(responseMessage).Response;
        }

        public override async Task<bool> IsEmptyAsync()
        {
            var requestMessage = SetIsEmptyCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return SetIsEmptyCodec.DecodeResponse(responseMessage).Response;
        }
    }
}
