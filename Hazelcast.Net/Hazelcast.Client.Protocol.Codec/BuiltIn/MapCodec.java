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
import java.util.ArrayList;
import java.util.Collection;
import java.util.HashMap;
import java.util.List;
import java.util.ListIterator;
import java.util.Map;
import java.util.function.BiConsumer;
import java.util.function.Function;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.NULL_FRAME;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.nextFrameIsDataStructureEndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.nextFrameIsNullEndFrame;

public final class MapCodec {

    private MapCodec() {
    }

    public static <K, V> void Encode(ClientMessage clientMessage, Collection<Map.Entry<K, V>> collection,
                                     BiConsumer<ClientMessage, K> EncodeKeyFunc,
                                     BiConsumer<ClientMessage, V> EncodeValueFunc) {
        clientMessage.add(BeginFrame);
        for (Map.Entry<K, V> entry : collection) {
            EncodeKeyFunc.accept(clientMessage, entry.getKey());
            EncodeValueFunc.accept(clientMessage, entry.getValue());
        }
        clientMessage.add(EndFrame);
    }

    public static <K, V> void EncodeNullable(ClientMessage clientMessage, Collection<Map.Entry<K, V>> collection,
                                             BiConsumer<ClientMessage, K> EncodeKeyFunc,
                                             BiConsumer<ClientMessage, V> EncodeValueFunc) {
        if (collection == null) {
            clientMessage.add(NULL_FRAME);
        } else {
            Encode(clientMessage, collection, EncodeKeyFunc, EncodeValueFunc);
        }
    }

    public static <K, V> void Encode(ClientMessage clientMessage, Map<K, V> map,
                                     BiConsumer<ClientMessage, K> EncodeKeyFunc,
                                     BiConsumer<ClientMessage, V> EncodeValueFunc) {
        Encode(clientMessage, map.entrySet(), EncodeKeyFunc, EncodeValueFunc);
    }

    public static <K, V> List<Map.Entry<K, V>> Decode(ListIterator<ClientMessage.Frame> iterator,
                                                      Function<ListIterator<ClientMessage.Frame>, K> DecodeKeyFunc,
                                                      Function<ListIterator<ClientMessage.Frame>, V> DecodeValueFunc) {
        List<Map.Entry<K, V>> result = new ArrayList<>();
        //begin frame, map
        iterator.next();
        while (!nextFrameIsDataStructureEndFrame(iterator)) {
            K key = DecodeKeyFunc.apply(iterator);
            V value = DecodeValueFunc.apply(iterator);
            result.add(new AbstractMap.SimpleEntry<>(key, value));
        }
        //end frame, map
        iterator.next();
        return result;
    }

    public static <K, V> List<Map.Entry<K, V>> DecodeNullable(ListIterator<ClientMessage.Frame> iterator,
                                                              Function<ListIterator<ClientMessage.Frame>, K> DecodeKeyFunc,
                                                              Function<ListIterator<ClientMessage.Frame>, V> DecodeValueFunc) {
        return nextFrameIsNullEndFrame(iterator) ? null : Decode(iterator, DecodeKeyFunc, DecodeValueFunc);
    }

    public static <K, V> Map<K, V> DecodeToMap(ListIterator<ClientMessage.Frame> iterator,
                                               Function<ListIterator<ClientMessage.Frame>, K> DecodeKeyFunc,
                                               Function<ListIterator<ClientMessage.Frame>, V> DecodeValueFunc) {
        Map<K, V> result = new HashMap<>();
        //begin frame, map
        iterator.next();
        while (!nextFrameIsDataStructureEndFrame(iterator)) {
            K key = DecodeKeyFunc.apply(iterator);
            V value = DecodeValueFunc.apply(iterator);
            result.put(key, value);
        }
        //end frame, map
        iterator.next();
        return result;
    }

    public static <K, V> Map<K, V> DecodeToNullableMap(ListIterator<ClientMessage.Frame> iterator,
                                                       Function<ListIterator<ClientMessage.Frame>, K> DecodeKeyFunc,
                                                       Function<ListIterator<ClientMessage.Frame>, V> DecodeValueFunc) {
        return nextFrameIsNullEndFrame(iterator) ? null : DecodeToMap(iterator, DecodeKeyFunc, DecodeValueFunc);
    }
}
