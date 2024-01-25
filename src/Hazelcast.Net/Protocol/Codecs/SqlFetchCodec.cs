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
//   Hazelcast Client Protocol Code Generator @0a5719d
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
    /// Fetches the next row page.
    ///</summary>
#if SERVER_CODEC
    internal static class SqlFetchServerCodec
#else
    internal static class SqlFetchCodec
#endif
    {
        public const int RequestMessageType = 2163968; // 0x210500
        public const int ResponseMessageType = 2163969; // 0x210501
        private const int RequestCursorBufferSizeFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestCursorBufferSizeFieldOffset + BytesExtensions.SizeOfInt;
        private const int ResponseInitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;

#if SERVER_CODEC
        public sealed class RequestParameters
        {

            /// <summary>
            /// Query ID.
            ///</summary>
            public Hazelcast.Sql.SqlQueryId QueryId { get; set; }

            /// <summary>
            /// Cursor buffer size.
            ///</summary>
            public int CursorBufferSize { get; set; }
        }
#endif

        public static ClientMessage EncodeRequest(Hazelcast.Sql.SqlQueryId queryId, int cursorBufferSize)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = false,
                OperationName = "Sql.Fetch"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteIntL(RequestCursorBufferSizeFieldOffset, cursorBufferSize);
            clientMessage.Append(initialFrame);
            SqlQueryIdCodec.Encode(clientMessage, queryId);
            return clientMessage;
        }

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.CursorBufferSize = initialFrame.Bytes.ReadIntL(RequestCursorBufferSizeFieldOffset);
            request.QueryId = SqlQueryIdCodec.Decode(iterator);
            return request;
        }
#endif

        public sealed class ResponseParameters
        {

            /// <summary>
            /// Row page.
            ///</summary>
            public Hazelcast.Sql.SqlPage RowPage { get; set; }

            /// <summary>
            /// Error object.
            ///</summary>
            public Hazelcast.Sql.SqlError Error { get; set; }
        }

#if SERVER_CODEC
        public static ClientMessage EncodeResponse(Hazelcast.Sql.SqlPage rowPage, Hazelcast.Sql.SqlError error)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            clientMessage.Append(initialFrame);
            CodecUtil.EncodeNullable(clientMessage, rowPage, SqlPageCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, error, SqlErrorCodec.Encode);
            return clientMessage;
        }
#endif

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            iterator.Take(); // empty initial frame
            response.RowPage = CodecUtil.DecodeNullable(iterator, SqlPageCodec.Decode);
            response.Error = CodecUtil.DecodeNullable(iterator, SqlErrorCodec.Decode);
            return response;
        }

    }
}
