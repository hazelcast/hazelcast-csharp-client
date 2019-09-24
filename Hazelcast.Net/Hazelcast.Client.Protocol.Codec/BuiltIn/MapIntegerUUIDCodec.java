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

import java.util.AbstractMap;
import java.util.Collection;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.List;
import java.util.ListIterator;
import java.util.Map;
import java.util.UUID;

import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.IntSizeInBytes;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.UUID_SIZE_IN_BYTES;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeInteger;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeUUID;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeInteger;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeUUID;

public final class MapIntegerUUIDCodec {

    private const int ENTRY_SIZE_IN_BYTES = IntSizeInBytes + UUID_SIZE_IN_BYTES;

    private MapIntegerUUIDCodec() {
    }

    public static void Encode(ClientMessage clientMessage, Collection<Map.Entry<Integer, UUID>> collection) {
        int itemCount = collection.size();
        ClientMessage.Frame frame = new ClientMessage.Frame(new byte[itemCount * ENTRY_SIZE_IN_BYTES]);
        Iterator<Map.Entry<Integer, UUID>> iterator = collection.iterator();
        for (int i = 0; i < itemCount; i++) {
            Map.Entry<Integer, UUID> entry = iterator.next();
            EncodeInteger(frame.content, i * ENTRY_SIZE_IN_BYTES, entry.getKey());
            EncodeUUID(frame.content, i * ENTRY_SIZE_IN_BYTES + IntSizeInBytes, entry.getValue());
        }
        clientMessage.add(frame);
    }

    public static List<Map.Entry<Integer, UUID>> Decode(ListIterator<ClientMessage.Frame> iterator) {
        ClientMessage.Frame frame = iterator.next();
        int itemCount = frame.content.length / ENTRY_SIZE_IN_BYTES;
        List<Map.Entry<Integer, UUID>> result = new LinkedList<>();
        for (int i = 0; i < itemCount; i++) {
            int key = DecodeInteger(frame.content, i * ENTRY_SIZE_IN_BYTES);
            UUID value = DecodeUUID(frame.content, i * ENTRY_SIZE_IN_BYTES + IntSizeInBytes);
            result.add(new AbstractMap.SimpleEntry<>(key, value));
        }
        return result;
    }
}
