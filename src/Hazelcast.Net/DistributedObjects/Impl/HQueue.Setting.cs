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
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HQueue<T> // Setting (Enqueue)
    {
        // <inheritdoc />
        // need that one because we are an HCollection - weird?
        // tries to enqueue immediately, does not wait & does not throw
        public override async Task<bool> AddAsync(T item) => await OfferAsync(item).CfAwait();

        // <inheritdoc />
        public async Task<bool> OfferAsync(T item, TimeSpan timeToWait = default)
            => await TryEnqueueAsync(item, timeToWait, CancellationToken.None).CfAwait();

        // <inheritdoc />
        // was: Put - enqueue, wait indefinitely, may throw
        public Task PutAsync(T item) => EnqueueAsync(item, CancellationToken.None);

        private async Task EnqueueAsync(T item, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);
            using var requestMessage = QueuePutCodec.EncodeRequest(Name, itemData);
            using var _ = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId, cancellationToken).CfAwait();
        }

        private async Task<bool> TryEnqueueAsync(T item, TimeSpan timeToWait, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);

            var timeToWaitMilliseconds = (long)timeToWait.TotalMilliseconds;
            using var requestMessage = QueueOfferCodec.EncodeRequest(Name, itemData, timeToWaitMilliseconds);
            using var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId, cancellationToken).CfAwait();
            return QueueOfferCodec.DecodeResponse(responseMessage).Response;
        }

        // <inheritdoc />
        // need to have it because HCollection but feels weird
        // maybe we need to have something above HCollection?
        public override async Task<bool> AddAll<TItem>(ICollection<TItem> items)
        {
            var itemsData = ToSafeData(items);
            using var requestMessage = QueueAddAllCodec.EncodeRequest(Name, itemsData);
            using var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return QueueAddAllCodec.DecodeResponse(responseMessage).Response;
        }
    }
}
