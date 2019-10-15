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
    /// Adds an continuous entry listener for this map. The listener will be notified for map add/remove/update/evict
    /// events filtered by the given predicate.
    ///</summary>
    internal static class ReplicatedMapAddEntryListenerToKeyWithPredicateCodec 
    {
        //hex: 0x0E0A00
        public const int RequestMessageType = 920064;
        //hex: 0x0E0A01
        public const int ResponseMessageType = 920065;
        private const int RequestLocalOnlyFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BoolSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int EventEntryeventTypeFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventEntryuuidFieldOffset = EventEntryEventTypeFieldOffset + IntSizeInBytes;
        private const int EventEntrynumberOfAffectedEntriesFieldOffset = EventEntryUuidFieldOffset + GuidSizeInBytes;
        private const int EventEntryInitialFrameSize = EventEntrynumberOfAffectedEntriesFieldOffset + IntSizeInBytes;
        // hex: 0x0E0A02
        private const int EventEntryMessageType = 920066;

        public class RequestParameters 
        {

            /// <summary>
            /// Name of the Replicated Map
            ///</summary>
            public string Name;

            /// <summary>
            /// Key with which the specified value is to be associated.
            ///</summary>
            public IData Key;

            /// <summary>
            /// The predicate for filtering entries
            ///</summary>
            public IData Predicate;

            /// <summary>
            /// if true fires events that originated from this node only, otherwise fires all events
            ///</summary>
            public bool LocalOnly;
        }

        public static ClientMessage EncodeRequest(string name, IData key, IData predicate, bool localOnly) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "ReplicatedMap.AddEntryListenerToKeyWithPredicate";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeBool(initialFrame.Content, RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, key);
            DataCodec.Encode(clientMessage, predicate);
            return clientMessage;
        }

        public static ReplicatedMapAddEntryListenerToKeyWithPredicateCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.LocalOnly =  DecodeBool(initialFrame.Content, RequestLocalOnlyFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            request.Key = DataCodec.Decode(ref iterator);
            request.Predicate = DataCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// A unique string  which is used as a key to remove the listener.
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

        public static ReplicatedMapAddEntryListenerToKeyWithPredicateCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
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
            EncodeInt(initialFrame.Content, EventEntryeventTypeFieldOffset, eventType);
            EncodeGuid(initialFrame.Content, EventEntryuuidFieldOffset, uuid);
            EncodeInt(initialFrame.Content, EventEntrynumberOfAffectedEntriesFieldOffset, numberOfAffectedEntries);
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
                    int eventType =  DecodeInt(initialFrame.Content, EventEntryeventTypeFieldOffset);
                    Guid uuid =  DecodeGuid(initialFrame.Content, EventEntryuuidFieldOffset);
                    int numberOfAffectedEntries =  DecodeInt(initialFrame.Content, EventEntrynumberOfAffectedEntriesFieldOffset);
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