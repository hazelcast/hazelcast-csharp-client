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
import com.hazelcast.client.impl.protocol.task.dynamicconfig.EvictionConfigHolder;
import com.hazelcast.client.impl.protocol.task.dynamicconfig.ListenerConfigHolder;
import com.hazelcast.client.impl.protocol.task.dynamicconfig.PredicateConfigHolder;
import com.hazelcast.client.impl.protocol.task.dynamicconfig.QueryCacheConfigHolder;
import com.hazelcast.config.MapIndexConfig;
import com.hazelcast.nio.Bits;

import java.util.List;
import java.util.ListIterator;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeBoolean;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeInt;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeBoolean;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeInt;

public final class QueryCacheConfigHolderCodec {
    private const int BATCH_SIZE_OFFSET = 0;
    private const int BUFFER_SIZE_OFFSET = BATCH_SIZE_OFFSET + Bits.IntSizeInBytes;
    private const int DELAY_SECONDS_OFFSET = BUFFER_SIZE_OFFSET + Bits.IntSizeInBytes;
    private const int INCLUDE_VALUE_OFFSET = DELAY_SECONDS_OFFSET + Bits.IntSizeInBytes;
    private const int POPULATE_OFFSET = INCLUDE_VALUE_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;
    private const int COALESCE_OFFSET = POPULATE_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;
    private const int InitialFrameSize = COALESCE_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;

    private QueryCacheConfigHolderCodec() {
    }

    public static void Encode(ClientMessage clientMessage, QueryCacheConfigHolder configHolder) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeInt(initialFrame.content, BATCH_SIZE_OFFSET, configHolder.getBatchSize());
        EncodeInt(initialFrame.content, BUFFER_SIZE_OFFSET, configHolder.getBufferSize());
        EncodeInt(initialFrame.content, DELAY_SECONDS_OFFSET, configHolder.getDelaySeconds());
        EncodeBoolean(initialFrame.content, INCLUDE_VALUE_OFFSET, configHolder.isIncludeValue());
        EncodeBoolean(initialFrame.content, POPULATE_OFFSET, configHolder.isPopulate());
        EncodeBoolean(initialFrame.content, COALESCE_OFFSET, configHolder.isCoalesce());
        clientMessage.add(initialFrame);

        StringCodec.Encode(clientMessage, configHolder.getInMemoryFormat());
        StringCodec.Encode(clientMessage, configHolder.getName());
        PredicateConfigHolderCodec.Encode(clientMessage, configHolder.getPredicateConfigHolder());
        EvictionConfigHolderCodec.Encode(clientMessage, configHolder.getEvictionConfigHolder());
        ListMultiFrameCodec.EncodeNullable(clientMessage, configHolder.getListenerConfigs(), ListenerConfigHolderCodec::Encode);
        ListMultiFrameCodec.EncodeNullable(clientMessage, configHolder.getIndexConfigs(), MapIndexConfigCodec::Encode);

        clientMessage.add(EndFrame);
    }

    public static QueryCacheConfigHolder Decode(ref ClientMessage.FrameIterator iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        int batchSize = DecodeInt(initialFrame.content, BATCH_SIZE_OFFSET);
        int bufferSize = DecodeInt(initialFrame.content, BUFFER_SIZE_OFFSET);
        int delaySeconds = DecodeInt(initialFrame.content, DELAY_SECONDS_OFFSET);
        boolean includeValue = DecodeBoolean(initialFrame.content, INCLUDE_VALUE_OFFSET);
        boolean populate = DecodeBoolean(initialFrame.content, POPULATE_OFFSET);
        boolean coalesce = DecodeBoolean(initialFrame.content, COALESCE_OFFSET);

        String inMemoryFormat = StringCodec.Decode(iterator);
        String name = StringCodec.Decode(iterator);
        PredicateConfigHolder predicateConfigHolder = PredicateConfigHolderCodec.Decode(iterator);
        EvictionConfigHolder evictionConfigHolder = EvictionConfigHolderCodec.Decode(iterator);
        List<ListenerConfigHolder> listenerConfigs = ListMultiFrameCodec.DecodeNullable(iterator, ListenerConfigHolderCodec::Decode);
        List<MapIndexConfig> indexConfigs = ListMultiFrameCodec.DecodeNullable(iterator, MapIndexConfigCodec::Decode);

        fastForwardToEndFrame(iterator);

        QueryCacheConfigHolder queryCacheConfigHolder = new QueryCacheConfigHolder();
        queryCacheConfigHolder.setBatchSize(batchSize);
        queryCacheConfigHolder.setBufferSize(bufferSize);
        queryCacheConfigHolder.setDelaySeconds(delaySeconds);
        queryCacheConfigHolder.setIncludeValue(includeValue);
        queryCacheConfigHolder.setPopulate(populate);
        queryCacheConfigHolder.setCoalesce(coalesce);
        queryCacheConfigHolder.setInMemoryFormat(inMemoryFormat);
        queryCacheConfigHolder.setName(name);
        queryCacheConfigHolder.setPredicateConfigHolder(predicateConfigHolder);
        queryCacheConfigHolder.setEvictionConfigHolder(evictionConfigHolder);
        queryCacheConfigHolder.setListenerConfigs(listenerConfigs);
        queryCacheConfigHolder.setIndexConfigs(indexConfigs);
        return queryCacheConfigHolder;
    }
}
