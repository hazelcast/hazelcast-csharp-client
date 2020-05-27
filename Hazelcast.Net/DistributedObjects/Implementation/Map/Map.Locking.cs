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
    // ReSharper disable twice UnusedTypeParameter
    internal partial class Map<TKey, TValue> // Locking
    {
        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task LockAsync(TKey key, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = LockAsync(key, cancellation.Token).OrTimeout(cancellation);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task LockAsync(TKey key, CancellationToken cancellationToken)
        {
            var task = LockForAsync(key, TimeToLive.InfiniteTimeSpan, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task LockForAsync(TKey key, TimeSpan leaseTime, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = LockForAsync(key, leaseTime, cancellation.Token).OrTimeout(cancellation);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task LockForAsync(TKey key, TimeSpan leaseTime, CancellationToken cancellationToken)
        {
            var task = WaitLockForAsync(key, leaseTime, Timeout.InfiniteTimeSpan, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task<bool> TryLockAsync(TKey key, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = TryLockAsync(key, cancellation.Token).OrTimeout(cancellation);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task<bool> TryLockAsync(TKey key, CancellationToken cancellationToken)
        {
            var task = WaitLockForAsync(key, LeaseTime.InfiniteTimeSpan, Timeout.InfiniteTimeSpan, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task<bool> WaitLockAsync(TKey key, TimeSpan serverTimeout, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = WaitLockAsync(key, serverTimeout, cancellation.Token).OrTimeout(cancellation);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task<bool> WaitLockAsync(TKey key, TimeSpan serverTimeout, CancellationToken cancellationToken)
        {
            var task = WaitLockForAsync(key, LeaseTime.InfiniteTimeSpan, serverTimeout, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<bool> WaitLockForAsync(TKey key, TimeSpan leaseTime, TimeSpan serverTimeout, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = WaitLockForAsync(key, leaseTime, serverTimeout, cancellation.Token).OrTimeout(cancellation);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<bool> WaitLockForAsync(TKey key, TimeSpan leaseTime, TimeSpan serverTimeout, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.Next;
            var leaseTimeMs = leaseTime.CodecMilliseconds(long.MaxValue);
            var timeoutMs = serverTimeout.CodecMilliseconds(0);

            var requestMessage = MapTryLockCodec.EncodeRequest(Name, keyData, ThreadId, leaseTimeMs, timeoutMs, refId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CAF();
            var response = MapTryLockCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<bool> IsLockedAsync(TKey key, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = IsLockedAsync(key, cancellation.Token).OrTimeout(cancellation);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<bool> IsLockedAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);

            var requestMessage = MapIsLockedCodec.EncodeRequest(Name, keyData);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CAF();
            var response = MapIsLockedCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task UnlockAsync(TKey key, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = UnlockAsync(key, cancellation.Token).OrTimeout(cancellation);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task UnlockAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.Next;

            var requestMessage = MapUnlockCodec.EncodeRequest(Name, keyData, ThreadId, refId);
            var task = Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task ForceUnlockAsync(TKey key, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = ForceUnlockAsync(key, cancellation.Token).OrTimeout(cancellation);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task ForceUnlockAsync(TKey key, CancellationToken cancellationToken)
        {
            var keyData = ToSafeData(key);

            var refId = _lockReferenceIdSequence.Next;

            var requestMessage = MapForceUnlockCodec.EncodeRequest(Name, keyData, refId);
            var task = Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }
    }
}