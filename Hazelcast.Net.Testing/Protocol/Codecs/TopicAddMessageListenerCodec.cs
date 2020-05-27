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
    /// Subscribes to this topic. When someone publishes a message on this topic. onMessage() function of the given
    /// MessageListener is called. More than one message listener can be added on one instance.
    ///</summary>
    internal static class TopicAddMessageListenerServerCodec
    {
        public const int RequestMessageType = 262656; // 0x040200
        public const int ResponseMessageType = 262657; // 0x040201
        private const int RequestLocalOnlyFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BoolSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int TopicEventPublishTimeFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int TopicEventUuidFieldOffset = TopicEventPublishTimeFieldOffset + LongSizeInBytes;
        private const int TopicEventInitialFrameSize = TopicEventUuidFieldOffset + GuidSizeInBytes;
        private const int TopicEventMessageType = 262658; // 0x040202

        public sealed class RequestParameters
        {

            /// <summary>
            /// Name of the Topic
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// if true listens only local events on registered member
            ///</summary>
            public bool LocalOnly { get; set; }
        }

        public static ClientMessage EncodeRequest(string name, bool localOnly)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Topic.AddMessageListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
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
            request.LocalOnly = DecodeBool(initialFrame, RequestLocalOnlyFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            return request;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// returns the registration id
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

        public static ClientMessage EncodeTopicEvent(IData item, long publishTime, Guid uuid)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[TopicEventInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, TopicEventMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeLong(initialFrame, TopicEventPublishTimeFieldOffset, publishTime);
            EncodeGuid(initialFrame, TopicEventUuidFieldOffset, uuid);
            clientMessage.Add(initialFrame);
            clientMessage.Flags |= ClientMessageFlags.Event;
            DataCodec.Encode(clientMessage, item);
            return clientMessage;
        }

        public static void HandleEvent(ClientMessage clientMessage, HandleTopicEvent handleTopicEvent, ILoggerFactory loggerFactory)
        {
            var messageType = clientMessage.MessageType;
            var iterator = clientMessage.GetIterator();
            if (messageType == TopicEventMessageType) {
                var initialFrame = iterator.Take();
                var publishTime =  DecodeLong(initialFrame, TopicEventPublishTimeFieldOffset);
                var uuid =  DecodeGuid(initialFrame, TopicEventUuidFieldOffset);
                var item = DataCodec.Decode(iterator);
                handleTopicEvent(item, publishTime, uuid);
                return;
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
        }

        public delegate void HandleTopicEvent(IData item, long publishTime, Guid uuid);
    }
}