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
using System.Threading;
using System.Threading.Tasks;
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
    /// Adds an listener for this collection. Listener will be notified or all collection add/remove events.
    ///</summary>
    internal static class QueueAddListenerCodec
    {
        public const int RequestMessageType = 200960; // 0x031100
        public const int ResponseMessageType = 200961; // 0x031101
        private const int RequestIncludeValueFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestLocalOnlyFieldOffset = RequestIncludeValueFieldOffset + BytesExtensions.SizeOfBool;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BytesExtensions.SizeOfBool;
        private const int ResponseResponseFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BytesExtensions.SizeOfGuid;
        private const int EventItemUuidFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int EventItemEventTypeFieldOffset = EventItemUuidFieldOffset + BytesExtensions.SizeOfGuid;
        private const int EventItemInitialFrameSize = EventItemEventTypeFieldOffset + BytesExtensions.SizeOfInt;
        // hex: 0x031102
        private const int EventItemMessageType = 200962;

        public static ClientMessage EncodeRequest(string name, bool includeValue, bool localOnly)
        {
            var clientMessage = new ClientMessage();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Queue.AddListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteBool(RequestIncludeValueFieldOffset, includeValue);
            initialFrame.Bytes.WriteBool(RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// The registration id
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

        public static ValueTask HandleEventAsync(ClientMessage clientMessage, HandleItemEventAsync handleItemEventAsync, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
        {
            var messageType = clientMessage.MessageType;
            var iterator = clientMessage.GetEnumerator();
            if (messageType == EventItemMessageType) {
                var initialFrame = iterator.Take();
                var uuid =  initialFrame.Bytes.ReadGuid(EventItemUuidFieldOffset);
                var eventType =  initialFrame.Bytes.ReadInt(EventItemEventTypeFieldOffset);
                var item = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                return handleItemEventAsync(item, uuid, eventType, cancellationToken);
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
            return default;
        }

        public delegate ValueTask HandleItemEventAsync(IData item, Guid uuid, int eventType, CancellationToken cancellationToken);
    }
}

#pragma warning restore IDE0051 // Remove unused private members