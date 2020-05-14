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
using static Hazelcast.Messaging.Portability;

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Adds an item listener for this collection. Listener will be notified for all collection add/remove events.
    ///</summary>
    internal static class ListAddListenerCodec
    {
        public const int RequestMessageType = 330496; // 0x050B00
        public const int ResponseMessageType = 330497; // 0x050B01
        private const int RequestIncludeValueFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestLocalOnlyFieldOffset = RequestIncludeValueFieldOffset + BoolSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BoolSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int EventItemUuidFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventItemEventTypeFieldOffset = EventItemUuidFieldOffset + GuidSizeInBytes;
        private const int EventItemInitialFrameSize = EventItemEventTypeFieldOffset + IntSizeInBytes;
        // hex: 0x050B02
        private const int EventItemMessageType = 330498;

        public static ClientMessage EncodeRequest(string name, bool includeValue, bool localOnly)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "List.AddListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeBool(initialFrame, RequestIncludeValueFieldOffset, includeValue);
            EncodeBool(initialFrame, RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// Registration id for the listener.
            ///</summary>
            public Guid Response { get; set; }
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = DecodeGuid(initialFrame, ResponseResponseFieldOffset);
            return response;
        }

        public static void HandleEvent(ClientMessage clientMessage, HandleItemEvent handleItemEvent, ILoggerFactory loggerFactory)
        {
            var messageType = clientMessage.MessageType;
            var iterator = clientMessage.GetIterator();
            if (messageType == EventItemMessageType) {
                var initialFrame = iterator.Take();
                var uuid =  DecodeGuid(initialFrame, EventItemUuidFieldOffset);
                var eventType =  DecodeInt(initialFrame, EventItemEventTypeFieldOffset);
                var item = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                handleItemEvent(item, uuid, eventType);
                return;
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
        }

        public delegate void HandleItemEvent(IData item, Guid uuid, int eventType);
    }
}

#pragma warning restore IDE0051 // Remove unused private members