﻿// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Core;
using Hazelcast.Messaging;
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
#if SERVER_CODEC
    internal static class RingbufferReadManyServerCodec
#else
    internal static class RingbufferReadManyCodec
#endif
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

#if SERVER_CODEC
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
#endif

        public static ClientMessage EncodeRequest(string name, long startSequence, int minCount, int maxCount, IData filter)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = true,
                OperationName = "Ringbuffer.ReadMany"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteLongL(RequestStartSequenceFieldOffset, startSequence);
            initialFrame.Bytes.WriteIntL(RequestMinCountFieldOffset, minCount);
            initialFrame.Bytes.WriteIntL(RequestMaxCountFieldOffset, maxCount);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            CodecUtil.EncodeNullable(clientMessage, filter, DataCodec.Encode);
            return clientMessage;
        }

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.StartSequence = initialFrame.Bytes.ReadLongL(RequestStartSequenceFieldOffset);
            request.MinCount = initialFrame.Bytes.ReadIntL(RequestMinCountFieldOffset);
            request.MaxCount = initialFrame.Bytes.ReadIntL(RequestMaxCountFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            request.Filter = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
            return request;
        }
#endif

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

#if SERVER_CODEC
        public static ClientMessage EncodeResponse(int readCount, ICollection<IData> items, long[] itemSeqs, long nextSeq)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            initialFrame.Bytes.WriteIntL(ResponseReadCountFieldOffset, readCount);
            initialFrame.Bytes.WriteLongL(ResponseNextSeqFieldOffset, nextSeq);
            clientMessage.Append(initialFrame);
            ListMultiFrameCodec.Encode(clientMessage, items, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, itemSeqs, LongArrayCodec.Encode);
            return clientMessage;
        }
#endif

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.ReadCount = initialFrame.Bytes.ReadIntL(ResponseReadCountFieldOffset);
            response.NextSeq = initialFrame.Bytes.ReadLongL(ResponseNextSeqFieldOffset);
            response.Items = ListMultiFrameCodec.Decode(iterator, DataCodec.Decode);
            response.ItemSeqs = CodecUtil.DecodeNullable(iterator, LongArrayCodec.Decode);
            return response;
        }

    }
}
