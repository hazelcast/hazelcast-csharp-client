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

using System;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Messaging;
using Hazelcast.Logging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;
using Hazelcast.Protocol.Portability;
using static Hazelcast.Protocol.Portability.Temp;

// <auto-generated>
//   This code was generated by a tool.
//     Hazelcast Client Protocol Code Generator
//     https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Fetches the specified number of entries from the specified partition starting from specified table index
    /// that match the predicate and applies the projection logic on them.
    ///</summary>
    internal static class MapFetchWithQueryCodec
    {
        public const int RequestMessageType = 81920; // 0x014000
        public const int ResponseMessageType = 81921; // 0x014001
        private const int RequestBatchFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestBatchFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseBackupAcksFieldOffset + ByteSizeInBytes;

        public static ClientMessage EncodeRequest(string name, ICollection<KeyValuePair<int, int>> iterationPointers, int batch, IData projection, IData predicate)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.OperationName = "Map.FetchWithQuery";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeInt(initialFrame, RequestBatchFieldOffset, batch);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            EntryListIntegerIntegerCodec.Encode(clientMessage, iterationPointers);
            DataCodec.Encode(clientMessage, projection);
            DataCodec.Encode(clientMessage, predicate);
            return clientMessage;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// List of fetched entries.
            ///</summary>
            public IList<IData> Results;

            /// <summary>
            /// The index-size pairs that define the state of iteration
            ///</summary>
            public IList<KeyValuePair<int, int>> IterationPointers;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Take();
            response.Results = ListMultiFrameCodec.DecodeContainsNullable(iterator, DataCodec.Decode);
            response.IterationPointers = EntryListIntegerIntegerCodec.Decode(iterator);
            return response;
        }

    }
}

#pragma warning restore IDE0051 // Remove unused private members