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
    /// Adds an item to the tail of the Ringbuffer. If there is space in the ringbuffer, the call
    /// will return the sequence of the written item. If there is no space, it depends on the overflow policy what happens:
    /// OverflowPolicy OVERWRITE we just overwrite the oldest item in the ringbuffer and we violate the ttl
    /// OverflowPolicy FAIL we return -1. The reason that FAIL exist is to give the opportunity to obey the ttl.
    /// <p/>
    /// This sequence will always be unique for this Ringbuffer instance so it can be used as a unique id generator if you are
    /// publishing items on this Ringbuffer. However you need to take care of correctly determining an initial id when any node
    /// uses the ringbuffer for the first time. The most reliable way to do that is to write a dummy item into the ringbuffer and
    /// use the returned sequence as initial  id. On the reading side, this dummy item should be discard. Please keep in mind that
    /// this id is not the sequence of the item you are about to publish but from a previously published item. So it can't be used
    /// to find that item.
    ///</summary>
    internal static class RingbufferAddCodec
    {
        public const int RequestMessageType = 1508864; // 0x170600
        public const int ResponseMessageType = 1508865; // 0x170601
        private const int RequestOverflowPolicyFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestOverflowPolicyFieldOffset + IntSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + LongSizeInBytes;

        public static ClientMessage EncodeRequest(string name, int overflowPolicy, IData @value)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Ringbuffer.Add";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeInt(initialFrame, RequestOverflowPolicyFieldOffset, overflowPolicy);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            DataCodec.Encode(clientMessage, @value);
            return clientMessage;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// the sequence of the added item, or -1 if the add failed.
            ///</summary>
            public long Response;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = DecodeLong(initialFrame, ResponseResponseFieldOffset);
            return response;
        }

    }
}

#pragma warning restore IDE0051 // Remove unused private members