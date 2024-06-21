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
//   Hazelcast Client Protocol Code Generator @c89bc95
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
    /// Adds listener for backup acks
    ///</summary>
#if SERVER_CODEC
    internal static class ClientLocalBackupListenerServerCodec
#else
    internal static class ClientLocalBackupListenerCodec
#endif
    {
        public const int RequestMessageType = 3840; // 0x000F00
        public const int ResponseMessageType = 3841; // 0x000F01
        private const int RequestInitialFrameSize = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int ResponseResponseFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BytesExtensions.SizeOfCodecGuid;
        private const int EventBackupSourceInvocationCorrelationIdFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int EventBackupInitialFrameSize = EventBackupSourceInvocationCorrelationIdFieldOffset + BytesExtensions.SizeOfLong;
        private const int EventBackupMessageType = 3842; // 0x000F02

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
                OperationName = "Client.LocalBackupListener"
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

            /// <summary>
            /// Returns the registration id for the listener.
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
        public static ClientMessage EncodeBackupEvent(long sourceInvocationCorrelationId)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[EventBackupInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, EventBackupMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteLongL(EventBackupSourceInvocationCorrelationIdFieldOffset, sourceInvocationCorrelationId);
            clientMessage.Append(initialFrame);
            clientMessage.Flags |= ClientMessageFlags.Event;
            return clientMessage;
        }
#endif
        public static ValueTask HandleEventAsync(ClientMessage clientMessage, Func<long, object, ValueTask> handleBackupEventAsync, object state, ILoggerFactory loggerFactory)
        {
            using var iterator = clientMessage.GetEnumerator();
            var messageType = clientMessage.MessageType;
            if (messageType == EventBackupMessageType)
            {
                var initialFrame = iterator.Take();
                var sourceInvocationCorrelationId =  initialFrame.Bytes.ReadLongL(EventBackupSourceInvocationCorrelationIdFieldOffset);
                return handleBackupEventAsync(sourceInvocationCorrelationId, state);
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
            return default;
        }
    }
}
