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
    /// Returns a view of the portion of this list between the specified from, inclusive, and to, exclusive.(If from and
    /// to are equal, the returned list is empty.) The returned list is backed by this list, so non-structural changes in
    /// the returned list are reflected in this list, and vice-versa. The returned list supports all of the optional list
    /// operations supported by this list.
    /// This method eliminates the need for explicit range operations (of the sort that commonly exist for arrays).
    /// Any operation that expects a list can be used as a range operation by passing a subList view instead of a whole list.
    /// Similar idioms may be constructed for indexOf and lastIndexOf, and all of the algorithms in the Collections class
    /// can be applied to a subList.
    /// The semantics of the list returned by this method become undefined if the backing list (i.e., this list) is
    /// structurally modified in any way other than via the returned list.(Structural modifications are those that change
    /// the size of this list, or otherwise perturb it in such a fashion that iterations in progress may yield incorrect results.)
    ///</summary>
    internal static class ListSubCodec 
    {
        //hex: 0x051500
        public const int RequestMessageType = 333056;
        //hex: 0x051501
        public const int ResponseMessageType = 333057;
        private const int RequestFromFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestToFieldOffset = RequestFromFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestToFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseBackupAcksFieldOffset + IntSizeInBytes;

        public class RequestParameters 
        {

            /// <summary>
            /// Name of the List
            ///</summary>
            public string Name;

            /// <summary>
            /// Low endpoint (inclusive) of the subList
            ///</summary>
            public int From;

            /// <summary>
            /// High endpoint (exclusive) of the subList
            ///</summary>
            public int To;
        }

        public static ClientMessage EncodeRequest(string name, int from, int to) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "List.Sub";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame.Content, RequestFromFieldOffset, from);
            EncodeInt(initialFrame.Content, RequestToFieldOffset, to);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static ListSubCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.From =  DecodeInt(initialFrame.Content, RequestFromFieldOffset);
            request.To =  DecodeInt(initialFrame.Content, RequestToFieldOffset);
            request.Name = StringCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters 
        {

            /// <summary>
            /// A view of the specified range within this list
            ///</summary>
            public IEnumerable<IData> Response;
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

        public static ListSubCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
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