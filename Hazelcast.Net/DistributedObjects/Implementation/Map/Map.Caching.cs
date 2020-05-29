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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Implementation.Map
{
    // ReSharper disable once UnusedTypeParameter
    internal partial class Map<TKey, TValue> // Caching
    {
        /// <inheritdoc />
        public async Task<bool> EvictAsync(TKey key, TimeSpan timeout = default)
        {
            var keyData = ToSafeData(key);
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);

            var requestMessage = MapEvictCodec.EncodeRequest(Name, keyData, ThreadId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellation.Token).OrTimeout(cancellation).CAF();
            var response = MapEvictCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<bool> EvictAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);

            var requestMessage = MapEvictCodec.EncodeRequest(Name, keyData, ThreadId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CAF();
            var response = MapEvictCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task EvictAllAsync(TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var requestMessage = MapEvictAllCodec.EncodeRequest(Name);
            await Cluster.SendAsync(requestMessage, cancellation.Token).OrTimeout(cancellation).CAF();
        }

        /// <inheritdoc />
        public async Task EvictAllAsync(CancellationToken cancellationToken)
        {
            var requestMessage = MapEvictAllCodec.EncodeRequest(Name);
            await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
        }

        /// <inheritdoc />
        public async Task FlushAsync(TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var requestMessage = MapFlushCodec.EncodeRequest(Name);
            await Cluster.SendAsync(requestMessage, cancellation.Token).OrTimeout(cancellation).CAF();
        }

        /// <inheritdoc />
        public async Task FlushAsync(CancellationToken cancellationToken)
        {
            var requestMessage = MapFlushCodec.EncodeRequest(Name);
            await Cluster.SendAsync(requestMessage, cancellationToken).CAF();
        }
    }
}
