// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections;
using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec.BuiltIn;
using Hazelcast.Client.Protocol.Codec.Custom;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using static Hazelcast.Client.Protocol.Codec.BuiltIn.FixedSizeTypesCodec;
using static Hazelcast.Client.Protocol.ClientMessage;
using static Hazelcast.IO.Bits;

namespace Hazelcast.Client.Protocol.Codec
{
    // This file is auto-generated by the Hazelcast Client Protocol Code Generator.
    // To change this file, edit the templates or the protocol
    // definitions on the https://github.com/hazelcast/hazelcast-client-protocol
    // and regenerate it.

    /// <summary>
    /// Creates a session for the caller on the given CP group.
    ///</summary>
    internal static class CPSessionCreateSessionCodec 
    {
        //hex: 0x1F0100
        public const int RequestMessageType = 2031872;
        //hex: 0x1F0101
        public const int ResponseMessageType = 2031873;
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseSessionIdFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseTtlMillisFieldOffset = ResponseSessionIdFieldOffset + LongSizeInBytes;
        private const int ResponseHeartbeatMillisFieldOffset = ResponseTtlMillisFieldOffset + LongSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseHeartbeatMillisFieldOffset + LongSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// ID of the CP group
            ///</summary>
            public Hazelcast.CP.RaftGroupId GroupId;

            /// <summary>
            /// Name of the caller HazelcastInstance
            ///</summary>
            public string EndpointName;
        }

        public static ClientMessage EncodeRequest(Hazelcast.CP.RaftGroupId groupId, string endpointName) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "CPSession.CreateSession";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            clientMessage.Add(initialFrame);
            RaftGroupIdCodec.Encode(clientMessage, groupId);
            StringCodec.Encode(clientMessage, endpointName);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            //empty initial frame
            iterator.Next();
            request.GroupId = RaftGroupIdCodec.Decode(ref iterator);
            request.EndpointName = StringCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// TODO DOC
            ///</summary>
            public long SessionId;

            /// <summary>
            /// TODO DOC
            ///</summary>
            public long TtlMillis;

            /// <summary>
            /// TODO DOC
            ///</summary>
            public long HeartbeatMillis;
        }

        public static ClientMessage EncodeResponse(long sessionId, long ttlMillis, long heartbeatMillis) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            EncodeLong(initialFrame.Content, ResponseSessionIdFieldOffset, sessionId);
            EncodeLong(initialFrame.Content, ResponseTtlMillisFieldOffset, ttlMillis);
            EncodeLong(initialFrame.Content, ResponseHeartbeatMillisFieldOffset, heartbeatMillis);
            return clientMessage;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.SessionId = DecodeLong(initialFrame.Content, ResponseSessionIdFieldOffset);
            response.TtlMillis = DecodeLong(initialFrame.Content, ResponseTtlMillisFieldOffset);
            response.HeartbeatMillis = DecodeLong(initialFrame.Content, ResponseHeartbeatMillisFieldOffset);
            return response;
        }
    }
}