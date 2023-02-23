﻿// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
//   Hazelcast Client Protocol Code Generator @8aed6958e
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
    internal static class SimpleEntryViewCodec
    {
        private const int CostFieldOffset = 0;
        private const int CreationTimeFieldOffset = CostFieldOffset + BytesExtensions.SizeOfLong;
        private const int ExpirationTimeFieldOffset = CreationTimeFieldOffset + BytesExtensions.SizeOfLong;
        private const int HitsFieldOffset = ExpirationTimeFieldOffset + BytesExtensions.SizeOfLong;
        private const int LastAccessTimeFieldOffset = HitsFieldOffset + BytesExtensions.SizeOfLong;
        private const int LastStoredTimeFieldOffset = LastAccessTimeFieldOffset + BytesExtensions.SizeOfLong;
        private const int LastUpdateTimeFieldOffset = LastStoredTimeFieldOffset + BytesExtensions.SizeOfLong;
        private const int VersionFieldOffset = LastUpdateTimeFieldOffset + BytesExtensions.SizeOfLong;
        private const int TtlFieldOffset = VersionFieldOffset + BytesExtensions.SizeOfLong;
        private const int MaxIdleFieldOffset = TtlFieldOffset + BytesExtensions.SizeOfLong;
        private const int InitialFrameSize = MaxIdleFieldOffset + BytesExtensions.SizeOfLong;

        public static void Encode(ClientMessage clientMessage, Hazelcast.Models.MapEntryStats<IData, IData> simpleEntryView)
        {
            clientMessage.Append(Frame.CreateBeginStruct());

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            initialFrame.Bytes.WriteLongL(CostFieldOffset, simpleEntryView.Cost);
            initialFrame.Bytes.WriteLongL(CreationTimeFieldOffset, simpleEntryView.CreationTime);
            initialFrame.Bytes.WriteLongL(ExpirationTimeFieldOffset, simpleEntryView.ExpirationTime);
            initialFrame.Bytes.WriteLongL(HitsFieldOffset, simpleEntryView.Hits);
            initialFrame.Bytes.WriteLongL(LastAccessTimeFieldOffset, simpleEntryView.LastAccessTime);
            initialFrame.Bytes.WriteLongL(LastStoredTimeFieldOffset, simpleEntryView.LastStoredTime);
            initialFrame.Bytes.WriteLongL(LastUpdateTimeFieldOffset, simpleEntryView.LastUpdateTime);
            initialFrame.Bytes.WriteLongL(VersionFieldOffset, simpleEntryView.Version);
            initialFrame.Bytes.WriteLongL(TtlFieldOffset, simpleEntryView.Ttl);
            initialFrame.Bytes.WriteLongL(MaxIdleFieldOffset, simpleEntryView.MaxIdle);
            clientMessage.Append(initialFrame);

            DataCodec.Encode(clientMessage, simpleEntryView.Key);
            DataCodec.Encode(clientMessage, simpleEntryView.Value);

            clientMessage.Append(Frame.CreateEndStruct());
        }

        public static Hazelcast.Models.MapEntryStats<IData, IData> Decode(IEnumerator<Frame> iterator)
        {
            // begin frame
            iterator.Take();

            var initialFrame = iterator.Take();
            var cost = initialFrame.Bytes.ReadLongL(CostFieldOffset);

            var creationTime = initialFrame.Bytes.ReadLongL(CreationTimeFieldOffset);
            var expirationTime = initialFrame.Bytes.ReadLongL(ExpirationTimeFieldOffset);
            var hits = initialFrame.Bytes.ReadLongL(HitsFieldOffset);
            var lastAccessTime = initialFrame.Bytes.ReadLongL(LastAccessTimeFieldOffset);
            var lastStoredTime = initialFrame.Bytes.ReadLongL(LastStoredTimeFieldOffset);
            var lastUpdateTime = initialFrame.Bytes.ReadLongL(LastUpdateTimeFieldOffset);
            var version = initialFrame.Bytes.ReadLongL(VersionFieldOffset);
            var ttl = initialFrame.Bytes.ReadLongL(TtlFieldOffset);
            var maxIdle = initialFrame.Bytes.ReadLongL(MaxIdleFieldOffset);
            var key = DataCodec.Decode(iterator);
            var @value = DataCodec.Decode(iterator);

            iterator.SkipToStructEnd();
            return CustomTypeFactory.CreateSimpleEntryView(key, @value, cost, creationTime, expirationTime, hits, lastAccessTime, lastStoredTime, lastUpdateTime, version, ttl, maxIdle);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members
