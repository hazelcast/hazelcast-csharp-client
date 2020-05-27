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
// ReSharper disable CheckNamespace

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
    /// Adds an listener for this collection. Listener will be notified or all collection add/remove events.
    ///</summary>
    internal static class QueueAddListenerServerCodec
    {
        public const int RequestMessageType = 200960; // 0x031100
        public const int ResponseMessageType = 200961; // 0x031101
        private const int RequestIncludeValueFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestLocalOnlyFieldOffset = RequestIncludeValueFieldOffset + BoolSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BoolSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int ItemEventUuidFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ItemEventEventTypeFieldOffset = ItemEventUuidFieldOffset + GuidSizeInBytes;
        private const int ItemEventInitialFrameSize = ItemEventEventTypeFieldOffset + IntSizeInBytes;
        private const int ItemEventMessageType = 200962; // 0x031102

        public sealed class RequestParameters
        {

            /// <summary>
            /// Name of the Queue
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// <tt>true</tt> if the updated item should be passed to the item listener, <tt>false</tt> otherwise.
            ///</summary>
            public bool IncludeValue { get; set; }

            /// <summary>
            /// if true fires events that originated from this node only, otherwise fires all events
            ///</summary>
            public bool LocalOnly { get; set; }
        }

        public static ClientMessage EncodeRequest(string name, bool includeValue, bool localOnly)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Queue.AddListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeBool(initialFrame, RequestIncludeValueFieldOffset, includeValue);
            EncodeBool(initialFrame, RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.IncludeValue = DecodeBool(initialFrame, RequestIncludeValueFieldOffset);
            request.LocalOnly = DecodeBool(initialFrame, RequestLocalOnlyFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            return request;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// The registration id
            ///</summary>
            public Guid Response { get; set; }
        }

        public static ClientMessage EncodeResponse(Guid response)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, ResponseMessageType);
            EncodeGuid(initialFrame, ResponseResponseFieldOffset, response);
            clientMessage.Add(initialFrame);
            return clientMessage;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = DecodeGuid(initialFrame, ResponseResponseFieldOffset);
            return response;
        }

        public static ClientMessage EncodeItemEvent(IData item, Guid uuid, int eventType)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ItemEventInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, ItemEventMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeGuid(initialFrame, ItemEventUuidFieldOffset, uuid);
            EncodeInt(initialFrame, ItemEventEventTypeFieldOffset, eventType);
            clientMessage.Add(initialFrame);
            clientMessage.Flags |= ClientMessageFlags.Event;
            CodecUtil.EncodeNullable(clientMessage, item, DataCodec.Encode);
            return clientMessage;
        }

        public static void HandleEvent(ClientMessage clientMessage, HandleItemEvent handleItemEvent, ILoggerFactory loggerFactory)
        {
            var messageType = clientMessage.MessageType;
            var iterator = clientMessage.GetIterator();
            if (messageType == ItemEventMessageType) {
                var initialFrame = iterator.Take();
                var uuid =  DecodeGuid(initialFrame, ItemEventUuidFieldOffset);
                var eventType =  DecodeInt(initialFrame, ItemEventEventTypeFieldOffset);
                var item = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                handleItemEvent(item, uuid, eventType);
                return;
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
        }

        public delegate void HandleItemEvent(IData item, Guid uuid, int eventType);
    }
}