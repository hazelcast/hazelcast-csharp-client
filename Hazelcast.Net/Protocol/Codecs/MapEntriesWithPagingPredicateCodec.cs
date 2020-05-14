// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

// <auto-generated>
//   This code was generated by a tool.
//     Hazelcast Client Protocol Code Generator
//     https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Logging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;
using static Hazelcast.Messaging.Portability;

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Queries the map based on the specified predicate and returns the matching entries. Specified predicate
    /// runs on all members in parallel. The collection is NOT backed by the map, so changes to the map are NOT reflected
    /// in the collection, and vice-versa. This method is always executed by a distributed query, so it may throw a
    /// QueryResultSizeExceededException if query result size limit is configured.
    ///</summary>
    internal static class MapEntriesWithPagingPredicateCodec
    {
        public const int RequestMessageType = 79360; // 0x013600
        public const int ResponseMessageType = 79361; // 0x013601
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseBackupAcksFieldOffset + ByteSizeInBytes;

        public static ClientMessage EncodeRequest(string name, Hazelcast.Protocol.Data.PagingPredicateHolder predicate)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.OperationName = "Map.EntriesWithPagingPredicate";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            PagingPredicateHolderCodec.Encode(clientMessage, predicate);
            return clientMessage;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// key-value pairs for the query.
            ///</summary>
            public IList<KeyValuePair<IData, IData>> Response { get; set; }

            /// <summary>
            /// The updated anchor list.
            ///</summary>
            public Hazelcast.Protocol.Data.AnchorDataListHolder AnchorDataList { get; set; }
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Take();
            response.Response = EntryListCodec.Decode(iterator, DataCodec.Decode, DataCodec.Decode);
            response.AnchorDataList = AnchorDataListHolderCodec.Decode(iterator);
            return response;
        }

    }
}

#pragma warning restore IDE0051 // Remove unused private members