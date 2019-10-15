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

    internal static class NearCacheConfigHolderCodec 
    {
        private const int SerializeKeysFieldOffset = 0;
        private const int InvalidateOnChangeFieldOffset = SerializeKeysFieldOffset + BoolSizeInBytes;
        private const int TimeToLiveSecondsFieldOffset = InvalidateOnChangeFieldOffset + BoolSizeInBytes;
        private const int MaxIdleSecondsFieldOffset = TimeToLiveSecondsFieldOffset + IntSizeInBytes;
        private const int CacheLocalEntriesFieldOffset = MaxIdleSecondsFieldOffset + IntSizeInBytes;
        private const int InitialFrameSize = CacheLocalEntriesFieldOffset + BoolSizeInBytes;

        public static void Encode(ClientMessage clientMessage, com.hazelcast.client.impl.protocol.task.dynamicconfig.NearCacheConfigHolder nearCacheConfigHolder) 
        {
            clientMessage.Add(BeginFrame);

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            EncodeBool(initialFrame.Content, SerializeKeysFieldOffset, nearCacheConfigHolder.IsSerializeKeys);
            EncodeBool(initialFrame.Content, InvalidateOnChangeFieldOffset, nearCacheConfigHolder.IsInvalidateOnChange);
            EncodeInt(initialFrame.Content, TimeToLiveSecondsFieldOffset, nearCacheConfigHolder.TimeToLiveSeconds);
            EncodeInt(initialFrame.Content, MaxIdleSecondsFieldOffset, nearCacheConfigHolder.MaxIdleSeconds);
            EncodeBool(initialFrame.Content, CacheLocalEntriesFieldOffset, nearCacheConfigHolder.IsCacheLocalEntries);
            clientMessage.Add(initialFrame);

            StringCodec.Encode(clientMessage, nearCacheConfigHolder.Name);
            StringCodec.Encode(clientMessage, nearCacheConfigHolder.InMemoryFormat);
            EvictionConfigHolderCodec.Encode(clientMessage, nearCacheConfigHolder.EvictionConfigHolder);
            StringCodec.Encode(clientMessage, nearCacheConfigHolder.LocalUpdatePolicy);
            CodecUtil.EncodeNullable(clientMessage, nearCacheConfigHolder.PreloaderConfig, NearCachePreloaderConfigCodec.Encode);

            clientMessage.Add(EndFrame);
        }

        public static com.hazelcast.client.impl.protocol.task.dynamicconfig.NearCacheConfigHolder Decode(ref FrameIterator iterator) 
        {
            // begin frame
            iterator.Next();

            ref var initialFrame = ref iterator.Next();
            var serializeKeys = DecodeBool(initialFrame.Content, SerializeKeysFieldOffset);
            var invalidateOnChange = DecodeBool(initialFrame.Content, InvalidateOnChangeFieldOffset);
            var timeToLiveSeconds = DecodeInt(initialFrame.Content, TimeToLiveSecondsFieldOffset);
            var maxIdleSeconds = DecodeInt(initialFrame.Content, MaxIdleSecondsFieldOffset);
            var cacheLocalEntries = DecodeBool(initialFrame.Content, CacheLocalEntriesFieldOffset);

            var name = StringCodec.Decode(ref iterator);
            var inMemoryFormat = StringCodec.Decode(ref iterator);
            var evictionConfigHolder = EvictionConfigHolderCodec.Decode(ref iterator);
            var localUpdatePolicy = StringCodec.Decode(ref iterator);
            var preloaderConfig = CodecUtil.DecodeNullable(ref iterator, NearCachePreloaderConfigCodec.Decode);

            CodecUtil.FastForwardToEndFrame(ref iterator);

            return new com.hazelcast.client.impl.protocol.task.dynamicconfig.NearCacheConfigHolder(name, inMemoryFormat, serializeKeys, invalidateOnChange, timeToLiveSeconds, maxIdleSeconds, evictionConfigHolder, cacheLocalEntries, localUpdatePolicy, preloaderConfig);
        }
    }
}