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
    /// Adds all the items of a collection to the tail of the Ringbuffer. A addAll is likely to outperform multiple calls
    /// to add(Object) due to better io utilization and a reduced number of executed operations. If the batch is empty,
    /// the call is ignored. When the collection is not empty, the content is copied into a different data-structure.
    /// This means that: after this call completes, the collection can be re-used. the collection doesn't need to be serializable.
    /// If the collection is larger than the capacity of the ringbuffer, then the items that were written first will be
    /// overwritten. Therefor this call will not block. The items are inserted in the order of the Iterator of the collection.
    /// If an addAll is executed concurrently with an add or addAll, no guarantee is given that items are contiguous.
    /// The result of the future contains the sequenceId of the last written item
    ///</summary>
    internal static class RingbufferAddAllCodec
    {
        public const int RequestMessageType = 1509376; // 0x170800
        public const int ResponseMessageType = 1509377; // 0x170801
        private const int RequestOverflowPolicyFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestOverflowPolicyFieldOffset + IntSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + LongSizeInBytes;

        public static ClientMessage EncodeRequest(string name, ICollection<IData> valueList, int overflowPolicy)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.OperationName = "Ringbuffer.AddAll";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeInt(initialFrame, RequestOverflowPolicyFieldOffset, overflowPolicy);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            ListMultiFrameCodec.Encode(clientMessage, valueList, DataCodec.Encode);
            return clientMessage;
        }

        public class ResponseParameters
        {

            /// <summary>
            /// the CompletionStage to synchronize on completion.
            ///</summary>
            public long Response;
        }

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Response = DecodeLong(initialFrame, ResponseResponseFieldOffset);
            return response;
        }

    }
}

#pragma warning restore IDE0051 // Remove unused private members