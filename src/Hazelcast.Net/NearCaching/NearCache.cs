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
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Data;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.NearCaching
{
    /// <summary>
    /// Represents a Near Cache.
    /// </summary>
    internal class NearCache : NearCacheBase
    {
        private readonly int _maxToleratedMissCount;
        private Guid _subscriptionId;

        /// <summary>
        /// Initializes a new instance of the <see cref="NearCache"/> class.
        /// </summary>
        /// <param name="name">The name of the cache.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="serializationService">The localization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        /// <param name="nearCacheNamedOptions">NearCache options.</param>
        /// <param name="maxToleratedMissCount"></param>
        public NearCache(string name, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory, NearCacheNamedOptions nearCacheNamedOptions, int maxToleratedMissCount)
            : base(name, cluster, serializationService, loggerFactory, nearCacheNamedOptions)
        {
            _maxToleratedMissCount = maxToleratedMissCount;
        }

        /// <summary>
        /// Gets or sets the repairing handler.
        /// </summary>
        /// <remarks>
        /// <para>The repairing handler is used by the <see cref="NearCacheManager"/>.</para>
        /// </remarks>
        public RepairingHandler RepairingHandler { get; private set; }

        /// <inheritdoc />
        public override async ValueTask InitializeAsync()
        {
            if (Options.InvalidateOnChange)
            {
                try
                {
                    _subscriptionId = await SubscribeToInvalidationEventsAsync().CAF();
                    RepairingHandler = new RepairingHandler(Cluster.ClientId, this, _maxToleratedMissCount, Cluster.Partitioner, SerializationService, LoggerFactory);
                    IsInvalidating = true;
                }
                catch (Exception e)
                {
                    LoggerFactory.CreateLogger<NearCache>().LogCritical(e, "-----------------\n Near Cache is not initialized!!! \n-----------------");
                    IsInvalidating = false;
                }
            }
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeAsyncCore()
        {
            if (_subscriptionId != default)
                await Cluster.Events.RemoveSubscriptionAsync(_subscriptionId, CancellationToken.None).CAF();
        }

        /// <inheritdoc />
        protected override NearCacheEntry CreateCacheEntry(IData keyData, object value)
        {
            var entry = base.CreateCacheEntry(keyData, value);
            if (entry == null) return null;

            // do not manage invalidation, just return the entry
            if (!IsInvalidating) return entry;

            // otherwise, populate the entry with repairing meta data
            var partitionHash = entry.KeyData.PartitionHash;
            var partitionId = Cluster.Partitioner.GetPartitionId(partitionHash);
            var metadata = RepairingHandler.GetMetadata(partitionId);
            entry.PartitionId = partitionId;
            entry.Sequence = metadata.Sequence;
            entry.Guid = metadata.Guid;

            return entry;
        }

        /// <inheritdoc />
        protected override bool IsStaleRead(NearCacheEntry entry)
        {
            // do not manage invalidation = cannot be stale
            if (!IsInvalidating) return false;

            // otherwise, check meta data
            var metadata = RepairingHandler.GetMetadata(entry.PartitionId);

            // FIXME how can this report 'false' but still we count stale reads?
            var stale = entry.Guid != metadata.Guid || entry.Sequence < metadata.StaleSequence;
            Console.WriteLine($"IsStale: {stale}, {(entry.Guid==metadata.Guid?"same":"different")} guid, {entry.Sequence} ? {metadata.StaleSequence}, value={entry.ValueObject}");

            return entry.Guid != metadata.Guid || entry.Sequence < metadata.StaleSequence;
        }

        /// <summary>
        /// Whether the cache is invalidating.
        /// </summary>
        public bool IsInvalidating { get; private set; }

        /// <summary>
        /// Subscribes to invalidation events.
        /// </summary>
        private async ValueTask<Guid> SubscribeToInvalidationEventsAsync()
        {
            var subscription = new ClusterSubscription(
                MapAddNearCacheInvalidationListenerCodec.EncodeRequest(Name, (int) HDictionaryEventTypes.Invalidated, false),
                (message, state) => MapAddNearCacheInvalidationListenerCodec.DecodeResponse(message).Response,
                (id, state) => MapRemoveEntryListenerCodec.EncodeRequest(((EventState) state).Name, id),
                (message, state) => MapRemoveEntryListenerCodec.DecodeResponse(message).Response,
                (message, state) => MapAddNearCacheInvalidationListenerCodec.HandleEventAsync(message, HandleInvalidationEventAsync, HandleBatchInvalidationEventAsync, LoggerFactory),
                new EventState { Name = Name });

            await Cluster.Events.InstallSubscriptionAsync(subscription, CancellationToken.None).CAF();
            return subscription.Id;
        }

        /// <summary>
        /// Represents the invalidation events state.
        /// </summary>
        private class EventState
        {
            public string Name { get; set; }
        }

        /// <summary>
        /// Handle batch invalidation events.
        /// </summary>
        /// <param name="keys">The invalidated keys.</param>
        /// <param name="sourceuuids"></param>
        /// <param name="partitionuuids"></param>
        /// <param name="sequences"></param>
        private ValueTask HandleBatchInvalidationEventAsync(IEnumerable<IData> keys, IEnumerable<Guid> sourceuuids, IEnumerable<Guid> partitionuuids, IEnumerable<long> sequences)
        {
            RepairingHandler.Handle(keys, sourceuuids, partitionuuids, sequences);
            return default;
        }

        /// <summary>
        /// Handle invalidation events.
        /// </summary>
        /// <param name="key">The invalidated key.</param>
        /// <param name="sourceUuid"></param>
        /// <param name="partitionUuid"></param>
        /// <param name="sequence"></param>
        private ValueTask HandleInvalidationEventAsync(IData key, Guid sourceUuid, Guid partitionUuid, long sequence)
        {
            RepairingHandler.Handle(key, sourceUuid, partitionUuid, sequence);
            return default;
        }
    }
}
