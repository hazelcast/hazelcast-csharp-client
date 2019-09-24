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

import java.util.Collection;
import java.util.LinkedList;
import java.util.List;
import java.util.ListIterator;
import java.util.function.BiConsumer;
import java.util.function.Function;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.NULL_FRAME;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.nextFrameIsDataStructureEndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.nextFrameIsNullEndFrame;

public final class ListMultiFrameCodec {

    private ListMultiFrameCodec() {
    }

    public static <T> void Encode(ClientMessage clientMessage, Collection<T> collection,
                                  BiConsumer<ClientMessage, T> EncodeFunction) {
        clientMessage.add(BeginFrame);
        for (T item : collection) {
            EncodeFunction.accept(clientMessage, item);
        }
        clientMessage.add(EndFrame);
    }

    public static <T> void EncodeContainsNullable(ClientMessage clientMessage, Collection<T> collection,
                                                  BiConsumer<ClientMessage, T> EncodeFunction) {
        clientMessage.add(BeginFrame);
        for (T item : collection) {
            if (item == null) {
                clientMessage.add(NULL_FRAME);
            } else {
                EncodeFunction.accept(clientMessage, item);
            }
        }
        clientMessage.add(EndFrame);
    }

    public static <T> void EncodeNullable(ClientMessage clientMessage, Collection<T> collection,
                                          BiConsumer<ClientMessage, T> EncodeFunction) {
        if (collection == null) {
            clientMessage.add(NULL_FRAME);
        } else {
            Encode(clientMessage, collection, EncodeFunction);
        }
    }

    public static <T> List<T> Decode(ref ClientMessage.FrameIterator iterator,
                                     Function<ListIterator<ClientMessage.Frame>, T> DecodeFunction) {
        List<T> result = new LinkedList<>();
        //begin frame, list
        iterator.next();
        while (!nextFrameIsDataStructureEndFrame(iterator)) {
            result.add(DecodeFunction.apply(iterator));
        }
        //end frame, list
        iterator.next();
        return result;
    }

    public static <T> List<T> DecodeContainsNullable(ref ClientMessage.FrameIterator iterator,
                                     Function<ListIterator<ClientMessage.Frame>, T> DecodeFunction) {
        List<T> result = new LinkedList<>();
        //begin frame, list
        iterator.next();
        while (!nextFrameIsDataStructureEndFrame(iterator)) {
            result.add(nextFrameIsNullEndFrame(iterator) ? null : DecodeFunction.apply(iterator));
        }
        //end frame, list
        iterator.next();
        return result;
    }


    public static <T> List<T> DecodeNullable(ref ClientMessage.FrameIterator iterator,
                                             Function<ListIterator<ClientMessage.Frame>, T> DecodeFunction) {
        return nextFrameIsNullEndFrame(iterator) ? null : Decode(iterator, DecodeFunction);
    }
}
