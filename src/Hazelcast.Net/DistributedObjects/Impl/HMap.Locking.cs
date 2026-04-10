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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.DistributedObjects.Impl
{
    // ReSharper disable UnusedTypeParameter
    internal partial class HMap<TKey, TValue> // Locking
    {
        /// <inheritdoc />
        public Task LockAsync(TKey key) => LockAsync(key, TimeSpanExtensions.MinusOneMillisecond);

        /// <inheritdoc />
        public async Task LockAsync(TKey key, TimeSpan leaseTime)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.GetNext();

            // codec wants -1 for server config, 0 for zero (useless), "max" for max = server config
            var leaseTimeMs = leaseTime.RoundedMilliseconds();

            using var requestMessage = MapLockCodec.EncodeRequest(Name, keyData, ContextId, leaseTimeMs, refId);
            using var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CfAwait();
            _ = MapLockCodec.DecodeResponse(responseMessage);
        }

        /// <inheritdoc />
        public Task<bool> TryLockAsync(TKey key) => TryLockAsync(key, TimeSpan.Zero, TimeSpanExtensions.MinusOneMillisecond);

        /// <inheritdoc />
        public Task<bool> TryLockAsync(TKey key, TimeSpan timeToWait)
            => TryLockAsync(key, timeToWait, TimeSpanExtensions.MinusOneMillisecond);

        /// <inheritdoc />
        public async Task<bool> TryLockAsync(TKey key, TimeSpan timeToWait, TimeSpan leaseTime)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.GetNext();

            // codec wants -1 for server config, 0 for zero (useless), "max" for max = server config
            var leaseTimeMs = leaseTime.RoundedMilliseconds();

            // codec wants -1 for infinite, 0 for zero
            var timeToWaitMs = timeToWait.RoundedMilliseconds();

            using var requestMessage = MapTryLockCodec.EncodeRequest(Name, keyData, ContextId, leaseTimeMs, timeToWaitMs, refId);
            using var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CfAwait();
            var response = MapTryLockCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public Task<bool> IsLockedAsync(TKey key)
            => IsLockedAsync(key, CancellationToken.None);

        private async Task<bool> IsLockedAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);

            using var requestMessage = MapIsLockedCodec.EncodeRequest(Name, keyData);
            using var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CfAwait();
            var response = MapIsLockedCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public Task UnlockAsync(TKey key)
            => UnlockAsync(key, CancellationToken.None);

        private async Task UnlockAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.GetNext();

            using var requestMessage = MapUnlockCodec.EncodeRequest(Name, keyData, ContextId, refId);
            using var task = Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken);
            await task.CfAwait();
        }

        /// <inheritdoc />
        public Task ForceUnlockAsync(TKey key)
            => ForceUnlockAsync(key, CancellationToken.None);

        private async Task ForceUnlockAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.GetNext();

            using var requestMessage = MapForceUnlockCodec.EncodeRequest(Name, keyData, refId);
            using var task = Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken);

            await task.CfAwait();
        }
    }
}
