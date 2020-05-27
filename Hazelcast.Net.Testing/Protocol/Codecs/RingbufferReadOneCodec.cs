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
    /// Reads one item from the Ringbuffer. If the sequence is one beyond the current tail, this call blocks until an
    /// item is added. This method is not destructive unlike e.g. a queue.take. So the same item can be read by multiple
    /// readers or it can be read multiple times by the same reader. Currently it isn't possible to control how long this
    /// call is going to block. In the future we could add e.g. tryReadOne(long sequence, long timeout, TimeUnit unit).
    ///</summary>
    internal static class RingbufferReadOneServerCodec
    {
        public const int RequestMessageType = 1509120; // 0x170700
        public const int ResponseMessageType = 1509121; // 0x170701
        private const int RequestSequenceFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestSequenceFieldOffset + LongSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseBackupAcksFieldOffset + ByteSizeInBytes;

        public sealed class RequestParameters
        {

            /// <summary>
            /// Name of the Ringbuffer
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// the sequence of the item to read.
            ///</summary>
            public long Sequence { get; set; }
        }
    
        public static ClientMessage EncodeRequest(string name, long sequence)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.OperationName = "Ringbuffer.ReadOne";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeLong(initialFrame, RequestSequenceFieldOffset, sequence);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.Sequence = DecodeLong(initialFrame, RequestSequenceFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            return request;
        }
        
        public sealed class ResponseParameters
        {

            /// <summary>
            /// the read item
            ///</summary>
            public IData Response { get; set; }
        }

        public static ClientMessage EncodeResponse(IData response)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);
            CodecUtil.EncodeNullable(clientMessage, response, DataCodec.Encode);
            return clientMessage;
        }
    
        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Take();
            response.Response = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
            return response;
        }

    
    }
}