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
import com.hazelcast.config.NearCachePreloaderConfig;
import com.hazelcast.nio.Bits;

import java.util.ListIterator;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeBoolean;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeInt;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeBoolean;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeInt;

public final class NearCachePreloaderConfigCodec {
    private const int ENABLED_OFFSET = 0;
    private const int STORE_INITIAL_DELAY_SECONDS_OFFSET = ENABLED_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;
    private const int STORE_INTERVAL_SECONDS_OFFSET = STORE_INITIAL_DELAY_SECONDS_OFFSET + Bits.IntSizeInBytes;
    private const int InitialFrameSize = STORE_INTERVAL_SECONDS_OFFSET + Bits.IntSizeInBytes;

    private NearCachePreloaderConfigCodec() {
    }

    public static void Encode(ClientMessage clientMessage, NearCachePreloaderConfig config) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeBoolean(initialFrame.content, ENABLED_OFFSET, config.isEnabled());
        EncodeInt(initialFrame.content, STORE_INITIAL_DELAY_SECONDS_OFFSET, config.getStoreInitialDelaySeconds());
        EncodeInt(initialFrame.content, STORE_INTERVAL_SECONDS_OFFSET, config.getStoreIntervalSeconds());
        clientMessage.add(initialFrame);

        StringCodec.Encode(clientMessage, config.getDirectory());

        clientMessage.add(EndFrame);
    }

    public static NearCachePreloaderConfig Decode(ListIterator<ClientMessage.Frame> iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        boolean enabled = DecodeBoolean(initialFrame.content, ENABLED_OFFSET);
        int storeInitialDelaySeconds = DecodeInt(initialFrame.content, STORE_INITIAL_DELAY_SECONDS_OFFSET);
        int storeIntervalSeconds = DecodeInt(initialFrame.content, STORE_INTERVAL_SECONDS_OFFSET);

        String directory = StringCodec.Decode(iterator);

        fastForwardToEndFrame(iterator);

        NearCachePreloaderConfig config = new NearCachePreloaderConfig();
        config.setEnabled(enabled);
        config.setStoreInitialDelaySeconds(storeInitialDelaySeconds);
        config.setStoreIntervalSeconds(storeIntervalSeconds);
        config.setDirectory(directory);
        return config;
    }
}
