﻿// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
//   Hazelcast Client Protocol Code Generator @2a8aced
//   https://github.com/hazelcast/hazelcast-client-protocol
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
    /// Adds a cluster view listener to a connection.
    ///</summary>
#if SERVER_CODEC
    internal static class ClientAddClusterViewListenerServerCodec
#else
    internal static class ClientAddClusterViewListenerCodec
#endif
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

#if SERVER_CODEC
        public sealed class RequestParameters
        {
        }
#endif

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

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            iterator.Take(); // empty initial frame
            return request;
        }
#endif

        public sealed class ResponseParameters
        {
        }

#if SERVER_CODEC
        public static ClientMessage EncodeResponse()
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }
#endif

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            iterator.Take(); // empty initial frame
            return response;
        }

#if SERVER_CODEC
        public static ClientMessage EncodeMembersViewEvent(int version, ICollection<Hazelcast.Models.MemberInfo> memberInfos)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[EventMembersViewInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, EventMembersViewMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteIntL(EventMembersViewVersionFieldOffset, version);
            clientMessage.Append(initialFrame);
            clientMessage.Flags |= ClientMessageFlags.Event;
            ListMultiFrameCodec.Encode(clientMessage, memberInfos, MemberInfoCodec.Encode);
            return clientMessage;
        }
        public static ClientMessage EncodePartitionsViewEvent(int version, ICollection<KeyValuePair<Guid, IList<int>>> partitions)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[EventPartitionsViewInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, EventPartitionsViewMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteIntL(EventPartitionsViewVersionFieldOffset, version);
            clientMessage.Append(initialFrame);
            clientMessage.Flags |= ClientMessageFlags.Event;
            EntryListUUIDListIntegerCodec.Encode(clientMessage, partitions);
            return clientMessage;
        }
#endif
        public static ValueTask HandleEventAsync(ClientMessage clientMessage, Func<int, IList<Hazelcast.Models.MemberInfo>, object, ValueTask> handleMembersViewEventAsync, Func<int, IList<KeyValuePair<Guid, IList<int>>>, object, ValueTask> handlePartitionsViewEventAsync, object state, ILoggerFactory loggerFactory)
        {
            using var iterator = clientMessage.GetEnumerator();
            var messageType = clientMessage.MessageType;
            if (messageType == EventMembersViewMessageType)
            {
                var initialFrame = iterator.Take();
                var version =  initialFrame.Bytes.ReadIntL(EventMembersViewVersionFieldOffset);
                var memberInfos = ListMultiFrameCodec.Decode(iterator, MemberInfoCodec.Decode);
                return handleMembersViewEventAsync(version, memberInfos, state);
            }
            if (messageType == EventPartitionsViewMessageType)
            {
                var initialFrame = iterator.Take();
                var version =  initialFrame.Bytes.ReadIntL(EventPartitionsViewVersionFieldOffset);
                var partitions = EntryListUUIDListIntegerCodec.Decode(iterator);
                return handlePartitionsViewEventAsync(version, partitions, state);
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
            return default;
        }
    }
}
