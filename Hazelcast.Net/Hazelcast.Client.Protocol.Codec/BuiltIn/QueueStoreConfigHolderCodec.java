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
import com.hazelcast.client.impl.protocol.task.dynamicconfig.QueueStoreConfigHolder;
import com.hazelcast.nio.Bits;
import com.hazelcast.nio.serialization.Data;

import java.util.ListIterator;
import java.util.Map;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.DecodeNullable;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.EncodeNullable;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeBoolean;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeBoolean;

public final class QueueStoreConfigHolderCodec {
    private const int ENABLED_OFFSET = 0;
    private const int InitialFrameSize = ENABLED_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;

    private QueueStoreConfigHolderCodec() {
    }

    public static void Encode(ClientMessage clientMessage, QueueStoreConfigHolder configHolder) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeBoolean(initialFrame.content, ENABLED_OFFSET, configHolder.isEnabled());
        clientMessage.add(initialFrame);

        EncodeNullable(clientMessage, configHolder.getClassName(), StringCodec::Encode);
        EncodeNullable(clientMessage, configHolder.getImplementation(), DataCodec::Encode);
        EncodeNullable(clientMessage, configHolder.getFactoryClassName(), StringCodec::Encode);
        EncodeNullable(clientMessage, configHolder.getFactoryImplementation(), DataCodec::Encode);
        MapCodec.EncodeNullable(clientMessage, configHolder.getProperties().entrySet(), StringCodec::Encode, StringCodec::Encode);

        clientMessage.add(EndFrame);
    }

    public static QueueStoreConfigHolder Decode(ref ClientMessage.FrameIterator iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        boolean enabled = DecodeBoolean(initialFrame.content, ENABLED_OFFSET);

        String className = DecodeNullable(iterator, StringCodec::Decode);
        Data implementation = DecodeNullable(iterator, DataCodec::Decode);
        String factoryClassName = DecodeNullable(iterator, StringCodec::Decode);
        Data factoryImplementation = DecodeNullable(iterator, DataCodec::Decode);
        Map<String, String> properties = MapCodec.DecodeToNullableMap(iterator, StringCodec::Decode, StringCodec::Decode);

        fastForwardToEndFrame(iterator);

        return new QueueStoreConfigHolder(className, factoryClassName, implementation, factoryImplementation, properties, enabled);
    }
}
