﻿// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Adds a MapPartitionLostListener. The addPartitionLostListener returns a register-id. This id is needed to remove
    /// the MapPartitionLostListener using the removePartitionLostListener(String) method.
    /// There is no check for duplicate registrations, so if you register the listener twice, it will get events twice.
    /// IMPORTANT: Please see com.hazelcast.partition.PartitionLostListener for weaknesses.
    /// IMPORTANT: Listeners registered from HazelcastClient may miss some of the map partition lost events due
    /// to design limitations.
    ///</summary>
#if SERVER_CODEC
    internal static class MapAddPartitionLostListenerServerCodec
#else
    internal static class MapAddPartitionLostListenerCodec
#endif
    {
        public const int RequestMessageType = 72448; // 0x011B00
        public const int ResponseMessageType = 72449; // 0x011B01
        private const int RequestLocalOnlyFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BytesExtensions.SizeOfBool;
        private const int ResponseResponseFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BytesExtensions.SizeOfCodecGuid;
        private const int EventMapPartitionLostPartitionIdFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int EventMapPartitionLostUuidFieldOffset = EventMapPartitionLostPartitionIdFieldOffset + BytesExtensions.SizeOfInt;
        private const int EventMapPartitionLostInitialFrameSize = EventMapPartitionLostUuidFieldOffset + BytesExtensions.SizeOfCodecGuid;
        private const int EventMapPartitionLostMessageType = 72450; // 0x011B02

#if SERVER_CODEC
        public sealed class RequestParameters
        {

            /// <summary>
            /// name of map
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// if true fires events that originated from this node only, otherwise fires all events
            ///</summary>
            public bool LocalOnly { get; set; }
        }
#endif

        public static ClientMessage EncodeRequest(string name, bool localOnly)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = false,
                OperationName = "Map.AddPartitionLostListener"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteBoolL(RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.LocalOnly = initialFrame.Bytes.ReadBoolL(RequestLocalOnlyFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            return request;
        }
#endif

        public sealed class ResponseParameters
        {

            /// <summary>
            /// returns the registration id for the MapPartitionLostListener.
            ///</summary>
            public Guid Response { get; set; }
        }

#if SERVER_CODEC
        public static ClientMessage EncodeResponse(Guid response)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            initialFrame.Bytes.WriteGuidL(ResponseResponseFieldOffset, response);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }
#endif

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = initialFrame.Bytes.ReadGuidL(ResponseResponseFieldOffset);
            return response;
        }

#if SERVER_CODEC
        public static ClientMessage EncodeMapPartitionLostEvent(int partitionId, Guid uuid)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[EventMapPartitionLostInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, EventMapPartitionLostMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteIntL(EventMapPartitionLostPartitionIdFieldOffset, partitionId);
            initialFrame.Bytes.WriteGuidL(EventMapPartitionLostUuidFieldOffset, uuid);
            clientMessage.Append(initialFrame);
            clientMessage.Flags |= ClientMessageFlags.Event;
            return clientMessage;
        }
#endif
        public static ValueTask HandleEventAsync(ClientMessage clientMessage, Func<int, Guid, object, ValueTask> handleMapPartitionLostEventAsync, object state, ILoggerFactory loggerFactory)
        {
            using var iterator = clientMessage.GetEnumerator();
            var messageType = clientMessage.MessageType;
            if (messageType == EventMapPartitionLostMessageType)
            {
                var initialFrame = iterator.Take();
                var partitionId =  initialFrame.Bytes.ReadIntL(EventMapPartitionLostPartitionIdFieldOffset);
                var uuid =  initialFrame.Bytes.ReadGuidL(EventMapPartitionLostUuidFieldOffset);
                return handleMapPartitionLostEventAsync(partitionId, uuid, state);
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
            return default;
        }
    }
}
