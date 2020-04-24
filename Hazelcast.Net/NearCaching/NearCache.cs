using System;
using System.Collections.Generic;
using Hazelcast.Clustering;
using Hazelcast.Configuration;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.NearCaching
{
    internal class NearCache : NearCacheBase
    {
        private RepairingHandler _repairingHandler;

        public NearCache(string name, Cluster cluster, ISerializationService serializationService, NearCacheConfig nearCacheConfig)
            : base(name, cluster, serializationService, nearCacheConfig)
        { }

        public RepairingHandler RepairingHandler => _repairingHandler;

        public override void Init()
        {
            if (InvalidateOnChange)
            {
                _repairingHandler = new RepairingHandler(_cluster.ClientId, this, _cluster.Partitioner);
                RegisterInvalidateListener();
            }
        }

        protected override NearCacheRecord CreateRecord(IData key, object value)
        {
            var record = base.CreateRecord(key, value);
            InitInvalidationMetadata(record);
            return record;
        }

        protected override bool IsStaleRead(IData key, NearCacheRecord record)
        {
            if (_repairingHandler == null)
            {
                return false;
            }
            var latestMetaData = _repairingHandler.GetMetaDataContainer(record.PartitionId);
            return record.Guid != latestMetaData.Guid || record.Sequence < latestMetaData.StaleSequence;
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

        private void InitInvalidationMetadata(NearCacheRecord newRecord)
        {
            if (_repairingHandler == null)
            {
                return;
            }
            var partitionId = _cluster.Partitioner.GetPartitionId(newRecord.Key);
            var metadataContainer = _repairingHandler.GetMetaDataContainer(partitionId);
            newRecord.PartitionId = partitionId;
            newRecord.Sequence = metadataContainer.Sequence;
            newRecord.Guid = metadataContainer.Guid;
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

                // FIXME the NAME could be a property of a BASE state object (class EventState)
                var subscription = new ClusterEventSubscription(
                    MapAddNearCacheInvalidationListenerCodec.EncodeRequest(Name, (int) EntryEventType.Invalidation, false),
                    (message, state) => MapAddNearCacheInvalidationListenerCodec.DecodeResponse(message).Response,
                    (id, state) => MapRemoveEntryListenerCodec.EncodeRequest(Name, id),
                    (message, state) => MapAddNearCacheInvalidationListenerCodec.EventHandler.HandleEvent(message, HandleIMapInvalidationEvent, HandleIMapBatchInvalidationEvent));
                _cluster.SubscribeAsync(subscription).Wait(); // FIXME ASYNC!
                RegistrationId = subscription.Id;
            }
            catch (Exception e)
            {
                Logger.LogCritical(e, "-----------------\n Near Cache is not initialized!!! \n-----------------");
            }
        }
    }
}
