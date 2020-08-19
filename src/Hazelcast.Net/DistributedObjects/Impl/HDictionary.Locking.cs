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

namespace Hazelcast.DistributedObjects.Impl
{
    // ReSharper disable UnusedTypeParameter
    internal partial class HDictionary<TKey, TValue> // Locking
    // ReSharper restore NonReadonlyMemberInGetHashCode
    {
        /// <inheritdoc />
        public Task LockAsync(TKey key)
            => LockAsync(key, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task LockAsync(TKey key, CancellationToken cancellationToken)
        {
            var task = LockForAsync(key, TimeToLive.InfiniteTimeSpan, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public Task LockForAsync(TKey key, TimeSpan leaseTime)
            => LockForAsync(key, leaseTime, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task LockForAsync(TKey key, TimeSpan leaseTime, CancellationToken cancellationToken)
        {
            var task = WaitLockForAsync(key, leaseTime, Timeout.InfiniteTimeSpan, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public Task<bool> TryLockAsync(TKey key)
            => TryLockAsync(key, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<bool> TryLockAsync(TKey key, CancellationToken cancellationToken)
        {
            var task = WaitLockForAsync(key, LeaseTime.InfiniteTimeSpan, Timeout.InfiniteTimeSpan, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public Task<bool> WaitLockAsync(TKey key, TimeSpan timeToWait)
            => WaitLockAsync(key, timeToWait, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<bool> WaitLockAsync(TKey key, TimeSpan timeToWait, CancellationToken cancellationToken)
        {
            var task = WaitLockForAsync(key, LeaseTime.InfiniteTimeSpan, timeToWait, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public Task<bool> WaitLockForAsync(TKey key, TimeSpan timeToWait, TimeSpan leaseTime)
            => WaitLockForAsync(key, timeToWait, leaseTime, CancellationToken.None);

        private async Task<bool> WaitLockForAsync(TKey key, TimeSpan timeToWait, TimeSpan leaseTime, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.GetNext();
            var leaseTimeMs = leaseTime.CodecMilliseconds(long.MaxValue);
            var timeToWaitMs = timeToWait.CodecMilliseconds(0);

            var requestMessage = MapTryLockCodec.EncodeRequest(Name, keyData, ContextId, leaseTimeMs, timeToWaitMs, refId);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CAF();
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
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CAF();
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
            await task.CAF();
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
            await task.CAF();
#endif
        }
    }
}
