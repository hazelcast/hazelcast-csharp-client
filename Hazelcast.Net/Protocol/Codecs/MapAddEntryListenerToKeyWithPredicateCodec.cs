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
    /// Adds a MapListener for this map. To receive an event, you should implement a corresponding MapListener
    /// sub-interface for that event.
    ///</summary>
    internal static class MapAddEntryListenerToKeyWithPredicateCodec
    {
        public const int RequestMessageType = 71168; // 0x011600
        public const int ResponseMessageType = 71169; // 0x011601
        private const int RequestIncludeValueFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestListenerFlagsFieldOffset = RequestIncludeValueFieldOffset + BoolSizeInBytes;
        private const int RequestLocalOnlyFieldOffset = RequestListenerFlagsFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BoolSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int EventEntryEventTypeFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventEntryUuidFieldOffset = EventEntryEventTypeFieldOffset + IntSizeInBytes;
        private const int EventEntryNumberOfAffectedEntriesFieldOffset = EventEntryUuidFieldOffset + GuidSizeInBytes;
        private const int EventEntryInitialFrameSize = EventEntryNumberOfAffectedEntriesFieldOffset + IntSizeInBytes;
        // hex: 0x011602
        private const int EventEntryMessageType = 71170;

        public static ClientMessage EncodeRequest(string name, IData key, IData predicate, bool includeValue, int listenerFlags, bool localOnly)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Map.AddEntryListenerToKeyWithPredicate";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeBool(initialFrame, RequestIncludeValueFieldOffset, includeValue);
            EncodeInt(initialFrame, RequestListenerFlagsFieldOffset, listenerFlags);
            EncodeBool(initialFrame, RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, key);
            DataCodec.Encode(clientMessage, predicate);
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
            var initialFrame = iterator.Next();
            response.Response = DecodeGuid(initialFrame, ResponseResponseFieldOffset);
            return response;
        }

        public static class EventHandler
        {
            public static void HandleEvent(ClientMessage clientMessage, HandleEntryEvent handleEntryEvent)
            {
                var messageType = clientMessage.MessageType;
                var iterator = clientMessage.GetIterator();
                if (messageType == EventEntryMessageType) {
                    var initialFrame = iterator.Next();
                    int eventType =  DecodeInt(initialFrame, EventEntryEventTypeFieldOffset);
                    Guid uuid =  DecodeGuid(initialFrame, EventEntryUuidFieldOffset);
                    int numberOfAffectedEntries =  DecodeInt(initialFrame, EventEntryNumberOfAffectedEntriesFieldOffset);
                    IData key = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                    IData @value = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                    IData oldValue = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                    IData mergingValue = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                    handleEntryEvent(key, @value, oldValue, mergingValue, eventType, uuid, numberOfAffectedEntries);
                    return;
                }
                Services.Get.LoggerFactory().CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
            }
            public delegate void HandleEntryEvent(IData key, IData @value, IData oldValue, IData mergingValue, int eventType, Guid uuid, int numberOfAffectedEntries);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members