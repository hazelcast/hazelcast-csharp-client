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
    /// Returns a list iterator over the elements in this list (in proper sequence), starting at the specified position
    /// in the list. The specified index indicates the first element that would be returned by an initial call to
    /// ListIterator#next next. An initial call to ListIterator#previous previous would return the element with the
    /// specified index minus one.
    ///</summary>
    internal static class ListListIteratorCodec
    {
        //hex: 0x051700
        public const int RequestMessageType = 333568;
        //hex: 0x051701
        public const int ResponseMessageType = 333569;
        private const int RequestIndexFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestIndexFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseBackupAcksFieldOffset + IntSizeInBytes;

        public class RequestParameters
        {

            /// <summary>
            /// Name of the List
            ///</summary>
            public string Name;

            /// <summary>
            /// index of the first element to be returned from the list iterator next
            ///</summary>
            public int Index;
        }

        public static ClientMessage EncodeRequest(string name, int index)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "List.ListIterator";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame.Content, RequestIndexFieldOffset, index);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.Index =  DecodeInt(initialFrame.Content, RequestIndexFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// a list iterator over the elements in this list (in proper
            /// sequence), starting at the specified position in the list
            ///</summary>
            public IList<IData> Response;
        }

        public static ClientMessage EncodeResponse(IEnumerable<IData> response)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            ListMultiFrameCodec.Encode(clientMessage, response, DataCodec.Encode);
            return clientMessage;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Next();
            response.Response = ListMultiFrameCodec.Decode(ref iterator, DataCodec.Decode);
            return response;
        }
    }
}