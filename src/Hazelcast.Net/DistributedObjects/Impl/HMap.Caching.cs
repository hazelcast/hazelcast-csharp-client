// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Impl
{
    // ReSharper disable once UnusedTypeParameter
    internal partial class HMap<TKey, TValue> // Caching
    {
        /// <inheritdoc />
        public Task<bool> EvictAsync(TKey key)
            => EvictAsync(key, CancellationToken.None);

        private async Task<bool> EvictAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);

            var requestMessage = MapEvictCodec.EncodeRequest(Name, keyData, ContextId);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CfAwait();
            var response = MapEvictCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public Task EvictAllAsync()
            => EvictAllAsync(CancellationToken.None);

        private async Task EvictAllAsync(CancellationToken cancellationToken)
        {
            var requestMessage = MapEvictAllCodec.EncodeRequest(Name);
            await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
        }

        /// <inheritdoc />
        public Task FlushAsync()
            => FlushAsync(CancellationToken.None);

        private async Task FlushAsync(CancellationToken cancellationToken)
        {
            var requestMessage = MapFlushCodec.EncodeRequest(Name);
            await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
        }

        /// <inheritdoc />
        public async Task LoadAllAsync(bool replaceExistingValues)
        {
            var requestMessage = MapLoadAllCodec.EncodeRequest(Name, replaceExistingValues);
            await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
        }

        /// <inheritdoc />
        public async Task LoadAllAsync(ICollection<TKey> keys, bool replaceExistingValues)
        {
            var keysData = keys.Select(key => ToSafeData(key)).ToList();
            var requestMessage = MapLoadGivenKeysCodec.EncodeRequest(Name, keysData, replaceExistingValues);
            await Cluster.Messaging.SendAsync(requestMessage).CfAwait();
        }
    }
}
