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
    /// Adds listener for backup acks
    ///</summary>
    internal static class ClientLocalBackupListenerServerCodec
    {
        public const int RequestMessageType = 3840; // 0x000F00
        public const int ResponseMessageType = 3841; // 0x000F01
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int BackupEventSourceInvocationCorrelationIdFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int BackupEventInitialFrameSize = BackupEventSourceInvocationCorrelationIdFieldOffset + LongSizeInBytes;
        private const int BackupEventMessageType = 3842; // 0x000F02

        public sealed class RequestParameters
        {
        }
    
        public static ClientMessage EncodeRequest()
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Client.LocalBackupListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            clientMessage.Add(initialFrame);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            //empty initial frame
            iterator.Take();
            return request;
        }
        
        public sealed class ResponseParameters
        {

            /// <summary>
            /// Returns the registration id for the listener.
            ///</summary>
            public Guid Response { get; set; }
        }

        public static ClientMessage EncodeResponse(Guid response)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, ResponseMessageType);
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

        public static ClientMessage EncodeBackupEvent(long sourceInvocationCorrelationId)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[BackupEventInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, BackupEventMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeLong(initialFrame, BackupEventSourceInvocationCorrelationIdFieldOffset, sourceInvocationCorrelationId);
            clientMessage.Add(initialFrame);
            clientMessage.Flags |= ClientMessageFlags.Event;
            return clientMessage;
        }
    
        public static void HandleEvent(ClientMessage clientMessage, HandleBackupEvent handleBackupEvent, ILoggerFactory loggerFactory)
        {
            var messageType = clientMessage.MessageType;
            var iterator = clientMessage.GetIterator();
            if (messageType == BackupEventMessageType) {
                var initialFrame = iterator.Take();
                var sourceInvocationCorrelationId =  DecodeLong(initialFrame, BackupEventSourceInvocationCorrelationIdFieldOffset);
                handleBackupEvent(sourceInvocationCorrelationId);
                return;
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
        }

        public delegate void HandleBackupEvent(long sourceInvocationCorrelationId);
    }
}