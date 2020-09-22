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

using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HSet<T> // Removing
    {
        public override async Task<bool> RemoveAsync(T item)
        {
            var itemData = ToSafeData(item);
            var requestMessage = SetRemoveCodec.EncodeRequest(Name, itemData);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CAF();
            return SetRemoveCodec.DecodeResponse(responseMessage).Response;
        }

        public override async Task<bool> RemoveAllAsync<TItem>(ICollection<TItem> items)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = SetCompareAndRemoveAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CAF();
            return SetCompareAndRemoveAllCodec.DecodeResponse(responseMessage).Response;
        }

        public override async Task<bool> RetainAllAsync<TItem>(ICollection<TItem> items)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = SetCompareAndRetainAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CAF();
            return SetCompareAndRetainAllCodec.DecodeResponse(responseMessage).Response;
        }

        public override async Task ClearAsync()
        {
            var requestMessage = SetClearCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CAF();
            _ = SetClearCodec.DecodeResponse(responseMessage);
        }
    }
}
