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
    /// Makes an authentication request to the cluster.
    ///</summary>
    internal static class ClientAuthenticationCodec
    {
        public const int RequestMessageType = 256; // 0x000100
        public const int ResponseMessageType = 257; // 0x000101
        private const int RequestUuidFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestSerializationVersionFieldOffset = RequestUuidFieldOffset + BytesExtensions.SizeOfGuid;
        private const int RequestInitialFrameSize = RequestSerializationVersionFieldOffset + BytesExtensions.SizeOfByte;
        private const int ResponseStatusFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseMemberUuidFieldOffset = ResponseStatusFieldOffset + BytesExtensions.SizeOfByte;
        private const int ResponseSerializationVersionFieldOffset = ResponseMemberUuidFieldOffset + BytesExtensions.SizeOfGuid;
        private const int ResponsePartitionCountFieldOffset = ResponseSerializationVersionFieldOffset + BytesExtensions.SizeOfByte;
        private const int ResponseClusterIdFieldOffset = ResponsePartitionCountFieldOffset + BytesExtensions.SizeOfInt;
        private const int ResponseFailoverSupportedFieldOffset = ResponseClusterIdFieldOffset + BytesExtensions.SizeOfGuid;
        private const int ResponseInitialFrameSize = ResponseFailoverSupportedFieldOffset + BytesExtensions.SizeOfBool;

        public static ClientMessage EncodeRequest(string clusterName, string username, string password, Guid uuid, string clientType, byte serializationVersion, string clientHazelcastVersion, string clientName, ICollection<string> labels)
        {
            var clientMessage = new ClientMessage();
            clientMessage.IsRetryable = true;
            clientMessage.OperationName = "Client.Authentication";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteGuid(RequestUuidFieldOffset, uuid);
            initialFrame.Bytes.WriteByte(RequestSerializationVersionFieldOffset, serializationVersion);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, clusterName);
            CodecUtil.EncodeNullable(clientMessage, username, StringCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, password, StringCodec.Encode);
            StringCodec.Encode(clientMessage, clientType);
            StringCodec.Encode(clientMessage, clientHazelcastVersion);
            StringCodec.Encode(clientMessage, clientName);
            ListMultiFrameCodec.Encode(clientMessage, labels, StringCodec.Encode);
            return clientMessage;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// A byte that represents the authentication status. It can be AUTHENTICATED(0), CREDENTIALS_FAILED(1),
            /// SERIALIZATION_VERSION_MISMATCH(2) or NOT_ALLOWED_IN_CLUSTER(3).
            ///</summary>
            public byte Status { get; set; }

            /// <summary>
            /// Address of the Hazelcast member which sends the authentication response.
            ///</summary>
            public Hazelcast.Networking.NetworkAddress Address { get; set; }

            /// <summary>
            /// UUID of the Hazelcast member which sends the authentication response.
            ///</summary>
            public Guid MemberUuid { get; set; }

            /// <summary>
            /// client side supported version to inform server side
            ///</summary>
            public byte SerializationVersion { get; set; }

            /// <summary>
            /// Version of the Hazelcast member which sends the authentication response.
            ///</summary>
            public string ServerHazelcastVersion { get; set; }

            /// <summary>
            /// Partition count of the cluster.
            ///</summary>
            public int PartitionCount { get; set; }

            /// <summary>
            /// UUID of the cluster that the client authenticated.
            ///</summary>
            public Guid ClusterId { get; set; }

            /// <summary>
            /// Returns true if server supports clients with failover feature.
            ///</summary>
            public bool FailoverSupported { get; set; }
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Status = initialFrame.Bytes.ReadByte(ResponseStatusFieldOffset);
            response.MemberUuid = initialFrame.Bytes.ReadGuid(ResponseMemberUuidFieldOffset);
            response.SerializationVersion = initialFrame.Bytes.ReadByte(ResponseSerializationVersionFieldOffset);
            response.PartitionCount = initialFrame.Bytes.ReadInt(ResponsePartitionCountFieldOffset);
            response.ClusterId = initialFrame.Bytes.ReadGuid(ResponseClusterIdFieldOffset);
            response.FailoverSupported = initialFrame.Bytes.ReadBool(ResponseFailoverSupportedFieldOffset);
            response.Address = CodecUtil.DecodeNullable(iterator, AddressCodec.Decode);
            response.ServerHazelcastVersion = StringCodec.Decode(iterator);
            return response;
        }

    }
}

#pragma warning restore IDE0051 // Remove unused private members
