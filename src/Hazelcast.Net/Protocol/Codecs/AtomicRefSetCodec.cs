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
//   Hazelcast Client Protocol Code Generator @c31f40c
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
    /// Atomically sets the given value
    ///</summary>
#if SERVER_CODEC
    internal static class AtomicRefSetServerCodec
#else
    internal static class AtomicRefSetCodec
#endif
    {
        public const int RequestMessageType = 656640; // 0x0A0500
        public const int ResponseMessageType = 656641; // 0x0A0501
        private const int RequestReturnOldValueFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestReturnOldValueFieldOffset + BytesExtensions.SizeOfBool;
        private const int ResponseInitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;

#if SERVER_CODEC
        public sealed class RequestParameters
        {

            /// <summary>
            /// CP group id of this IAtomicReference instance.
            ///</summary>
            public Hazelcast.CP.CPGroupId GroupId { get; set; }

            /// <summary>
            /// Name of this IAtomicReference instance.
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// The value to set
            ///</summary>
            public IData NewValue { get; set; }

            /// <summary>
            /// Denotes whether the old value is returned or not
            ///</summary>
            public bool ReturnOldValue { get; set; }
        }
#endif

        public static ClientMessage EncodeRequest(Hazelcast.CP.CPGroupId groupId, string name, IData newValue, bool returnOldValue)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = false,
                OperationName = "AtomicRef.Set"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteBoolL(RequestReturnOldValueFieldOffset, returnOldValue);
            clientMessage.Append(initialFrame);
            RaftGroupIdCodec.Encode(clientMessage, groupId);
            StringCodec.Encode(clientMessage, name);
            CodecUtil.EncodeNullable(clientMessage, newValue, DataCodec.Encode);
            return clientMessage;
        }

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.ReturnOldValue = initialFrame.Bytes.ReadBoolL(RequestReturnOldValueFieldOffset);
            request.GroupId = RaftGroupIdCodec.Decode(iterator);
            request.Name = StringCodec.Decode(iterator);
            request.NewValue = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
            return request;
        }
#endif

        public sealed class ResponseParameters
        {

            /// <summary>
            /// the old value or null, depending on
            /// the {
            ///</summary>
            public IData Response { get; set; }
        }

#if SERVER_CODEC
        public static ClientMessage EncodeResponse(IData response)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            clientMessage.Append(initialFrame);
            CodecUtil.EncodeNullable(clientMessage, response, DataCodec.Encode);
            return clientMessage;
        }
#endif

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            iterator.Take(); // empty initial frame
            response.Response = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
            return response;
        }

    }
}
