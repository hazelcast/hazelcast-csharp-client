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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Implementation.Map
{
    // partial: removing
    internal partial class Map<TKey, TValue>
    {
        /// <inheritdoc />
        public async Task<bool> TryRemoveAsync(TKey key, TimeSpan timeout)
            => await TryRemoveAsync(ToSafeData(key), timeout);

        /// <summary>
        /// Tries to remove an entry from the map within a timeout.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>true if the entry was removed; otherwise false.</returns>
        /// <remarks>
        /// <para>This method returns false when no lock on the key could be
        /// acquired within the timeout.</para>
        /// TODO or when there was no value with that key?
        /// </remarks>
        protected virtual async Task<bool> TryRemoveAsync(IData keyData, TimeSpan timeout)
        {
            var timeoutMs = timeout.CodecMilliseconds(0);

            var requestMessage = MapTryRemoveCodec.EncodeRequest(Name, keyData, ThreadId, timeoutMs);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData);
            var response = MapTryRemoveCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<TValue> RemoveAsync(TKey key)
            => await RemoveAsync(ToSafeData(key));

        /// <summary>
        /// Removes an entry from this map, and returns the corresponding value if any.
        /// </summary>
        /// <param name="keyData">The key.</param>
        /// <returns>The value, if any, or default(TValue).</returns>
        protected virtual async Task<TValue> RemoveAsync(IData keyData)
        {
            var requestMessage = MapRemoveCodec.EncodeRequest(Name, keyData, ThreadId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData);
            var response = MapRemoveCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public async Task<bool> RemoveAsync(TKey key, TValue value)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            return await RemoveAsync(keyData, valueData);
        }

        /// <summary>
        /// Removes an entry from this map.
        /// </summary>
        /// <param name="keyData">The key.</param>
        /// <param name="valueData">The value.</param>
        /// <returns>The value, if any, or default(TValue).</returns>
        /// <remarks>
        /// <para>This method removes an entry if the key and the value both match the
        /// specified key and value.</para>
        /// </remarks>
        protected virtual async Task<bool> RemoveAsync(IData keyData, IData valueData)
        {
            var requestMessage = MapRemoveIfSameCodec.EncodeRequest(Name, keyData, valueData, ThreadId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData);
            var response = MapRemoveIfSameCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task DeleteAsync(TKey key)
            => await DeleteAsync(ToSafeData(key));

        /// <summary>
        /// Removes an entry from this map.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <remarks>
        /// <para>For performance reasons, this method does not return the value. Prefer
        /// <see cref="RemoveAsync(TKey)"/> if the value is required.</para>
        /// </remarks>
        protected virtual async Task DeleteAsync(IData keyData)
        {
            var requestMessage = MapDeleteCodec.EncodeRequest(Name, keyData, ThreadId);
            await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData);
        }

        /// <inheritdoc />
        public virtual async Task ClearAsync()
        {
            var requestMessage = MapClearCodec.EncodeRequest(Name);
            await Cluster.SendAsync(requestMessage);
        }
    }
}