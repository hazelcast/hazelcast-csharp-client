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

import com.hazelcast.cache.CacheEventType;
import com.hazelcast.cache.impl.CacheEventData;
import com.hazelcast.cache.impl.CacheEventDataImpl;
import com.hazelcast.client.impl.protocol.ClientMessage;
import com.hazelcast.nio.Bits;
import com.hazelcast.nio.serialization.Data;

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

public final class CacheEventDataCodec {
    private const int CACHE_EVENT_TYPE_OFFSET = 0;
    private const int IS_OLD_VALUE_AVAILABLE_OFFSET = CACHE_EVENT_TYPE_OFFSET + Bits.IntSizeInBytes;
    private const int InitialFrameSize = IS_OLD_VALUE_AVAILABLE_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;

    private CacheEventDataCodec() {
    }

    public static void Encode(ClientMessage clientMessage, CacheEventData eventData) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeInt(initialFrame.content, CACHE_EVENT_TYPE_OFFSET, eventData.getCacheEventType().getType());
        EncodeBoolean(initialFrame.content, IS_OLD_VALUE_AVAILABLE_OFFSET, eventData.isOldValueAvailable());
        clientMessage.add(initialFrame);

        StringCodec.Encode(clientMessage, eventData.getName());
        EncodeNullable(clientMessage, eventData.getDataKey(), DataCodec::Encode);
        EncodeNullable(clientMessage, eventData.getDataValue(), DataCodec::Encode);
        EncodeNullable(clientMessage, eventData.getDataOldValue(), DataCodec::Encode);

        clientMessage.add(EndFrame);
    }

    public static CacheEventData Decode(ref ClientMessage.FrameIterator iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        int cacheEventType = DecodeInt(initialFrame.content, CACHE_EVENT_TYPE_OFFSET);
        boolean isOldValueAvailable = DecodeBoolean(initialFrame.content, IS_OLD_VALUE_AVAILABLE_OFFSET);

        String name = StringCodec.Decode(iterator);
        Data key = DecodeNullable(iterator, DataCodec::Decode);
        Data value = DecodeNullable(iterator, DataCodec::Decode);
        Data oldValue = DecodeNullable(iterator, DataCodec::Decode);

        fastForwardToEndFrame(iterator);

        return new CacheEventDataImpl(name, CacheEventType.getByType(cacheEventType), key, value, oldValue, isOldValueAvailable);
    }
}
