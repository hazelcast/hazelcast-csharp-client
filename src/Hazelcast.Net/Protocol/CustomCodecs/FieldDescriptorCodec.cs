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
//   Hazelcast Client Protocol Code Generator @c31f40c
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
    internal static class FieldDescriptorCodec
    {
        private const int KindFieldOffset = 0;
        private const int InitialFrameSize = KindFieldOffset + BytesExtensions.SizeOfInt;

        public static void Encode(ClientMessage clientMessage, Hazelcast.Serialization.Compact.SchemaField fieldDescriptor)
        {
            clientMessage.Append(Frame.CreateBeginStruct());

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            initialFrame.Bytes.WriteIntL(KindFieldOffset, fieldDescriptor.Kind);
            clientMessage.Append(initialFrame);

            StringCodec.Encode(clientMessage, fieldDescriptor.FieldName);

            clientMessage.Append(Frame.CreateEndStruct());
        }

        public static Hazelcast.Serialization.Compact.SchemaField Decode(IEnumerator<Frame> iterator)
        {
            // begin frame
            iterator.Take();

            var initialFrame = iterator.Take();
            var kind = initialFrame.Bytes.ReadIntL(KindFieldOffset);

            var fieldName = StringCodec.Decode(iterator);

            iterator.SkipToStructEnd();
            return CustomTypeFactory.CreateFieldDescriptor(fieldName, kind);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members
