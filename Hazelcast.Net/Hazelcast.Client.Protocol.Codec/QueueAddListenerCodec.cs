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
    /// Adds an listener for this collection. Listener will be notified or all collection add/remove events.
    ///</summary>
    internal static class QueueAddListenerCodec 
    {
        //hex: 0x031100
        public const int RequestMessageType = 200960;
        //hex: 0x031101
        public const int ResponseMessageType = 200961;
        private const int RequestIncludeValueFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestLocalOnlyFieldOffset = RequestIncludeValueFieldOffset + BoolSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BoolSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int EventItemuuidFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventItemeventTypeFieldOffset = EventItemUuidFieldOffset + GuidSizeInBytes;
        private const int EventItemInitialFrameSize = EventItemeventTypeFieldOffset + IntSizeInBytes;
        // hex: 0x031102
        private const int EventItemMessageType = 200962;

        public class RequestParameters 
        {

            /// <summary>
            /// Name of the Queue
            ///</summary>
            public string Name;

            /// <summary>
            /// <tt>true</tt> if the updated item should be passed to the item listener, <tt>false</tt> otherwise.
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
            clientMessage.OperationName = "Queue.AddListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeBool(initialFrame.Content, RequestIncludeValueFieldOffset, includeValue);
            EncodeBool(initialFrame.Content, RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static QueueAddListenerCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
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
            /// The registration id
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

        public static QueueAddListenerCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
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
            EncodeGuid(initialFrame.Content, EventItemuuidFieldOffset, uuid);
            EncodeInt(initialFrame.Content, EventItemeventTypeFieldOffset, eventType);
            clientMessage.Add(initialFrame);
            CodecUtil.EncodeNullable(clientMessage, item, DataCodec.Encode);
            return clientMessage;
        }

        public abstract class AbstractEventHandler 
        {
            public void Handle(ClientMessage clientMessage) 
            {
                var messageType = clientMessage.MessageType;
                var iterator = clientMessage.GetIterator();
                if (messageType == EventItemMessageType) {
                    var initialFrame = iterator.Next();
                    Guid uuid =  DecodeGuid(initialFrame.Content, EventItemuuidFieldOffset);
                    int eventType =  DecodeInt(initialFrame.Content, EventItemeventTypeFieldOffset);
                    IData item = CodecUtil.DecodeNullable(ref iterator, DataCodec.Decode);
                    HandleItemEvent(item, uuid, eventType);
                    return;
                }
                Logger.GetLogger(GetType()).Finest("Unknown message type received on event handler :" + messageType);
            }

            public abstract void HandleItemEvent(IData item, Guid uuid, int eventType);
        }
    }
}