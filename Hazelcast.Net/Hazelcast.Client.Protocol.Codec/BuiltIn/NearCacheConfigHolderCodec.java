/*
 * Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package com.hazelcast.client.impl.protocol.codec.builtin;

import com.hazelcast.client.impl.protocol.ClientMessage;
import com.hazelcast.client.impl.protocol.task.dynamicconfig.EvictionConfigHolder;
import com.hazelcast.client.impl.protocol.task.dynamicconfig.NearCacheConfigHolder;
import com.hazelcast.config.NearCachePreloaderConfig;
import com.hazelcast.nio.Bits;

import java.util.ListIterator;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.DecodeNullable;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.EncodeNullable;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeBoolean;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeInt;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeBoolean;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeInt;

public final class NearCacheConfigHolderCodec {
    private const int SERIALIZE_KEYS_OFFSET = 0;
    private const int INVALIDATE_ON_CHANGE_OFFSET = SERIALIZE_KEYS_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;
    private const int TIME_TO_LIVE_SECONDS_OFFSET = INVALIDATE_ON_CHANGE_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;
    private const int MAX_IDLE_SECONDS_OFFSET = TIME_TO_LIVE_SECONDS_OFFSET + Bits.IntSizeInBytes;
    private const int CACHE_LOCAL_ENTRIES_OFFSET = MAX_IDLE_SECONDS_OFFSET + Bits.IntSizeInBytes;
    private const int InitialFrameSize = CACHE_LOCAL_ENTRIES_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;

    private NearCacheConfigHolderCodec() {
    }

    public static void Encode(ClientMessage clientMessage, NearCacheConfigHolder configHolder) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeBoolean(initialFrame.content, SERIALIZE_KEYS_OFFSET, configHolder.isSerializeKeys());
        EncodeBoolean(initialFrame.content, INVALIDATE_ON_CHANGE_OFFSET, configHolder.isInvalidateOnChange());
        EncodeInt(initialFrame.content, TIME_TO_LIVE_SECONDS_OFFSET, configHolder.getTimeToLiveSeconds());
        EncodeInt(initialFrame.content, MAX_IDLE_SECONDS_OFFSET, configHolder.getMaxIdleSeconds());
        EncodeBoolean(initialFrame.content, CACHE_LOCAL_ENTRIES_OFFSET, configHolder.isCacheLocalEntries());
        clientMessage.add(initialFrame);

        StringCodec.Encode(clientMessage, configHolder.getName());
        StringCodec.Encode(clientMessage, configHolder.getInMemoryFormat());
        EvictionConfigHolderCodec.Encode(clientMessage, configHolder.getEvictionConfigHolder());
        StringCodec.Encode(clientMessage, configHolder.getLocalUpdatePolicy());
        EncodeNullable(clientMessage, configHolder.getPreloaderConfig(), NearCachePreloaderConfigCodec::Encode);

        clientMessage.add(EndFrame);
    }

    public static NearCacheConfigHolder Decode(ref ClientMessage.FrameIterator iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        boolean serializeKeys = DecodeBoolean(initialFrame.content, SERIALIZE_KEYS_OFFSET);
        boolean invalidateOnChange = DecodeBoolean(initialFrame.content, INVALIDATE_ON_CHANGE_OFFSET);
        int timeToLiveSeconds = DecodeInt(initialFrame.content, TIME_TO_LIVE_SECONDS_OFFSET);
        int maxIdleSeconds = DecodeInt(initialFrame.content, MAX_IDLE_SECONDS_OFFSET);
        boolean cacheLocalEntries = DecodeBoolean(initialFrame.content, CACHE_LOCAL_ENTRIES_OFFSET);

        String name = StringCodec.Decode(iterator);
        String inMemoryFormat = StringCodec.Decode(iterator);
        EvictionConfigHolder evictionConfigHolder = EvictionConfigHolderCodec.Decode(iterator);
        String localUpdatePolicy = StringCodec.Decode(iterator);
        NearCachePreloaderConfig preloaderConfig = DecodeNullable(iterator, NearCachePreloaderConfigCodec::Decode);

        fastForwardToEndFrame(iterator);

        return new NearCacheConfigHolder(name, inMemoryFormat, serializeKeys, invalidateOnChange, timeToLiveSeconds,
                maxIdleSeconds, evictionConfigHolder, cacheLocalEntries, localUpdatePolicy, preloaderConfig);
    }
}
