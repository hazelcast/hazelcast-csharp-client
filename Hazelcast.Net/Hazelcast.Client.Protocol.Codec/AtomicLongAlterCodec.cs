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
    /// Alters the currently stored value by applying a function on it.
    ///</summary>
    internal static class AtomicLongAlterCodec 
    {
        //hex: 0x0A0200
        public const int RequestMessageType = 655872;
        //hex: 0x0A0201
        public const int ResponseMessageType = 655873;
        private const int RequestReturnValueTypeFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestReturnValueTypeFieldOffset + IntSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + LongSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// CP group id of this IAtomicLong instance.
            ///</summary>
            public Hazelcast.CP.RaftGroupId GroupId;

            /// <summary>
            /// Name of this IAtomicLong instance.
            ///</summary>
            public string Name;

            /// <summary>
            /// The function applied to the currently stored value.
            ///</summary>
            public IData Function;

            /// <summary>
            /// 0 returns the old value, 1 returns the new value
            ///</summary>
            public int ReturnValueType;
        }

        public static ClientMessage EncodeRequest(Hazelcast.CP.RaftGroupId groupId, string name, IData function, int returnValueType) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "AtomicLong.Alter";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame.Content, RequestReturnValueTypeFieldOffset, returnValueType);
            clientMessage.Add(initialFrame);
            RaftGroupIdCodec.Encode(clientMessage, groupId);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, function);
            return clientMessage;
        }

        public static AtomicLongAlterCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.ReturnValueType =  DecodeInt(initialFrame.Content, RequestReturnValueTypeFieldOffset);
            request.GroupId = RaftGroupIdCodec.Decode(ref iterator);
            request.Name = StringCodec.Decode(ref iterator);
            request.Function = DataCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// TODO DOC
            ///</summary>
            public long Response;
        }

        public static ClientMessage EncodeResponse(long response) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            EncodeLong(initialFrame.Content, ResponseResponseFieldOffset, response);
            return clientMessage;
        }

        public static AtomicLongAlterCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Response = DecodeLong(initialFrame.Content, ResponseResponseFieldOffset);
            return response;
        }
    }
}