﻿// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
    internal class NearCachePre38 : BaseNearCache
    {
        public NearCachePre38(string name, HazelcastClient client, NearCacheConfig nearCacheConfig) : base(name, client,
            nearCacheConfig)
        {
        }

        public override void Init()
        {
            if (InvalidateOnChange)
            {
                RegisterInvalidateListener();
            }
        }

        private void HandleIMapBatchInvalidationEvent_v1_0(IList<IData> keys)
        {
            foreach (var data in keys)
            {
                Invalidate(data);
            }
        }

        private void HandleIMapBatchInvalidationEvent_v1_4(IList<IData> keys, IList<string> sourceuuids,
            IList<Guid> partitionuuids, IList<long> sequences)
        {
            HandleIMapBatchInvalidationEvent_v1_0(keys);
        }

        private void HandleIMapInvalidationEvent_v1_0(IData key)
        {
            if (key == null)
            {
                InvalidateAll();
            }
            else
            {
                Invalidate(key);
            }
        }

        private void HandleIMapInvalidationEvent_v1_4(IData key, string sourceUuid, Guid partitionUuid, long sequence)
        {
            HandleIMapInvalidationEvent_v1_0(key);
        }

        private void RegisterInvalidateListener()
        {
            try
            {
                var request =
                    MapAddNearCacheEntryListenerCodec.EncodeRequest(Name, (int) EntryEventType.Invalidation, false);
                DistributedEventHandler handler = message =>
                    MapAddNearCacheEntryListenerCodec.EventHandler.HandleEvent(message,
                        HandleIMapInvalidationEvent_v1_0, HandleIMapInvalidationEvent_v1_4,
                        HandleIMapBatchInvalidationEvent_v1_0, HandleIMapBatchInvalidationEvent_v1_4);

                RegistrationId = Client.GetListenerService().RegisterListener(request,
                    message => MapAddNearCacheEntryListenerCodec.DecodeResponse(message).response,
                    id => MapRemoveEntryListenerCodec.EncodeRequest(Name, id), handler);
            }
            catch (Exception e)
            {
                Logger.Severe("-----------------\n Near Cache is not initialized!!! \n-----------------", e);
            }
        }
    }
}