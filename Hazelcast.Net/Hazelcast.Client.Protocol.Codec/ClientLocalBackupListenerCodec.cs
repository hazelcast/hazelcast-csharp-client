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
using System.Collections;
using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec.BuiltIn;
using Hazelcast.Client.Protocol.Codec.Custom;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using static Hazelcast.Client.Protocol.Codec.BuiltIn.FixedSizeTypesCodec;
using static Hazelcast.Client.Protocol.ClientMessage;
using static Hazelcast.IO.Bits;
using Hazelcast.Logging;

namespace Hazelcast.Client.Protocol.Codec
{
    // This file is auto-generated by the Hazelcast Client Protocol Code Generator.
    // To change this file, edit the templates or the protocol
    // definitions on the https://github.com/hazelcast/hazelcast-client-protocol
    // and regenerate it.

    /// <summary>
    /// Adds listener for backup acks
    ///</summary>
    internal static class ClientLocalBackupListenerCodec
    {
        //hex: 0x000F00
        public const int RequestMessageType = 3840;
        //hex: 0x000F01
        public const int ResponseMessageType = 3841;
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int EventBackupSourceInvocationCorrelationIdFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventBackupInitialFrameSize = EventBackupSourceInvocationCorrelationIdFieldOffset + LongSizeInBytes;
        // hex: 0x000F02
        private const int EventBackupMessageType = 3842;

        public static ClientMessage EncodeRequest()
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Client.LocalBackupListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame.Content, PartitionIdFieldOffset, -1);
            clientMessage.Add(initialFrame);
            return clientMessage;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// Returns the registration id for the listener.
            ///</summary>
            public Guid Response;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Response = DecodeGuid(initialFrame.Content, ResponseResponseFieldOffset);
            return response;
        }

        public static class EventHandler
        {
            public static void HandleEvent(ClientMessage clientMessage, HandleBackupEvent handleBackupEvent)
            {
                var messageType = clientMessage.MessageType;
                var iterator = clientMessage.GetIterator();
                if (messageType == EventBackupMessageType) {
                    var initialFrame = iterator.Next();
                    long sourceInvocationCorrelationId =  DecodeLong(initialFrame.Content, EventBackupSourceInvocationCorrelationIdFieldOffset);
                    handleBackupEvent(sourceInvocationCorrelationId);
                    return;
                }
                Logger.GetLogger(typeof(EventHandler)).Finest("Unknown message type received on event handler :" + messageType);
            }
            public delegate void HandleBackupEvent(long sourceInvocationCorrelationId);
        }
    }
}