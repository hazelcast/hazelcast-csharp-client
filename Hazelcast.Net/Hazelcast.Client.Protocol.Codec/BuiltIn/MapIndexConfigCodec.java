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
import com.hazelcast.config.MapIndexConfig;
import com.hazelcast.nio.Bits;

import java.util.ListIterator;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeBoolean;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeBoolean;

public final class MapIndexConfigCodec {
    private const int ORDERED_OFFSET = 0;
    private const int InitialFrameSize = ORDERED_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;

    private MapIndexConfigCodec() {
    }

    public static void Encode(ClientMessage clientMessage, MapIndexConfig config) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeBoolean(initialFrame.content, ORDERED_OFFSET, config.isOrdered());
        clientMessage.add(initialFrame);

        StringCodec.Encode(clientMessage, config.getAttribute());

        clientMessage.add(EndFrame);
    }

    public static MapIndexConfig Decode(ref ClientMessage.FrameIterator iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        boolean ordered = DecodeBoolean(initialFrame.content, ORDERED_OFFSET);

        String attribute = StringCodec.Decode(iterator);

        fastForwardToEndFrame(iterator);

        return new MapIndexConfig(attribute, ordered);
    }
}
