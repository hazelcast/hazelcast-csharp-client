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

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Appends the specified element to the end of this list (optional operation). Lists that support this operation may
    /// place limitations on what elements may be added to this list.  In particular, some lists will refuse to add null
    /// elements, and others will impose restrictions on the type of elements that may be added. List classes should
    /// clearly specify in their documentation any restrictions on what elements may be added.
    ///</summary>
    internal static class ListAddCodec
    {
        public const int RequestMessageType = 328704; // 0x050400
        public const int ResponseMessageType = 328705; // 0x050401
        private const int RequestInitialFrameSize = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int ResponseResponseFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BytesExtensions.SizeOfBool;

        public static ClientMessage EncodeRequest(string name, IData @value)
        {
            var clientMessage = new ClientMessage();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "List.Add";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.PartitionId, -1);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, @value);
            return clientMessage;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// true if this list changed as a result of the call, false otherwise
            ///</summary>
            public bool Response { get; set; }
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = initialFrame.Bytes.ReadBool(ResponseResponseFieldOffset);
            return response;
        }

    }
}

#pragma warning restore IDE0051 // Remove unused private members
