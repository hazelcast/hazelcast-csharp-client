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

    internal static class CacheSimpleEntryListenerConfigCodec 
    {
        private const int OldValueRequiredFieldOffset = 0;
        private const int SynchronousFieldOffset = OldValueRequiredFieldOffset + BoolSizeInBytes;
        private const int InitialFrameSize = SynchronousFieldOffset + BoolSizeInBytes;

        public static void Encode(ClientMessage clientMessage, com.hazelcast.config.CacheSimpleEntryListenerConfig cacheSimpleEntryListenerConfig) 
        {
            clientMessage.Add(BeginFrame);

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            EncodeBool(initialFrame.Content, OldValueRequiredFieldOffset, cacheSimpleEntryListenerConfig.IsOldValueRequired);
            EncodeBool(initialFrame.Content, SynchronousFieldOffset, cacheSimpleEntryListenerConfig.IsSynchronous);
            clientMessage.Add(initialFrame);

            CodecUtil.EncodeNullable(clientMessage, cacheSimpleEntryListenerConfig.CacheEntryListenerFactory, StringCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, cacheSimpleEntryListenerConfig.CacheEntryEventFilterFactory, StringCodec.Encode);

            clientMessage.Add(EndFrame);
        }

        public static com.hazelcast.config.CacheSimpleEntryListenerConfig Decode(ref FrameIterator iterator) 
        {
            // begin frame
            iterator.Next();

            ref var initialFrame = ref iterator.Next();
            var oldValueRequired = DecodeBool(initialFrame.Content, OldValueRequiredFieldOffset);
            var synchronous = DecodeBool(initialFrame.Content, SynchronousFieldOffset);

            var cacheEntryListenerFactory = CodecUtil.DecodeNullable(ref iterator, StringCodec.Decode);
            var cacheEntryEventFilterFactory = CodecUtil.DecodeNullable(ref iterator, StringCodec.Decode);

            CodecUtil.FastForwardToEndFrame(ref iterator);

            return CustomTypeFactory.CreateCacheSimpleEntryListenerConfig(oldValueRequired, synchronous, cacheEntryListenerFactory, cacheEntryEventFilterFactory);
        }
    }
}