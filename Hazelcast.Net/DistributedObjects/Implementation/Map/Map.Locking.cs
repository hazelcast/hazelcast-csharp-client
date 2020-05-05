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
    // partial: locking
    internal partial class Map<TKey, TValue>
    {
        /// <inheritdoc />
        public async Task LockAsync(TKey key)
            => await LockAsync(key, Timeout.InfiniteTimeSpan);

        /// <inheritdoc />
        public async Task LockAsync(TKey key, TimeSpan leaseTime)
            => await TryLockAsync(key, leaseTime, Timeout.InfiniteTimeSpan);

        /// <inheritdoc />
        public async Task<bool> TryLockAsync(TKey key)
            => await TryLockAsync(key, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

        /// <inheritdoc />
        public async Task<bool> TryLockAsync(TKey key, TimeSpan timeout)
            => await TryLockAsync(key, Timeout.InfiniteTimeSpan, timeout);

        /// <inheritdoc />
        public async Task<bool> TryLockAsync(TKey key, TimeSpan leaseTime, TimeSpan timeout)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.Next;
            var leaseTimeMs = leaseTime.CodecMilliseconds(long.MaxValue);
            var timeoutMs = timeout.CodecMilliseconds(0);

            var requestMessage = MapTryLockCodec.EncodeRequest(Name, keyData, ThreadId, leaseTimeMs, timeoutMs, refId);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapTryLockCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<bool> IsLockedAsync(TKey key)
        {
            var keyData = ToSafeData(key);

            var requestMessage = MapIsLockedCodec.EncodeRequest(Name, keyData);
            var responseMessage = await Cluster.SendAsync(requestMessage, keyData);
            var response = MapIsLockedCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task UnlockAsync(TKey key)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.Next;

            var requestMessage = MapUnlockCodec.EncodeRequest(Name, keyData, ThreadId, refId);
            await Cluster.SendAsync(requestMessage, keyData);
        }

        /// <inheritdoc />
        public async Task ForceUnlockAsync(TKey key)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.Next;

            var requestMessage = MapForceUnlockCodec.EncodeRequest(Name, keyData, refId);
            await Cluster.SendAsync(requestMessage, keyData);
        }
    }
}