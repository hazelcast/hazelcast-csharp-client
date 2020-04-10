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

using System;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Messaging;
using Hazelcast.Logging;
using Hazelcast.Clustering;
using Microsoft.Extensions.Logging;
using Hazelcast.Protocol.Portability;
using static Hazelcast.Protocol.Portability.Temp;

// <auto-generated>
//   This code was generated by a tool.
//     Hazelcast Client Protocol Code Generator
//     https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Adds a partition lost listener to the cluster.
    ///</summary>
    internal static class ClientAddPartitionLostListenerCodec
    {
        public const int RequestMessageType = 1536; // 0x000600
        public const int ResponseMessageType = 1537; // 0x000601
        private const int RequestLocalOnlyFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BoolSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int EventPartitionLostPartitionIdFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventPartitionLostLostBackupCountFieldOffset = EventPartitionLostPartitionIdFieldOffset + IntSizeInBytes;
        private const int EventPartitionLostSourceFieldOffset = EventPartitionLostLostBackupCountFieldOffset + IntSizeInBytes;
        private const int EventPartitionLostInitialFrameSize = EventPartitionLostSourceFieldOffset + GuidSizeInBytes;
        // hex: 0x000602
        private const int EventPartitionLostMessageType = 1538;

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

        public class ResponseParameters
        {

            /// <summary>
            /// The listener registration id.
            ///</summary>
            public Guid Response;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Response = DecodeGuid(initialFrame, ResponseResponseFieldOffset);
            return response;
        }

        public static class EventHandler
        {
            public static void HandleEvent(ClientMessage clientMessage, HandlePartitionLostEvent handlePartitionLostEvent)
            {
                var messageType = clientMessage.MessageType;
                var iterator = clientMessage.GetIterator();
                if (messageType == EventPartitionLostMessageType) {
                    var initialFrame = iterator.Next();
                    int partitionId =  DecodeInt(initialFrame, EventPartitionLostPartitionIdFieldOffset);
                    int lostBackupCount =  DecodeInt(initialFrame, EventPartitionLostLostBackupCountFieldOffset);
                    Guid source =  DecodeGuid(initialFrame, EventPartitionLostSourceFieldOffset);
                    handlePartitionLostEvent(partitionId, lostBackupCount, source);
                    return;
                }
                Services.Get.LoggerFactory().CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
            }
            public delegate void HandlePartitionLostEvent(int partitionId, int lostBackupCount, Guid source);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members