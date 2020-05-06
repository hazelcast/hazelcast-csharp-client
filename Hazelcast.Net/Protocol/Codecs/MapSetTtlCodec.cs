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
using static Hazelcast.Protocol.Portability;

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Updates TTL (time to live) value of the entry specified by {@code key} with a new TTL value.
    /// New TTL value is valid from this operation is invoked, not from the original creation of the entry.
    /// If the entry does not exist or already expired, then this call has no effect.
    /// <p>
    /// The entry will expire and get evicted after the TTL. If the TTL is 0,
    /// then the entry lives forever. If the TTL is negative, then the TTL
    /// from the map configuration will be used (default: forever).
    /// 
    /// If there is no entry with key {@code key}, this call has no effect.
    /// 
    /// <b>Warning:</b>
    /// <p>
    /// Time resolution for TTL is seconds. The given TTL value is rounded to the next closest second value.
    ///</summary>
    internal static class MapSetTtlCodec
    {
        public const int RequestMessageType = 82688; // 0x014300
        public const int ResponseMessageType = 82689; // 0x014301
        private const int RequestTtlFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestTtlFieldOffset + LongSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BoolSizeInBytes;

        public static ClientMessage EncodeRequest(string name, IData key, long ttl)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Map.SetTtl";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeLong(initialFrame, RequestTtlFieldOffset, ttl);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, key);
            return clientMessage;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// 'true' if the entry is affected, 'false' otherwise
            ///</summary>
            public bool Response;
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

#pragma warning restore IDE0051 // Remove unused private members