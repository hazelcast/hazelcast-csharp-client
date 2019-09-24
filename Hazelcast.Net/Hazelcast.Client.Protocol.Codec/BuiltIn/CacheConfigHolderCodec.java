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
import com.hazelcast.client.impl.protocol.codec.holder.CacheConfigHolder;
import com.hazelcast.client.impl.protocol.task.dynamicconfig.EvictionConfigHolder;
import com.hazelcast.config.EventJournalConfig;
import com.hazelcast.config.HotRestartConfig;
import com.hazelcast.config.MergePolicyConfig;
import com.hazelcast.config.WanReplicationRef;
import com.hazelcast.nio.Bits;
import com.hazelcast.nio.serialization.Data;

import java.util.List;
import java.util.ListIterator;

import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.DecodeNullable;
import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.EncodeNullable;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeBoolean;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.DecodeInt;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeBoolean;
import static com.hazelcast.client.impl.protocol.codec.builtin.FixedSizeTypesCodec.EncodeInt;

public final class CacheConfigHolderCodec {
    private const int BACKUP_COUNT_OFFSET = 0;
    private const int ASYNC_BACKUP_COUNT_OFFSET = BACKUP_COUNT_OFFSET + Bits.IntSizeInBytes;
    private const int IS_READ_THROUGH_OFFSET = ASYNC_BACKUP_COUNT_OFFSET + Bits.IntSizeInBytes;
    private const int IS_WRITE_THROUGH_OFFSET = IS_READ_THROUGH_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;
    private const int IS_STORE_BY_VALUE_OFFSET = IS_WRITE_THROUGH_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;
    private const int IS_MANAGEMENT_ENABLED_OFFSET = IS_STORE_BY_VALUE_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;
    private const int IS_STATISTICS_ENABLED_OFFSET = IS_MANAGEMENT_ENABLED_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;
    private const int DISABLE_PER_ENTRY_INVALIDATION_EVENTS_OFFSET =
            IS_STATISTICS_ENABLED_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;

    private const int InitialFrameSize = DISABLE_PER_ENTRY_INVALIDATION_EVENTS_OFFSET + Bits.BOOLEAN_SIZE_IN_BYTES;

    private CacheConfigHolderCodec() {
    }

    public static <K, V> void Encode(ClientMessage clientMessage, CacheConfigHolder config) {
        clientMessage.add(BeginFrame);

        ClientMessage.Frame initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
        EncodeInt(initialFrame.content, BACKUP_COUNT_OFFSET, config.getBackupCount());
        EncodeInt(initialFrame.content, ASYNC_BACKUP_COUNT_OFFSET, config.getAsyncBackupCount());
        EncodeBoolean(initialFrame.content, IS_READ_THROUGH_OFFSET, config.isReadThrough());
        EncodeBoolean(initialFrame.content, IS_WRITE_THROUGH_OFFSET, config.isWriteThrough());
        EncodeBoolean(initialFrame.content, IS_STORE_BY_VALUE_OFFSET, config.isStoreByValue());
        EncodeBoolean(initialFrame.content, IS_MANAGEMENT_ENABLED_OFFSET, config.isManagementEnabled());
        EncodeBoolean(initialFrame.content, IS_STATISTICS_ENABLED_OFFSET, config.isStatisticsEnabled());
        EncodeBoolean(initialFrame.content, DISABLE_PER_ENTRY_INVALIDATION_EVENTS_OFFSET,
                config.isDisablePerEntryInvalidationEvents());
        clientMessage.add(initialFrame);

        StringCodec.Encode(clientMessage, config.getName());
        EncodeNullable(clientMessage, config.getManagerPrefix(), StringCodec::Encode);
        EncodeNullable(clientMessage, config.getUriString(), StringCodec::Encode);
        StringCodec.Encode(clientMessage, config.getInMemoryFormat());

        EvictionConfigHolderCodec.Encode(clientMessage, config.getEvictionConfigHolder());

        EncodeNullable(clientMessage, config.getWanReplicationRef(), WanReplicationRefCodec::Encode);
        StringCodec.Encode(clientMessage, config.getKeyClassName());
        StringCodec.Encode(clientMessage, config.getValueClassName());
        EncodeNullable(clientMessage, config.getCacheLoaderFactory(), DataCodec::Encode);
        EncodeNullable(clientMessage, config.getCacheWriterFactory(), DataCodec::Encode);
        EncodeNullable(clientMessage, config.getExpiryPolicyFactory(), DataCodec::Encode);
        EncodeNullable(clientMessage, config.getHotRestartConfig(), HotRestartConfigCodec::Encode);
        EncodeNullable(clientMessage, config.getEventJournalConfig(), EventJournalConfigCodec::Encode);
        EncodeNullable(clientMessage, config.getSplitBrainProtectionName(), StringCodec::Encode);
        ListMultiFrameCodec.EncodeNullable(clientMessage, config.getListenerConfigurations(), DataCodec::Encode);
        MergePolicyConfigCodec.Encode(clientMessage, config.getMergePolicyConfig());

        clientMessage.add(EndFrame);
    }

    public static CacheConfigHolder Decode(ref ClientMessage.FrameIterator iterator) {
        // begin frame
        iterator.next();

        ClientMessage.Frame initialFrame = iterator.next();
        int backupCount = DecodeInt(initialFrame.content, BACKUP_COUNT_OFFSET);
        int asyncBackupCount = DecodeInt(initialFrame.content, ASYNC_BACKUP_COUNT_OFFSET);
        boolean isReadThrough = DecodeBoolean(initialFrame.content, IS_READ_THROUGH_OFFSET);
        boolean isWriteThrough = DecodeBoolean(initialFrame.content, IS_WRITE_THROUGH_OFFSET);
        boolean isStoreByValue = DecodeBoolean(initialFrame.content, IS_STORE_BY_VALUE_OFFSET);
        boolean isManagementEnabled = DecodeBoolean(initialFrame.content, IS_MANAGEMENT_ENABLED_OFFSET);
        boolean isStatisticsEnabled = DecodeBoolean(initialFrame.content, IS_STATISTICS_ENABLED_OFFSET);
        boolean disablePerEntryInvalidationEvents = DecodeBoolean(initialFrame.content,
                DISABLE_PER_ENTRY_INVALIDATION_EVENTS_OFFSET);

        String name = StringCodec.Decode(iterator);
        String managerPrefix = DecodeNullable(iterator, StringCodec::Decode);
        String uriString = DecodeNullable(iterator, StringCodec::Decode);
        String inMemoryFormat = StringCodec.Decode(iterator);

        EvictionConfigHolder evictionConfigHolder = EvictionConfigHolderCodec.Decode(iterator);

        WanReplicationRef wanReplicationRef = DecodeNullable(iterator, WanReplicationRefCodec::Decode);

        String keyClassName = StringCodec.Decode(iterator);
        String valueClassName = StringCodec.Decode(iterator);

        Data cacheLoaderFactory = DecodeNullable(iterator, DataCodec::Decode);
        Data cacheWriterFactory = DecodeNullable(iterator, DataCodec::Decode);
        Data expiryPolicyFactory = DecodeNullable(iterator, DataCodec::Decode);

        HotRestartConfig hotRestartConfig = DecodeNullable(iterator, HotRestartConfigCodec::Decode);
        EventJournalConfig eventJournalConfig = DecodeNullable(iterator, EventJournalConfigCodec::Decode);
        String splitBrainProtectionName = DecodeNullable(iterator, StringCodec::Decode);
        List<Data> listenerConfigurations = ListMultiFrameCodec.DecodeNullable(iterator, DataCodec::Decode);
        MergePolicyConfig mergePolicyConfig = MergePolicyConfigCodec.Decode(iterator);

        return new CacheConfigHolder(name, managerPrefix, uriString, backupCount, asyncBackupCount, inMemoryFormat,
                evictionConfigHolder, wanReplicationRef, keyClassName, valueClassName, cacheLoaderFactory, cacheWriterFactory,
                expiryPolicyFactory, isReadThrough, isWriteThrough, isStoreByValue, isManagementEnabled, isStatisticsEnabled,
                hotRestartConfig, eventJournalConfig, splitBrainProtectionName, listenerConfigurations, mergePolicyConfig,
                disablePerEntryInvalidationEvents);
    }
}
