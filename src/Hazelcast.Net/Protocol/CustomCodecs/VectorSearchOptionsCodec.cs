﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
//   Hazelcast Client Protocol Code Generator @2d41b6a
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
    internal static class VectorSearchOptionsCodec
    {
        private const int IncludeValueFieldOffset = 0;
        private const int IncludeVectorsFieldOffset = IncludeValueFieldOffset + BytesExtensions.SizeOfBool;
        private const int LimitFieldOffset = IncludeVectorsFieldOffset + BytesExtensions.SizeOfBool;
        private const int InitialFrameSize = LimitFieldOffset + BytesExtensions.SizeOfInt;

        public static void Encode(ClientMessage clientMessage, Hazelcast.Models.VectorSearchOptions vectorSearchOptions)
        {
            clientMessage.Append(Frame.CreateBeginStruct());

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            initialFrame.Bytes.WriteBoolL(IncludeValueFieldOffset, vectorSearchOptions.IncludeValue);
            initialFrame.Bytes.WriteBoolL(IncludeVectorsFieldOffset, vectorSearchOptions.IncludeVectors);
            initialFrame.Bytes.WriteIntL(LimitFieldOffset, vectorSearchOptions.Limit);
            clientMessage.Append(initialFrame);

            MapCodec.EncodeNullable(clientMessage, vectorSearchOptions.Hints, StringCodec.Encode, StringCodec.Encode);

            clientMessage.Append(Frame.CreateEndStruct());
        }

        public static Hazelcast.Models.VectorSearchOptions Decode(IEnumerator<Frame> iterator)
        {
            // begin frame
            iterator.Take();

            var initialFrame = iterator.Take();
            var includeValue = initialFrame.Bytes.ReadBoolL(IncludeValueFieldOffset);

            var includeVectors = initialFrame.Bytes.ReadBoolL(IncludeVectorsFieldOffset);
            var limit = initialFrame.Bytes.ReadIntL(LimitFieldOffset);
            var hints = MapCodec.DecodeNullable(iterator, StringCodec.Decode, StringCodec.Decode);

            iterator.SkipToStructEnd();
            return CustomTypeFactory.CreateVectorSearchOptions(includeValue, includeVectors, limit, hints);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members
