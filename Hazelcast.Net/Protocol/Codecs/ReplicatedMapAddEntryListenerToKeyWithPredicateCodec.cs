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

// <auto-generated>
//   This code was generated by a tool.
//     Hazelcast Client Protocol Code Generator
//     https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Logging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Adds an continuous entry listener for this map. The listener will be notified for map add/remove/update/evict
    /// events filtered by the given predicate.
    ///</summary>
    internal static class ReplicatedMapAddEntryListenerToKeyWithPredicateCodec
    {
        public const int RequestMessageType = 854528; // 0x0D0A00
        public const int ResponseMessageType = 854529; // 0x0D0A01
        private const int RequestLocalOnlyFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BytesExtensions.SizeOfBool;
        private const int ResponseResponseFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BytesExtensions.SizeOfGuid;
        private const int EventEntryEventTypeFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int EventEntryUuidFieldOffset = EventEntryEventTypeFieldOffset + BytesExtensions.SizeOfInt;
        private const int EventEntryNumberOfAffectedEntriesFieldOffset = EventEntryUuidFieldOffset + BytesExtensions.SizeOfGuid;
        private const int EventEntryInitialFrameSize = EventEntryNumberOfAffectedEntriesFieldOffset + BytesExtensions.SizeOfInt;
        // hex: 0x0D0A02
        private const int EventEntryMessageType = 854530;

        public static ClientMessage EncodeRequest(string name, IData key, IData predicate, bool localOnly)
        {
            var clientMessage = new ClientMessage();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "ReplicatedMap.AddEntryListenerToKeyWithPredicate";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteBool(RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, key);
            DataCodec.Encode(clientMessage, predicate);
            return clientMessage;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// A unique string  which is used as a key to remove the listener.
            ///</summary>
            public Guid Response { get; set; }
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = initialFrame.Bytes.ReadGuid(ResponseResponseFieldOffset);
            return response;
        }

        public static void HandleEvent(ClientMessage clientMessage, HandleEntryEvent handleEntryEvent, ILoggerFactory loggerFactory)
        {
            var messageType = clientMessage.MessageType;
            var iterator = clientMessage.GetEnumerator();
            if (messageType == EventEntryMessageType) {
                var initialFrame = iterator.Take();
                var eventType =  initialFrame.Bytes.ReadInt(EventEntryEventTypeFieldOffset);
                var uuid =  initialFrame.Bytes.ReadGuid(EventEntryUuidFieldOffset);
                var numberOfAffectedEntries =  initialFrame.Bytes.ReadInt(EventEntryNumberOfAffectedEntriesFieldOffset);
                var key = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                var @value = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                var oldValue = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                var mergingValue = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                handleEntryEvent(key, @value, oldValue, mergingValue, eventType, uuid, numberOfAffectedEntries);
                return;
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
        }

        public delegate void HandleEntryEvent(IData key, IData @value, IData oldValue, IData mergingValue, int eventType, Guid uuid, int numberOfAffectedEntries);
    }
}

#pragma warning restore IDE0051 // Remove unused private members