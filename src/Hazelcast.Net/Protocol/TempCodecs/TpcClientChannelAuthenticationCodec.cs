// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Protocol.BuiltInCodecs;

namespace Hazelcast.Protocol.TempCodecs;

#if SERVER_CODEC
internal static class TpcClientChannelAuthenticationServerCodec
#else
internal static class TpcClientChannelAuthenticationCodec
#endif
{
    public const int RequestMessageType = 16581376;
    public const int ResponseMessageType = 16581377;

    private const int SizeOfTpcToken = 64; // token is 64 bytes

    private const int RequestClientUuidFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
    private const int RequestTokenFieldOffset = RequestClientUuidFieldOffset + BytesExtensions.SizeOfCodecGuid;
    private const int RequestInitialFrameSize = RequestTokenFieldOffset + SizeOfTpcToken;
    private const int ResponseInitialFrameSize = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;

#if SERVER_CODEC
    public sealed class RequestParameters
    {
        public Guid ClientId { get; set; }

        public byte[] Token { get; set; }
    }
#endif

    public static ClientMessage EncodeRequest(Guid clientUuid, byte[] tpcToken)
    {
        if (tpcToken is not { Length: 64 })
            throw new ArgumentException("Invalid token.", nameof(tpcToken));

        var clientMessage = new ClientMessage
        {
            IsRetryable = false,
            OperationName = "Client.TpcAuthentication"
        };

        var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
        initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
        initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
        initialFrame.Bytes.WriteGuidL(RequestClientUuidFieldOffset, clientUuid);
        clientMessage.Append(initialFrame);
        ByteArrayCodec.Encode(clientMessage, tpcToken);

        return clientMessage;
    }


#if SERVER_CODEC
    public static RequestParameters DecodeRequest(ClientMessage clientMessage)
    {
        using var iterator = clientMessage.GetEnumerator();
        var request = new RequestParameters();
        var initialFrame = iterator.Take();
        request.ClientId = initialFrame.Bytes.ReadGuidL(RequestClientUuidFieldOffset);
        request.Token = ByteArrayCodec.Decode(iterator);
        return request;
    }
#endif

    public sealed class ResponseParameters
    { }

#if SERVER_CODEC
    public static ClientMessage EncodeResponse()
    {
        var clientMessage = new ClientMessage();
        var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
        initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
        clientMessage.Append(initialFrame);
        return clientMessage;
    }
#endif

    public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
    {
        return new ResponseParameters(); // response is empty
    }
}
