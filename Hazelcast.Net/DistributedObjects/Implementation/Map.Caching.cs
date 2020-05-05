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

using System.Threading.Tasks;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Implementation
{
    // partial: caching
    internal partial class Map<TKey, TValue>
    {
        /// <inheritdoc />
        public async Task<bool> EvictAsync(TKey key)
        {
            var keyData = ToSafeData(key);

            var requestMessage = MapEvictCodec.EncodeRequest(Name, keyData, ThreadId);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapEvictCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task EvictAllAsync()
        {
            var requestMessage = MapEvictAllCodec.EncodeRequest(Name);
            await Cluster.SendAsync(requestMessage);
        }

        /// <inheritdoc />
        public async Task FlushAsync()
        {
            var requestMessage = MapFlushCodec.EncodeRequest(Name);
            await Cluster.SendAsync(requestMessage);
        }
    }
}