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
using static Hazelcast.Protocol.Portability;

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
    internal static class MapAddPartitionLostListenerCodec
    {
        public const int RequestMessageType = 72448; // 0x011B00
        public const int ResponseMessageType = 72449; // 0x011B01
        private const int RequestLocalOnlyFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BoolSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int EventMapPartitionLostPartitionIdFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventMapPartitionLostUuidFieldOffset = EventMapPartitionLostPartitionIdFieldOffset + IntSizeInBytes;
        private const int EventMapPartitionLostInitialFrameSize = EventMapPartitionLostUuidFieldOffset + GuidSizeInBytes;
        // hex: 0x011B02
        private const int EventMapPartitionLostMessageType = 72450;

        public static ClientMessage EncodeRequest(string name, bool localOnly)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Map.AddPartitionLostListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeBool(initialFrame, RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// returns the registration id for the MapPartitionLostListener.
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

        public static void HandleEvent(ClientMessage clientMessage, HandleMapPartitionLostEvent handleMapPartitionLostEvent, ILoggerFactory loggerFactory)
        {
            var messageType = clientMessage.MessageType;
            var iterator = clientMessage.GetIterator();
            if (messageType == EventMapPartitionLostMessageType) {
                var initialFrame = iterator.Take();
                var partitionId =  DecodeInt(initialFrame, EventMapPartitionLostPartitionIdFieldOffset);
                var uuid =  DecodeGuid(initialFrame, EventMapPartitionLostUuidFieldOffset);
                handleMapPartitionLostEvent(partitionId, uuid);
                return;
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
        }

        public delegate void HandleMapPartitionLostEvent(int partitionId, Guid uuid);
    }
}

#pragma warning restore IDE0051 // Remove unused private members