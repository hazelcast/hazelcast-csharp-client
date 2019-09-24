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
import com.hazelcast.config.CacheSimpleEntryListenerConfig;
import com.hazelcast.nio.Bits;

import java.util.ListIterator;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.DecodeNullable;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.EncodeNullable;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeBoolean;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeBoolean;

public final class CacheSimpleEntryListenerConfigCodec {
    private const int OLD_VALUE_REQUIRED_OFFSET = 0;
    private const int SYNCHRONOUS_OFFSET = OLD_VALUE_REQUIRED_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;
    private const int InitialFrameSize = SYNCHRONOUS_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;

    private CacheSimpleEntryListenerConfigCodec() {
    }

    public static void Encode(ClientMessage clientMessage, CacheSimpleEntryListenerConfig config) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeBoolean(initialFrame.content, OLD_VALUE_REQUIRED_OFFSET, config.isOldValueRequired());
        EncodeBoolean(initialFrame.content, SYNCHRONOUS_OFFSET, config.isSynchronous());
        clientMessage.add(initialFrame);

        EncodeNullable(clientMessage, config.getCacheEntryListenerFactory(), StringCodec::Encode);
        EncodeNullable(clientMessage, config.getCacheEntryEventFilterFactory(), StringCodec::Encode);

        clientMessage.add(EndFrame);
    }

    public static CacheSimpleEntryListenerConfig Decode(ListIterator<ClientMessage.Frame> iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        boolean oldValueRequired = DecodeBoolean(initialFrame.content, OLD_VALUE_REQUIRED_OFFSET);
        boolean synchronous = DecodeBoolean(initialFrame.content, SYNCHRONOUS_OFFSET);

        String cacheEntryListenerFactory = DecodeNullable(iterator, StringCodec::Decode);
        String cacheEntryEventFilterFactory = DecodeNullable(iterator, StringCodec::Decode);

        fastForwardToEndFrame(iterator);

        CacheSimpleEntryListenerConfig config = new CacheSimpleEntryListenerConfig();
        config.setOldValueRequired(oldValueRequired);
        config.setSynchronous(synchronous);
        config.setCacheEntryListenerFactory(cacheEntryListenerFactory);
        config.setCacheEntryEventFilterFactory(cacheEntryEventFilterFactory);
        return config;
    }
}
