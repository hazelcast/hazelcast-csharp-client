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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.HListImpl
{
    internal partial class HList<T> // Remove
    {
        /// <inheritdoc />
        public override async Task<bool> RemoveAsync(T item, CancellationToken cancellationToken = default)
        {
            var itemData = ToSafeData(item);
            var requestMessage = ListRemoveCodec.EncodeRequest(Name, itemData);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            return ListRemoveCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<T> RemoveAtAsync(int index, CancellationToken cancellationToken = default)
        {
            var requestMessage = ListRemoveWithIndexCodec.EncodeRequest(Name, index);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            var response = ListRemoveWithIndexCodec.DecodeResponse(responseMessage).Response;
            return ToObject<T>(response);
        }

        /// <inheritdoc />
        public override async Task<bool> RemoveAllAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken = default)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = ListCompareAndRemoveAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            return ListCompareAndRemoveAllCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public override async Task<bool> RetainAllAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken = default)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = ListCompareAndRetainAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            return ListCompareAndRetainAllCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public override async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            var requestMessage = ListClearCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            _ = ListClearCodec.DecodeResponse(responseMessage);
        }
    }
}
