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
using Hazelcast.Query;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HMap<TKey, TValue> // Removing
    {
        /// <inheritdoc />
        public Task<bool> TryRemoveAsync(TKey key, TimeSpan timeToWait = default)
            => TryRemoveAsync(key, timeToWait, CancellationToken.None);

        private Task<bool> TryRemoveAsync(TKey key, TimeSpan timeToWait, CancellationToken cancellationToken)
            => TryRemoveAsync(ToSafeData(key), timeToWait, cancellationToken);


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

            using var requestMessage = MapTryRemoveCodec.EncodeRequest(Name, keyData, ContextId, timeoutMs);
            using var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CfAwait();
            var response = MapTryRemoveCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public Task<TValue> RemoveAsync(TKey key)
            => GetAndRemoveAsync(key, CancellationToken.None);

        private Task<TValue> GetAndRemoveAsync(TKey key, CancellationToken cancellationToken)
            => GetAndRemoveAsync(ToSafeData(key), cancellationToken);

        /// <summary>
        /// Removes an entry from this map, and returns the corresponding value if any.
        /// </summary>
        /// <param name="keyData">The key.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The value, if any, or default(TValue).</returns>
        protected virtual async Task<TValue> GetAndRemoveAsync(IData keyData, CancellationToken cancellationToken)
        {
            using var requestMessage = MapRemoveCodec.EncodeRequest(Name, keyData, ContextId);
            using var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CfAwait();
            var response = MapRemoveCodec.DecodeResponse(responseMessage).Response;
            return await ToObjectAsync<TValue>(response).CfAwait();
        }

        /// <inheritdoc />
        public Task<bool> RemoveAsync(TKey key, TValue value)
            => RemoveAsync(key, value, CancellationToken.None);

        private Task<bool> RemoveAsync(TKey key, TValue value, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var task = RemoveAsync(keyData, valueData, cancellationToken);
            return task;
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
            using var requestMessage = MapRemoveIfSameCodec.EncodeRequest(Name, keyData, valueData, ContextId);
            using var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CfAwait();
            var response = MapRemoveIfSameCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public Task DeleteAsync(TKey key)
            => RemoveAsync(key, CancellationToken.None);

        private Task RemoveAsync(TKey key, CancellationToken cancellationToken)
            => RemoveAsync(ToSafeData(key), cancellationToken);

        /// <inheritdoc />
        public Task RemoveAllAsync(IPredicate predicate)
            => RemoveAsync(predicate, CancellationToken.None);

        protected virtual async Task RemoveAsync(IPredicate predicate, CancellationToken cancellationToken)
        {
            var predicateData = ToSafeData(predicate);

            using var requestMessage = MapRemoveAllCodec.EncodeRequest(Name, predicateData);
            using var responseMessage = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
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
        protected virtual async Task RemoveAsync(IData keyData, CancellationToken cancellationToken = default)
        {
            using var requestMessage = MapDeleteCodec.EncodeRequest(Name, keyData, ContextId);
            using var task = Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken);

            await task.CfAwait();
        }

        /// <inheritdoc />
        public Task ClearAsync()
            => ClearAsync(CancellationToken.None);

        protected virtual async Task ClearAsync(CancellationToken cancellationToken)
        {
            using var requestMessage = MapClearCodec.EncodeRequest(Name);
            using var task = await Cluster.Messaging.SendAsync(requestMessage, cancellationToken).CfAwait();
        }
    }
}
