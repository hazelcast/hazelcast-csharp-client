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
using static Hazelcast.Client.Protocol.Codec.BuiltIn.FixedSizeTypesCodec;
using static Hazelcast.Client.Protocol.ClientMessage;
using static Hazelcast.IO.Bits;

namespace Hazelcast.Client.Protocol.Codec.Custom
{
    // This file is auto-generated by the Hazelcast Client Protocol Code Generator.
    // To change this file, edit the templates or the protocol
    // definitions on the https://github.com/hazelcast/hazelcast-client-protocol
    // and regenerate it.

    internal static class SimpleEntryViewCodec 
    {
        private const int CostFieldOffset = 0;
        private const int CreationTimeFieldOffset = CostFieldOffset + LongSizeInBytes;
        private const int ExpirationTimeFieldOffset = CreationTimeFieldOffset + LongSizeInBytes;
        private const int HitsFieldOffset = ExpirationTimeFieldOffset + LongSizeInBytes;
        private const int LastAccessTimeFieldOffset = HitsFieldOffset + LongSizeInBytes;
        private const int LastStoredTimeFieldOffset = LastAccessTimeFieldOffset + LongSizeInBytes;
        private const int LastUpdateTimeFieldOffset = LastStoredTimeFieldOffset + LongSizeInBytes;
        private const int VersionFieldOffset = LastUpdateTimeFieldOffset + LongSizeInBytes;
        private const int TtlFieldOffset = VersionFieldOffset + LongSizeInBytes;
        private const int MaxIdleFieldOffset = TtlFieldOffset + LongSizeInBytes;
        private const int InitialFrameSize = MaxIdleFieldOffset + LongSizeInBytes;

        public static void Encode(ClientMessage clientMessage, com.hazelcast.map.impl.SimpleEntryView<IData, IData> simpleEntryView) 
        {
            clientMessage.Add(BeginFrame);

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            EncodeLong(initialFrame.Content, CostFieldOffset, simpleEntryView.Cost);
            EncodeLong(initialFrame.Content, CreationTimeFieldOffset, simpleEntryView.CreationTime);
            EncodeLong(initialFrame.Content, ExpirationTimeFieldOffset, simpleEntryView.ExpirationTime);
            EncodeLong(initialFrame.Content, HitsFieldOffset, simpleEntryView.Hits);
            EncodeLong(initialFrame.Content, LastAccessTimeFieldOffset, simpleEntryView.LastAccessTime);
            EncodeLong(initialFrame.Content, LastStoredTimeFieldOffset, simpleEntryView.LastStoredTime);
            EncodeLong(initialFrame.Content, LastUpdateTimeFieldOffset, simpleEntryView.LastUpdateTime);
            EncodeLong(initialFrame.Content, VersionFieldOffset, simpleEntryView.Version);
            EncodeLong(initialFrame.Content, TtlFieldOffset, simpleEntryView.Ttl);
            EncodeLong(initialFrame.Content, MaxIdleFieldOffset, simpleEntryView.MaxIdle);
            clientMessage.Add(initialFrame);

            DataCodec.Encode(clientMessage, simpleEntryView.Key);
            DataCodec.Encode(clientMessage, simpleEntryView.Value);

            clientMessage.Add(EndFrame);
        }

        public static com.hazelcast.map.impl.SimpleEntryView<IData, IData> Decode(ref FrameIterator iterator) 
        {
            // begin frame
            iterator.Next();

            ref var initialFrame = ref iterator.Next();
            var cost = DecodeLong(initialFrame.Content, CostFieldOffset);
            var creationTime = DecodeLong(initialFrame.Content, CreationTimeFieldOffset);
            var expirationTime = DecodeLong(initialFrame.Content, ExpirationTimeFieldOffset);
            var hits = DecodeLong(initialFrame.Content, HitsFieldOffset);
            var lastAccessTime = DecodeLong(initialFrame.Content, LastAccessTimeFieldOffset);
            var lastStoredTime = DecodeLong(initialFrame.Content, LastStoredTimeFieldOffset);
            var lastUpdateTime = DecodeLong(initialFrame.Content, LastUpdateTimeFieldOffset);
            var version = DecodeLong(initialFrame.Content, VersionFieldOffset);
            var ttl = DecodeLong(initialFrame.Content, TtlFieldOffset);
            var maxIdle = DecodeLong(initialFrame.Content, MaxIdleFieldOffset);

            var key = DataCodec.Decode(ref iterator);
            var value = DataCodec.Decode(ref iterator);

            CodecUtil.FastForwardToEndFrame(ref iterator);

            return CustomTypeFactory.CreateSimpleEntryView(key, value, cost, creationTime, expirationTime, hits, lastAccessTime, lastStoredTime, lastUpdateTime, version, ttl, maxIdle);
        }
    }
}