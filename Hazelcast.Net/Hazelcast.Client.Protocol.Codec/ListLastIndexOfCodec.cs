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
    /// Returns the index of the last occurrence of the specified element in this list, or -1 if this list does not
    /// contain the element.
    ///</summary>
    internal static class ListLastIndexOfCodec 
    {
        //hex: 0x051300
        public const int RequestMessageType = 332544;
        //hex: 0x051301
        public const int ResponseMessageType = 332545;
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + IntSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// Name of the List
            ///</summary>
            public string Name;

            /// <summary>
            /// Element to search for
            ///</summary>
            public IData Value;
        }

        public static ClientMessage EncodeRequest(string name, IData value) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "List.LastIndexOf";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, value);
            return clientMessage;
        }

        public static ListLastIndexOfCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
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
            /// the index of the last occurrence of the specified element in
            /// this list, or -1 if this list does not contain the element
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

        public static ListLastIndexOfCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Response = DecodeInt(initialFrame.Content, ResponseResponseFieldOffset);
            return response;
        }
    }
}