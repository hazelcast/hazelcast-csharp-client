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
import com.hazelcast.map.impl.querycache.event.DefaultQueryCacheEventData;
import com.hazelcast.map.impl.querycache.event.QueryCacheEventData;
import com.hazelcast.nio.Bits;
import com.hazelcast.nio.serialization.Data;

import java.util.ListIterator;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.DecodeNullable;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.EncodeNullable;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeInt;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeLong;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeInt;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeLong;

public final class QueryCacheEventDataCodec {

    private const int SEQUENCE_OFFSET = 0;
    private const int EVENT_TYPE_OFFSET = SEQUENCE_OFFSET + Bits.LONG_SIZE_IN_BYTES;
    private const int PARTITION_ID_OFFSET = EVENT_TYPE_OFFSET + Bits.IntSizeInBytes;
    private const int InitialFrameSize = PARTITION_ID_OFFSET + Bits.IntSizeInBytes;

    private QueryCacheEventDataCodec() {
    }

    public static void Encode(ClientMessage clientMessage, QueryCacheEventData eventData) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeLong(initialFrame.content, SEQUENCE_OFFSET, eventData.getSequence());
        EncodeInt(initialFrame.content, EVENT_TYPE_OFFSET, eventData.getEventType());
        EncodeInt(initialFrame.content, PARTITION_ID_OFFSET, eventData.getPartitionId());
        clientMessage.add(initialFrame);

        EncodeNullable(clientMessage, eventData.getDataKey(), DataCodec::Encode);
        EncodeNullable(clientMessage, eventData.getDataNewValue(), DataCodec::Encode);

        clientMessage.add(EndFrame);
    }

    public static QueryCacheEventData Decode(ref ClientMessage.FrameIterator iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        long sequence = DecodeLong(initialFrame.content, SEQUENCE_OFFSET);
        int eventType = DecodeInt(initialFrame.content, EVENT_TYPE_OFFSET);
        int partitionId = DecodeInt(initialFrame.content, PARTITION_ID_OFFSET);

        Data key = DecodeNullable(iterator, DataCodec::Decode);
        Data newValue = DecodeNullable(iterator, DataCodec::Decode);

        fastForwardToEndFrame(iterator);

        DefaultQueryCacheEventData queryCacheEventData = new DefaultQueryCacheEventData();
        queryCacheEventData.setSequence(sequence);
        queryCacheEventData.setEventType(eventType);
        queryCacheEventData.setPartitionId(partitionId);
        queryCacheEventData.setDataKey(key);
        queryCacheEventData.setDataNewValue(newValue);
        return queryCacheEventData;
    }
}
