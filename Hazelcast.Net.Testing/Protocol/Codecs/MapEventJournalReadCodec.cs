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
    /// Reads from the map event journal in batches. You may specify the start sequence,
    /// the minumum required number of items in the response, the maximum number of items
    /// in the response, a predicate that the events should pass and a projection to
    /// apply to the events in the journal.
    /// If the event journal currently contains less events than {@code minSize}, the
    /// call will wait until it has sufficient items.
    /// The predicate, filter and projection may be {@code null} in which case all elements are returned
    /// and no projection is applied.
    ///</summary>
    internal static class MapEventJournalReadServerCodec
    {
        public const int RequestMessageType = 82432; // 0x014200
        public const int ResponseMessageType = 82433; // 0x014201
        private const int RequestStartSequenceFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestMinSizeFieldOffset = RequestStartSequenceFieldOffset + LongSizeInBytes;
        private const int RequestMaxSizeFieldOffset = RequestMinSizeFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestMaxSizeFieldOffset + IntSizeInBytes;
        private const int ResponseReadCountFieldOffset = ResponseBackupAcksFieldOffset + ByteSizeInBytes;
        private const int ResponseNextSeqFieldOffset = ResponseReadCountFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseNextSeqFieldOffset + LongSizeInBytes;

        public sealed class RequestParameters
        {

            /// <summary>
            /// name of the map
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// the startSequence of the first item to read
            ///</summary>
            public long StartSequence { get; set; }

            /// <summary>
            /// the minimum number of items to read.
            ///</summary>
            public int MinSize { get; set; }

            /// <summary>
            /// the maximum number of items to read.
            ///</summary>
            public int MaxSize { get; set; }

            /// <summary>
            /// the predicate to apply before processing events
            ///</summary>
            public IData Predicate { get; set; }

            /// <summary>
            /// the projection to apply to journal events
            ///</summary>
            public IData Projection { get; set; }
        }
    
        public static ClientMessage EncodeRequest(string name, long startSequence, int minSize, int maxSize, IData predicate, IData projection)
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = true;
            clientMessage.OperationName = "Map.EventJournalRead";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame, TypeFieldOffset, RequestMessageType);
            EncodeInt(initialFrame, PartitionIdFieldOffset, -1);
            EncodeLong(initialFrame, RequestStartSequenceFieldOffset, startSequence);
            EncodeInt(initialFrame, RequestMinSizeFieldOffset, minSize);
            EncodeInt(initialFrame, RequestMaxSizeFieldOffset, maxSize);
            clientMessage.Add(initialFrame);
            StringCodec.Encode(clientMessage, name);
            CodecUtil.EncodeNullable(clientMessage, predicate, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, projection, DataCodec.Encode);
            return clientMessage;
        }

        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.StartSequence = DecodeLong(initialFrame, RequestStartSequenceFieldOffset);
            request.MinSize = DecodeInt(initialFrame, RequestMinSizeFieldOffset);
            request.MaxSize = DecodeInt(initialFrame, RequestMaxSizeFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            request.Predicate = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
            request.Projection = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
            return request;
        }
        
        public sealed class ResponseParameters
        {

            /// <summary>
            /// Number of items that have been read.
            ///</summary>
            public int ReadCount { get; set; }

            /// <summary>
            /// List of items that have been read.
            ///</summary>
            public IList<IData> Items { get; set; }

            /// <summary>
            /// Sequence numbers of items in the event journal.
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