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
    /// Adds a partition lost listener to the cluster.
    ///</summary>
    internal static class ClientAddPartitionLostListenerCodec
    {
        public const int RequestMessageType = 1536; // 0x000600
        public const int ResponseMessageType = 1537; // 0x000601
        private const int RequestLocalOnlyFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BytesExtensions.SizeOfBool;
        private const int ResponseResponseFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BytesExtensions.SizeOfGuid;
        private const int EventPartitionLostPartitionIdFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int EventPartitionLostLostBackupCountFieldOffset = EventPartitionLostPartitionIdFieldOffset + BytesExtensions.SizeOfInt;
        private const int EventPartitionLostSourceFieldOffset = EventPartitionLostLostBackupCountFieldOffset + BytesExtensions.SizeOfInt;
        private const int EventPartitionLostInitialFrameSize = EventPartitionLostSourceFieldOffset + BytesExtensions.SizeOfGuid;
        // hex: 0x000602
        private const int EventPartitionLostMessageType = 1538;

        public static ClientMessage EncodeRequest(bool localOnly)
        {
            var clientMessage = new ClientMessage();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Client.AddPartitionLostListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteBool(RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// The listener registration id.
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

        public static ValueTask HandleEventAsync(ClientMessage clientMessage, HandlePartitionLostEventAsync handlePartitionLostEventAsync, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
        {
            var messageType = clientMessage.MessageType;
            var iterator = clientMessage.GetEnumerator();
            if (messageType == EventPartitionLostMessageType) {
                var initialFrame = iterator.Take();
                var partitionId =  initialFrame.Bytes.ReadInt(EventPartitionLostPartitionIdFieldOffset);
                var lostBackupCount =  initialFrame.Bytes.ReadInt(EventPartitionLostLostBackupCountFieldOffset);
                var source =  initialFrame.Bytes.ReadGuid(EventPartitionLostSourceFieldOffset);
                return handlePartitionLostEventAsync(partitionId, lostBackupCount, source, cancellationToken);
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
            return default;
        }

        public delegate ValueTask HandlePartitionLostEventAsync(int partitionId, int lostBackupCount, Guid source, CancellationToken cancellationToken);
    }
}

#pragma warning restore IDE0051 // Remove unused private members