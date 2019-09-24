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
import java.util.List;
import java.util.ListIterator;
import java.util.Map;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;

public final class MapStringLongCodec {

    private MapStringLongCodec() {
    }

    public static void Encode(ClientMessage clientMessage, Collection<Map.Entry<String, Long>> collection) {
        List<Long> valueList = new ArrayList<>(collection.size());
        clientMessage.add(BeginFrame);
        for (Map.Entry<String, Long> entry : collection) {
            StringCodec.Encode(clientMessage, entry.getKey());
            valueList.add(entry.getValue());
        }
        clientMessage.add(EndFrame);

        ListLongCodec.Encode(clientMessage, valueList);
    }

    public static List<Map.Entry<String, Long>> Decode(ListIterator<ClientMessage.Frame> iterator) {
        List<String> listK = ListMultiFrameCodec.Decode(iterator, StringCodec::Decode);
        List<Long> listV = ListLongCodec.Decode(iterator);

        List<Map.Entry<String, Long>> result = new ArrayList<>(listK.size());
        for (int i = 0; i < listK.size(); i++) {
            result.add(new AbstractMap.SimpleEntry<>(listK.get(i), listV.get(i)));
        }
        return result;
    }

}
