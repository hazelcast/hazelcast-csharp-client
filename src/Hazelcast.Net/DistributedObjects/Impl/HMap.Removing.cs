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
using Hazelcast.Query;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HMap<TKey, TValue> // Removing
    {
        /// <inheritdoc />
        public Task<bool> TryRemoveAsync(TKey key, TimeSpan timeToWait = default)
            => TryRemoveAsync(key, timeToWait, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<bool> TryRemoveAsync(TKey key, TimeSpan timeToWait, CancellationToken cancellationToken)
        {
            var task = TryRemoveAsync(ToSafeData(key), timeToWait, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <summary>
        /// Tries to remove an entry from the map within a timeout.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="timeToWait">A timeout.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>true if the entry was removed; otherwise false.</returns>
        /// <remarks>
        /// <para>This method returns false when no lock on the key could be
        /// acquired within the timeout.</para>
        /// TODO or when there was no value with that key?
        /// </remarks>
        protected virtual async Task<bool> TryRemoveAsync(IData keyData, TimeSpan timeToWait, CancellationToken cancellationToken)
        {
            var timeoutMs = timeToWait.RoundedMilliseconds();

            var requestMessage = MapTryRemoveCodec.EncodeRequest(Name, keyData, ContextId, timeoutMs);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CfAwait();
            var response = MapTryRemoveCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public Task<TValue> RemoveAsync(TKey key)
            => GetAndRemoveAsync(key, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<TValue> GetAndRemoveAsync(TKey key, CancellationToken cancellationToken)
        {
            var task = GetAndRemoveAsync(ToSafeData(key), cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <summary>
        /// Removes an entry from this map, and returns the corresponding value if any.
        /// </summary>
        /// <param name="keyData">The key.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The value, if any, or default(TValue).</returns>
        protected virtual async Task<TValue> GetAndRemoveAsync(IData keyData, CancellationToken cancellationToken)
        {
            var requestMessage = MapRemoveCodec.EncodeRequest(Name, keyData, ContextId);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CfAwait();
            var response = MapRemoveCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public Task<bool> RemoveAsync(TKey key, TValue value)
            => RemoveAsync(key, value, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<bool> RemoveAsync(TKey key, TValue value, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var task = RemoveAsync(keyData, valueData, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <summary>
        /// Removes an entry from this map.
        /// </summary>
        /// <param name="keyData">The key.</param>
        /// <param name="valueData">The value.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The value, if any, or default(TValue).</returns>
        /// <remarks>
        /// <para>This method removes an entry if the key and the value both match the
        /// specified key and value.</para>
        /// </remarks>
        protected virtual async Task<bool> RemoveAsync(IData keyData, IData valueData, CancellationToken cancellationToken)
        {
            var requestMessage = MapRemoveIfSameCodec.EncodeRequest(Name, keyData, valueData, ContextId);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CfAwait();
            var response = MapRemoveIfSameCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public Task DeleteAsync(TKey key)
            => RemoveAsync(key, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task RemoveAsync(TKey key, CancellationToken cancellationToken)
        {
            var task = RemoveAsync(ToSafeData(key), cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CfAwait();
#endif
        }

        /// <inheritdoc />
        public Task RemoveAllAsync(IPredicate predicate)
            => RemoveAsync(predicate, CancellationToken.None);

        protected virtual
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task RemoveAsync(IPredicate predicate, CancellationToken cancellationToken)
        {
            var predicateData = ToSafeData(predicate);

            var requestMessage = MapRemoveAllCodec.EncodeRequest(Name, predicateData);
            var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
            _ = MapRemoveAllCodec.DecodeResponse(responseMessage);
        }

        /// <summary>
        /// Removes an entry from this map.
        /// </summary>
        /// <param name="keyData">The key.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// <para>For performance reasons, this method does not return the value. Prefer
        /// <see cref="DeleteAsync"/> if the value is required.</para>
        /// </remarks>
        protected virtual
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task RemoveAsync(IData keyData, CancellationToken cancellationToken = default)
        {
            var requestMessage = MapDeleteCodec.EncodeRequest(Name, keyData, ContextId);
            var task = Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CfAwait();
#endif
        }

        /// <inheritdoc />
        public Task ClearAsync()
            => ClearAsync(CancellationToken.None);

        protected virtual
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task ClearAsync(CancellationToken cancellationToken)
        {
            var requestMessage = MapClearCodec.EncodeRequest(Name);
            var task = Cluster.Messaging.SendAsync(requestMessage, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CfAwait();
#endif
        }
    }
}
