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
using Hazelcast.Clustering;
using Hazelcast.Data;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.NearCaching
{
    internal class NearCache : NearCacheBase
    {
        private readonly int _maxToleratedMissCount;

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

        public override void Init()
        {
            if (InvalidateOnChange)
            {
                RepairingHandler = new RepairingHandler(Cluster.ClientId, this, _maxToleratedMissCount, Cluster.Partitioner, SerializationService, LoggerFactory);
                RegisterInvalidateListener();
            }
        }

        protected override NearCacheEntry CreateEntry(IData key, object value)
        {
            var entry = base.CreateEntry(key, value);
            InitInvalidationMetadata(entry);
            return entry;
        }

        protected override bool IsStaleRead(IData key, NearCacheEntry entry)
        {
            if (RepairingHandler == null)
            {
                return false;
            }
            var latestMetaData = RepairingHandler.GetMetaDataContainer(entry.PartitionId);
            return entry.Guid != latestMetaData.Guid || entry.Sequence < latestMetaData.StaleSequence;
        }

        private void HandleIMapBatchInvalidationEvent(IEnumerable<IData> keys, IEnumerable<Guid> sourceuuids,
            IEnumerable<Guid> partitionuuids, IEnumerable<long> sequences)
        {
            RepairingHandler.Handle(keys, sourceuuids, partitionuuids, sequences);
        }

        private void HandleIMapInvalidationEvent(IData key, Guid sourceUuid, Guid partitionUuid, long sequence)
        {
            RepairingHandler.Handle(key, sourceUuid, partitionUuid, sequence);
        }

        private void InitInvalidationMetadata(NearCacheEntry newEntry)
        {
            if (RepairingHandler == null)
            {
                return;
            }
            var partitionId = Cluster.Partitioner.GetPartitionId(newEntry.Key);
            var metadataContainer = RepairingHandler.GetMetaDataContainer(partitionId);
            newEntry.PartitionId = partitionId;
            newEntry.Sequence = metadataContainer.Sequence;
            newEntry.Guid = metadataContainer.Guid;
        }


        private void RegisterInvalidateListener()
        {
            try
            {
                /*
                var request =
                    MapAddNearCacheInvalidationListenerCodec.EncodeRequest(Name, (int) EntryEventType.Invalidation, false);

                void Handle(ClientMessage message)
                    => MapAddNearCacheInvalidationListenerCodec.EventHandler.HandleEvent(message, HandleIMapInvalidationEvent, HandleIMapBatchInvalidationEvent);

                RegistrationId = Client.ListenerService.RegisterListener(request,
                    message => MapAddNearCacheInvalidationListenerCodec.DecodeResponse(message).Response,
                    id => MapRemoveEntryListenerCodec.EncodeRequest(Name, id), (Action<ClientMessage>) Handle);
                */

                var subscription = new ClusterSubscription(
                    MapAddNearCacheInvalidationListenerCodec.EncodeRequest(Name, (int) MapEventTypes.Invalidated, false),
                    (message, state) => MapAddNearCacheInvalidationListenerCodec.DecodeResponse(message).Response,
                    (id, state) => MapRemoveEntryListenerCodec.EncodeRequest(((EventState) state).Name, id),
                    (message, state) => MapRemoveEntryListenerCodec.DecodeResponse(message).Response,
                    (message, state) => MapAddNearCacheInvalidationListenerCodec.HandleEvent(message, HandleIMapInvalidationEvent, HandleIMapBatchInvalidationEvent, LoggerFactory),
                    new EventState { Name = Name });

                Cluster.InstallSubscriptionAsync(subscription, CancellationToken.None).Wait(); // FIXME: async oops!
                RegistrationId = subscription.Id;
            }
            catch (Exception e)
            {
                LoggerFactory.CreateLogger<NearCache>().LogCritical(e, "-----------------\n Near Cache is not initialized!!! \n-----------------");
            }
        }

        private class EventState
        {
            public string Name { get; set; }
        }
    }
}
