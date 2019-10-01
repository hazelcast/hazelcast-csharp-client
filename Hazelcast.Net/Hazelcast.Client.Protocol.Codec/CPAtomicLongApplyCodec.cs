/*
 * Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections;
using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec.BuiltIn;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using static Hazelcast.Client.Protocol.Codec.BuiltIn.FixedSizeTypesCodec;
using static Hazelcast.Client.Protocol.ClientMessage;
using static Hazelcast.IO.Bits;

namespace Hazelcast.Client.Protocol.Codec
{
    /*
    * This file is auto-generated by the Hazelcast Client Protocol Code Generator.
    * To change this file, edit the templates or the protocol
    * definitions on the https://github.com/hazelcast/hazelcast-client-protocol
    * and regenerate it.
    */

    /// <summary>
    /// Applies a function on the value, the actual stored value will not change
    ///</summary>
    internal static class CPAtomicLongApplyCodec 
    {
        public const int RequestMessageType = 0x230100;
        public const int ResponseMessageType = 0x230101;
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = CorrelationIdFieldOffset + LongSizeInBytes;

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
            /// The function applied to the value and the value is not
            /// changed.
            ///</summary>
            public IData Function;
        }

        public static ClientMessage EncodeRequest(Hazelcast.CP.RaftGroupId groupId, string name, IData function) 
        {
            var clientMessage = ClientMessage.CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "CPAtomicLong.Apply";
            var initialFrame = new ClientMessage.Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            clientMessage.Add(initialFrame);
            RaftGroupIdCodec.Encode(clientMessage, groupId);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, function);
            return clientMessage;
        }

        public static CPAtomicLongApplyCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            //empty initial frame
            iterator.Next();
            request.GroupId = RaftGroupIdCodec.Decode(ref iterator);
            request.Name = StringCodec.Decode(ref iterator);
            request.Function = DataCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

             /// <summary>
            /// The result of the function application.
            ///</summary>
            public IData Response;
        }

        public static ClientMessage EncodeResponse(IData response) 
        {
            var clientMessage = ClientMessage.CreateForEncode();
            var initialFrame = new ClientMessage.Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            CodecUtil.EncodeNullable(clientMessage, response, DataCodec.Encode);
            return clientMessage;
        }

        public static CPAtomicLongApplyCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Next();
            response.Response = CodecUtil.DecodeNullable(ref iterator, DataCodec.Decode);
            return response;
        }
    }
}