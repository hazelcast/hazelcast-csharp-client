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
import com.hazelcast.map.impl.SimpleEntryView;
import com.hazelcast.nio.Bits;
import com.hazelcast.nio.serialization.Data;

import java.util.ListIterator;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeLong;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeLong;

public final class SimpleEntryViewCodec {
    private const int COST_OFFSET = 0;
    private const int CREATION_TIME_OFFSET = COST_OFFSET + Bits.LONG_SIZE_IN_BYTES;
    private const int EXPIRATION_TIME_OFFSET = CREATION_TIME_OFFSET + Bits.LONG_SIZE_IN_BYTES;
    private const int HITS_OFFSET = EXPIRATION_TIME_OFFSET + Bits.LONG_SIZE_IN_BYTES;
    private const int LAST_ACCESS_TIME_OFFSET = HITS_OFFSET + Bits.LONG_SIZE_IN_BYTES;
    private const int LAST_STORED_TIME_OFFSET = LAST_ACCESS_TIME_OFFSET + Bits.LONG_SIZE_IN_BYTES;
    private const int LAST_UPDATE_TIME_OFFSET = LAST_STORED_TIME_OFFSET + Bits.LONG_SIZE_IN_BYTES;
    private const int VERSION_OFFSET = LAST_UPDATE_TIME_OFFSET + Bits.LONG_SIZE_IN_BYTES;
    private const int TTL_OFFSET = VERSION_OFFSET + Bits.LONG_SIZE_IN_BYTES;
    private const int MAX_IDLE_OFFSET = TTL_OFFSET + Bits.LONG_SIZE_IN_BYTES;
    private const int InitialFrameSize = MAX_IDLE_OFFSET + Bits.LONG_SIZE_IN_BYTES;

    private SimpleEntryViewCodec() {
    }

    public static void Encode(ClientMessage clientMessage, SimpleEntryView<Data, Data> entryView) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeLong(initialFrame.content, COST_OFFSET, entryView.getCost());
        EncodeLong(initialFrame.content, CREATION_TIME_OFFSET, entryView.getCreationTime());
        EncodeLong(initialFrame.content, EXPIRATION_TIME_OFFSET, entryView.getExpirationTime());
        EncodeLong(initialFrame.content, HITS_OFFSET, entryView.getHits());
        EncodeLong(initialFrame.content, LAST_ACCESS_TIME_OFFSET, entryView.getLastAccessTime());
        EncodeLong(initialFrame.content, LAST_STORED_TIME_OFFSET, entryView.getLastStoredTime());
        EncodeLong(initialFrame.content, LAST_UPDATE_TIME_OFFSET, entryView.getLastUpdateTime());
        EncodeLong(initialFrame.content, VERSION_OFFSET, entryView.getVersion());
        EncodeLong(initialFrame.content, TTL_OFFSET, entryView.getTtl());
        EncodeLong(initialFrame.content, MAX_IDLE_OFFSET, entryView.getMaxIdle());
        clientMessage.add(initialFrame);

        DataCodec.Encode(clientMessage, entryView.getKey());
        DataCodec.Encode(clientMessage, entryView.getValue());

        clientMessage.add(EndFrame);
    }

    public static SimpleEntryView<Data, Data> Decode(ListIterator<ClientMessage.Frame> iterator) {
        // begin frame
        iterator.next();
        ClientMessage.Frame initialFrame = iterator.next();

        long cost = DecodeLong(initialFrame.content, COST_OFFSET);
        long creationTime = DecodeLong(initialFrame.content, CREATION_TIME_OFFSET);
        long expirationTime = DecodeLong(initialFrame.content, EXPIRATION_TIME_OFFSET);
        long hits = DecodeLong(initialFrame.content, HITS_OFFSET);
        long lastAccessTime = DecodeLong(initialFrame.content, LAST_ACCESS_TIME_OFFSET);
        long lastStoredTime = DecodeLong(initialFrame.content, LAST_STORED_TIME_OFFSET);
        long lastUpdateTime = DecodeLong(initialFrame.content, LAST_UPDATE_TIME_OFFSET);
        long version = DecodeLong(initialFrame.content, VERSION_OFFSET);
        long ttl = DecodeLong(initialFrame.content, TTL_OFFSET);
        long maxIdle = DecodeLong(initialFrame.content, MAX_IDLE_OFFSET);

        Data key = DataCodec.Decode(iterator);
        Data value = DataCodec.Decode(iterator);

        fastForwardToEndFrame(iterator);

        SimpleEntryView<Data, Data> entryView = new SimpleEntryView<>(key, value);
        entryView.setCost(cost);
        entryView.setCreationTime(creationTime);
        entryView.setExpirationTime(expirationTime);
        entryView.setHits(hits);
        entryView.setLastAccessTime(lastAccessTime);
        entryView.setLastStoredTime(lastStoredTime);
        entryView.setLastUpdateTime(lastUpdateTime);
        entryView.setVersion(version);
        entryView.setTtl(ttl);
        entryView.setMaxIdle(maxIdle);
        return entryView;
    }
}
