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
    /// Subscribes to this topic. When someone publishes a message on this topic. onMessage() function of the given
    /// MessageListener is called. More than one message listener can be added on one instance.
    ///</summary>
    internal static class TopicAddMessageListenerCodec
    {
        public const int RequestMessageType = 262656; // 0x040200
        public const int ResponseMessageType = 262657; // 0x040201
        private const int RequestLocalOnlyFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BytesExtensions.SizeOfBool;
        private const int ResponseResponseFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BytesExtensions.SizeOfGuid;
        private const int EventTopicPublishTimeFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int EventTopicUuidFieldOffset = EventTopicPublishTimeFieldOffset + BytesExtensions.SizeOfLong;
        private const int EventTopicInitialFrameSize = EventTopicUuidFieldOffset + BytesExtensions.SizeOfGuid;
        // hex: 0x040202
        private const int EventTopicMessageType = 262658;

        public static ClientMessage EncodeRequest(string name, bool localOnly)
        {
            var clientMessage = new ClientMessage();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Topic.AddMessageListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteBool(RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// returns the registration id
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

        public static void HandleEvent(ClientMessage clientMessage, HandleTopicEvent handleTopicEvent, ILoggerFactory loggerFactory)
        {
            var messageType = clientMessage.MessageType;
            var iterator = clientMessage.GetEnumerator();
            if (messageType == EventTopicMessageType) {
                var initialFrame = iterator.Take();
                var publishTime =  initialFrame.Bytes.ReadLong(EventTopicPublishTimeFieldOffset);
                var uuid =  initialFrame.Bytes.ReadGuid(EventTopicUuidFieldOffset);
                var item = DataCodec.Decode(iterator);
                handleTopicEvent(item, publishTime, uuid);
                return;
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
        }

        public delegate void HandleTopicEvent(IData item, long publishTime, Guid uuid);
    }
}

#pragma warning restore IDE0051 // Remove unused private members