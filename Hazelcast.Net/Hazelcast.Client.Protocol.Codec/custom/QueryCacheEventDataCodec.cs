// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections;
using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec.BuiltIn;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using static Hazelcast.Client.Protocol.Codec.BuiltIn.FixedSizeTypesCodec;
using static Hazelcast.Client.Protocol.ClientMessage;
using static Hazelcast.IO.Bits;

namespace Hazelcast.Client.Protocol.Codec.Custom
{
    // This file is auto-generated by the Hazelcast Client Protocol Code Generator.
    // To change this file, edit the templates or the protocol
    // definitions on the https://github.com/hazelcast/hazelcast-client-protocol
    // and regenerate it.

    internal static class QueryCacheEventDataCodec 
    {
        private const int SequenceFieldOffset = 0;
        private const int EventTypeFieldOffset = SequenceFieldOffset + LongSizeInBytes;
        private const int PartitionIdFieldOffset = EventTypeFieldOffset + IntSizeInBytes;
        private const int InitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;

        public static void Encode(ClientMessage clientMessage, Hazelcast.Map.QueryCacheEventData queryCacheEventData) 
        {
            clientMessage.Add(BeginFrame);

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            EncodeLong(initialFrame.Content, SequenceFieldOffset, queryCacheEventData.Sequence);
            EncodeInt(initialFrame.Content, EventTypeFieldOffset, queryCacheEventData.EventType);
            EncodeInt(initialFrame.Content, PartitionIdFieldOffset, queryCacheEventData.PartitionId);
            clientMessage.Add(initialFrame);

            CodecUtil.EncodeNullable(clientMessage, queryCacheEventData.DataKey, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, queryCacheEventData.DataNewValue, DataCodec.Encode);

            clientMessage.Add(EndFrame);
        }

        public static Hazelcast.Map.QueryCacheEventData Decode(ref FrameIterator iterator) 
        {
            // begin frame
            iterator.Next();

            ref var initialFrame = ref iterator.Next();
            var sequence = DecodeLong(initialFrame.Content, SequenceFieldOffset);
            var eventType = DecodeInt(initialFrame.Content, EventTypeFieldOffset);
            var partitionId = DecodeInt(initialFrame.Content, PartitionIdFieldOffset);

            var dataKey = CodecUtil.DecodeNullable(ref iterator, DataCodec.Decode);
            var dataNewValue = CodecUtil.DecodeNullable(ref iterator, DataCodec.Decode);

            CodecUtil.FastForwardToEndFrame(ref iterator);

            return CustomTypeFactory.CreateQueryCacheEventData(dataKey, dataNewValue, sequence, eventType, partitionId);
        }
    }
}