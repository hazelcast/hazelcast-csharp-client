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
    internal static class MapAddPartitionLostListenerServerCodec
    {
        public const int RequestMessageType = 72448; // 0x011B00
        public const int ResponseMessageType = 72449; // 0x011B01
        private const int RequestLocalOnlyFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BytesExtensions.SizeOfBool;
        private const int ResponseResponseFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BytesExtensions.SizeOfGuid;
        private const int MapPartitionLostEventPartitionIdFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int MapPartitionLostEventUuidFieldOffset = MapPartitionLostEventPartitionIdFieldOffset + BytesExtensions.SizeOfInt;
        private const int MapPartitionLostEventInitialFrameSize = MapPartitionLostEventUuidFieldOffset + BytesExtensions.SizeOfGuid;
        private const int MapPartitionLostEventMessageType = 72450; // 0x011B02

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
    
        public static ClientMessage EncodeRequest(string name, bool localOnly)
        {
            var clientMessage = new ClientMessage();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Map.AddPartitionLostListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteBool(RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.LocalOnly = initialFrame.Bytes.ReadBool(RequestLocalOnlyFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            return request;
        }
        
        public sealed class ResponseParameters
        {

            /// <summary>
            /// returns the registration id for the MapPartitionLostListener.
            ///</summary>
            public Guid Response { get; set; }
        }

        public static ClientMessage EncodeResponse(Guid response)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            initialFrame.Bytes.WriteGuid(ResponseResponseFieldOffset, response);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }
    
        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = initialFrame.Bytes.ReadGuid(ResponseResponseFieldOffset);
            return response;
        }

        public static ClientMessage EncodeMapPartitionLostEvent(int partitionId, Guid uuid)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[MapPartitionLostEventInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.MessageType, MapPartitionLostEventMessageType);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteInt(MapPartitionLostEventPartitionIdFieldOffset, partitionId);
            initialFrame.Bytes.WriteGuid(MapPartitionLostEventUuidFieldOffset, uuid);
            clientMessage.Append(initialFrame);
            clientMessage.Flags |= ClientMessageFlags.Event;
            return clientMessage;
        }
    
        public static void HandleEvent(ClientMessage clientMessage, HandleMapPartitionLostEvent handleMapPartitionLostEvent, ILoggerFactory loggerFactory)
        {
            var messageType = clientMessage.MessageType;
            var iterator = clientMessage.GetEnumerator();
            if (messageType == MapPartitionLostEventMessageType) {
                var initialFrame = iterator.Take();
                var partitionId =  initialFrame.Bytes.ReadInt(MapPartitionLostEventPartitionIdFieldOffset);
                var uuid =  initialFrame.Bytes.ReadGuid(MapPartitionLostEventUuidFieldOffset);
                handleMapPartitionLostEvent(partitionId, uuid);
                return;
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
        }

        public delegate void HandleMapPartitionLostEvent(int partitionId, Guid uuid);
    }
}