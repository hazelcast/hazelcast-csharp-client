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
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Messaging;
using Hazelcast.Logging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;
using Hazelcast.Protocol.Portability;
using static Hazelcast.Protocol.Portability.Temp;

// <auto-generated>
//   This code was generated by a tool.
//     Hazelcast Client Protocol Code Generator
//     https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Adds listener to map. This listener will be used to listen near cache invalidation events.
    ///</summary>
    internal static class MapAddNearCacheInvalidationListenerCodec
    {
        public const int RequestMessageType = 81664; // 0x013F00
        public const int ResponseMessageType = 81665; // 0x013F01
        private const int RequestListenerFlagsFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestLocalOnlyFieldOffset = RequestListenerFlagsFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BoolSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int EventIMapInvalidationSourceUuidFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventIMapInvalidationPartitionUuidFieldOffset = EventIMapInvalidationSourceUuidFieldOffset + GuidSizeInBytes;
        private const int EventIMapInvalidationSequenceFieldOffset = EventIMapInvalidationPartitionUuidFieldOffset + GuidSizeInBytes;
        private const int EventIMapInvalidationInitialFrameSize = EventIMapInvalidationSequenceFieldOffset + LongSizeInBytes;
        // hex: 0x013F02
        private const int EventIMapInvalidationMessageType = 81666;
        private const int EventIMapBatchInvalidationInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        // hex: 0x013F03
        private const int EventIMapBatchInvalidationMessageType = 81667;

        public static ClientMessage EncodeRequest(string name, int listenerFlags, bool localOnly)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Map.AddNearCacheInvalidationListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeInt(initialFrame, RequestListenerFlagsFieldOffset, listenerFlags);
            EncodeBool(initialFrame, RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// A unique string which is used as a key to remove the listener.
            ///</summary>
            public Guid Response;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = DecodeGuid(initialFrame, ResponseResponseFieldOffset);
            return response;
        }

        public static class EventHandler
        {
            public static void HandleEvent(ClientMessage clientMessage, HandleIMapInvalidationEvent handleIMapInvalidationEvent, HandleIMapBatchInvalidationEvent handleIMapBatchInvalidationEvent)
            {
                var messageType = clientMessage.MessageType;
                var iterator = clientMessage.GetIterator();
                if (messageType == EventIMapInvalidationMessageType) {
                    var initialFrame = iterator.Take();
                    Guid sourceUuid =  DecodeGuid(initialFrame, EventIMapInvalidationSourceUuidFieldOffset);
                    Guid partitionUuid =  DecodeGuid(initialFrame, EventIMapInvalidationPartitionUuidFieldOffset);
                    long sequence =  DecodeLong(initialFrame, EventIMapInvalidationSequenceFieldOffset);
                    IData key = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                    handleIMapInvalidationEvent(key, sourceUuid, partitionUuid, sequence);
                    return;
                }
                if (messageType == EventIMapBatchInvalidationMessageType) {
                    //empty initial frame
                    iterator.Take();
                    IList<IData> keys = ListMultiFrameCodec.Decode(iterator, DataCodec.Decode);
                    IList<Guid> sourceUuids = ListUUIDCodec.Decode(iterator);
                    IList<Guid> partitionUuids = ListUUIDCodec.Decode(iterator);
                    IList<long> sequences = ListLongCodec.Decode(iterator);
                    handleIMapBatchInvalidationEvent(keys, sourceUuids, partitionUuids, sequences);
                    return;
                }
                Services.Get.LoggerFactory().CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
            }
            public delegate void HandleIMapInvalidationEvent(IData key, Guid sourceUuid, Guid partitionUuid, long sequence);
            public delegate void HandleIMapBatchInvalidationEvent(ICollection<IData> keys, ICollection<Guid> sourceUuids, ICollection<Guid> partitionUuids, ICollection<long> sequences);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members