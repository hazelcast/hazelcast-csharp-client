﻿// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
//   Hazelcast Client Protocol Code Generator @2e80de297
//   https://github.com/hazelcast/hazelcast-client-protocol
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
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Queries the map based on the specified predicate and returns the keys of matching entries. Specified predicate
    /// runs on all members in parallel. The collection is NOT backed by the map, so changes to the map are NOT reflected
    /// in the collection, and vice-versa. This method is always executed by a distributed query, so it may throw a
    /// QueryResultSizeExceededException if query result size limit is configured.
    ///</summary>
#if SERVER_CODEC
    internal static class MapKeySetWithPagingPredicateServerCodec
#else
    internal static class MapKeySetWithPagingPredicateCodec
#endif
    {
        public const int RequestMessageType = 78848; // 0x013400
        public const int ResponseMessageType = 78849; // 0x013401
        private const int RequestInitialFrameSize = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int ResponseInitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;

#if SERVER_CODEC
        public sealed class RequestParameters
        {

            /// <summary>
            /// name of map
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// specified query criteria.
            ///</summary>
            public Hazelcast.Protocol.Models.PagingPredicateHolder Predicate { get; set; }
        }
#endif

        public static ClientMessage EncodeRequest(string name, Hazelcast.Protocol.Models.PagingPredicateHolder predicate)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = true,
                OperationName = "Map.KeySetWithPagingPredicate"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            PagingPredicateHolderCodec.Encode(clientMessage, predicate);
            return clientMessage;
        }

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            iterator.Take(); // empty initial frame
            request.Name = StringCodec.Decode(iterator);
            request.Predicate = PagingPredicateHolderCodec.Decode(iterator);
            return request;
        }
#endif

        public sealed class ResponseParameters
        {

            /// <summary>
            /// result keys for the query.
            ///</summary>
            public IList<IData> Response { get; set; }

            /// <summary>
            /// The updated anchor list.
            ///</summary>
            public Hazelcast.Protocol.Models.AnchorDataListHolder AnchorDataList { get; set; }
        }

#if SERVER_CODEC
        public static ClientMessage EncodeResponse(ICollection<IData> response, Hazelcast.Protocol.Models.AnchorDataListHolder anchorDataList)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            clientMessage.Append(initialFrame);
            ListMultiFrameCodec.Encode(clientMessage, response, DataCodec.Encode);
            AnchorDataListHolderCodec.Encode(clientMessage, anchorDataList);
            return clientMessage;
        }
#endif

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
