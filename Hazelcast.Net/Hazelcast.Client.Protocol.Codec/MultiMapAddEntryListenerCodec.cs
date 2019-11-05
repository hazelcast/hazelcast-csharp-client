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
    /// Adds an entry listener for this multimap. The listener will be notified for all multimap add/remove/update/evict events.
    ///</summary>
    internal static class MultiMapAddEntryListenerCodec 
    {
        //hex: 0x020E00
        public const int RequestMessageType = 134656;
        //hex: 0x020E01
        public const int ResponseMessageType = 134657;
        private const int RequestIncludeValueFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestLocalOnlyFieldOffset = RequestIncludeValueFieldOffset + BoolSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BoolSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int EventEntryEventTypeFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventEntryUuidFieldOffset = EventEntryEventTypeFieldOffset + IntSizeInBytes;
        private const int EventEntryNumberOfAffectedEntriesFieldOffset = EventEntryUuidFieldOffset + GuidSizeInBytes;
        private const int EventEntryInitialFrameSize = EventEntryNumberOfAffectedEntriesFieldOffset + IntSizeInBytes;
        // hex: 0x020E02
        private const int EventEntryMessageType = 134658;

        public class RequestParameters 
        {

            /// <summary>
            /// Name of the MultiMap
            ///</summary>
            public string Name;

            /// <summary>
            /// True if EntryEvent should contain the value,false otherwise
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
            clientMessage.OperationName = "MultiMap.AddEntryListener";
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
            /// Returns registration id for the entry listener
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
    
        public static ClientMessage EncodeEntryEvent(IData key, IData value, IData oldValue, IData mergingValue, int eventType, Guid uuid, int numberOfAffectedEntries) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[EventEntryInitialFrameSize], UnfragmentedMessage);
            initialFrame.Flags |= IsEventFlag;
            EncodeInt(initialFrame.Content, TypeFieldOffset, EventEntryMessageType);
            EncodeInt(initialFrame.Content, EventEntryEventTypeFieldOffset, eventType);
            EncodeGuid(initialFrame.Content, EventEntryUuidFieldOffset, uuid);
            EncodeInt(initialFrame.Content, EventEntryNumberOfAffectedEntriesFieldOffset, numberOfAffectedEntries);
            clientMessage.Add(initialFrame);
            CodecUtil.EncodeNullable(clientMessage, key, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, value, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, oldValue, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, mergingValue, DataCodec.Encode);
            return clientMessage;
        }

        public abstract class AbstractEventHandler 
        {
            public void Handle(ClientMessage clientMessage) 
            {
                var messageType = clientMessage.MessageType;
                var iterator = clientMessage.GetIterator();
                if (messageType == EventEntryMessageType) {
                    var initialFrame = iterator.Next();
                    int eventType =  DecodeInt(initialFrame.Content, EventEntryEventTypeFieldOffset);
                    Guid uuid =  DecodeGuid(initialFrame.Content, EventEntryUuidFieldOffset);
                    int numberOfAffectedEntries =  DecodeInt(initialFrame.Content, EventEntryNumberOfAffectedEntriesFieldOffset);
                    IData key = CodecUtil.DecodeNullable(ref iterator, DataCodec.Decode);
                    IData value = CodecUtil.DecodeNullable(ref iterator, DataCodec.Decode);
                    IData oldValue = CodecUtil.DecodeNullable(ref iterator, DataCodec.Decode);
                    IData mergingValue = CodecUtil.DecodeNullable(ref iterator, DataCodec.Decode);
                    HandleEntryEvent(key, value, oldValue, mergingValue, eventType, uuid, numberOfAffectedEntries);
                    return;
                }
                Logger.GetLogger(GetType()).Finest("Unknown message type received on event handler :" + messageType);
            }

            public abstract void HandleEntryEvent(IData key, IData value, IData oldValue, IData mergingValue, int eventType, Guid uuid, int numberOfAffectedEntries);
        }
    }
}