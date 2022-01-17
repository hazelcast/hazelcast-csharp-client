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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HMap<TKey, TValue> // Setting
    {
        /// <inheritdoc />
        public Task SetAsync(TKey key, TValue value)
            => SetAsync(key, value, TimeSpanExtensions.MinusOneMillisecond, TimeSpanExtensions.MinusOneMillisecond);

        /// <inheritdoc />
        public Task<TValue> PutAsync(TKey key, TValue value)
            => PutAsync(key, value, TimeSpanExtensions.MinusOneMillisecond, TimeSpanExtensions.MinusOneMillisecond);

        /// <inheritdoc />
        public Task SetAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            return SetAsync(keyData, valueData, timeToLive, TimeSpanExtensions.MinusOneMillisecond);
        }

        /// <inheritdoc />
        public Task SetAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan maxIdle)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            return SetAsync(keyData, valueData, timeToLive, maxIdle);
        }

        protected virtual async Task SetAsync(IData keyData, IData valueData, TimeSpan timeToLive, TimeSpan maxIdle)
        {
            var timeToLiveMs = timeToLive.RoundedMilliseconds(false); // codec: 0 is infinite, -1 is server
            var maxIdleMs = maxIdle.RoundedMilliseconds(false); // codec: 0 is infinite, -1 is server
            var withMaxIdle = maxIdleMs != -1;

            var requestMessage = withMaxIdle
                ? MapSetWithMaxIdleCodec.EncodeRequest(Name, keyData, valueData, ContextId, timeToLiveMs, maxIdleMs)
                : MapSetCodec.EncodeRequest(Name, keyData, valueData, ContextId, timeToLiveMs);

            await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CfAwait();
        }

        /// <inheritdoc />
        public Task<TValue> PutAsync(TKey key, TValue value, TimeSpan timeToLive)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            return GetAndSetAsync(keyData, valueData, timeToLive, TimeSpanExtensions.MinusOneMillisecond);
        }

        /// <inheritdoc />
        public Task<TValue> PutAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan maxIdle)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            return GetAndSetAsync(keyData, valueData, timeToLive, maxIdle);
        }

        public async Task<TValue> PutAsync2(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));

            // FIXME this is where we need to introduce some async stuff
            //var keyData = SerializationService.ToData(key);
            //var valueData = SerializationService.ToData(value);

            // so, doing this means we've turned this method into an async state machine,
            // because of the potential awaits that were not there before, and then why
            // do it this way and not push the serialization to the GetAndSetAsync so at
            // least there is only 1 state machine running?

            var (keyData, keySchemaId) = SerializationService.ToData2(key);
            while (keySchemaId > 0) // may happen several times
            {
                await SerializationService.FetchSchema(keySchemaId).CfAwait();
                (keyData, keySchemaId) = SerializationService.ToData2(key);
            }

            var (valueData, valueSchemaId) = SerializationService.ToData2(key);
            while (valueSchemaId > 0) // may happen several times
            {
                await SerializationService.FetchSchema(keySchemaId).CfAwait();
                (valueData, valueSchemaId) = SerializationService.ToData2(key);
            }

            var timeToLiveMs = TimeSpanExtensions.MinusOneMillisecond.RoundedMilliseconds(false); // codec: 0 is infinite, -1 is server
            var maxIdleMs = TimeSpanExtensions.MinusOneMillisecond.RoundedMilliseconds(false); // codec: 0 is infinite, -1 is server
            var withMaxIdle = maxIdleMs != -1;

            var requestMessage = withMaxIdle
                ? MapPutWithMaxIdleCodec.EncodeRequest(Name, keyData, valueData, ContextId, timeToLiveMs, maxIdleMs)
                : MapPutCodec.EncodeRequest(Name, keyData, valueData, ContextId, timeToLiveMs);

            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CfAwait();

            var response = withMaxIdle
                ? MapPutWithMaxIdleCodec.DecodeResponse(responseMessage).Response
                : MapPutCodec.DecodeResponse(responseMessage).Response;

            var (result, resultSchemaId) = SerializationService.ToObject2<TValue>(response);
            while (resultSchemaId > 0) // may happen several times
            {
                await SerializationService.PublishSchema(resultSchemaId).CfAwait();
                (result, resultSchemaId) = SerializationService.ToObject2<TValue>(response);
            }

            return result;
        }

        protected virtual async Task<TValue> GetAndSetAsync(IData keyData, IData valueData, TimeSpan timeToLive, TimeSpan maxIdle)
        {
            var timeToLiveMs = timeToLive.RoundedMilliseconds(false); // codec: 0 is infinite, -1 is server
            var maxIdleMs = maxIdle.RoundedMilliseconds(false); // codec: 0 is infinite, -1 is server
            var withMaxIdle = maxIdleMs != -1;

            var requestMessage = withMaxIdle
                ? MapPutWithMaxIdleCodec.EncodeRequest(Name, keyData, valueData, ContextId, timeToLiveMs, maxIdleMs)
                : MapPutCodec.EncodeRequest(Name, keyData, valueData, ContextId, timeToLiveMs);

            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData).CfAwait();

            var response = withMaxIdle
                ? MapPutWithMaxIdleCodec.DecodeResponse(responseMessage).Response
                : MapPutCodec.DecodeResponse(responseMessage).Response;

            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public Task SetAllAsync(IDictionary<TKey, TValue> entries)
            => SetAllAsync(entries, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task SetAllAsync(IDictionary<TKey, TValue> entries, CancellationToken cancellationToken)
        {
            // TODO: is this transactional? can some entries be created and others be missing?

            var ownerEntries = new Dictionary<Guid, Dictionary<int, List<KeyValuePair<IData, IData>>>>();

            // verify entries + group by owner and partitions
            foreach (var (key, value) in entries)
            {
                var (keyData, valueData) = ToSafeData(key, value);

                var partitionId = Cluster.Partitioner.GetPartitionId(keyData.PartitionHash);
                var ownerId = Cluster.Partitioner.GetPartitionOwner(partitionId);
                if (!ownerEntries.TryGetValue(ownerId, out var part))
                    part = ownerEntries[ownerId] = new Dictionary<int, List<KeyValuePair<IData, IData>>>();
                if (!part.TryGetValue(partitionId, out var list))
                    list = part[partitionId] = new List<KeyValuePair<IData, IData>>();
                list.Add(new KeyValuePair<IData, IData>(keyData, valueData));
            }

            var task = SetAsync(ownerEntries, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CfAwait();
#endif
        }

        /// <summary>
        /// Adds or replaces entries.
        /// </summary>
        /// <param name="ownerEntries">Entries.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Nothing.</returns>
        protected virtual
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task SetAsync(Dictionary<Guid, Dictionary<int, List<KeyValuePair<IData, IData>>>> ownerEntries, CancellationToken cancellationToken)
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

                    var requestMessage = MapPutAllCodec.EncodeRequest(Name, list, false);
                    requestMessage.PartitionId = partitionId;
                    var ownerTask = Cluster.Messaging.SendToMemberAsync(requestMessage, ownerId, cancellationToken);
                    tasks.Add(ownerTask);
                }
            }

            // and wait on all tasks, ignoring the responses
            var task = Task.WhenAll(tasks);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CfAwait();
#endif
        }

        /// <inheritdoc />
        public Task<TValue> ReplaceAsync(TKey key, TValue newValue)
            => TryUpdateAsync(key, newValue, CancellationToken.None);

        private async Task<TValue> TryUpdateAsync(TKey key, TValue newValue, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, newValue);

            var requestMessage = MapReplaceCodec.EncodeRequest(Name, keyData, valueData, ContextId);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CfAwait();
            var response = MapReplaceCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public Task<bool> ReplaceAsync(TKey key, TValue comparisonValue, TValue newValue)
            => TryUpdateAsync(key, comparisonValue, newValue, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<bool> TryUpdateAsync(TKey key, TValue expectedValue, TValue newValue, CancellationToken cancellationToken)
        {
            var (keyData, expectedData, newData) = ToSafeData(key, expectedValue, newValue);
            var task = TryUpdateAsync(keyData, expectedData, newData, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <summary>
        /// Replaces an existing entry.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="expectedData">The expected value.</param>
        /// <param name="newData">The new value.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>true if the entry was replaced; otherwise false.</returns>
        protected async Task<bool> TryUpdateAsync(IData keyData, IData expectedData, IData newData, CancellationToken cancellationToken)
        {
            var requestMessage = MapReplaceIfSameCodec.EncodeRequest(Name, keyData, expectedData, newData, ContextId);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CfAwait();
            var response = MapReplaceIfSameCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public Task<bool> TryPutAsync(TKey key, TValue value, TimeSpan timeToWait)
            => TrySetAsync(key, value, timeToWait, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<bool> TrySetAsync(TKey key, TValue value, TimeSpan serverTimeout, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var task = TrySetAsync(keyData, valueData, serverTimeout, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <summary>
        /// Tries to set an entry within a timeout.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="valueData">A value.</param>
        /// <param name="serverTimeout">A timeout.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>true if the entry was set; otherwise false.</returns>
        /// <remarks>
        /// <para>This method returns false when no lock on the key could be
        /// acquired within the timeout.</para>
        /// </remarks>
        protected virtual async Task<bool> TrySetAsync(IData keyData, IData valueData, TimeSpan serverTimeout, CancellationToken cancellationToken)
        {
            var timeoutMs = serverTimeout.RoundedMilliseconds(false); // codec: 0 = server, -1 = infinite

            var requestMessage = MapTryPutCodec.EncodeRequest(Name, keyData, valueData, ContextId, timeoutMs);
            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CfAwait();
            var response = MapTryPutCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public Task<TValue> PutIfAbsentAsync(TKey key, TValue value)
            => GetOrAddAsync(key, value, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<TValue> GetOrAddAsync(TKey key, TValue value, CancellationToken cancellationToken)
        {
            var task = GetOrAddAsync(key, value, TimeSpan.Zero, TimeSpanExtensions.MinusOneMillisecond, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <inheritdoc />
        public Task<TValue> PutIfAbsentAsync(TKey key, TValue value, TimeSpan timeToLive)
            => GetOrAddAsync(key, value, timeToLive, TimeSpanExtensions.MinusOneMillisecond, CancellationToken.None);

        /// <inheritdoc />
        public Task<TValue> PutIfAbsentAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan maxIdle)
            => GetOrAddAsync(key, value, timeToLive, maxIdle, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task<TValue> GetOrAddAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan maxIdle, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var task = GetOrAdd(keyData, valueData, timeToLive, maxIdle, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CfAwait();
#endif
        }

        /// <summary>
        /// Adds an entry with a time-to-live, if no entry with the key exists.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="valueData">The value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The existing value, if any; otherwise the default value.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is -1ms, the entry lives forever.</para>
        /// </remarks>
        protected virtual async Task<TValue> GetOrAdd(IData keyData, IData valueData, TimeSpan timeToLive, TimeSpan maxIdle, CancellationToken cancellationToken)
        {
            var timeToLiveMs = timeToLive.RoundedMilliseconds(false); // codec: 0 is infinite, -1 is server
            var maxIdleMs = maxIdle.RoundedMilliseconds(false); // codec: 0 is infinite, -1 is server
            var withMaxIdle = maxIdleMs != -1;

            var requestMessage = withMaxIdle
                ? MapPutIfAbsentWithMaxIdleCodec.EncodeRequest(Name, keyData, valueData, ContextId, timeToLiveMs, maxIdleMs)
                : MapPutIfAbsentCodec.EncodeRequest(Name, keyData, valueData, ContextId, timeToLiveMs);

            var responseMessage = await Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CfAwait();

            var response = withMaxIdle
                ? MapPutIfAbsentWithMaxIdleCodec.DecodeResponse(responseMessage).Response
                : MapPutIfAbsentCodec.DecodeResponse(responseMessage).Response;

            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public Task PutTransientAsync(TKey key, TValue value, TimeSpan timeToLive)
            => SetTransientAsync(key, value, timeToLive, TimeSpanExtensions.MinusOneMillisecond, CancellationToken.None);

        /// <inheritdoc />
        public Task PutTransientAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan maxIdle)
            => SetTransientAsync(key, value, timeToLive, maxIdle, CancellationToken.None);

        private
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task SetTransientAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan maxIdle, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var task = SetTransientAsync(keyData, valueData, timeToLive, maxIdle, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CfAwait();
#endif
        }

        /// <summary>
        /// Adds a transient entry.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="valueData">The value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        protected virtual
#if !HZ_OPTIMIZE_ASYNC
        async
#endif
        Task SetTransientAsync(IData keyData, IData valueData, TimeSpan timeToLive, TimeSpan maxIdle, CancellationToken cancellationToken = default)
        {
            var timeToLiveMs = timeToLive.RoundedMilliseconds(false); // codec: 0 is infinite, -1 is server
            var maxIdleMs = maxIdle.RoundedMilliseconds(false); // codec: 0 is infinite, -1 is server
            var withMaxIdle = maxIdleMs != -1;

            var requestMessage = withMaxIdle
                ? MapPutTransientWithMaxIdleCodec.EncodeRequest(Name, keyData, valueData, ContextId, timeToLiveMs, maxIdleMs)
                : MapPutTransientCodec.EncodeRequest(Name, keyData, valueData, ContextId, timeToLiveMs);
            var task = Cluster.Messaging.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CfAwait();
#endif
        }

        public Task<bool> UpdateTimeToLive(TKey key, TimeSpan timeToLive)
        {
            throw new NotImplementedException();
        }
    }
}
