// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Messaging;

namespace Hazelcast.Protocol.TempCodecs;

/// <summary>
/// Makes an authentication request to the cluster using custom credentials.
///</summary>
internal static class TpcClientAuthenticationCustomCodec
{
    public const int RequestMessageType = 16581120;
    public const int ResponseMessageType = 16581121;
    private const int RequestUuidFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
    private const int RequestSerializationVersionFieldOffset = RequestUuidFieldOffset + BytesExtensions.SizeOfCodecGuid;
    private const int RequestInitialFrameSize = RequestSerializationVersionFieldOffset + BytesExtensions.SizeOfByte;
    private const int ResponseStatusFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
    private const int ResponseMemberUuidFieldOffset = ResponseStatusFieldOffset + BytesExtensions.SizeOfByte;
    private const int ResponseSerializationVersionFieldOffset = ResponseMemberUuidFieldOffset + BytesExtensions.SizeOfCodecGuid;
    private const int ResponsePartitionCountFieldOffset = ResponseSerializationVersionFieldOffset + BytesExtensions.SizeOfByte;
    private const int ResponseClusterIdFieldOffset = ResponsePartitionCountFieldOffset + BytesExtensions.SizeOfInt;
    private const int ResponseFailoverSupportedFieldOffset = ResponseClusterIdFieldOffset + BytesExtensions.SizeOfCodecGuid;

    public static ClientMessage EncodeRequest(string clusterName, byte[] credentials, Guid uuid, string clientType, byte serializationVersion, string clientHazelcastVersion, string clientName, ICollection<string> labels)
    {
        var clientMessage = new ClientMessage
        {
            IsRetryable = true,
            OperationName = "Client.AuthenticationCustom"
        };
        var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
        initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
        initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
        initialFrame.Bytes.WriteGuidL(RequestUuidFieldOffset, uuid);
        initialFrame.Bytes.WriteByteL(RequestSerializationVersionFieldOffset, serializationVersion);
        clientMessage.Append(initialFrame);
        StringCodec.Encode(clientMessage, clusterName);
        ByteArrayCodec.Encode(clientMessage, credentials);
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
        public Networking.NetworkAddress Address { get; set; }

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
        /// The cluster id of the cluster.
        ///</summary>
        public Guid ClusterId { get; set; }

        /// <summary>
        /// Returns true if server supports clients with failover feature.
        ///</summary>
        public bool FailoverSupported { get; set; }

        /// <summary>
        /// Returns the list of TPC ports or null if TPC is not supported by the server.
        ///</summary>
        public IList<int> TpcPorts { get; set; }

        /// <summary>
        /// Returns the TPC token or null if TPC is not supported by the server.
        /// </summary>
        public byte[] TpcToken { get; set; }
    }

    public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
    {
        using var iterator = clientMessage.GetEnumerator();
        var response = new ResponseParameters();
        var initialFrame = iterator.Take();
        response.Status = initialFrame.Bytes.ReadByteL(ResponseStatusFieldOffset);
        response.MemberUuid = initialFrame.Bytes.ReadGuidL(ResponseMemberUuidFieldOffset);
        response.SerializationVersion = initialFrame.Bytes.ReadByteL(ResponseSerializationVersionFieldOffset);
        response.PartitionCount = initialFrame.Bytes.ReadIntL(ResponsePartitionCountFieldOffset);
        response.ClusterId = initialFrame.Bytes.ReadGuidL(ResponseClusterIdFieldOffset);
        response.FailoverSupported = initialFrame.Bytes.ReadBoolL(ResponseFailoverSupportedFieldOffset);
        response.Address = CodecUtil.DecodeNullable(iterator, AddressCodec.Decode);
        response.ServerHazelcastVersion = StringCodec.Decode(iterator);

        if (iterator.Current != null)
        {
            response.TpcPorts = CodecUtil.DecodeNullable(iterator, ListIntegerCodec.Decode);
            response.TpcToken = CodecUtil.DecodeNullable(iterator, ByteArrayCodec.Decode);
        }

        return response;
    }
}