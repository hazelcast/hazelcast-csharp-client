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
    /// Removes the specified element from this set if it is present (optional operation).
    /// Returns true if this set contained the element (or equivalently, if this set changed as a result of the call).
    /// (This set will not contain the element once the call returns.)
    ///</summary>
    internal static class SetRemoveCodec 
    {
        //hex: 0x060500
        public const int RequestMessageType = 394496;
        //hex: 0x060501
        public const int ResponseMessageType = 394497;
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BoolSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// Name of the Set
            ///</summary>
            public string Name;

            /// <summary>
            /// Object to be removed from this set, if present
            ///</summary>
            public IData Value;
        }

        public static ClientMessage EncodeRequest(string name, IData value) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Set.Remove";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, value);
            return clientMessage;
        }

        public static SetRemoveCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            //empty initial frame
            iterator.Next();
            request.Name = StringCodec.Decode(ref iterator);
            request.Value = DataCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// True if this set contained the specified element and it is removed successfully
            ///</summary>
            public bool Response;
        }

        public static ClientMessage EncodeResponse(bool response) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            EncodeBool(initialFrame.Content, ResponseResponseFieldOffset, response);
            return clientMessage;
        }

        public static SetRemoveCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Response = DecodeBool(initialFrame.Content, ResponseResponseFieldOffset);
            return response;
        }
    }
}