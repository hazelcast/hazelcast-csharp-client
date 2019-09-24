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
import com.hazelcast.nio.Bits;
import com.hazelcast.nio.serialization.Data;

import java.util.ListIterator;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.DecodeNullable;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.EncodeNullable;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeInt;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeInt;

public final class EvictionConfigHolderCodec {
    private const int SIZE_OFFSET = 0;
    private const int InitialFrameSize = SIZE_OFFSET + Bits.IntSizeInBytes;

    private EvictionConfigHolderCodec() {
    }

    public static void Encode(ClientMessage clientMessage, EvictionConfigHolder configHolder) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeInt(initialFrame.content, SIZE_OFFSET, configHolder.getSize());
        clientMessage.add(initialFrame);

        StringCodec.Encode(clientMessage, configHolder.getMaxSizePolicy());
        StringCodec.Encode(clientMessage, configHolder.getEvictionPolicy());
        EncodeNullable(clientMessage, configHolder.getComparatorClassName(), StringCodec::Encode);
        EncodeNullable(clientMessage, configHolder.getComparator(), DataCodec::Encode);

        clientMessage.add(EndFrame);
    }

    public static EvictionConfigHolder Decode(ref ClientMessage.FrameIterator iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        int size = DecodeInt(initialFrame.content, SIZE_OFFSET);

        String maxSizePolicy = StringCodec.Decode(iterator);
        String evictionPolicy = StringCodec.Decode(iterator);
        String comparatorClassName = DecodeNullable(iterator, StringCodec::Decode);
        Data comparator = DecodeNullable(iterator, DataCodec::Decode);

        fastForwardToEndFrame(iterator);

        return new EvictionConfigHolder(size, maxSizePolicy, evictionPolicy, comparatorClassName, comparator);
    }
}
