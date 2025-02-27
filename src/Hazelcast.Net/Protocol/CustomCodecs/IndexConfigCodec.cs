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
    internal static class IndexConfigCodec
    {
        private const int TypeFieldOffset = 0;
        private const int InitialFrameSize = TypeFieldOffset + BytesExtensions.SizeOfInt;

        public static void Encode(ClientMessage clientMessage, Hazelcast.Models.IndexOptions indexConfig)
        {
            clientMessage.Append(Frame.CreateBeginStruct());

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            initialFrame.Bytes.WriteIntL(TypeFieldOffset, indexConfig.Type);
            clientMessage.Append(initialFrame);

            CodecUtil.EncodeNullable(clientMessage, indexConfig.Name, StringCodec.Encode);
            ListMultiFrameCodec.Encode(clientMessage, indexConfig.Attributes, StringCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, indexConfig.BitmapIndex, BitmapIndexOptionsCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, indexConfig.BTreeIndex, BTreeIndexConfigCodec.Encode);

            clientMessage.Append(Frame.CreateEndStruct());
        }

        public static Hazelcast.Models.IndexOptions Decode(IEnumerator<Frame> iterator)
        {
            // begin frame
            iterator.Take();

            var initialFrame = iterator.Take();
            var type = initialFrame.Bytes.ReadIntL(TypeFieldOffset);

            var name = CodecUtil.DecodeNullable(iterator, StringCodec.Decode);
            var attributes = ListMultiFrameCodec.Decode(iterator, StringCodec.Decode);
            var bitmapIndexOptions = CodecUtil.DecodeNullable(iterator, BitmapIndexOptionsCodec.Decode);
            var isBTreeIndexConfigExists = false;
            Hazelcast.Models.BTreeIndexOptions bTreeIndexConfig = default;
            if (iterator.NextIsNotTheEnd())
            {
                bTreeIndexConfig = CodecUtil.DecodeNullable(iterator, BTreeIndexConfigCodec.Decode);
                isBTreeIndexConfigExists = true;
            }

            iterator.SkipToStructEnd();
            return CustomTypeFactory.CreateIndexConfig(name, type, attributes, bitmapIndexOptions, isBTreeIndexConfigExists, bTreeIndexConfig);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members
