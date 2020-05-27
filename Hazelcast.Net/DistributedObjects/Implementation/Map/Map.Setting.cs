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
    internal partial class Map<TKey, TValue> // Setting
    {
        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task<TValue> AddOrReplaceWithValueAsync(TKey key, TValue value, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = AddOrReplaceWithValueAsync(key, value, cancellation.Token).OrTimeout(cancellation);

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
        Task<TValue> AddOrReplaceWithValueAsync(TKey key, TValue value, CancellationToken cancellationToken)
        {
            var task = AddOrReplaceWithValueTtlAsync(key, value, TimeToLive.InfiniteTimeSpan, cancellationToken);

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
        Task AddOrReplaceAsync(TKey key, TValue value, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = AddOrReplaceAsync(key, value, cancellation.Token).OrTimeout(cancellation);

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
        Task AddOrReplaceAsync(TKey key, TValue value, CancellationToken cancellationToken)
        {
            var task = AddOrReplaceTtlAsync(key, value, TimeToLive.InfiniteTimeSpan, cancellationToken);

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
        Task<TValue> AddOrReplaceWithValueTtlAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = AddOrReplaceWithValueTtlAsync(key, value, timeToLive, cancellation.Token).OrTimeout(cancellation);

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
        Task<TValue> AddOrReplaceWithValueTtlAsync(TKey key, TValue value, TimeSpan timeToLive, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var task = AddOrReplaceWithValueAsync(keyData, valueData, timeToLive, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <summary>
        /// Adds or replaces an entry with a time-to-live and returns the previous value.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="valueData">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The value previously associated with the key in the map, if any; otherwise default(<typeparamref name="TValue"/>).</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// </remarks>
        protected virtual async Task<TValue> AddOrReplaceWithValueAsync(IData keyData, IData valueData, TimeSpan timeToLive, CancellationToken cancellationToken)
        {
            var timeToLiveMs = timeToLive.CodecMilliseconds(-1000);
            var requestMessage = MapPutCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeToLiveMs);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CAF();
            var response = MapPutCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task AddOrReplaceTtlAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = AddOrReplaceTtlAsync(key, value, timeToLive, cancellation.Token).OrTimeout(cancellation);

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
        Task AddOrReplaceTtlAsync(TKey key, TValue value, TimeSpan timeToLive, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var task = AddOrReplaceTtlAsync(keyData, valueData, timeToLive, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <summary>
        /// Adds or replaces an entry with a time-to-live.
        /// </summary>
        /// <param name="keyData">A key.</param>
        /// <param name="valueData">A value.</param>
        /// <param name="timeToLive">A time to live.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Nothing.</returns>
        /// <remarks>
        /// <para>The value is automatically expired, evicted and removed after the <paramref name="timeToLive"/> has elapsed..</para>
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// </remarks>
        protected virtual
#if !OPTIMIZE_ASYNC
            async
#endif
        Task AddOrReplaceTtlAsync(IData keyData, IData valueData, TimeSpan timeToLive, CancellationToken cancellationToken)
        {
            var timeToLiveMs = timeToLive.CodecMilliseconds(-1000);

            var requestMessage = MapSetCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeToLiveMs);
            var task = Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken);

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
            Task AddOrReplaceAsync(IDictionary<TKey, TValue> entries, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = AddOrReplaceAsync(entries, cancellation.Token).OrTimeout(cancellation);

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
        Task AddOrReplaceAsync(IDictionary<TKey, TValue> entries, CancellationToken cancellationToken)
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

            var task = AddOrReplaceAsync(ownerEntries, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        /// <summary>
        /// Adds or replaces entries.
        /// </summary>
        /// <param name="ownerEntries">Entries.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>Nothing.</returns>
        protected virtual
#if !OPTIMIZE_ASYNC
            async
#endif
        Task AddOrReplaceAsync(Dictionary<Guid, Dictionary<int, List<KeyValuePair<IData, IData>>>> ownerEntries, CancellationToken cancellationToken)
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
                    var ownerTask = Cluster.SendToMemberAsync(requestMessage, ownerId, cancellationToken);
                    tasks.Add(ownerTask);
                }
            }

            // and wait on all tasks, ignoring the responses
            var task = Task.WhenAll(tasks);

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
        Task<TValue> ReplaceAsync(TKey key, TValue newValue, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = ReplaceAsync(key, newValue, cancellation.Token).OrTimeout(cancellation);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<TValue> ReplaceAsync(TKey key, TValue newValue, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, newValue);

            var requestMessage = MapReplaceCodec.EncodeRequest(Name, keyData, valueData, ThreadId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken).CAF();
            var response = MapReplaceCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task<bool> ReplaceAsync(TKey key, TValue expectedValue, TValue newValue, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = ReplaceAsync(key, expectedValue, newValue, cancellation.Token).OrTimeout(cancellation);

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
        Task<bool> ReplaceAsync(TKey key, TValue expectedValue, TValue newValue, CancellationToken cancellationToken)
        {
            var (keyData, expectedData, newData) = ToSafeData(key, expectedValue, newValue);
            var task = ReplaceAsync(keyData, expectedData, newData, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
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
        protected async Task<bool> ReplaceAsync(IData keyData, IData expectedData, IData newData, CancellationToken cancellationToken)
        {
            var requestMessage = MapReplaceIfSameCodec.EncodeRequest(Name, keyData, expectedData, newData, ThreadId);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken);
            var response = MapReplaceIfSameCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
            Task<bool> TryAddOrReplaceAsync(TKey key, TValue value, TimeSpan serverTimeout, TimeSpan timeout)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = TryAddOrReplaceAsync(key, value, serverTimeout, cancellation.Token).OrTimeout(cancellation);

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
        Task<bool> TryAddOrReplaceAsync(TKey key, TValue value, TimeSpan serverTimeout, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var task = TryAddOrReplaceAsync(keyData, valueData, serverTimeout, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
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
        protected virtual async Task<bool> TryAddOrReplaceAsync(IData keyData, IData valueData, TimeSpan serverTimeout, CancellationToken cancellationToken)
        {
            var timeoutMs = serverTimeout.CodecMilliseconds(0);

            var requestMessage = MapTryPutCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeoutMs);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken);
            var response = MapTryPutCodec.DecodeResponse(responseMessage).Response;
            return response;
        }

        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task<TValue> AddIfMissingAsync(TKey key, TValue value, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = AddIfMissingAsync(key, value, cancellation.Token).OrTimeout(cancellation);

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
        Task<TValue> AddIfMissingAsync(TKey key, TValue value, CancellationToken cancellationToken)
        {
            var task = AddIfMissingTtlAsync(key, value, TimeToLive.InfiniteTimeSpan, cancellationToken);

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
        Task<TValue> AddIfMissingTtlAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan timeout)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = AddIfMissingTtlAsync(key, value, timeToLive, cancellation.Token).OrTimeout(cancellation);

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
        Task<TValue> AddIfMissingTtlAsync(TKey key, TValue value, TimeSpan timeToLive, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var task = AddIfMissingAsync(keyData, valueData, timeToLive, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
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
        /// <para>If the <paramref name="timeToLive"/> is <see cref="Timeout.InfiniteTimeSpan"/>, the entry lives forever.</para>
        /// </remarks>
        protected virtual async Task<TValue> AddIfMissingAsync(IData keyData, IData valueData, TimeSpan timeToLive, CancellationToken cancellationToken)
        {
            var timeToLiveMs = timeToLive.CodecMilliseconds(-1000);

            var requestMessage = MapPutIfAbsentCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeToLiveMs);
            var responseMessage = await Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken);
            var response = MapPutIfAbsentCodec.DecodeResponse(responseMessage).Response;
            return ToObject<TValue>(response);
        }

        /// <inheritdoc />
        public
#if !OPTIMIZE_ASYNC
            async
#endif
        Task AddOrReplaceTransientAsync(TKey key, TValue value, TimeSpan timeToLive, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(Constants.DistributedObjects.DefaultOperationTimeoutMilliseconds);
            var task = AddOrReplaceTransientAsync(key, value, timeToLive, cancellation.Token).OrTimeout(cancellation);

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
        Task AddOrReplaceTransientAsync(TKey key, TValue value, TimeSpan timeToLive, CancellationToken cancellationToken)
        {
            var (keyData, valueData) = ToSafeData(key, value);
            var task = AddOrReplaceTransientAsync(keyData, valueData, timeToLive, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
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
#if !OPTIMIZE_ASYNC
            async
#endif
        Task AddOrReplaceTransientAsync(IData keyData, IData valueData, TimeSpan timeToLive, CancellationToken cancellationToken)
        {
            var timeToLiveMs = timeToLive.CodecMilliseconds(-1000);

            var requestMessage = MapPutTransientCodec.EncodeRequest(Name, keyData, valueData, ThreadId, timeToLiveMs);
            var task = Cluster.SendToKeyPartitionOwnerAsync(requestMessage, keyData, cancellationToken);

#if OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }
    }
}