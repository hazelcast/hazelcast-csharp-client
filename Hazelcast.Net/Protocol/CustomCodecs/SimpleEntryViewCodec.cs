// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
//     Hazelcast Client Protocol Code Generator
//     https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Logging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;
using static Hazelcast.Protocol.Portability;

namespace Hazelcast.Protocol.CustomCodecs
{
    internal static class SimpleEntryViewCodec
    {
        private const int CostFieldOffset = 0;
        private const int CreationTimeFieldOffset = CostFieldOffset + LongSizeInBytes;
        private const int ExpirationTimeFieldOffset = CreationTimeFieldOffset + LongSizeInBytes;
        private const int HitsFieldOffset = ExpirationTimeFieldOffset + LongSizeInBytes;
        private const int LastAccessTimeFieldOffset = HitsFieldOffset + LongSizeInBytes;
        private const int LastStoredTimeFieldOffset = LastAccessTimeFieldOffset + LongSizeInBytes;
        private const int LastUpdateTimeFieldOffset = LastStoredTimeFieldOffset + LongSizeInBytes;
        private const int VersionFieldOffset = LastUpdateTimeFieldOffset + LongSizeInBytes;
        private const int TtlFieldOffset = VersionFieldOffset + LongSizeInBytes;
        private const int MaxIdleFieldOffset = TtlFieldOffset + LongSizeInBytes;
        private const int InitialFrameSize = MaxIdleFieldOffset + LongSizeInBytes;

        public static void Encode(ClientMessage clientMessage, Hazelcast.Data.Map.MapEntry<IData, IData> simpleEntryView)
        {
            clientMessage.Add(Frame.CreateBeginStruct());

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            EncodeLong(initialFrame, CostFieldOffset, simpleEntryView.Cost);
            EncodeLong(initialFrame, CreationTimeFieldOffset, simpleEntryView.CreationTime);
            EncodeLong(initialFrame, ExpirationTimeFieldOffset, simpleEntryView.ExpirationTime);
            EncodeLong(initialFrame, HitsFieldOffset, simpleEntryView.Hits);
            EncodeLong(initialFrame, LastAccessTimeFieldOffset, simpleEntryView.LastAccessTime);
            EncodeLong(initialFrame, LastStoredTimeFieldOffset, simpleEntryView.LastStoredTime);
            EncodeLong(initialFrame, LastUpdateTimeFieldOffset, simpleEntryView.LastUpdateTime);
            EncodeLong(initialFrame, VersionFieldOffset, simpleEntryView.Version);
            EncodeLong(initialFrame, TtlFieldOffset, simpleEntryView.Ttl);
            EncodeLong(initialFrame, MaxIdleFieldOffset, simpleEntryView.MaxIdle);
            clientMessage.Add(initialFrame);

            DataCodec.Encode(clientMessage, simpleEntryView.Key);
            DataCodec.Encode(clientMessage, simpleEntryView.Value);

            clientMessage.Add(Frame.CreateEndStruct());
        }

        public static Hazelcast.Data.Map.MapEntry<IData, IData> Decode(IEnumerator<Frame> iterator)
        {
            // begin frame
            iterator.Take();

            var initialFrame = iterator.Take();
            var cost = DecodeLong(initialFrame, CostFieldOffset);
            var creationTime = DecodeLong(initialFrame, CreationTimeFieldOffset);
            var expirationTime = DecodeLong(initialFrame, ExpirationTimeFieldOffset);
            var hits = DecodeLong(initialFrame, HitsFieldOffset);
            var lastAccessTime = DecodeLong(initialFrame, LastAccessTimeFieldOffset);
            var lastStoredTime = DecodeLong(initialFrame, LastStoredTimeFieldOffset);
            var lastUpdateTime = DecodeLong(initialFrame, LastUpdateTimeFieldOffset);
            var version = DecodeLong(initialFrame, VersionFieldOffset);
            var ttl = DecodeLong(initialFrame, TtlFieldOffset);
            var maxIdle = DecodeLong(initialFrame, MaxIdleFieldOffset);

            var key = DataCodec.Decode(iterator);
            var @value = DataCodec.Decode(iterator);

            iterator.SkipToStructEnd();

            return CustomTypeFactory.CreateSimpleEntryView(key, @value, cost, creationTime, expirationTime, hits, lastAccessTime, lastStoredTime, lastUpdateTime, version, ttl, maxIdle);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members