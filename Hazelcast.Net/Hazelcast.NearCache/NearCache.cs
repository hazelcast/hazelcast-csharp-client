// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Client;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.NearCache
{
    internal class NearCache : BaseNearCache
    {
        private RepairingHandler _repairingHandler;

        public NearCache(string name, HazelcastClient client, NearCacheConfig nearCacheConfig) : base(name, client,
            nearCacheConfig)
        {
        }

        public RepairingHandler RepairingHandler
        {
            get { return _repairingHandler; }
        }

        public override void Init()
        {
            if (InvalidateOnChange)
            {
                var localUuid = Client.GetClientClusterService().GetLocalClient().GetUuid();
                _repairingHandler = new RepairingHandler(localUuid, this, Client.GetClientPartitionService());
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

        private void HandleIMapBatchInvalidationEvent_v1_0(IList<IData> keys)
        {
            Logger.Severe("Unexpected event from server");
        }

        private void HandleIMapBatchInvalidationEvent_v1_4(IList<IData> keys, IList<string> sourceuuids,
            IList<Guid> partitionuuids, IList<long> sequences)
        {
            _repairingHandler.Handle(keys, sourceuuids, partitionuuids, sequences);
        }

        private void HandleIMapInvalidationEvent_v1_0(IData key)
        {
            Logger.Severe("Unexpected event from server");
        }

        private void HandleIMapInvalidationEvent_v1_4(IData key, string sourceUuid, Guid partitionUuid, long sequence)
        {
            _repairingHandler.Handle(key, sourceUuid, partitionUuid, sequence);
        }

        private void InitInvalidationMetadata(NearCacheRecord newRecord)
        {
            if (_repairingHandler == null)
            {
                return;
            }
            var partitionId = Client.GetClientPartitionService().GetPartitionId(newRecord.Key);
            var metadataContainer = _repairingHandler.GetMetaDataContainer(partitionId);
            newRecord.PartitionId = partitionId;
            newRecord.Sequence = metadataContainer.Sequence;
            newRecord.Guid = metadataContainer.Guid;
        }


        private void RegisterInvalidateListener()
        {
            try
            {
                var request =
                    MapAddNearCacheInvalidationListenerCodec.EncodeRequest(Name, (int)EntryEventType.Invalidation,
                        false);
                DistributedEventHandler handler = message =>
                    MapAddNearCacheInvalidationListenerCodec.EventHandler.HandleEvent(message,
                        HandleIMapInvalidationEvent_v1_0, HandleIMapInvalidationEvent_v1_4,
                        HandleIMapBatchInvalidationEvent_v1_0, HandleIMapBatchInvalidationEvent_v1_4);

                RegistrationId = Client.GetListenerService().RegisterListener(request,
                    message => MapAddNearCacheInvalidationListenerCodec.DecodeResponse(message).response,
                    id => MapRemoveEntryListenerCodec.EncodeRequest(Name, id), handler);
            }
            catch (Exception e)
            {
                Logger.Severe("-----------------\n Near Cache is not initialized!!! \n-----------------", e);
            }
        }
    }
}