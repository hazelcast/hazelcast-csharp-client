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
    /// Adds listener to cache. This listener will be used to listen near cache invalidation events.
    /// Eventually consistent client near caches should use this method to add invalidation listeners
    /// instead of {@link #addInvalidationListener(String, boolean)}
    ///</summary>
    internal static class CacheAddNearCacheInvalidationListenerCodec 
    {
        //hex: 0x131E00
        public const int RequestMessageType = 1252864;
        //hex: 0x131E01
        public const int ResponseMessageType = 1252865;
        private const int RequestLocalOnlyFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BoolSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int EventCacheInvalidationSourceUuidFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventCacheInvalidationPartitionUuidFieldOffset = EventCacheInvalidationSourceUuidFieldOffset + GuidSizeInBytes;
        private const int EventCacheInvalidationSequenceFieldOffset = EventCacheInvalidationPartitionUuidFieldOffset + GuidSizeInBytes;
        private const int EventCacheInvalidationInitialFrameSize = EventCacheInvalidationSequenceFieldOffset + LongSizeInBytes;
        // hex: 0x131E02
        private const int EventCacheInvalidationMessageType = 1252866;
        private const int EventCacheBatchInvalidationInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        // hex: 0x131E03
        private const int EventCacheBatchInvalidationMessageType = 1252867;

        public class RequestParameters 
        {

            /// <summary>
            /// Name of the cache.
            ///</summary>
            public string Name;

            /// <summary>
            /// if true fires events that originated from this node only, otherwise fires all events
            ///</summary>
            public bool LocalOnly;
        }

        public static ClientMessage EncodeRequest(string name, bool localOnly) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Cache.AddNearCacheInvalidationListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
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
            request.LocalOnly =  DecodeBool(initialFrame.Content, RequestLocalOnlyFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// Registration id for the registered listener.
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
    
        public static ClientMessage EncodeCacheInvalidationEvent(string name, IData key, Guid sourceUuid, Guid partitionUuid, long sequence) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[EventCacheInvalidationInitialFrameSize], UnfragmentedMessage);
            initialFrame.Flags |= IsEventFlag;
            EncodeInt(initialFrame.Content, TypeFieldOffset, EventCacheInvalidationMessageType);
            EncodeGuid(initialFrame.Content, EventCacheInvalidationSourceUuidFieldOffset, sourceUuid);
            EncodeGuid(initialFrame.Content, EventCacheInvalidationPartitionUuidFieldOffset, partitionUuid);
            EncodeLong(initialFrame.Content, EventCacheInvalidationSequenceFieldOffset, sequence);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            CodecUtil.EncodeNullable(clientMessage, key, DataCodec.Encode);
            return clientMessage;
        }
    
        public static ClientMessage EncodeCacheBatchInvalidationEvent(string name, IEnumerable<IData> keys, IEnumerable<Guid> sourceUuids, IEnumerable<Guid> partitionUuids, IEnumerable<long> sequences) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[EventCacheBatchInvalidationInitialFrameSize], UnfragmentedMessage);
            initialFrame.Flags |= IsEventFlag;
            EncodeInt(initialFrame.Content, TypeFieldOffset, EventCacheBatchInvalidationMessageType);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
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
                if (messageType == EventCacheInvalidationMessageType) {
                    var initialFrame = iterator.Next();
                    Guid sourceUuid =  DecodeGuid(initialFrame.Content, EventCacheInvalidationSourceUuidFieldOffset);
                    Guid partitionUuid =  DecodeGuid(initialFrame.Content, EventCacheInvalidationPartitionUuidFieldOffset);
                    long sequence =  DecodeLong(initialFrame.Content, EventCacheInvalidationSequenceFieldOffset);
                    string name = StringCodec.Decode(ref iterator);
                    IData key = CodecUtil.DecodeNullable(ref iterator, DataCodec.Decode);
                    HandleCacheInvalidationEvent(name, key, sourceUuid, partitionUuid, sequence);
                    return;
                }
                if (messageType == EventCacheBatchInvalidationMessageType) {
                    //empty initial frame
                    iterator.Next();
                    string name = StringCodec.Decode(ref iterator);
                    IList<IData> keys = ListMultiFrameCodec.Decode(ref iterator, DataCodec.Decode);
                    IList<Guid> sourceUuids = ListUUIDCodec.Decode(ref iterator);
                    IList<Guid> partitionUuids = ListUUIDCodec.Decode(ref iterator);
                    IList<long> sequences = ListLongCodec.Decode(ref iterator);
                    HandleCacheBatchInvalidationEvent(name, keys, sourceUuids, partitionUuids, sequences);
                    return;
                }
                Logger.GetLogger(GetType()).Finest("Unknown message type received on event handler :" + messageType);
            }

            public abstract void HandleCacheInvalidationEvent(string name, IData key, Guid sourceUuid, Guid partitionUuid, long sequence);

            public abstract void HandleCacheBatchInvalidationEvent(string name, IEnumerable<IData> keys, IEnumerable<Guid> sourceUuids, IEnumerable<Guid> partitionUuids, IEnumerable<long> sequences);
        }
    }
}