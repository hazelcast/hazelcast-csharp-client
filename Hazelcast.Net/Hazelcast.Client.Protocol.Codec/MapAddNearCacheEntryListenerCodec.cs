// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;

// Client Protocol version, Since:1.0 - Update:1.4
namespace Hazelcast.Client.Protocol.Codec
{
    internal static class MapAddNearCacheEntryListenerCodec
    {
        private static int CalculateRequestDataSize(string name, int listenerFlags, bool localOnly)
        {
            var dataSize = ClientMessage.HeaderSize;
            dataSize += ParameterUtil.CalculateDataSize(name);
            dataSize += Bits.IntSizeInBytes;
            dataSize += Bits.BooleanSizeInBytes;
            return dataSize;
        }

        internal static ClientMessage EncodeRequest(string name, int listenerFlags, bool localOnly)
        {
            var requiredDataSize = CalculateRequestDataSize(name, listenerFlags, localOnly);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) MapMessageType.MapAddNearCacheEntryListener);
            clientMessage.SetRetryable(false);
            clientMessage.Set(name);
            clientMessage.Set(listenerFlags);
            clientMessage.Set(localOnly);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        internal class ResponseParameters
        {
            public string response;
        }

        internal static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var response = clientMessage.GetStringUtf8();
            parameters.response = response;
            return parameters;
        }

        internal class EventHandler
        {
            internal static void HandleEvent(IClientMessage clientMessage,
                HandleIMapInvalidationEventV10 handleIMapInvalidationEventV10,
                HandleIMapInvalidationEventV14 handleIMapInvalidationEventV14,
                HandleIMapBatchInvalidationEventV10 handleIMapBatchInvalidationEventV10,
                HandleIMapBatchInvalidationEventV14 handleIMapBatchInvalidationEventV14)
            {
                var messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventIMapInvalidation)
                {
                    IData key = null;
                    var keyIsNull = clientMessage.GetBoolean();
                    if (!keyIsNull)
                    {
                        key = clientMessage.GetData();
                    }
                    if (clientMessage.IsComplete())
                    {
                        handleIMapInvalidationEventV10(key);
                        return;
                    }
                    var sourceUuid = clientMessage.GetStringUtf8();
                    var partitionUuid = GuidCodec.Decode(clientMessage);
                    var sequence = clientMessage.GetLong();
                    handleIMapInvalidationEventV14(key, sourceUuid, partitionUuid, sequence);
                    return;
                }
                if (messageType == EventMessageConst.EventIMapBatchInvalidation)
                {
                    var keysSize = clientMessage.GetInt();
                    var keys = new List<IData>(keysSize);
                    for (var keysIndex = 0; keysIndex < keysSize; keysIndex++)
                    {
                        var keysItem = clientMessage.GetData();
                        keys.Add(keysItem);
                    }
                    if (clientMessage.IsComplete())
                    {
                        handleIMapBatchInvalidationEventV10(keys);
                        return;
                    }
                    var sourceUuidsSize = clientMessage.GetInt();
                    var sourceUuids = new List<string>(sourceUuidsSize);
                    for (var sourceUuidsIndex = 0; sourceUuidsIndex < sourceUuidsSize; sourceUuidsIndex++)
                    {
                        var sourceUuidsItem = clientMessage.GetStringUtf8();
                        sourceUuids.Add(sourceUuidsItem);
                    }
                    var partitionUuidsSize = clientMessage.GetInt();
                    var partitionUuids = new List<Guid>(partitionUuidsSize);
                    for (var partitionUuidsIndex = 0; partitionUuidsIndex < partitionUuidsSize; partitionUuidsIndex++)
                    {
                        var partitionUuidsItem = GuidCodec.Decode(clientMessage);
                        partitionUuids.Add(partitionUuidsItem);
                    }
                    var sequencesSize = clientMessage.GetInt();
                    var sequences = new List<long>(sequencesSize);
                    for (var sequencesIndex = 0; sequencesIndex < sequencesSize; sequencesIndex++)
                    {
                        var sequencesItem = clientMessage.GetLong();
                        sequences.Add(sequencesItem);
                    }
                    handleIMapBatchInvalidationEventV14(keys, sourceUuids, partitionUuids, sequences);
                    return;
                }
                Logger.GetLogger(typeof(EventHandler)).Warning("Unknown message type received on event handler :" + messageType);
            }

            internal delegate void HandleIMapInvalidationEventV10(IData key);

            internal delegate void
                HandleIMapInvalidationEventV14(IData key, string sourceUuid, Guid partitionUuid, long sequence);

            internal delegate void HandleIMapBatchInvalidationEventV10(IList<IData> keys);

            internal delegate void HandleIMapBatchInvalidationEventV14(IList<IData> keys, IList<string> sourceUuids,
                IList<Guid> partitionUuids, IList<long> sequences);
        }
    }
}