// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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

// Client Protocol version, Since:1.0 - Update:1.4

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class MapAddNearCacheEntryListenerCodec
    {
        public static readonly MapMessageType RequestType = MapMessageType.MapAddNearCacheEntryListener;
        public const int ResponseType = 104;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly MapMessageType TYPE = RequestType;
            public string name;
            public int listenerFlags;
            public bool localOnly;

            public static int CalculateDataSize(string name, int listenerFlags, bool localOnly)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += ParameterUtil.CalculateDataSize(name);
                dataSize += Bits.IntSizeInBytes;
                dataSize += Bits.BooleanSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(string name, int listenerFlags, bool localOnly)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(name, listenerFlags, localOnly);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(name);
            clientMessage.Set(listenerFlags);
            clientMessage.Set(localOnly);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//
        public class ResponseParameters
        {
            public string response;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            var response = clientMessage.GetStringUtf8();
            parameters.response = response;
            return parameters;
        }

//************************ EVENTS *************************//
        public abstract class AbstractEventHandler
        {
            public static void Handle(IClientMessage clientMessage, HandleIMapInvalidation handleIMapInvalidation,
                HandleIMapBatchInvalidation handleIMapBatchInvalidation)
            {
                var messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventIMapInvalidation)
                {
                    var IMapInvalidationMessageFinished = false;
                    IData key = null;
                    var keyIsNull = clientMessage.GetBoolean();
                    if (!keyIsNull)
                    {
                        key = clientMessage.GetData();
                    }
                    if (!IMapInvalidationMessageFinished)
                    {
                        IMapInvalidationMessageFinished = clientMessage.IsComplete();
                    }
                    string sourceUuid = null;
                    if (!IMapInvalidationMessageFinished)
                    {
                        sourceUuid = clientMessage.GetStringUtf8();
                    }
                    Guid? partitionUuid = null;
                    if (!IMapInvalidationMessageFinished)
                    {
                        partitionUuid = UUIDCodec.Decode(clientMessage);
                    }
                    long? sequence = null;
                    if (!IMapInvalidationMessageFinished)
                    {
                        sequence = clientMessage.GetLong();
                    }
                    handleIMapInvalidation(key, sourceUuid, partitionUuid, sequence);
                    return;
                }
                if (messageType == EventMessageConst.EventIMapBatchInvalidation)
                {
                    var IMapBatchInvalidationMessageFinished = false;
                    var keys = new List<IData>();
                    var keysSize = clientMessage.GetInt();
                    for (var keysIndex = 0; keysIndex < keysSize; keysIndex++)
                    {
                        var keysItem = clientMessage.GetData();
                        keys.Add(keysItem);
                    }
                    if (!IMapBatchInvalidationMessageFinished)
                    {
                        IMapBatchInvalidationMessageFinished = clientMessage.IsComplete();
                    }
                    IList<string> sourceUuids = null;
                    if (!IMapBatchInvalidationMessageFinished)
                    {
                        sourceUuids = new List<string>();
                        var sourceUuidsSize = clientMessage.GetInt();
                        for (var sourceUuidsIndex = 0; sourceUuidsIndex < sourceUuidsSize; sourceUuidsIndex++)
                        {
                            var sourceUuidsItem = clientMessage.GetStringUtf8();
                            sourceUuids.Add(sourceUuidsItem);
                        }
                    }
                    IList<Guid> partitionUuids = null;
                    if (!IMapBatchInvalidationMessageFinished)
                    {
                        partitionUuids = new List<Guid>();
                        var partitionUuidsSize = clientMessage.GetInt();
                        for (var partitionUuidsIndex = 0;
                            partitionUuidsIndex < partitionUuidsSize;
                            partitionUuidsIndex++)
                        {
                            var partitionUuidsItem = UUIDCodec.Decode(clientMessage);
                            partitionUuids.Add(partitionUuidsItem);
                        }
                    }
                    IList<long> sequences = null;
                    if (!IMapBatchInvalidationMessageFinished)
                    {
                        sequences = new List<long>();
                        var sequencesSize = clientMessage.GetInt();
                        for (var sequencesIndex = 0; sequencesIndex < sequencesSize; sequencesIndex++)
                        {
                            var sequencesItem = clientMessage.GetLong();
                            sequences.Add(sequencesItem);
                        }
                    }
                    handleIMapBatchInvalidation(keys, sourceUuids, partitionUuids, sequences);
                    return;
                }
                Hazelcast.Logging.Logger.GetLogger(typeof(AbstractEventHandler))
                    .Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
            }

            public delegate void HandleIMapInvalidation(
                IData key, string sourceUuid, Guid? partitionUuid, long? sequence);

            public delegate void HandleIMapBatchInvalidation(
                IList<IData> keys, IList<string> sourceUuids, IList<Guid> partitionUuids, IList<long> sequences);
        }
    }
}