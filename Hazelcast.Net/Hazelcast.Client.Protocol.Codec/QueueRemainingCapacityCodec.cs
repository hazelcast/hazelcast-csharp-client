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
    /// Returns the number of additional elements that this queue can ideally (in the absence of memory or resource
    /// constraints) accept without blocking, or Integer.MAX_VALUE if there is no intrinsic limit. Note that you cannot
    /// always tell if an attempt to insert an element will succeed by inspecting remainingCapacity because it may be
    /// the case that another thread is about to insert or remove an element.
    ///</summary>
    internal static class QueueRemainingCapacityCodec
    {
        //hex: 0x031300
        public const int RequestMessageType = 201472;
        //hex: 0x031301
        public const int ResponseMessageType = 201473;
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + IntSizeInBytes;

        public class RequestParameters
        {

            /// <summary>
            /// Name of the Queue
            ///</summary>
            public string Name;
        }

        public static ClientMessage EncodeRequest(string name)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Queue.RemainingCapacity";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            //empty initial frame
            iterator.Next();
            request.Name = StringCodec.Decode(ref iterator);
            return request;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// The remaining capacity
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

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Response = DecodeInt(initialFrame.Content, ResponseResponseFieldOffset);
            return response;
        }
    }
}