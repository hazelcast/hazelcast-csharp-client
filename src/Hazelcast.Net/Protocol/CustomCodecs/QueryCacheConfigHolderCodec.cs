﻿// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

// <auto-generated>
//   This code was generated by a tool.
//   Hazelcast Client Protocol Code Generator @c89bc95
//   https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantUsingDirective
// ReSharper disable CheckNamespace

using System;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Protocol.CustomCodecs
{
    internal static class QueryCacheConfigHolderCodec
    {
        private const int BatchSizeFieldOffset = 0;
        private const int BufferSizeFieldOffset = BatchSizeFieldOffset + BytesExtensions.SizeOfInt;
        private const int DelaySecondsFieldOffset = BufferSizeFieldOffset + BytesExtensions.SizeOfInt;
        private const int IncludeValueFieldOffset = DelaySecondsFieldOffset + BytesExtensions.SizeOfInt;
        private const int PopulateFieldOffset = IncludeValueFieldOffset + BytesExtensions.SizeOfBool;
        private const int CoalesceFieldOffset = PopulateFieldOffset + BytesExtensions.SizeOfBool;
        private const int SerializeKeysFieldOffset = CoalesceFieldOffset + BytesExtensions.SizeOfBool;
        private const int InitialFrameSize = SerializeKeysFieldOffset + BytesExtensions.SizeOfBool;

        public static void Encode(ClientMessage clientMessage, Hazelcast.Protocol.Models.QueryCacheConfigHolder queryCacheConfigHolder)
        {
            clientMessage.Append(Frame.CreateBeginStruct());

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            initialFrame.Bytes.WriteIntL(BatchSizeFieldOffset, queryCacheConfigHolder.BatchSize);
            initialFrame.Bytes.WriteIntL(BufferSizeFieldOffset, queryCacheConfigHolder.BufferSize);
            initialFrame.Bytes.WriteIntL(DelaySecondsFieldOffset, queryCacheConfigHolder.DelaySeconds);
            initialFrame.Bytes.WriteBoolL(IncludeValueFieldOffset, queryCacheConfigHolder.IsIncludeValue);
            initialFrame.Bytes.WriteBoolL(PopulateFieldOffset, queryCacheConfigHolder.IsPopulate);
            initialFrame.Bytes.WriteBoolL(CoalesceFieldOffset, queryCacheConfigHolder.IsCoalesce);
            initialFrame.Bytes.WriteBoolL(SerializeKeysFieldOffset, queryCacheConfigHolder.IsSerializeKeys);
            clientMessage.Append(initialFrame);

            StringCodec.Encode(clientMessage, queryCacheConfigHolder.InMemoryFormat);
            StringCodec.Encode(clientMessage, queryCacheConfigHolder.Name);
            PredicateConfigHolderCodec.Encode(clientMessage, queryCacheConfigHolder.PredicateConfigHolder);
            EvictionConfigHolderCodec.Encode(clientMessage, queryCacheConfigHolder.EvictionConfigHolder);
            ListMultiFrameCodec.EncodeNullable(clientMessage, queryCacheConfigHolder.ListenerConfigs, ListenerConfigHolderCodec.Encode);
            ListMultiFrameCodec.EncodeNullable(clientMessage, queryCacheConfigHolder.IndexConfigs, IndexConfigCodec.Encode);

            clientMessage.Append(Frame.CreateEndStruct());
        }

        public static Hazelcast.Protocol.Models.QueryCacheConfigHolder Decode(IEnumerator<Frame> iterator)
        {
            // begin frame
            iterator.Take();

            var initialFrame = iterator.Take();
            var batchSize = initialFrame.Bytes.ReadIntL(BatchSizeFieldOffset);

            var bufferSize = initialFrame.Bytes.ReadIntL(BufferSizeFieldOffset);
            var delaySeconds = initialFrame.Bytes.ReadIntL(DelaySecondsFieldOffset);
            var includeValue = initialFrame.Bytes.ReadBoolL(IncludeValueFieldOffset);
            var populate = initialFrame.Bytes.ReadBoolL(PopulateFieldOffset);
            var coalesce = initialFrame.Bytes.ReadBoolL(CoalesceFieldOffset);
            var isSerializeKeysExists = false;
            bool serializeKeys = default;
            if (initialFrame.Bytes.Length >= SerializeKeysFieldOffset + BytesExtensions.SizeOfBool)
            {
                serializeKeys = initialFrame.Bytes.ReadBoolL(SerializeKeysFieldOffset);
                isSerializeKeysExists = true;
            }
            var inMemoryFormat = StringCodec.Decode(iterator);
            var name = StringCodec.Decode(iterator);
            var predicateConfigHolder = PredicateConfigHolderCodec.Decode(iterator);
            var evictionConfigHolder = EvictionConfigHolderCodec.Decode(iterator);
            var listenerConfigs = ListMultiFrameCodec.DecodeNullable(iterator, ListenerConfigHolderCodec.Decode);
            var indexConfigs = ListMultiFrameCodec.DecodeNullable(iterator, IndexConfigCodec.Decode);

            iterator.SkipToStructEnd();
            return new Hazelcast.Protocol.Models.QueryCacheConfigHolder(batchSize, bufferSize, delaySeconds, includeValue, populate, coalesce, inMemoryFormat, name, predicateConfigHolder, evictionConfigHolder, listenerConfigs, indexConfigs, isSerializeKeysExists, serializeKeys);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members
