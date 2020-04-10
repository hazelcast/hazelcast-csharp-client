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

using System;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Messaging;
using Hazelcast.Logging;
using Hazelcast.Clustering;
using Microsoft.Extensions.Logging;
using Hazelcast.Protocol.Portability;
using static Hazelcast.Protocol.Portability.Temp;

// <auto-generated>
//   This code was generated by a tool.
//     Hazelcast Client Protocol Code Generator
//     https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Returns true if this map contains a mapping for the specified key.
    ///</summary>
    internal static class ReplicatedMapContainsKeyCodec
    {
        public const int RequestMessageType = 852992; // 0x0D0400
        public const int ResponseMessageType = 852993; // 0x0D0401
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BoolSizeInBytes;

        public static ClientMessage EncodeRequest(string name, IData key)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.OperationName = "ReplicatedMap.ContainsKey";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, key);
            return clientMessage;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// <tt>True</tt> if this map contains a mapping for the specified key
            ///</summary>
            public bool Response;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Response = DecodeBool(initialFrame, ResponseResponseFieldOffset);
            return response;
        }

    }
}

#pragma warning restore IDE0051 // Remove unused private members