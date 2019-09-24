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
import com.hazelcast.config.EventJournalConfig;
import com.hazelcast.nio.Bits;

import java.util.ListIterator;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeBoolean;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeInt;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeBoolean;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeInt;

public final class EventJournalConfigCodec {

    private const int ENABLED_OFFSET = 0;
    private const int CAPACITY_OFFSET = ENABLED_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;
    private const int TTL_OFFSET = CAPACITY_OFFSET + Bits.IntSizeInBytes;
    private const int InitialFrameSize = TTL_OFFSET + Bits.IntSizeInBytes;

    private EventJournalConfigCodec() {
    }

    public static void Encode(ClientMessage clientMessage, EventJournalConfig eventJournalConfig) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeBoolean(initialFrame.content, ENABLED_OFFSET, eventJournalConfig.isEnabled());
        EncodeInt(initialFrame.content, CAPACITY_OFFSET, eventJournalConfig.getCapacity());
        EncodeInt(initialFrame.content, TTL_OFFSET, eventJournalConfig.getTimeToLiveSeconds());
        clientMessage.add(initialFrame);

        clientMessage.add(EndFrame);

    }

    public static EventJournalConfig Decode(ref ClientMessage.FrameIterator iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        boolean enabled = DecodeBoolean(initialFrame.content, ENABLED_OFFSET);
        int capacity = DecodeInt(initialFrame.content, CAPACITY_OFFSET);
        int ttl = DecodeInt(initialFrame.content, TTL_OFFSET);

        fastForwardToEndFrame(iterator);

        EventJournalConfig eventJournalConfig = new EventJournalConfig();
        eventJournalConfig.setEnabled(enabled);
        eventJournalConfig.setCapacity(capacity);
        eventJournalConfig.setTimeToLiveSeconds(ttl);
        return eventJournalConfig;
    }
}
