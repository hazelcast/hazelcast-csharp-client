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
    /// Acquires all available permits at once and returns immediately.
    ///</summary>
    internal static class SemaphoreDrainCodec 
    {
        //hex: 0x0C0400
        public const int RequestMessageType = 787456;
        //hex: 0x0C0401
        public const int ResponseMessageType = 787457;
        private const int RequestSessionIdFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestThreadIdFieldOffset = RequestSessionIdFieldOffset + LongSizeInBytes;
        private const int RequestInvocationUidFieldOffset = RequestThreadIdFieldOffset + LongSizeInBytes;
        private const int RequestInitialFrameSize = RequestInvocationUidFieldOffset + GuidSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + IntSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// CP group id of this ISemaphore instance
            ///</summary>
            public Hazelcast.CP.RaftGroupId GroupId;

            /// <summary>
            /// Name of this ISemaphore instance
            ///</summary>
            public string Name;

            /// <summary>
            /// Session ID of the caller
            ///</summary>
            public long SessionId;

            /// <summary>
            /// ID of the caller thread
            ///</summary>
            public long ThreadId;

            /// <summary>
            /// UID of this invocation
            ///</summary>
            public Guid InvocationUid;
        }

        public static ClientMessage EncodeRequest(Hazelcast.CP.RaftGroupId groupId, string name, long sessionId, long threadId, Guid invocationUid) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Semaphore.Drain";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeLong(initialFrame.Content, RequestSessionIdFieldOffset, sessionId);
            EncodeLong(initialFrame.Content, RequestThreadIdFieldOffset, threadId);
            EncodeGuid(initialFrame.Content, RequestInvocationUidFieldOffset, invocationUid);
            clientMessage.Add(initialFrame);
            RaftGroupIdCodec.Encode(clientMessage, groupId);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static SemaphoreDrainCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.SessionId =  DecodeLong(initialFrame.Content, RequestSessionIdFieldOffset);
            request.ThreadId =  DecodeLong(initialFrame.Content, RequestThreadIdFieldOffset);
            request.InvocationUid =  DecodeGuid(initialFrame.Content, RequestInvocationUidFieldOffset);
            request.GroupId = RaftGroupIdCodec.Decode(ref iterator);
            request.Name = StringCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// number of acquired permits
            ///</summary>
            public int Response;
        }

        public static ClientMessage EncodeResponse(int response) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            EncodeInt(initialFrame.Content, ResponseResponseFieldOffset, response);
            return clientMessage;
        }

        public static SemaphoreDrainCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Response = DecodeInt(initialFrame.Content, ResponseResponseFieldOffset);
            return response;
        }
    }
}