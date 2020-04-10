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
using Microsoft.Extensions.Logging;
using Hazelcast.Protocol.Portability;
using static Hazelcast.Protocol.Portability.Temp;

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
    /// Makes an authentication request to the cluster using custom credentials.
    ///</summary>
    internal static class ClientAuthenticationCustomCodec
    {
        public const int RequestMessageType = 512; // 0x000200
        public const int ResponseMessageType = 513; // 0x000201
        private const int RequestUuidFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestSerializationVersionFieldOffset = RequestUuidFieldOffset + GuidSizeInBytes;
        private const int RequestInitialFrameSize = RequestSerializationVersionFieldOffset + ByteSizeInBytes;
        private const int ResponseStatusFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseMemberUuidFieldOffset = ResponseStatusFieldOffset + ByteSizeInBytes;
        private const int ResponseSerializationVersionFieldOffset = ResponseMemberUuidFieldOffset + GuidSizeInBytes;
        private const int ResponsePartitionCountFieldOffset = ResponseSerializationVersionFieldOffset + ByteSizeInBytes;
        private const int ResponseClusterIdFieldOffset = ResponsePartitionCountFieldOffset + IntSizeInBytes;
        private const int ResponseFailoverSupportedFieldOffset = ResponseClusterIdFieldOffset + GuidSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseFailoverSupportedFieldOffset + BoolSizeInBytes;

        public static ClientMessage EncodeRequest(string clusterName, byte[] credentials, Guid uuid, string clientType, byte serializationVersion, string clientHazelcastVersion, string clientName, ICollection<string> labels)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.OperationName = "Client.AuthenticationCustom";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeGuid(initialFrame, RequestUuidFieldOffset, uuid);
            EncodeByte(initialFrame, RequestSerializationVersionFieldOffset, serializationVersion);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, clusterName);
            ByteArrayCodec.Encode(clientMessage, credentials);
            StringCodec.Encode(clientMessage, clientType);
            StringCodec.Encode(clientMessage, clientHazelcastVersion);
            StringCodec.Encode(clientMessage, clientName);
            ListMultiFrameCodec.Encode(clientMessage, labels, StringCodec.Encode);
            return clientMessage;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// A byte that represents the authentication status. It can be AUTHENTICATED(0), CREDENTIALS_FAILED(1),
            /// SERIALIZATION_VERSION_MISMATCH(2) or NOT_ALLOWED_IN_CLUSTER(3).
            ///</summary>
            public byte Status;

            /// <summary>
            /// Address of the Hazelcast member which sends the authentication response.
            ///</summary>
            public Hazelcast.Networking.NetworkAddress Address;

            /// <summary>
            /// UUID of the Hazelcast member which sends the authentication response.
            ///</summary>
            public Guid MemberUuid;

            /// <summary>
            /// client side supported version to inform server side
            ///</summary>
            public byte SerializationVersion;

            /// <summary>
            /// Version of the Hazelcast member which sends the authentication response.
            ///</summary>
            public string ServerHazelcastVersion;

            /// <summary>
            /// Partition count of the cluster.
            ///</summary>
            public int PartitionCount;

            /// <summary>
            /// The cluster id of the cluster.
            ///</summary>
            public Guid ClusterId;

            /// <summary>
            /// Returns true if server supports clients with failover feature.
            ///</summary>
            public bool FailoverSupported;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Status = DecodeByte(initialFrame, ResponseStatusFieldOffset);
            response.MemberUuid = DecodeGuid(initialFrame, ResponseMemberUuidFieldOffset);
            response.SerializationVersion = DecodeByte(initialFrame, ResponseSerializationVersionFieldOffset);
            response.PartitionCount = DecodeInt(initialFrame, ResponsePartitionCountFieldOffset);
            response.ClusterId = DecodeGuid(initialFrame, ResponseClusterIdFieldOffset);
            response.FailoverSupported = DecodeBool(initialFrame, ResponseFailoverSupportedFieldOffset);
            response.Address = CodecUtil.DecodeNullable(iterator, AddressCodec.Decode);
            response.ServerHazelcastVersion = StringCodec.Decode(iterator);
            return response;
        }

    }
}

#pragma warning restore IDE0051 // Remove unused private members