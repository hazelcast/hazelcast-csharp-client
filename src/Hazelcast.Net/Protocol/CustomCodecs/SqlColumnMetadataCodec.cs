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
//   Hazelcast Client Protocol Code Generator @2a8aced
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
    internal static class SqlColumnMetadataCodec
    {
        private const int TypeFieldOffset = 0;
        private const int NullableFieldOffset = TypeFieldOffset + BytesExtensions.SizeOfInt;
        private const int InitialFrameSize = NullableFieldOffset + BytesExtensions.SizeOfBool;

        public static void Encode(ClientMessage clientMessage, Hazelcast.Sql.SqlColumnMetadata sqlColumnMetadata)
        {
            clientMessage.Append(Frame.CreateBeginStruct());

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            initialFrame.Bytes.WriteIntL(TypeFieldOffset, sqlColumnMetadata.Type);
            initialFrame.Bytes.WriteBoolL(NullableFieldOffset, sqlColumnMetadata.IsNullable);
            clientMessage.Append(initialFrame);

            StringCodec.Encode(clientMessage, sqlColumnMetadata.Name);

            clientMessage.Append(Frame.CreateEndStruct());
        }

        public static Hazelcast.Sql.SqlColumnMetadata Decode(IEnumerator<Frame> iterator)
        {
            // begin frame
            iterator.Take();

            var initialFrame = iterator.Take();
            var type = initialFrame.Bytes.ReadIntL(TypeFieldOffset);

            var isNullableExists = false;
            bool nullable = default;
            if (initialFrame.Bytes.Length >= NullableFieldOffset + BytesExtensions.SizeOfBool)
            {
                nullable = initialFrame.Bytes.ReadBoolL(NullableFieldOffset);
                isNullableExists = true;
            }
            var name = StringCodec.Decode(iterator);

            iterator.SkipToStructEnd();
            return CustomTypeFactory.CreateSqlColumnMetadata(name, type, isNullableExists, nullable);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members
