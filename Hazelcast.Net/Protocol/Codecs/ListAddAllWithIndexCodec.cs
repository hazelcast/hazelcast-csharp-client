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
using static Hazelcast.Protocol.Portability;

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Inserts all of the elements in the specified collection into this list at the specified position (optional operation).
    /// Shifts the element currently at that position (if any) and any subsequent elements to the right (increases their indices).
    /// The new elements will appear in this list in the order that they are returned by the specified collection's iterator.
    /// The behavior of this operation is undefined if the specified collection is modified while the operation is in progress.
    /// (Note that this will occur if the specified collection is this list, and it's nonempty.)
    ///</summary>
    internal static class ListAddAllWithIndexCodec
    {
        public const int RequestMessageType = 331264; // 0x050E00
        public const int ResponseMessageType = 331265; // 0x050E01
        private const int RequestIndexFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestIndexFieldOffset + IntSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BoolSizeInBytes;

        public static ClientMessage EncodeRequest(string name, int index, ICollection<IData> valueList)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "List.AddAllWithIndex";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeInt(initialFrame, RequestIndexFieldOffset, index);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            ListMultiFrameCodec.Encode(clientMessage, valueList, DataCodec.Encode);
            return clientMessage;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// True if this list changed as a result of the call, false otherwise.
            ///</summary>
            public bool Response;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = DecodeBool(initialFrame, ResponseResponseFieldOffset);
            return response;
        }

    }
}

#pragma warning restore IDE0051 // Remove unused private members