// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

// <auto-generated>
//   This code was generated by a tool.
//     Hazelcast Client Protocol Code Generator
//     https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantUsingDirective
// ReSharper disable CheckNamespace

using System;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Logging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;
using static Hazelcast.Messaging.Portability;

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Removes the first occurrence of the specified element from this list, if it is present (optional operation).
    /// If this list does not contain the element, it is unchanged.
    /// Returns true if this list contained the specified element (or equivalently, if this list changed as a result of the call).
    ///</summary>
    internal static class ListRemoveServerCodec
    {
        public const int RequestMessageType = 328960; // 0x050500
        public const int ResponseMessageType = 328961; // 0x050501
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BoolSizeInBytes;

        public sealed class RequestParameters
        {

            /// <summary>
            /// Name of the List
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// Element to be removed from this list, if present
            ///</summary>
            public IData Value { get; set; }
        }

        public static ClientMessage EncodeRequest(string name, IData @value)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "List.Remove";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, @value);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            //empty initial frame
            iterator.Take();
            request.Name = StringCodec.Decode(iterator);
            request.Value = DataCodec.Decode(iterator);
            return request;
        }

        public sealed class ResponseParameters
        {

            /// <summary>
            /// True if this list contained the specified element, false otherwise
            ///</summary>
            public bool Response { get; set; }
        }

        public static ClientMessage EncodeResponse(bool response)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, ResponseMessageType);
            EncodeBool(initialFrame, ResponseResponseFieldOffset, response);
            clientMessage.Add(initialFrame);
            return clientMessage;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = DecodeBool(initialFrame, ResponseResponseFieldOffset);
            return response;
        }


    }
}