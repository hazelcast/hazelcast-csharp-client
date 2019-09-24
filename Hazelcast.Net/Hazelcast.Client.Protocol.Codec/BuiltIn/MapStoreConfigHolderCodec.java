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
import com.hazelcast.client.impl.protocol.task.dynamicconfig.MapStoreConfigHolder;
import com.hazelcast.nio.Bits;
import com.hazelcast.nio.serialization.Data;

import java.util.ListIterator;
import java.util.Map;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.*;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.*;

public final class MapStoreConfigHolderCodec {
    private const int ENABLED_OFFSET = 0;
    private const int WRITE_COALESCING_OFFSET = ENABLED_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;
    private const int WRITE_DELAY_SECONDS_OFFSET = WRITE_COALESCING_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;
    private const int WRITE_BATCH_SIZE_OFFSET = WRITE_DELAY_SECONDS_OFFSET + Bits.IntSizeInBytes;
    private const int InitialFrameSize = WRITE_BATCH_SIZE_OFFSET + Bits.IntSizeInBytes;

    private MapStoreConfigHolderCodec() {
    }

    public static void Encode(ClientMessage clientMessage, MapStoreConfigHolder configHolder) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeBoolean(initialFrame.content, ENABLED_OFFSET, configHolder.isEnabled());
        EncodeBoolean(initialFrame.content, WRITE_COALESCING_OFFSET, configHolder.isWriteCoalescing());
        EncodeInt(initialFrame.content, WRITE_DELAY_SECONDS_OFFSET, configHolder.getWriteDelaySeconds());
        EncodeInt(initialFrame.content, WRITE_BATCH_SIZE_OFFSET, configHolder.getWriteBatchSize());
        clientMessage.add(initialFrame);

        EncodeNullable(clientMessage, configHolder.getClassName(), StringCodec::Encode);
        EncodeNullable(clientMessage, configHolder.getFactoryClassName(), StringCodec::Encode);
        EncodeNullable(clientMessage, configHolder.getImplementation(), DataCodec::Encode);
        EncodeNullable(clientMessage, configHolder.getFactoryImplementation(), DataCodec::Encode);
        MapCodec.EncodeNullable(clientMessage, configHolder.getProperties().entrySet(), StringCodec::Encode, StringCodec::Encode);
        StringCodec.Encode(clientMessage, configHolder.getInitialLoadMode());

        clientMessage.add(EndFrame);
    }

    public static MapStoreConfigHolder Decode(ref ClientMessage.FrameIterator iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        boolean enabled = DecodeBoolean(initialFrame.content, ENABLED_OFFSET);
        boolean writeCoalescing = DecodeBoolean(initialFrame.content, WRITE_COALESCING_OFFSET);
        int writeDelaySeconds = DecodeInt(initialFrame.content, WRITE_DELAY_SECONDS_OFFSET);
        int writeBatchSize = DecodeInt(initialFrame.content, WRITE_BATCH_SIZE_OFFSET);

        String className = DecodeNullable(iterator, StringCodec::Decode);
        String factoryClassName = DecodeNullable(iterator, StringCodec::Decode);
        Data implementation = DecodeNullable(iterator, DataCodec::Decode);
        Data factoryImplementation = DecodeNullable(iterator, DataCodec::Decode);
        Map<String, String> properties = MapCodec.DecodeToNullableMap(iterator, StringCodec::Decode, StringCodec::Decode);
        String initialLoadMode = StringCodec.Decode(iterator);

        fastForwardToEndFrame(iterator);

        MapStoreConfigHolder configHolder = new MapStoreConfigHolder();
        configHolder.setEnabled(enabled);
        configHolder.setWriteCoalescing(writeCoalescing);
        configHolder.setWriteDelaySeconds(writeDelaySeconds);
        configHolder.setWriteBatchSize(writeBatchSize);
        configHolder.setClassName(className);
        configHolder.setFactoryClassName(factoryClassName);
        configHolder.setImplementation(implementation);
        configHolder.setFactoryImplementation(factoryImplementation);
        configHolder.setProperties(properties);
        configHolder.setInitialLoadMode(initialLoadMode);
        return configHolder;
    }
}
