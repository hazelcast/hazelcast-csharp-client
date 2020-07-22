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

namespace Hazelcast.DistributedObjects.HMapImpl
{
    // ReSharper disable UnusedTypeParameter
    internal partial class HMap<TKey, TValue> // Locking
    // ReSharper restore NonReadonlyMemberInGetHashCode
    {
        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task LockAsync(TKey key, CancellationToken cancellationToken = default)
        {
            var task = LockForAsync(key, TimeToLive.InfiniteTimeSpan, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task LockForAsync(TKey key, TimeSpan leaseTime, CancellationToken cancellationToken = default)
        {
            var task = WaitLockForAsync(key, leaseTime, Timeout.InfiniteTimeSpan, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<bool> TryLockAsync(TKey key, CancellationToken cancellationToken = default)
        {
            var task = WaitLockForAsync(key, LeaseTime.InfiniteTimeSpan, Timeout.InfiniteTimeSpan, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<bool> WaitLockAsync(TKey key, TimeSpan timeToWait, CancellationToken cancellationToken = default)
        {
            var task = WaitLockForAsync(key, LeaseTime.InfiniteTimeSpan, timeToWait, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<bool> WaitLockForAsync(TKey key, TimeSpan timeToWait, TimeSpan leaseTime, CancellationToken cancellationToken = default)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.GetNext();
            var leaseTimeMs = leaseTime.CodecMilliseconds(long.MaxValue);
            var timeToWaitMs = timeToWait.CodecMilliseconds(0);

            var requestMessage = MapTryLockCodec.EncodeRequest(Name, keyData, ContextId, leaseTimeMs, timeToWaitMs, refId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CAF();
            var response = MapTryLockCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<bool> IsLockedAsync(TKey key, CancellationToken cancellationToken = default)
        {
            var keyData = ToSafeData(key);

            var requestMessage = MapIsLockedCodec.EncodeRequest(Name, keyData);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CAF();
            var response = MapIsLockedCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task UnlockAsync(TKey key, CancellationToken cancellationToken = default)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.GetNext();

            var requestMessage = MapUnlockCodec.EncodeRequest(Name, keyData, ContextId, refId);
            var task = Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task ForceUnlockAsync(TKey key, CancellationToken cancellationToken = default)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.GetNext();

            var requestMessage = MapForceUnlockCodec.EncodeRequest(Name, keyData, refId);
            var task = Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }
    }
}
