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

import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.IntSizeInBytes;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeInteger;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeInteger;

public final class ListIntegerCodec {

    private ListIntegerCodec() {
    }

    public static void Encode(ClientMessage clientMessage, Collection<Integer> collection) {
        int itemCount = collection.size();
        ClientMessage.Frame frame = new ClientMessage.Frame(new byte[itemCount * IntSizeInBytes]);
        Iterator<Integer> iterator = collection.iterator();
        for (int i = 0; i < itemCount; i++) {
            EncodeInteger(frame.content, i * IntSizeInBytes, iterator.next());
        }
        clientMessage.add(frame);
    }

    public static List<Integer> Decode(ListIterator<ClientMessage.Frame> iterator) {
        return Decode(iterator.next());
    }

    public static List<Integer> Decode(ClientMessage.Frame frame) {
        int itemCount = frame.content == null ? 0 : frame.content.length / IntSizeInBytes;
        List<Integer> result = new ArrayList<>(itemCount);
        for (int i = 0; i < itemCount; i++) {
            result.add(DecodeInteger(frame.content, i * IntSizeInBytes));
        }
        return result;
    }

}
