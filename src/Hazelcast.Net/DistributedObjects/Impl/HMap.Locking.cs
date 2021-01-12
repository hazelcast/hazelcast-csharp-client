// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task LockAsync(TKey key)
        {
            var task = LockAsync(key, TimeSpanExtensions.MinusOneMillisecond);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CfAwait();
#endif
        }

        /// <inheritdoc />
        public async Task LockAsync(TKey key, TimeSpan leaseTime)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.GetNext();

            // codec wants -1 for server config, 0 for zero (useless), "max" for max = server config
            var leaseTimeMs = leaseTime.RoundedMilliseconds();

            var requestMessage = MapLockCodec.EncodeRequest(Name, keyData, ContextId, leaseTimeMs, refId);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CfAwait();
            _ = MapLockCodec.DecodeResponse(responseMessage);
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<bool> TryLockAsync(TKey key)
        {
            var task = TryLockAsync(key, TimeSpan.Zero, TimeSpanExtensions.MinusOneMillisecond);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
            Task<bool> TryLockAsync(TKey key, TimeSpan timeToWait)
        {
            var task = TryLockAsync(key, timeToWait, TimeSpanExtensions.MinusOneMillisecond);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <inheritdoc />
        public async Task<bool> TryLockAsync(TKey key, TimeSpan timeToWait, TimeSpan leaseTime)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.GetNext();

            // codec wants -1 for server config, 0 for zero (useless), "max" for max = server config
            var leaseTimeMs = leaseTime.RoundedMilliseconds();

            // codec wants -1 for infinite, 0 for zero
            var timeToWaitMs = timeToWait.RoundedMilliseconds();

            var requestMessage = MapTryLockCodec.EncodeRequest(Name, keyData, ContextId, leaseTimeMs, timeToWaitMs, refId);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CfAwait();
            var response = MapTryLockCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public Task<bool> IsLockedAsync(TKey key)
            => IsLockedAsync(key, CancellationToken.None);

        private async Task<bool> IsLockedAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);

            var requestMessage = MapIsLockedCodec.EncodeRequest(Name, keyData);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CfAwait();
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

            var requestMessage = MapUnlockCodec.EncodeRequest(Name, keyData, ContextId, refId);
            var task = Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CfAwait();
#endif
        }

        /// <inheritdoc />
        public Task ForceUnlockAsync(TKey key)
            => ForceUnlockAsync(key, CancellationToken.None);

        private async Task ForceUnlockAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.GetNext();

            var requestMessage = MapForceUnlockCodec.EncodeRequest(Name, keyData, refId);
            var task = Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CfAwait();
#endif
        }
    }
}
