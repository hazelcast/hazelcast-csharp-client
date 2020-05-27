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
    /// Creates a transaction with the given parameters.
    ///</summary>
    internal static class TransactionCreateServerCodec
    {
        public const int RequestMessageType = 1376768; // 0x150200
        public const int ResponseMessageType = 1376769; // 0x150201
        private const int RequestTimeoutFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestDurabilityFieldOffset = RequestTimeoutFieldOffset + LongSizeInBytes;
        private const int RequestTransactionTypeFieldOffset = RequestDurabilityFieldOffset + IntSizeInBytes;
        private const int RequestThreadIdFieldOffset = RequestTransactionTypeFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestThreadIdFieldOffset + LongSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;

        public sealed class RequestParameters
        {

            /// <summary>
            /// The maximum allowed duration for the transaction operations.
            ///</summary>
            public long Timeout { get; set; }

            /// <summary>
            /// The durability of the transaction
            ///</summary>
            public int Durability { get; set; }

            /// <summary>
            /// Identifies the type of the transaction. Possible values are:
            /// 1 (Two phase):  The two phase commit is more than the classic two phase commit (if you want a regular
            /// two phase commit, use local). Before it commits, it copies the commit-log to other members, so in
            /// case of member failure, another member can complete the commit.
            /// 2 (Local): Unlike the name suggests, local is a two phase commit. So first all cohorts are asked
            /// to prepare if everyone agrees then all cohorts are asked to commit. The problem happens when during
            /// the commit phase one or more members crash, that the system could be left in an inconsistent state.
            ///</summary>
            public int TransactionType { get; set; }

            /// <summary>
            /// The thread id for the transaction.
            ///</summary>
            public long ThreadId { get; set; }
        }

        public static ClientMessage EncodeRequest(long timeout, int durability, int transactionType, long threadId)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Transaction.Create";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeLong(initialFrame, RequestTimeoutFieldOffset, timeout);
            EncodeInt(initialFrame, RequestDurabilityFieldOffset, durability);
            EncodeInt(initialFrame, RequestTransactionTypeFieldOffset, transactionType);
            EncodeLong(initialFrame, RequestThreadIdFieldOffset, threadId);
            clientMessage.Add(initialFrame);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.Timeout = DecodeLong(initialFrame, RequestTimeoutFieldOffset);
            request.Durability = DecodeInt(initialFrame, RequestDurabilityFieldOffset);
            request.TransactionType = DecodeInt(initialFrame, RequestTransactionTypeFieldOffset);
            request.ThreadId = DecodeLong(initialFrame, RequestThreadIdFieldOffset);
            return request;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// The transaction id for the created transaction.
            ///</summary>
            public Guid Response { get; set; }
        }

        public static ClientMessage EncodeResponse(Guid response)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, ResponseMessageType);
            EncodeGuid(initialFrame, ResponseResponseFieldOffset, response);
            clientMessage.Add(initialFrame);
            return clientMessage;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = DecodeGuid(initialFrame, ResponseResponseFieldOffset);
            return response;
        }


    }
}