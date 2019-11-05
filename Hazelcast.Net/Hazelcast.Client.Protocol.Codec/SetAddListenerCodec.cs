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
    /// Adds an item listener for this collection. Listener will be notified for all collection add/remove events.
    ///</summary>
    internal static class SetAddListenerCodec 
    {
        //hex: 0x060B00
        public const int RequestMessageType = 396032;
        //hex: 0x060B01
        public const int ResponseMessageType = 396033;
        private const int RequestIncludeValueFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestLocalOnlyFieldOffset = RequestIncludeValueFieldOffset + BoolSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BoolSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int EventItemUuidFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventItemEventTypeFieldOffset = EventItemUuidFieldOffset + GuidSizeInBytes;
        private const int EventItemInitialFrameSize = EventItemEventTypeFieldOffset + IntSizeInBytes;
        // hex: 0x060B02
        private const int EventItemMessageType = 396034;

        public class RequestParameters 
        {

            /// <summary>
            /// Name of the Set
            ///</summary>
            public string Name;

            /// <summary>
            /// if set to true, the event shall also include the value.
            ///</summary>
            public bool IncludeValue;

            /// <summary>
            /// if true fires events that originated from this node only, otherwise fires all events
            ///</summary>
            public bool LocalOnly;
        }

        public static ClientMessage EncodeRequest(string name, bool includeValue, bool localOnly) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Set.AddListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeBool(initialFrame.Content, RequestIncludeValueFieldOffset, includeValue);
            EncodeBool(initialFrame.Content, RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.IncludeValue =  DecodeBool(initialFrame.Content, RequestIncludeValueFieldOffset);
            request.LocalOnly =  DecodeBool(initialFrame.Content, RequestLocalOnlyFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// The registration id.
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
    
        public static ClientMessage EncodeItemEvent(IData item, Guid uuid, int eventType) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[EventItemInitialFrameSize], UnfragmentedMessage);
            initialFrame.Flags |= IsEventFlag;
            EncodeInt(initialFrame.Content, TypeFieldOffset, EventItemMessageType);
            EncodeGuid(initialFrame.Content, EventItemUuidFieldOffset, uuid);
            EncodeInt(initialFrame.Content, EventItemEventTypeFieldOffset, eventType);
            clientMessage.Add(initialFrame);
            CodecUtil.EncodeNullable(clientMessage, item, DataCodec.Encode);
            return clientMessage;
        }

        public static class EventHandler 
        {
            public static void HandleEvent(ClientMessage clientMessage, HandleItemEvent handleItemEvent)
            {
                var messageType = clientMessage.MessageType;
                var iterator = clientMessage.GetIterator();
                if (messageType == EventItemMessageType) {
                    var initialFrame = iterator.Next();
                    Guid uuid =  DecodeGuid(initialFrame.Content, EventItemUuidFieldOffset);
                    int eventType =  DecodeInt(initialFrame.Content, EventItemEventTypeFieldOffset);
                    IData item = CodecUtil.DecodeNullable(ref iterator, DataCodec.Decode);
                    handleItemEvent(item, uuid, eventType);
                    return;
                }
                Logger.GetLogger(typeof(EventHandler)).Finest("Unknown message type received on event handler :" + messageType);
            }
        
            public delegate void HandleItemEvent(IData item, Guid uuid, int eventType);
        }
    }
}