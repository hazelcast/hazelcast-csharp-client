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
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Logging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;
using static Hazelcast.Protocol.Portability;

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
    /// Returns the EntryView for the specified key.
    /// This method returns a clone of original mapping, modifying the returned value does not change the actual value
    /// in the map. One should put modified value back to make changes visible to all nodes.
    ///</summary>
    internal static class MapGetEntryViewCodec
    {
        public const int RequestMessageType = 72960; // 0x011D00
        public const int ResponseMessageType = 72961; // 0x011D01
        private const int RequestThreadIdFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestThreadIdFieldOffset + LongSizeInBytes;
        private const int ResponseMaxIdleFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseMaxIdleFieldOffset + LongSizeInBytes;

        public static ClientMessage EncodeRequest(string name, IData key, long threadId)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.OperationName = "Map.GetEntryView";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeLong(initialFrame, RequestThreadIdFieldOffset, threadId);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, key);
            return clientMessage;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// Entry view of the specified key.
            ///</summary>
            public Hazelcast.Data.Map.SimpleEntryView<IData, IData> Response;

            /// <summary>
            /// Last set max idle in millis.
            ///</summary>
            public long MaxIdle;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.MaxIdle = DecodeLong(initialFrame, ResponseMaxIdleFieldOffset);
            response.Response = CodecUtil.DecodeNullable(iterator, SimpleEntryViewCodec.Decode);
            return response;
        }

    }
}

#pragma warning restore IDE0051 // Remove unused private members