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
    /// Inserts the specified element into this queue, waiting up to the specified wait time if necessary for space to
    /// become available.
    ///</summary>
    internal static class TransactionalQueueOfferServerCodec
    {
        public const int RequestMessageType = 1179904; // 0x120100
        public const int ResponseMessageType = 1179905; // 0x120101
        private const int RequestTxnIdFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestThreadIdFieldOffset = RequestTxnIdFieldOffset + BytesExtensions.SizeOfGuid;
        private const int RequestTimeoutFieldOffset = RequestThreadIdFieldOffset + BytesExtensions.SizeOfLong;
        private const int RequestInitialFrameSize = RequestTimeoutFieldOffset + BytesExtensions.SizeOfLong;
        private const int ResponseResponseFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BytesExtensions.SizeOfBool;

        public sealed class RequestParameters
        {

            /// <summary>
            /// Name of the Transcational Queue
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// ID of the transaction
            ///</summary>
            public Guid TxnId { get; set; }

            /// <summary>
            /// The id of the user thread performing the operation. It is used to guarantee that only the lock holder thread (if a lock exists on the entry) can perform the requested operation.
            ///</summary>
            public long ThreadId { get; set; }

            /// <summary>
            /// The element to add
            ///</summary>
            public IData Item { get; set; }

            /// <summary>
            /// How long to wait before giving up, in milliseconds
            ///</summary>
            public long Timeout { get; set; }
        }
    
        public static ClientMessage EncodeRequest(string name, Guid txnId, long threadId, IData item, long timeout)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = false,
                OperationName = "TransactionalQueue.Offer"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteGuidL(RequestTxnIdFieldOffset, txnId);
            initialFrame.Bytes.WriteLongL(RequestThreadIdFieldOffset, threadId);
            initialFrame.Bytes.WriteLongL(RequestTimeoutFieldOffset, timeout);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, item);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.TxnId = initialFrame.Bytes.ReadGuidL(RequestTxnIdFieldOffset);
            request.ThreadId = initialFrame.Bytes.ReadLongL(RequestThreadIdFieldOffset);
            request.Timeout = initialFrame.Bytes.ReadLongL(RequestTimeoutFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            request.Item = DataCodec.Decode(iterator);
            return request;
        }
        
        public sealed class ResponseParameters
        {

            /// <summary>
            /// <tt>true</tt> if successful, or <tt>false</tt> if the specified waiting time elapses before space is available
            ///</summary>
            public bool Response { get; set; }
        }

        public static ClientMessage EncodeResponse(bool response)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            initialFrame.Bytes.WriteBoolL(ResponseResponseFieldOffset, response);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }
    
        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = initialFrame.Bytes.ReadBoolL(ResponseResponseFieldOffset);
            return response;
        }

    
    }
}