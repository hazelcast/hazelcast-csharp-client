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
    /// TODO DOC
    ///</summary>
    internal static class ClientAddDistributedObjectListenerCodec
    {
        //hex: 0x000B00
        public const int RequestMessageType = 2816;
        //hex: 0x000B01
        public const int ResponseMessageType = 2817;
        private const int RequestLocalOnlyFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BoolSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int EventDistributedObjectInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        // hex: 0x000B02
        private const int EventDistributedObjectMessageType = 2818;

        public class RequestParameters
        {

            /// <summary>
            /// If set to true, the server adds the listener only to itself, otherwise the listener is is added for all
            /// members in the cluster.
            ///</summary>
            public bool LocalOnly;
        }

        public static ClientMessage EncodeRequest(bool localOnly)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Client.AddDistributedObjectListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeBool(initialFrame.Content, RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Add(initialFrame);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.LocalOnly =  DecodeBool(initialFrame.Content, RequestLocalOnlyFieldOffset);
            return request;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// The registration id for the distributed object listener.
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

        public static ClientMessage EncodeDistributedObjectEvent(string name, string serviceName, string eventType)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[EventDistributedObjectInitialFrameSize], UnfragmentedMessage);
            initialFrame.Flags |= IsEventFlag;
            EncodeInt(initialFrame.Content, TypeFieldOffset, EventDistributedObjectMessageType);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            StringCodec.Encode(clientMessage, serviceName);
            StringCodec.Encode(clientMessage, eventType);
            return clientMessage;
        }

        public static class EventHandler
        {
            public static void HandleEvent(ClientMessage clientMessage, HandleDistributedObjectEvent handleDistributedObjectEvent)
            {
                var messageType = clientMessage.MessageType;
                var iterator = clientMessage.GetIterator();
                if (messageType == EventDistributedObjectMessageType) {
                    //empty initial frame
                    iterator.Next();
                    string name = StringCodec.Decode(ref iterator);
                    string serviceName = StringCodec.Decode(ref iterator);
                    string eventType = StringCodec.Decode(ref iterator);
                    handleDistributedObjectEvent(name, serviceName, eventType);
                    return;
                }
                Logger.GetLogger(typeof(EventHandler)).Finest("Unknown message type received on event handler :" + messageType);
            }
            public delegate void HandleDistributedObjectEvent(string name, string serviceName, string eventType);
        }
    }
}