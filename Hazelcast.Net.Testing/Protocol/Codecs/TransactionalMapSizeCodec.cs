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
    /// Returns the number of entries in this map.
    ///</summary>
    internal static class TransactionalMapSizeServerCodec
    {
        public const int RequestMessageType = 918528; // 0x0E0400
        public const int ResponseMessageType = 918529; // 0x0E0401
        private const int RequestTxnIdFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestThreadIdFieldOffset = RequestTxnIdFieldOffset + GuidSizeInBytes;
        private const int RequestInitialFrameSize = RequestThreadIdFieldOffset + LongSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + IntSizeInBytes;

        public sealed class RequestParameters
        {

            /// <summary>
            /// Name of the Transactional Map
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// ID of the this transaction operation
            ///</summary>
            public Guid TxnId { get; set; }

            /// <summary>
            /// The id of the user thread performing the operation. It is used to guarantee that only the lock holder thread (if a lock exists on the entry) can perform the requested operation.
            ///</summary>
            public long ThreadId { get; set; }
        }
    
        public static ClientMessage EncodeRequest(string name, Guid txnId, long threadId)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "TransactionalMap.Size";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeGuid(initialFrame, RequestTxnIdFieldOffset, txnId);
            EncodeLong(initialFrame, RequestThreadIdFieldOffset, threadId);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.TxnId = DecodeGuid(initialFrame, RequestTxnIdFieldOffset);
            request.ThreadId = DecodeLong(initialFrame, RequestThreadIdFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            return request;
        }
        
        public sealed class ResponseParameters
        {

            /// <summary>
            /// The number of entries in this map.
            ///</summary>
            public int Response { get; set; }
        }

        public static ClientMessage EncodeResponse(int response)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeInt(initialFrame, ResponseResponseFieldOffset, response);
            clientMessage.Add(initialFrame);
            return clientMessage;
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