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
    /// Returns the entries for the given keys. If any keys are not present in the Map, it will call loadAll The returned
    /// map is NOT backed by the original map, so changes to the original map are NOT reflected in the returned map, and vice-versa.
    /// Please note that all the keys in the request should belong to the partition id to which this request is being sent, all keys
    /// matching to a different partition id shall be ignored. The API implementation using this request may need to send multiple
    /// of these request messages for filling a request for a key set if the keys belong to different partitions.
    ///</summary>
    internal static class MapGetAllCodec 
    {
        //hex: 0x012700
        public const int RequestMessageType = 75520;
        //hex: 0x012701
        public const int ResponseMessageType = 75521;
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseBackupAcksFieldOffset + IntSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// name of map
            ///</summary>
            public string Name;

            /// <summary>
            /// keys to get
            ///</summary>
            public IEnumerable<IData> Keys;
        }

        public static ClientMessage EncodeRequest(string name, IEnumerable<IData> keys) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Map.GetAll";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            ListMultiFrameCodec.Encode(clientMessage, keys, DataCodec.Encode);
            return clientMessage;
        }

        public static MapGetAllCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            //empty initial frame
            iterator.Next();
            request.Name = StringCodec.Decode(ref iterator);
            request.Keys = ListMultiFrameCodec.Decode(ref iterator, DataCodec.Decode);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// values for the provided keys.
            ///</summary>
            public IEnumerable<KeyValuePair<IData, IData>> Response;
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

        public static MapGetAllCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
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