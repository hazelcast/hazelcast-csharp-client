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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HQueue<T> // Dequeue
    {
        /// <inheritdoc />
        public async Task<T> PeekAsync() // peek, or null
        {
            var requestMessage = QueuePeekCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            var response = QueuePeekCodec.DecodeResponse(responseMessage).Response;
            return await ToObjectAsync<T>(response).CfAwait();
        }

        /// <inheritdoc />
        public async Task<T> GetElementAsync()
        {
            return await PeekAsync().CfAwait() ?? throw new InvalidOperationException("The queue is empty.");
        }

        /// <inheritdoc />
        public async Task<T> PollAsync(TimeSpan timeToWait = default)
        {
            var timeToWaitMs = timeToWait.RoundedMilliseconds(); // codec: 0 = zero, -1 = infinite

            var requestMessage = QueuePollCodec.EncodeRequest(Name, timeToWaitMs);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            var response = QueuePollCodec.DecodeResponse(responseMessage).Response;
            return await ToObjectAsync<T>(response).CfAwait();
        }

        /// <inheritdoc />
        public async Task<T> TakeAsync()
        {
            var requestMessage = QueueTakeCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            var response = QueueTakeCodec.DecodeResponse(responseMessage).Response;
            return await ToObjectAsync<T>(response).CfAwait();
        }

         /// <inheritdoc />
        public async Task<int> DrainToAsync(ICollection<T> items)
        {
            var requestMessage = QueueDrainToCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            var response = QueueDrainToMaxSizeCodec.DecodeResponse(responseMessage).Response;

            foreach (var itemData in response) items.Add(await ToObjectAsync<T>(itemData).CfAwait());
            return response.Count;
        }

         /// <inheritdoc />
        public async Task<int> DrainToAsync(ICollection<T> items, int maxElements)
        {
            var requestMessage = QueueDrainToMaxSizeCodec.EncodeRequest(Name, maxElements);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            var response = QueueDrainToMaxSizeCodec.DecodeResponse(responseMessage).Response;

            foreach (var itemData in response) items.Add(await ToObjectAsync<T>(itemData).CfAwait());
            return response.Count;
        }
    }
}
