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
    internal static class RingbufferReadManyCodec
    {
        public const int RequestMessageType = 1509632; // 0x170900
        public const int ResponseMessageType = 1509633; // 0x170901
        private const int RequestStartSequenceFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestMinCountFieldOffset = RequestStartSequenceFieldOffset + BytesExtensions.SizeOfLong;
        private const int RequestMaxCountFieldOffset = RequestMinCountFieldOffset + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestMaxCountFieldOffset + BytesExtensions.SizeOfInt;
        private const int ResponseReadCountFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseNextSeqFieldOffset = ResponseReadCountFieldOffset + BytesExtensions.SizeOfInt;
        private const int ResponseInitialFrameSize = ResponseNextSeqFieldOffset + BytesExtensions.SizeOfLong;

        public static ClientMessage EncodeRequest(string name, long startSequence, int minCount, int maxCount, IData filter)
        {
            var clientMessage = new ClientMessage();
            clientMessage.IsRetryable = true;
            clientMessage.OperationName = "Ringbuffer.ReadMany";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteInt(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteLong(RequestStartSequenceFieldOffset, startSequence);
            initialFrame.Bytes.WriteInt(RequestMinCountFieldOffset, minCount);
            initialFrame.Bytes.WriteInt(RequestMaxCountFieldOffset, maxCount);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            CodecUtil.EncodeNullable(clientMessage, filter, DataCodec.Encode);
            return clientMessage;
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

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.ReadCount = initialFrame.Bytes.ReadInt(ResponseReadCountFieldOffset);
            response.NextSeq = initialFrame.Bytes.ReadLong(ResponseNextSeqFieldOffset);
            response.Items = ListMultiFrameCodec.Decode(iterator, DataCodec.Decode);
            response.ItemSeqs = CodecUtil.DecodeNullable(iterator, LongArrayCodec.Decode);
            return response;
        }

    }
}

#pragma warning restore IDE0051 // Remove unused private members