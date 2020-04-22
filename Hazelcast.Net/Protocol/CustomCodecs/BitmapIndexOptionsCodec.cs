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

// <auto-generated>
//   This code was generated by a tool.
//     Hazelcast Client Protocol Code Generator
//     https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members

namespace Hazelcast.Protocol.CustomCodecs
{
    internal static class BitmapIndexOptionsCodec
    {
        private const int UniqueKeyTransformationFieldOffset = 0;
        private const int InitialFrameSize = UniqueKeyTransformationFieldOffset + IntSizeInBytes;

        public static void Encode(ClientMessage clientMessage, Hazelcast.Configuration.BitmapIndexOptions bitmapIndexOptions)
        {
            clientMessage.Add(Frame.CreateBeginStruct());

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            EncodeInt(initialFrame, UniqueKeyTransformationFieldOffset, bitmapIndexOptions.UniqueKeyTransformation);
            clientMessage.Add(initialFrame);

            StringCodec.Encode(clientMessage, bitmapIndexOptions.UniqueKey);

            clientMessage.Add(Frame.CreateEndStruct());
        }

        public static Hazelcast.Configuration.BitmapIndexOptions Decode(IEnumerator<Frame> iterator)
        {
            // begin frame
            iterator.Take();

            var initialFrame = iterator.Take();
            var uniqueKeyTransformation = DecodeInt(initialFrame, UniqueKeyTransformationFieldOffset);

            var uniqueKey = StringCodec.Decode(iterator);

            iterator.SkipToStructEnd();

            return CustomTypeFactory.CreateBitmapIndexOptions(uniqueKey, uniqueKeyTransformation);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members