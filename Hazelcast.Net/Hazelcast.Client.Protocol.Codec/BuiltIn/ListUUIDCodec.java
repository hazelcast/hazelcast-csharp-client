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

import java.util.ArrayList;
import java.util.Collection;
import java.util.Iterator;
import java.util.List;
import java.util.ListIterator;
import java.util.UUID;

import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.UUID_SIZE_IN_BYTES;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeUUID;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeUUID;

public final class ListUUIDCodec {

    private ListUUIDCodec() {
    }

    public static void Encode(ClientMessage clientMessage, Collection<UUID> collection) {
        int itemCount = collection.size();
        ClientMessage.Frame frame = new ClientMessage.Frame(new byte[itemCount * UUID_SIZE_IN_BYTES]);
        Iterator<UUID> iterator = collection.iterator();
        for (int i = 0; i < itemCount; i++) {
            EncodeUUID(frame.content, i * UUID_SIZE_IN_BYTES, iterator.next());
        }
        clientMessage.add(frame);
    }

    public static List<UUID> Decode(ListIterator<ClientMessage.Frame> iterator) {
        return Decode(iterator.next());
    }

    public static List<UUID> Decode(ClientMessage.Frame frame) {
        int itemCount = frame.content.length / UUID_SIZE_IN_BYTES;
        List<UUID> result = new ArrayList<>(itemCount);
        for (int i = 0; i < itemCount; i++) {
            result.add(DecodeUUID(frame.content, i * UUID_SIZE_IN_BYTES));
        }
        return result;
    }
}
