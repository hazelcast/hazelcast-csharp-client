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
    /// Adds a cluster view listener to a connection.
    ///</summary>
    internal static class ClientAddClusterViewListenerCodec
    {
        public const int RequestMessageType = 768; // 0x000300
        public const int ResponseMessageType = 769; // 0x000301
        private const int RequestInitialFrameSize = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int ResponseInitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int EventMembersViewVersionFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int EventMembersViewInitialFrameSize = EventMembersViewVersionFieldOffset + BytesExtensions.SizeOfInt;
        private const int EventMembersViewMessageType = 770; // 0x000302
        private const int EventPartitionsViewVersionFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int EventPartitionsViewInitialFrameSize = EventPartitionsViewVersionFieldOffset + BytesExtensions.SizeOfInt;
        private const int EventPartitionsViewMessageType = 771; // 0x000303

        public static ClientMessage EncodeRequest()
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = false,
                OperationName = "Client.AddClusterViewListener"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }

        public sealed class ResponseParameters
        {
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            iterator.Take(); // empty initial frame
            return response;
        }

        public static ValueTask HandleEventAsync(ClientMessage clientMessage, HandleMembersViewEventAsync handleMembersViewEventAsync, HandlePartitionsViewEventAsync handlePartitionsViewEventAsync, ILoggerFactory loggerFactory)
        {
            using var iterator = clientMessage.GetEnumerator();
            var messageType = clientMessage.MessageType;
            if (messageType == EventMembersViewMessageType) {
                var initialFrame = iterator.Take();
                var version =  initialFrame.Bytes.ReadIntL(EventMembersViewVersionFieldOffset);
                var memberInfos = ListMultiFrameCodec.Decode(iterator, MemberInfoCodec.Decode);
                return handleMembersViewEventAsync(version, memberInfos);
            }
            if (messageType == EventPartitionsViewMessageType) {
                var initialFrame = iterator.Take();
                var version =  initialFrame.Bytes.ReadIntL(EventPartitionsViewVersionFieldOffset);
                var partitions = EntryListUUIDListIntegerCodec.Decode(iterator);
                return handlePartitionsViewEventAsync(version, partitions);
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
            return default;
        }

        public delegate ValueTask HandleMembersViewEventAsync(int version, ICollection<Hazelcast.Data.MemberInfo> memberInfos);

        public delegate ValueTask HandlePartitionsViewEventAsync(int version, ICollection<KeyValuePair<Guid, IList<int>>> partitions);
    }
}