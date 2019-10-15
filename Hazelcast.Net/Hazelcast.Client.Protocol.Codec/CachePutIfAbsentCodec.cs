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
    /// Associates the specified key with the given value if and only if there is not yet a mapping defined for the
    /// specified key. If the cache is configured for write-through operation mode, the underlying configured
    /// javax.cache.integration.CacheWriter might be called to store the value of the key to any kind of external resource.
    ///</summary>
    internal static class CachePutIfAbsentCodec 
    {
        //hex: 0x151300
        public const int RequestMessageType = 1381120;
        //hex: 0x151301
        public const int ResponseMessageType = 1381121;
        private const int RequestCompletionIdFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestCompletionIdFieldOffset + IntSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BoolSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// Name of the cache.
            ///</summary>
            public string Name;

            /// <summary>
            /// The key that is associated with the specified value.
            ///</summary>
            public IData Key;

            /// <summary>
            /// The value that has the specified key associated with it.
            ///</summary>
            public IData Value;

            /// <summary>
            /// The custom expiry policy for this operation.
            /// A null value is equivalent to put(Object, Object).
            ///</summary>
            public IData ExpiryPolicy;

            /// <summary>
            /// User generated id which shall be received as a field of the cache event upon completion of
            /// the request in the cluster.
            ///</summary>
            public int CompletionId;
        }

        public static ClientMessage EncodeRequest(string name, IData key, IData value, IData expiryPolicy, int completionId) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Cache.PutIfAbsent";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame.Content, RequestCompletionIdFieldOffset, completionId);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, key);
            DataCodec.Encode(clientMessage, value);
            CodecUtil.EncodeNullable(clientMessage, expiryPolicy, DataCodec.Encode);
            return clientMessage;
        }

        public static CachePutIfAbsentCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.CompletionId =  DecodeInt(initialFrame.Content, RequestCompletionIdFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            request.Key = DataCodec.Decode(ref iterator);
            request.Value = DataCodec.Decode(ref iterator);
            request.ExpiryPolicy = CodecUtil.DecodeNullable(ref iterator, DataCodec.Decode);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// true if a value was set, false otherwise.
            ///</summary>
            public bool Response;
        }

        public static ClientMessage EncodeResponse(bool response) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            EncodeBool(initialFrame.Content, ResponseResponseFieldOffset, response);
            return clientMessage;
        }

        public static CachePutIfAbsentCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Response = DecodeBool(initialFrame.Content, ResponseResponseFieldOffset);
            return response;
        }
    }
}