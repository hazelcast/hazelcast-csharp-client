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

namespace Hazelcast.Client.Protocol.Codec
{
    // This file is auto-generated by the Hazelcast Client Protocol Code Generator.
    // To change this file, edit the templates or the protocol
    // definitions on the https://github.com/hazelcast/hazelcast-client-protocol
    // and regenerate it.

    /// <summary>
    /// Gets a collection of entries from the cache with custom expiry policy, returning them as Map of the values
    /// associated with the set of keys requested. If the cache is configured for read-through operation mode, the underlying
    /// configured javax.cache.integration.CacheLoader might be called to retrieve the values of the keys from any kind
    /// of external resource.
    ///</summary>
    internal static class CacheGetAllCodec 
    {
        //hex: 0x130A00
        public const int RequestMessageType = 1247744;
        //hex: 0x130A01
        public const int ResponseMessageType = 1247745;
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseBackupAcksFieldOffset + IntSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// Name of the cache.
            ///</summary>
            public string Name;

            /// <summary>
            /// The keys whose associated values are to be returned.
            ///</summary>
            public IList<IData> Keys;

            /// <summary>
            /// Expiry policy for the entry. Byte-array which is serialized from an object implementing
            /// javax.cache.expiry.ExpiryPolicy interface.
            ///</summary>
            public IData ExpiryPolicy;
        }

        public static ClientMessage EncodeRequest(string name, IEnumerable<IData> keys, IData expiryPolicy) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Cache.GetAll";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            ListMultiFrameCodec.Encode(clientMessage, keys, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, expiryPolicy, DataCodec.Encode);
            return clientMessage;
        }

        public static CacheGetAllCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            //empty initial frame
            iterator.Next();
            request.Name = StringCodec.Decode(ref iterator);
            request.Keys = ListMultiFrameCodec.Decode(ref iterator, DataCodec.Decode);
            request.ExpiryPolicy = CodecUtil.DecodeNullable(ref iterator, DataCodec.Decode);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// A map of entries that were found for the given keys. Keys not found
            /// in the cache are not in the returned map.
            ///</summary>
            public IList<KeyValuePair<IData, IData>> Response;
        }

        public static ClientMessage EncodeResponse(IEnumerable<KeyValuePair<IData, IData>> response) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            EntryListDataDataCodec.Encode(clientMessage, response);
            return clientMessage;
        }

        public static CacheGetAllCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Next();
            response.Response = EntryListDataDataCodec.Decode(ref iterator);
            return response;
        }
    }
}