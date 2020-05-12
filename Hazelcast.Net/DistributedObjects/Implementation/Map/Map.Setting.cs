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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Implementation.Map
{
    // partial: setting
    internal partial class Map<TKey, TValue>
    {
        /// <inheritdoc />
        public async Task<TValue> AddOrReplaceWithValueAsync(TKey key, TValue value)
            => await AddOrReplaceWithValueAsync(key, value, Timeout.InfiniteTimeSpan);

        /// <inheritdoc />
        public async Task AddOrReplaceAsync(TKey key, TValue value)
            => await AddOrReplaceAsync(key, value, Timeout.InfiniteTimeSpan);

        /// <inheritdoc />
        public async Task<TValue> AddOrReplaceWithValueAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            return await AddOrReplaceWithValueAsync(keyData, valueData, timeToLive);
        }

        /// <summary>
        /// Adds or replaces an entry with a time-to-live and returns the previous value.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="valueData">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>The value previously associated with the key in the map, if any; otherwise default(<typeparamref name="TValue"/>).</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// </remarks>
        protected virtual async Task<TValue> AddOrReplaceWithValueAsync(IData keyData, IData valueData, TimeSpan timeToLive)
        {
            var timeToLiveMs = timeToLive.CodecMilliseconds(-1000);

            var requestMessage = MapPutCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeToLiveMs);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData);
            var response = MapPutCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public async Task AddOrReplaceAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            await AddOrReplaceAsync(keyData, valueData, timeToLive);
        }

        /// <summary>
        /// Adds or replaces an entry with a time-to-live.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="valueData">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>Nothing.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// </remarks>
        protected virtual async Task AddOrReplaceAsync(IData keyData, IData valueData, TimeSpan timeToLive)
        {
            var timeToLiveMs = timeToLive.CodecMilliseconds(-1000);

            var requestMessage = MapSetCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeToLiveMs);
            await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData);
        }

        /// <inheritdoc />
        public async Task AddOrReplaceAsync(IDictionary<TKey, TValue> entries)
        {
            // TODO: is this transactional? can some entries be created and others be missing?

            var ownerEntries = new Dictionary<Guid, Dictionary<int, List<KeyValuePair<IData, IData>>>>();

            // verify entries + group by owner and partitions
            foreach (var (key, value) in entries)
            {
                var (keyData, valueData) = ToSafeData(key, value);

                var partitionId = Cluster.Partitioner.GetPartitionId(keyData);
                var ownerId = Cluster.Partitioner.GetPartitionOwner(partitionId);
                if (!ownerEntries.TryGetValue(ownerId, out var part))
                    part = ownerEntries[ownerId] = new Dictionary<int, List<KeyValuePair<IData, IData>>>();
                if (!part.TryGetValue(partitionId, out var list))
                    list = part[partitionId] = new List<KeyValuePair<IData, IData>>();
                list.Add(new KeyValuePair<IData, IData>(keyData, valueData));
            }

            await AddOrReplaceAsync(ownerEntries);
        }

        /// <summary>
        /// Adds or replaces entries.
        /// </summary>
        /// <param name="ownerEntries">Entries.</param>
        /// <returns>Nothing.</returns>
        protected virtual async Task AddOrReplaceAsync(Dictionary<Guid, Dictionary<int, List<KeyValuePair<IData, IData>>>> ownerEntries)
        {
            // TODO: add a SendAsync(...) to Cluster/Client
            // that can send multiple messages and use one single completion source
            // cannot inherit from TaskCompletionSource: it's not sealed but nothing is virtual

            // create parallel tasks to fire requests for each owner (each network client)
            // for each owner, serialize requests for each partition, because each message
            // needs to have its own partition id
            var tasks = new List<Task>();
            foreach (var (ownerId, part) in ownerEntries)
            {
                foreach (var (partitionId, list) in part)
                {
                    if (list.Count == 0) continue;

                    var requestMessage = MapPutAllCodec.EncodeRequest(Name, list);
                    requestMessage.PartitionId = partitionId;
                    var task = Cluster.SendToMemberAsync(requestMessage, ownerId).AsTask();
                    tasks.Add(task);
                }
            }

            // and wait on all tasks, ignoring the responses
            await Task.WhenAll(tasks);
        }

        /// <inheritdoc />
        public async Task<TValue> ReplaceAsync(TKey key, TValue newValue)
        {
            var (keyData, valueData) = ToSafeData(key, newValue);

            var requestMessage = MapReplaceCodec.EncodeRequest(Name, keyData, valueData, ThreadId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData);
            var response = MapReplaceCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public async Task<bool> ReplaceAsync(TKey key, TValue expectedValue, TValue newValue)
        {
            var (keyData, expectedData, newData) = ToSafeData(key, expectedValue, newValue);
            return await ReplaceAsync(keyData, expectedData, newData);
        }

        /// <summary>
        /// Replaces an existing entry.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="expectedData">The expected value.</param>
        /// <param name="newData">The new value.</param>
        /// <returns>true if the entry was replaced; otherwise false.</returns>
        protected async Task<bool> ReplaceAsync(IData keyData, IData expectedData, IData newData)
        {
            var requestMessage = MapReplaceIfSameCodec.EncodeRequest(Name, keyData, expectedData, newData, ThreadId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData);
            var response = MapReplaceIfSameCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<bool> TryAddOrReplaceAsync(TKey key, TValue value, TimeSpan timeout)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            return await TryAddOrReplaceAsync(keyData, valueData, timeout);
        }

        /// <summary>
        /// Tries to set an entry within a timeout.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="valueData">A value.</param>
        /// <param name="timeout">A timeout.</param>
        /// <returns>true if the entry was set; otherwise false.</returns>
        /// <remarks>
        /// <para>This method returns false when no lock on the key could be
        /// acquired within the timeout.</para>
        /// </remarks>
        protected virtual async Task<bool> TryAddOrReplaceAsync(IData keyData, IData valueData, TimeSpan timeout)
        {
            var timeoutMs = timeout.CodecMilliseconds(0);

            var requestMessage = MapTryPutCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeoutMs);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData);
            var response = MapTryPutCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public async Task<TValue> AddIfMissingAsync(TKey key, TValue value)
            => await AddIfMissingAsync(key, value, Timeout.InfiniteTimeSpan);

        /// <inheritdoc />
        public async Task<TValue> AddIfMissingAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            return await AddIfMissingAsync(keyData, valueData, timeToLive);
        }

        /// <summary>
        /// Adds an entry with a time-to-live, if no entry with the key exists.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="valueData">The value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <returns>The existing value, if any; otherwise the default value.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// </remarks>
        protected virtual async Task<TValue> AddIfMissingAsync(IData keyData, IData valueData, TimeSpan timeToLive)
        {
            var timeToLiveMs = timeToLive.CodecMilliseconds(-1000);

            var requestMessage = MapPutIfAbsentCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeToLiveMs);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData);
            var response = MapPutIfAbsentCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public async Task AddOrReplaceTransientAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            await AddOrReplaceTransientAsync(keyData, valueData, timeToLive);
        }

        /// <summary>
        /// Adds a transient entry.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="valueData">The value.</param>
        /// <param name="timeToLive">A time to live.</param>
        protected virtual async Task AddOrReplaceTransientAsync(IData keyData, IData valueData, TimeSpan timeToLive)
        {
            var timeToLiveMs = timeToLive.CodecMilliseconds(-1000);

            var requestMessage = MapPutTransientCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeToLiveMs);
            _ = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData);
        }
    }
}