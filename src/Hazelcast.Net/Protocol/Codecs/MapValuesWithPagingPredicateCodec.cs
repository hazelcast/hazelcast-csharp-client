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
// ReSharper disable CheckNamespace

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Logging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Queries the map based on the specified predicate and returns the values of matching entries. Specified predicate
    /// runs on all members in parallel. The collection is NOT backed by the map, so changes to the map are NOT reflected
    /// in the collection, and vice-versa. This method is always executed by a distributed query, so it may throw a
    /// QueryResultSizeExceededException if query result size limit is configured.
    ///</summary>
    internal static class MapValuesWithPagingPredicateCodec
    {
        public const int RequestMessageType = 79104; // 0x013500
        public const int ResponseMessageType = 79105; // 0x013501
        private const int RequestInitialFrameSize = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int ResponseInitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;

        public static ClientMessage EncodeRequest(string name, Hazelcast.Protocol.Data.PagingPredicateHolder predicate)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = true,
                OperationName = "Map.ValuesWithPagingPredicate"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            PagingPredicateHolderCodec.Encode(clientMessage, predicate);
            return clientMessage;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// values for the query.
            ///</summary>
            public IList<IData> Response { get; set; }

            /// <summary>
            /// The updated anchor list.
            ///</summary>
            public Hazelcast.Protocol.Data.AnchorDataListHolder AnchorDataList { get; set; }
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            iterator.Take(); // empty initial frame
            response.Response = ListMultiFrameCodec.Decode(iterator, DataCodec.Decode);
            response.AnchorDataList = AnchorDataListHolderCodec.Decode(iterator);
            return response;
        }

    }
}
