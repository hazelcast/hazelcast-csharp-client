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
using Hazelcast.Messaging;
using Hazelcast.Logging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;
using Hazelcast.Protocol.Portability;
using static Hazelcast.Protocol.Portability.Temp;

// <auto-generated>
//   This code was generated by a tool.
//     Hazelcast Client Protocol Code Generator
//     https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members

namespace Hazelcast.Protocol.CustomCodecs
{
    internal static class MemberVersionCodec
    {
        private const int MajorFieldOffset = 0;
        private const int MinorFieldOffset = MajorFieldOffset + ByteSizeInBytes;
        private const int PatchFieldOffset = MinorFieldOffset + ByteSizeInBytes;
        private const int InitialFrameSize = PatchFieldOffset + ByteSizeInBytes;

        public static void Encode(ClientMessage clientMessage, Hazelcast.Clustering.MemberVersion memberVersion)
        {
            clientMessage.Add(Frame.CreateBeginStruct());

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            EncodeByte(initialFrame, MajorFieldOffset, memberVersion.Major);
            EncodeByte(initialFrame, MinorFieldOffset, memberVersion.Minor);
            EncodeByte(initialFrame, PatchFieldOffset, memberVersion.Patch);
            clientMessage.Add(initialFrame);

            clientMessage.Add(Frame.CreateEndStruct());
        }

        public static Hazelcast.Clustering.MemberVersion Decode(FrameIterator iterator)
        {
            // begin frame
            iterator.Next();

            var initialFrame = iterator.Next();
            var major = DecodeByte(initialFrame, MajorFieldOffset);
            var minor = DecodeByte(initialFrame, MinorFieldOffset);
            var patch = DecodeByte(initialFrame, PatchFieldOffset);

            CodecUtil.FastForwardToEndFrame(iterator);

            return new Hazelcast.Clustering.MemberVersion(major, minor, patch);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members