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

using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization.Collections;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HList<T> // Getting
    {
        /// <inheritdoc />
        public override async Task<IReadOnlyList<T>> GetAsync()
        {
            var requestMessage = ListGetAllCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData).CAF();
            var response = ListGetAllCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<T>(response, SerializationService);
        }

        /// <inheritdoc />
        public async Task<T> GetAsync(int index)
        {
            var requestMessage = ListGetCodec.EncodeRequest(Name, index);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData).CAF();
            var response = ListGetCodec.DecodeResponse(responseMessage).Response;
            return ToObject<T>(response);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<T>> GetRangeAsync(int fromIndex, int toIndex)
        {
            var requestMessage = ListSubCodec.EncodeRequest(Name, fromIndex, toIndex);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData).CAF();
            var response = ListSubCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<T>(response, SerializationService);
        }

        /// <inheritdoc />
        public async Task<int> IndexOfAsync(T item)
        {
            var itemData = ToSafeData(item);
            var requestMessage = ListIndexOfCodec.EncodeRequest(Name, itemData);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData).CAF();
            return ListIndexOfCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<int> LastIndexOfAsync(T item)
        {
            var itemData = ToSafeData(item);
            var requestMessage = ListLastIndexOfCodec.EncodeRequest(Name, itemData);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData).CAF();
            return ListLastIndexOfCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public override async Task<bool> ContainsAsync(T item)
        {
            var itemData = ToSafeData(item);
            var requestMessage = ListContainsCodec.EncodeRequest(Name, itemData);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData).CAF();
            return ListContainsCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public override async Task<bool> ContainsAllAsync<TItem>(ICollection<TItem> items)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = ListContainsAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData).CAF();
            return ListContainsAllCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public override async Task<bool> IsEmptyAsync()
        {
            var requestMessage = ListIsEmptyCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData).CAF();
            return ListIsEmptyCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public override async Task<int> CountAsync()
        {
            var requestMessage = ListSizeCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData).CAF();
            return ListSizeCodec.DecodeResponse(responseMessage).Response;
        }
    }
}
