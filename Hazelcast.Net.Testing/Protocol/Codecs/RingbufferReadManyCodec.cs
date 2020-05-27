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
    /// Reads a batch of items from the Ringbuffer. If the number of available items after the first read item is smaller
    /// than the maxCount, these items are returned. So it could be the number of items read is smaller than the maxCount.
    /// If there are less items available than minCount, then this call blacks. Reading a batch of items is likely to
    /// perform better because less overhead is involved. A filter can be provided to only select items that need to be read.
    /// If the filter is null, all items are read. If the filter is not null, only items where the filter function returns
    /// true are returned. Using filters is a good way to prevent getting items that are of no value to the receiver.
    /// This reduces the amount of IO and the number of operations being executed, and can result in a significant performance improvement.
    ///</summary>
    internal static class RingbufferReadManyServerCodec
    {
        public const int RequestMessageType = 1509632; // 0x170900
        public const int ResponseMessageType = 1509633; // 0x170901
        private const int RequestStartSequenceFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestMinCountFieldOffset = RequestStartSequenceFieldOffset + LongSizeInBytes;
        private const int RequestMaxCountFieldOffset = RequestMinCountFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestMaxCountFieldOffset + IntSizeInBytes;
        private const int ResponseReadCountFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseNextSeqFieldOffset = ResponseReadCountFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseNextSeqFieldOffset + LongSizeInBytes;

        public sealed class RequestParameters
        {

            /// <summary>
            /// Name of the Ringbuffer
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// the startSequence of the first item to read
            ///</summary>
            public long StartSequence { get; set; }

            /// <summary>
            /// the minimum number of items to read.
            ///</summary>
            public int MinCount { get; set; }

            /// <summary>
            /// the maximum number of items to read.
            ///</summary>
            public int MaxCount { get; set; }

            /// <summary>
            /// Filter is allowed to be null, indicating there is no filter.
            ///</summary>
            public IData Filter { get; set; }
        }
    
        public static ClientMessage EncodeRequest(string name, long startSequence, int minCount, int maxCount, IData filter)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.OperationName = "Ringbuffer.ReadMany";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeLong(initialFrame, RequestStartSequenceFieldOffset, startSequence);
            EncodeInt(initialFrame, RequestMinCountFieldOffset, minCount);
            EncodeInt(initialFrame, RequestMaxCountFieldOffset, maxCount);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            CodecUtil.EncodeNullable(clientMessage, filter, DataCodec.Encode);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.StartSequence = DecodeLong(initialFrame, RequestStartSequenceFieldOffset);
            request.MinCount = DecodeInt(initialFrame, RequestMinCountFieldOffset);
            request.MaxCount = DecodeInt(initialFrame, RequestMaxCountFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            request.Filter = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
            return request;
        }
        
        public sealed class ResponseParameters
        {

            /// <summary>
            /// Number of items that have been read before filtering.
            ///</summary>
            public int ReadCount { get; set; }

            /// <summary>
            /// List of items that have beee read.
            ///</summary>
            public IList<IData> Items { get; set; }

            /// <summary>
            /// List of sequence numbers for the items that have been read.
            ///</summary>
            public long[] ItemSeqs { get; set; }

            /// <summary>
            /// Sequence number of the item following the last read item.
            ///</summary>
            public long NextSeq { get; set; }
        }

        public static ClientMessage EncodeResponse(int readCount, ICollection<IData> items, long[] itemSeqs, long nextSeq)
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, ResponseMessageType);
            EncodeInt(initialFrame, ResponseReadCountFieldOffset, readCount);
            EncodeLong(initialFrame, ResponseNextSeqFieldOffset, nextSeq);
            clientMessage.Add(initialFrame);
            ListMultiFrameCodec.Encode(clientMessage, items, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, itemSeqs, LongArrayCodec.Encode);
            return clientMessage;
        }
    
        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.ReadCount = DecodeInt(initialFrame, ResponseReadCountFieldOffset);
            response.NextSeq = DecodeLong(initialFrame, ResponseNextSeqFieldOffset);
            response.Items = ListMultiFrameCodec.Decode(iterator, DataCodec.Decode);
            response.ItemSeqs = CodecUtil.DecodeNullable(iterator, LongArrayCodec.Decode);
            return response;
        }

    
    }
}