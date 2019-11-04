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
    /// Queries the map based on the specified predicate and returns the matching entries.Specified predicate
    /// runs on all members in parallel. The collection is NOT backed by the map, so changes to the map are NOT reflected
    /// in the collection, and vice-versa. This method is always executed by a distributed query, so it may throw a
    /// QueryResultSizeExceededException if query result size limit is configured.
    ///</summary>
    internal static class MapEntriesWithPredicateCodec 
    {
        //hex: 0x012900
        public const int RequestMessageType = 76032;
        //hex: 0x012901
        public const int ResponseMessageType = 76033;
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseBackupAcksFieldOffset + IntSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// name of map
            ///</summary>
            public string Name;

            /// <summary>
            /// specified query criteria.
            ///</summary>
            public IData Predicate;
        }

        public static ClientMessage EncodeRequest(string name, IData predicate) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Map.EntriesWithPredicate";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, predicate);
            return clientMessage;
        }

        public static MapEntriesWithPredicateCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            //empty initial frame
            iterator.Next();
            request.Name = StringCodec.Decode(ref iterator);
            request.Predicate = DataCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// result key-value entry collection of the query.
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

        public static MapEntriesWithPredicateCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
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