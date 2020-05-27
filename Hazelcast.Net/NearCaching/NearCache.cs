using System;
using System.Collections.Generic;
using System.Threading;
using Hazelcast.Clustering;
using Hazelcast.Configuration;
using Hazelcast.Data.Map;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.NearCaching
{
    internal class NearCache : NearCacheBase
    {
        private RepairingHandler _repairingHandler;

        public NearCache(string name, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory, NearCacheConfiguration nearCacheConfiguration)
            : base(name, cluster, serializationService, loggerFactory, nearCacheConfiguration)
        { }

        // FIXME: why is this public?
        public RepairingHandler RepairingHandler => _repairingHandler;

        public override void Init()
        {
            if (InvalidateOnChange)
            {
                _repairingHandler = new RepairingHandler(Cluster.ClientId, this, Cluster.Partitioner, LoggerFactory);
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
            if (_repairingHandler == null)
            {
                return false;
            }
            var latestMetaData = _repairingHandler.GetMetaDataContainer(entry.PartitionId);
            return entry.Guid != latestMetaData.Guid || entry.Sequence < latestMetaData.StaleSequence;
        }

        private void HandleIMapBatchInvalidationEvent(IEnumerable<IData> keys, IEnumerable<Guid> sourceuuids,
            IEnumerable<Guid> partitionuuids, IEnumerable<long> sequences)
        {
            _repairingHandler.Handle(keys, sourceuuids, partitionuuids, sequences);
        }

        private void HandleIMapInvalidationEvent(IData key, Guid sourceUuid, Guid partitionUuid, long sequence)
        {
            _repairingHandler.Handle(key, sourceUuid, partitionUuid, sequence);
        }

        private void InitInvalidationMetadata(NearCacheEntry newEntry)
        {
            if (_repairingHandler == null)
            {
                return;
            }
            var partitionId = Cluster.Partitioner.GetPartitionId(newEntry.Key);
            var metadataContainer = _repairingHandler.GetMetaDataContainer(partitionId);
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
                    MapAddNearCacheInvalidationListenerCodec.EncodeRequest(Name, (int) MapEventType.Invalidated, false),
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
