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
using static Hazelcast.Protocol.Portability;

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
    /// Returns the number of elements in this collection.If this collection contains more than Integer.MAX_VALUE
    /// elements, returns Integer.MAX_VALUE.
    ///</summary>
    internal static class TransactionalQueueSizeCodec
    {
        public const int RequestMessageType = 1180928; // 0x120500
        public const int ResponseMessageType = 1180929; // 0x120501
        private const int RequestTxnIdFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestThreadIdFieldOffset = RequestTxnIdFieldOffset + GuidSizeInBytes;
        private const int RequestInitialFrameSize = RequestThreadIdFieldOffset + LongSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + IntSizeInBytes;

        public static ClientMessage EncodeRequest(string name, Guid txnId, long threadId)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "TransactionalQueue.Size";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeGuid(initialFrame, RequestTxnIdFieldOffset, txnId);
            EncodeLong(initialFrame, RequestThreadIdFieldOffset, threadId);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// The number of elements in this collection
            ///</summary>
            public int Response;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = DecodeInt(initialFrame, ResponseResponseFieldOffset);
            return response;
        }

    }
}

#pragma warning restore IDE0051 // Remove unused private members