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
    /// Adds an entry listener for this map. Listener will get notified for all map add/remove/update/evict events.
    ///</summary>
    internal static class MapAddNearCacheEntryListenerCodec 
    {
        //hex: 0x011A00
        public const int RequestMessageType = 72192;
        //hex: 0x011A01
        public const int ResponseMessageType = 72193;
        private const int RequestListenerFlagsFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestLocalOnlyFieldOffset = RequestListenerFlagsFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BoolSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int EventIMapInvalidationSourceUuidFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventIMapInvalidationPartitionUuidFieldOffset = EventIMapInvalidationSourceUuidFieldOffset + GuidSizeInBytes;
        private const int EventIMapInvalidationSequenceFieldOffset = EventIMapInvalidationPartitionUuidFieldOffset + GuidSizeInBytes;
        private const int EventIMapInvalidationInitialFrameSize = EventIMapInvalidationSequenceFieldOffset + LongSizeInBytes;
        // hex: 0x011A02
        private const int EventIMapInvalidationMessageType = 72194;
        private const int EventIMapBatchInvalidationInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        // hex: 0x011A03
        private const int EventIMapBatchInvalidationMessageType = 72195;

        public class RequestParameters 
        {

            /// <summary>
            /// name of map
            ///</summary>
            public string Name;

            /// <summary>
            /// flags of enabled listeners.
            ///</summary>
            public int ListenerFlags;

            /// <summary>
            /// if true fires events that originated from this node only, otherwise fires all events
            ///</summary>
            public bool LocalOnly;
        }

        public static ClientMessage EncodeRequest(string name, int listenerFlags, bool localOnly) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Map.AddNearCacheEntryListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame.Content, RequestListenerFlagsFieldOffset, listenerFlags);
            EncodeBool(initialFrame.Content, RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static MapAddNearCacheEntryListenerCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.ListenerFlags =  DecodeInt(initialFrame.Content, RequestListenerFlagsFieldOffset);
            request.LocalOnly =  DecodeBool(initialFrame.Content, RequestLocalOnlyFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// A unique string which is used as a key to remove the listener.
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

        public static MapAddNearCacheEntryListenerCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Response = DecodeGuid(initialFrame.Content, ResponseResponseFieldOffset);
            return response;
        }
    
        public static ClientMessage EncodeIMapInvalidationEvent(IData key, Guid sourceUuid, Guid partitionUuid, long sequence) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[EventIMapInvalidationInitialFrameSize], UnfragmentedMessage);
            initialFrame.Flags |= IsEventFlag;
            EncodeInt(initialFrame.Content, TypeFieldOffset, EventIMapInvalidationMessageType);
            EncodeGuid(initialFrame.Content, EventIMapInvalidationSourceUuidFieldOffset, sourceUuid);
            EncodeGuid(initialFrame.Content, EventIMapInvalidationPartitionUuidFieldOffset, partitionUuid);
            EncodeLong(initialFrame.Content, EventIMapInvalidationSequenceFieldOffset, sequence);
            clientMessage.Add(initialFrame);
            CodecUtil.EncodeNullable(clientMessage, key, DataCodec.Encode);
            return clientMessage;
        }
    
        public static ClientMessage EncodeIMapBatchInvalidationEvent(IEnumerable<IData> keys, IEnumerable<Guid> sourceUuids, IEnumerable<Guid> partitionUuids, IEnumerable<long> sequences) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[EventIMapBatchInvalidationInitialFrameSize], UnfragmentedMessage);
            initialFrame.Flags |= IsEventFlag;
            EncodeInt(initialFrame.Content, TypeFieldOffset, EventIMapBatchInvalidationMessageType);
            clientMessage.Add(initialFrame);
            ListMultiFrameCodec.Encode(clientMessage, keys, DataCodec.Encode);
            ListUUIDCodec.Encode(clientMessage, sourceUuids);
            ListUUIDCodec.Encode(clientMessage, partitionUuids);
            ListLongCodec.Encode(clientMessage, sequences);
            return clientMessage;
        }

        public abstract class AbstractEventHandler 
        {
            public void Handle(ClientMessage clientMessage) 
            {
                var messageType = clientMessage.MessageType;
                var iterator = clientMessage.GetIterator();
                if (messageType == EventIMapInvalidationMessageType) {
                    var initialFrame = iterator.Next();
                    Guid sourceUuid =  DecodeGuid(initialFrame.Content, EventIMapInvalidationSourceUuidFieldOffset);
                    Guid partitionUuid =  DecodeGuid(initialFrame.Content, EventIMapInvalidationPartitionUuidFieldOffset);
                    long sequence =  DecodeLong(initialFrame.Content, EventIMapInvalidationSequenceFieldOffset);
                    IData key = CodecUtil.DecodeNullable(ref iterator, DataCodec.Decode);
                    HandleIMapInvalidationEvent(key, sourceUuid, partitionUuid, sequence);
                    return;
                }
                if (messageType == EventIMapBatchInvalidationMessageType) {
                    //empty initial frame
                    iterator.Next();
                    IList<IData> keys = ListMultiFrameCodec.Decode(ref iterator, DataCodec.Decode);
                    IList<Guid> sourceUuids = ListUUIDCodec.Decode(ref iterator);
                    IList<Guid> partitionUuids = ListUUIDCodec.Decode(ref iterator);
                    IList<long> sequences = ListLongCodec.Decode(ref iterator);
                    HandleIMapBatchInvalidationEvent(keys, sourceUuids, partitionUuids, sequences);
                    return;
                }
                Logger.GetLogger(GetType()).Finest("Unknown message type received on event handler :" + messageType);
            }

            public abstract void HandleIMapInvalidationEvent(IData key, Guid sourceUuid, Guid partitionUuid, long sequence);

            public abstract void HandleIMapBatchInvalidationEvent(IEnumerable<IData> keys, IEnumerable<Guid> sourceUuids, IEnumerable<Guid> partitionUuids, IEnumerable<long> sequences);
        }
    }
}