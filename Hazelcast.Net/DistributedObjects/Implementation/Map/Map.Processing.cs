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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Implementation.Map
{
    // partial: processing
    internal partial class Map<TKey, TValue>
    {
        /// <inheritdoc />
        public async Task<object> ExecuteAsync(IEntryProcessor processor, TKey key)
        {
            var (keyData, processorData) = ToSafeData(key, processor);
            return await ExecuteAsync(keyData, processorData);
        }

        /// <summary>
        /// Processes an entry.
        /// </summary>
        /// <param name="keyData">The key.</param>
        /// <param name="processorData">An entry processor.</param>
        /// <returns>The result of the process.</returns>
        /// <remarks>
        /// <para>The <paramref name="processorData"/> must have a counterpart on the server.</para>
        /// </remarks>
        protected virtual async Task<object> ExecuteAsync(IData processorData, IData keyData)
        {
            var requestMessage = MapExecuteOnKeyCodec.EncodeRequest(Name, processorData, keyData, ThreadId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData);
            var response = MapExecuteOnKeyCodec.DecodeResponse(responseMessage).Response;
            return ToObject<object>(response);
        }

        /// <inheritdoc />
        public async Task<IDictionary<TKey, object>> ExecuteAsync(IEntryProcessor processor, IEnumerable<TKey> keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            var keysmap = keys.ToDictionary(x => ToSafeData(x), x => x);
            if (keysmap.Count == 0) return new Dictionary<TKey, object>();
            var processorData = ToSafeData(processor);

            var requestMessage = MapExecuteOnKeysCodec.EncodeRequest(Name, processorData, keysmap.Keys);
            var responseMessage = await Cluster.SendAsync(requestMessage);
            var response = MapExecuteOnKeysCodec.DecodeResponse(responseMessage).Response;

            var result = new Dictionary<TKey, object>();
            foreach (var (keyData, valueData) in response)
            {
                if (!keysmap.TryGetValue(keyData, out var key))
                    throw new InvalidOperationException("Server returned an unexpected key.");
                result[key] = ToObject<object>(valueData);
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<IDictionary<TKey, object>> ExecuteAsync(IEntryProcessor processor)
        {
            var processorData = ToSafeData(processor);

            var requestMessage = MapExecuteOnAllKeysCodec.EncodeRequest(Name, processorData);
            var responseMessage = await Cluster.SendAsync(requestMessage);
            var response = MapExecuteOnAllKeysCodec.DecodeResponse(responseMessage).Response;

            var result = new Dictionary<TKey, object>();
            foreach (var (keyData, valueData) in response)
                result[ToObject<TKey>(keyData)] = ToObject<object>(valueData);
            return result;

        }

        /// <inheritdoc />
        public async Task<object> ApplyAsync(IEntryProcessor processor, TKey key)
        {
            var (keyData, processorData) = ToSafeData(key, processor);
            return await ApplyAsync(processorData, keyData);
        }

        // FIXME: do we want this?
        protected virtual async Task<object> ApplyAsync(IData processorData, IData keyData)
        {
            var requestMessage = MapSubmitToKeyCodec.EncodeRequest(Name, processorData, keyData, ThreadId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData);
            var response = MapSubmitToKeyCodec.DecodeResponse(responseMessage).Response;
            return ToObject<object>(response);
        }
    }
}