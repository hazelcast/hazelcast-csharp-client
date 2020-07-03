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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HList<T> // Setting
    {
        /// <inheritdoc />
        public async Task<T> SetAsync(int index, T item, CancellationToken cancellationToken = default)
        {
            var itemData = ToSafeData(item);
            var requestMessage = ListSetCodec.EncodeRequest(Name, index, itemData);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            var response = ListSetCodec.DecodeResponse(responseMessage).Response;
            return ToObject<T>(response);
        }

        /// <inheritdoc />
        public async Task InsertAsync(int index, T item, CancellationToken cancellationToken = default)
        {
            var itemData = ToSafeData(item);
            var requestMessage = ListAddWithIndexCodec.EncodeRequest(Name, index, itemData);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            _ = ListSetCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public override async Task<bool> AddAsync(T item, CancellationToken cancellationToken = default)
        {
            var itemData = ToSafeData(item);
            var requestMessage = ListAddCodec.EncodeRequest(Name, itemData);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            return ListAddCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public override async Task<bool> AddRangeAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken = default)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = ListAddAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            return ListAddAllCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<bool> InsertRangeAsync<TItem>(int index, ICollection<TItem> items, CancellationToken cancellationToken = default)
            where TItem : T
        {
            var itemsData = ToSafeData(items);
            var requestMessage = ListAddAllWithIndexCodec.EncodeRequest(Name, index, itemsData);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, PartitionKeyData, cancellationToken).CAF();
            return ListAddAllWithIndexCodec.DecodeResponse(responseMessage).Response;
        }

    }
}
