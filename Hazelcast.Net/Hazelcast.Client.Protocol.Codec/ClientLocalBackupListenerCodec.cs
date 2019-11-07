// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
        //hex: 0x001300
        public const int RequestMessageType = 4864;
        //hex: 0x001301
        public const int ResponseMessageType = 4865;
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int EventBackupSourceInvocationCorrelationIdFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventBackupInitialFrameSize = EventBackupSourceInvocationCorrelationIdFieldOffset + LongSizeInBytes;
        // hex: 0x001302
        private const int EventBackupMessageType = 4866;

        public class RequestParameters
        {
        }

        public static ClientMessage EncodeRequest()
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Client.LocalBackupListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            clientMessage.Add(initialFrame);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            //empty initial frame
            iterator.Next();
            return request;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// Returns the registration id for the listener.
            ///</summary>
            public Guid Response;
        }

        public static ClientMessage EncodeResponse(Guid response)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            EncodeGuid(initialFrame.Content, ResponseResponseFieldOffset, response);
            return clientMessage;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Response = DecodeGuid(initialFrame.Content, ResponseResponseFieldOffset);
            return response;
        }

        public static ClientMessage EncodeBackupEvent(long sourceInvocationCorrelationId)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[EventBackupInitialFrameSize], UnfragmentedMessage);
            initialFrame.Flags |= IsEventFlag;
            EncodeInt(initialFrame.Content, TypeFieldOffset, EventBackupMessageType);
            EncodeLong(initialFrame.Content, EventBackupSourceInvocationCorrelationIdFieldOffset, sourceInvocationCorrelationId);
            clientMessage.Add(initialFrame);
            return clientMessage;
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