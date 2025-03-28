﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
//   Hazelcast Client Protocol Code Generator @c89bc95
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
    /// Removes the mapping for a key from this map if existing value equal to the this value
    ///</summary>
#if SERVER_CODEC
    internal static class MapRemoveIfSameServerCodec
#else
    internal static class MapRemoveIfSameCodec
#endif
    {
        public const int RequestMessageType = 67584; // 0x010800
        public const int ResponseMessageType = 67585; // 0x010801
        private const int RequestThreadIdFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestThreadIdFieldOffset + BytesExtensions.SizeOfLong;
        private const int ResponseResponseFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BytesExtensions.SizeOfBool;

#if SERVER_CODEC
        public sealed class RequestParameters
        {

            /// <summary>
            /// Name of the map.
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// Key for the map entry.
            ///</summary>
            public IData Key { get; set; }

            /// <summary>
            /// Test the existing value against this value to find if equal to this value. Only remove the entry from the map if the value is equal to this value.
            ///</summary>
            public IData Value { get; set; }

            /// <summary>
            /// The id of the user thread performing the operation. It is used to guarantee that only the lock holder thread (if a lock exists on the entry) can perform the requested operation.
            ///</summary>
            public long ThreadId { get; set; }
        }
#endif

        public static ClientMessage EncodeRequest(string name, IData key, IData @value, long threadId)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = false,
                OperationName = "Map.RemoveIfSame"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteLongL(RequestThreadIdFieldOffset, threadId);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, key);
            DataCodec.Encode(clientMessage, @value);
            return clientMessage;
        }

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.ThreadId = initialFrame.Bytes.ReadLongL(RequestThreadIdFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            request.Key = DataCodec.Decode(iterator);
            request.Value = DataCodec.Decode(iterator);
            return request;
        }
#endif

        public sealed class ResponseParameters
        {

            /// <summary>
            /// Returns true if the key exists and removed, otherwise returns false.
            ///</summary>
            public bool Response { get; set; }
        }

#if SERVER_CODEC
        public static ClientMessage EncodeResponse(bool response)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            initialFrame.Bytes.WriteBoolL(ResponseResponseFieldOffset, response);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }
#endif

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
