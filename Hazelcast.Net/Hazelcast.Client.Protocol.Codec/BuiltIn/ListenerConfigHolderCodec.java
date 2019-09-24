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
import com.hazelcast.client.impl.protocol.task.dynamicconfig.ListenerConfigHolder;
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

public final class ListenerConfigHolderCodec {
    private const int INCLUDE_VALUE_OFFSET = 0;
    private const int LOCAL_OFFSET = INCLUDE_VALUE_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;
    private const int LISTENER_TYPE_OFFSET = LOCAL_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;
    private const int InitialFrameSize = LISTENER_TYPE_OFFSET + Bits.IntSizeInBytes;

    private ListenerConfigHolderCodec() {
    }

    public static void Encode(ClientMessage clientMessage, ListenerConfigHolder configHolder) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeBoolean(initialFrame.content, INCLUDE_VALUE_OFFSET, configHolder.isIncludeValue());
        EncodeBoolean(initialFrame.content, LOCAL_OFFSET, configHolder.isLocal());
        EncodeInt(initialFrame.content, LISTENER_TYPE_OFFSET, configHolder.getListenerType());
        clientMessage.add(initialFrame);

        EncodeNullable(clientMessage, configHolder.getClassName(), StringCodec::Encode);
        EncodeNullable(clientMessage, configHolder.getListenerImplementation(), DataCodec::Encode);

        clientMessage.add(EndFrame);
    }

    public static ListenerConfigHolder Decode(ref ClientMessage.FrameIterator iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        boolean includeValue = DecodeBoolean(initialFrame.content, INCLUDE_VALUE_OFFSET);
        boolean local = DecodeBoolean(initialFrame.content, LOCAL_OFFSET);
        int listenerType = DecodeInt(initialFrame.content, LISTENER_TYPE_OFFSET);

        String className = DecodeNullable(iterator, StringCodec::Decode);
        Data listenerImplementation = DecodeNullable(iterator, DataCodec::Decode);

        fastForwardToEndFrame(iterator);

        if (className == null) {
            return new ListenerConfigHolder(listenerType, listenerImplementation, includeValue, local);
        } else {
            return new ListenerConfigHolder(listenerType, className, includeValue, local);
        }
    }
}
