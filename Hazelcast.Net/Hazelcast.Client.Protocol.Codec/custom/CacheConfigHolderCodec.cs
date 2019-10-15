// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec.BuiltIn;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using static Hazelcast.Client.Protocol.Codec.BuiltIn.FixedSizeTypesCodec;
using static Hazelcast.Client.Protocol.ClientMessage;
using static Hazelcast.IO.Bits;

namespace Hazelcast.Client.Protocol.Codec.Custom
{
    // This file is auto-generated by the Hazelcast Client Protocol Code Generator.
    // To change this file, edit the templates or the protocol
    // definitions on the https://github.com/hazelcast/hazelcast-client-protocol
    // and regenerate it.

    internal static class CacheConfigHolderCodec 
    {
        private const int BackupCountFieldOffset = 0;
        private const int AsyncBackupCountFieldOffset = BackupCountFieldOffset + IntSizeInBytes;
        private const int ReadThroughFieldOffset = AsyncBackupCountFieldOffset + IntSizeInBytes;
        private const int WriteThroughFieldOffset = ReadThroughFieldOffset + BoolSizeInBytes;
        private const int StoreByValueFieldOffset = WriteThroughFieldOffset + BoolSizeInBytes;
        private const int ManagementEnabledFieldOffset = StoreByValueFieldOffset + BoolSizeInBytes;
        private const int StatisticsEnabledFieldOffset = ManagementEnabledFieldOffset + BoolSizeInBytes;
        private const int DisablePerEntryInvalidationEventsFieldOffset = StatisticsEnabledFieldOffset + BoolSizeInBytes;
        private const int InitialFrameSize = DisablePerEntryInvalidationEventsFieldOffset + BoolSizeInBytes;

        public static void Encode(ClientMessage clientMessage, com.hazelcast.client.impl.protocol.codec.holder.CacheConfigHolder cacheConfigHolder) 
        {
            clientMessage.Add(BeginFrame);

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            EncodeInt(initialFrame.Content, BackupCountFieldOffset, cacheConfigHolder.BackupCount);
            EncodeInt(initialFrame.Content, AsyncBackupCountFieldOffset, cacheConfigHolder.AsyncBackupCount);
            EncodeBool(initialFrame.Content, ReadThroughFieldOffset, cacheConfigHolder.IsReadThrough);
            EncodeBool(initialFrame.Content, WriteThroughFieldOffset, cacheConfigHolder.IsWriteThrough);
            EncodeBool(initialFrame.Content, StoreByValueFieldOffset, cacheConfigHolder.IsStoreByValue);
            EncodeBool(initialFrame.Content, ManagementEnabledFieldOffset, cacheConfigHolder.IsManagementEnabled);
            EncodeBool(initialFrame.Content, StatisticsEnabledFieldOffset, cacheConfigHolder.IsStatisticsEnabled);
            EncodeBool(initialFrame.Content, DisablePerEntryInvalidationEventsFieldOffset, cacheConfigHolder.IsDisablePerEntryInvalidationEvents);
            clientMessage.Add(initialFrame);

            StringCodec.Encode(clientMessage, cacheConfigHolder.Name);
            CodecUtil.EncodeNullable(clientMessage, cacheConfigHolder.ManagerPrefix, StringCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, cacheConfigHolder.UriString, StringCodec.Encode);
            StringCodec.Encode(clientMessage, cacheConfigHolder.InMemoryFormat);
            EvictionConfigHolderCodec.Encode(clientMessage, cacheConfigHolder.EvictionConfigHolder);
            CodecUtil.EncodeNullable(clientMessage, cacheConfigHolder.WanReplicationRef, WanReplicationRefCodec.Encode);
            StringCodec.Encode(clientMessage, cacheConfigHolder.KeyClassName);
            StringCodec.Encode(clientMessage, cacheConfigHolder.ValueClassName);
            CodecUtil.EncodeNullable(clientMessage, cacheConfigHolder.CacheLoaderFactory, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, cacheConfigHolder.CacheWriterFactory, DataCodec.Encode);
            DataCodec.Encode(clientMessage, cacheConfigHolder.ExpiryPolicyFactory);
            CodecUtil.EncodeNullable(clientMessage, cacheConfigHolder.HotRestartConfig, HotRestartConfigCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, cacheConfigHolder.EventJournalConfig, EventJournalConfigCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, cacheConfigHolder.SplitBrainProtectionName, StringCodec.Encode);
            ListMultiFrameCodec.EncodeNullable(clientMessage, cacheConfigHolder.ListenerConfigurations, DataCodec.Encode);
            MergePolicyConfigCodec.Encode(clientMessage, cacheConfigHolder.MergePolicyConfig);
            ListMultiFrameCodec.EncodeNullable(clientMessage, cacheConfigHolder.CachePartitionLostListenerConfigs, ListenerConfigHolderCodec.Encode);

            clientMessage.Add(EndFrame);
        }

        public static com.hazelcast.client.impl.protocol.codec.holder.CacheConfigHolder Decode(ref FrameIterator iterator) 
        {
            // begin frame
            iterator.Next();

            ref var initialFrame = ref iterator.Next();
            var backupCount = DecodeInt(initialFrame.Content, BackupCountFieldOffset);
            var asyncBackupCount = DecodeInt(initialFrame.Content, AsyncBackupCountFieldOffset);
            var readThrough = DecodeBool(initialFrame.Content, ReadThroughFieldOffset);
            var writeThrough = DecodeBool(initialFrame.Content, WriteThroughFieldOffset);
            var storeByValue = DecodeBool(initialFrame.Content, StoreByValueFieldOffset);
            var managementEnabled = DecodeBool(initialFrame.Content, ManagementEnabledFieldOffset);
            var statisticsEnabled = DecodeBool(initialFrame.Content, StatisticsEnabledFieldOffset);
            var disablePerEntryInvalidationEvents = DecodeBool(initialFrame.Content, DisablePerEntryInvalidationEventsFieldOffset);

            var name = StringCodec.Decode(ref iterator);
            var managerPrefix = CodecUtil.DecodeNullable(ref iterator, StringCodec.Decode);
            var uriString = CodecUtil.DecodeNullable(ref iterator, StringCodec.Decode);
            var inMemoryFormat = StringCodec.Decode(ref iterator);
            var evictionConfigHolder = EvictionConfigHolderCodec.Decode(ref iterator);
            var wanReplicationRef = CodecUtil.DecodeNullable(ref iterator, WanReplicationRefCodec.Decode);
            var keyClassName = StringCodec.Decode(ref iterator);
            var valueClassName = StringCodec.Decode(ref iterator);
            var cacheLoaderFactory = CodecUtil.DecodeNullable(ref iterator, DataCodec.Decode);
            var cacheWriterFactory = CodecUtil.DecodeNullable(ref iterator, DataCodec.Decode);
            var expiryPolicyFactory = DataCodec.Decode(ref iterator);
            var hotRestartConfig = CodecUtil.DecodeNullable(ref iterator, HotRestartConfigCodec.Decode);
            var eventJournalConfig = CodecUtil.DecodeNullable(ref iterator, EventJournalConfigCodec.Decode);
            var splitBrainProtectionName = CodecUtil.DecodeNullable(ref iterator, StringCodec.Decode);
            var listenerConfigurations = ListMultiFrameCodec.DecodeNullable(ref iterator, DataCodec.Decode);
            var mergePolicyConfig = MergePolicyConfigCodec.Decode(ref iterator);
            var cachePartitionLostListenerConfigs = ListMultiFrameCodec.DecodeNullable(ref iterator, ListenerConfigHolderCodec.Decode);

            CodecUtil.FastForwardToEndFrame(ref iterator);

            return new com.hazelcast.client.impl.protocol.codec.holder.CacheConfigHolder(name, managerPrefix, uriString, backupCount, asyncBackupCount, inMemoryFormat, evictionConfigHolder, wanReplicationRef, keyClassName, valueClassName, cacheLoaderFactory, cacheWriterFactory, expiryPolicyFactory, readThrough, writeThrough, storeByValue, managementEnabled, statisticsEnabled, hotRestartConfig, eventJournalConfig, splitBrainProtectionName, listenerConfigurations, mergePolicyConfig, disablePerEntryInvalidationEvents, cachePartitionLostListenerConfigs);
        }
    }
}