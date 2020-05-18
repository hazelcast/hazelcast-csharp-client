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
    /// Adds a partition lost listener to the cluster.
    ///</summary>
    internal static class ClientAddPartitionLostListenerServerCodec
    {
        public const int RequestMessageType = 1536; // 0x000600
        public const int ResponseMessageType = 1537; // 0x000601
        private const int RequestLocalOnlyFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BoolSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int PartitionLostEventPartitionIdFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int PartitionLostEventLostBackupCountFieldOffset = PartitionLostEventPartitionIdFieldOffset + IntSizeInBytes;
        private const int PartitionLostEventSourceFieldOffset = PartitionLostEventLostBackupCountFieldOffset + IntSizeInBytes;
        private const int PartitionLostEventInitialFrameSize = PartitionLostEventSourceFieldOffset + GuidSizeInBytes;
        private const int PartitionLostEventMessageType = 1538; // 0x000602

        public sealed class RequestParameters
        {

            /// <summary>
            /// if true only node that has the partition sends the request, if false
            /// sends all partition lost events.
            ///</summary>
            public bool LocalOnly { get; set; }
        }
    
        public static ClientMessage EncodeRequest(bool localOnly)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Client.AddPartitionLostListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeBool(initialFrame, RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Add(initialFrame);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.LocalOnly = DecodeBool(initialFrame, RequestLocalOnlyFieldOffset);
            return request;
        }
        
        public sealed class ResponseParameters
        {

            /// <summary>
            /// The listener registration id.
            ///</summary>
            public Guid Response { get; set; }
        }

        public static ClientMessage EncodeResponse(Guid response)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
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

        public static ClientMessage EncodePartitionLostEvent(int partitionId, int lostBackupCount, Guid source)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, PartitionLostEventMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeInt(initialFrame, PartitionLostEventPartitionIdFieldOffset, partitionId);
            EncodeInt(initialFrame, PartitionLostEventLostBackupCountFieldOffset, lostBackupCount);
            EncodeGuid(initialFrame, PartitionLostEventSourceFieldOffset, source);
            clientMessage.Add(initialFrame);
            clientMessage.Flags |= ClientMessageFlags.Event;
            return clientMessage;
        }
    
        public static void HandleEvent(ClientMessage clientMessage, HandlePartitionLostEvent handlePartitionLostEvent, ILoggerFactory loggerFactory)
        {
            var messageType = clientMessage.MessageType;
            var iterator = clientMessage.GetIterator();
            if (messageType == PartitionLostEventMessageType) {
                var initialFrame = iterator.Take();
                var partitionId =  DecodeInt(initialFrame, PartitionLostEventPartitionIdFieldOffset);
                var lostBackupCount =  DecodeInt(initialFrame, PartitionLostEventLostBackupCountFieldOffset);
                var source =  DecodeGuid(initialFrame, PartitionLostEventSourceFieldOffset);
                handlePartitionLostEvent(partitionId, lostBackupCount, source);
                return;
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
        }

        public delegate void HandlePartitionLostEvent(int partitionId, int lostBackupCount, Guid source);
    }
}