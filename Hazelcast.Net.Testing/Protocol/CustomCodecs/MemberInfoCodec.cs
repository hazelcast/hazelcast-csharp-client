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
// ReSharper disable CheckNamespace

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
using static Hazelcast.Messaging.Portability;

namespace Hazelcast.Protocol.CustomCodecs
{
    internal static class MemberInfoServerCodec
    {
        private const int UuidFieldOffset = 0;
        private const int LiteMemberFieldOffset = UuidFieldOffset + GuidSizeInBytes;
        private const int InitialFrameSize = LiteMemberFieldOffset + BoolSizeInBytes;

        public static void Encode(ClientMessage clientMessage, Hazelcast.Data.MemberInfo memberInfo)
        {
            clientMessage.Add(Frame.CreateBeginStruct());

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            EncodeGuid(initialFrame, UuidFieldOffset, memberInfo.Uuid);
            EncodeBool(initialFrame, LiteMemberFieldOffset, memberInfo.IsLiteMember);
            clientMessage.Add(initialFrame);

            AddressCodec.Encode(clientMessage, memberInfo.Address);
            MapCodec.Encode(clientMessage, memberInfo.Attributes, StringCodec.Encode, StringCodec.Encode);
            MemberVersionCodec.Encode(clientMessage, memberInfo.Version);

            clientMessage.Add(Frame.CreateEndStruct());
        }

        public static Hazelcast.Data.MemberInfo Decode(IEnumerator<Frame> iterator)
        {
            // begin frame
            iterator.Take();

            var initialFrame = iterator.Take();
            var uuid = DecodeGuid(initialFrame, UuidFieldOffset);
            var liteMember = DecodeBool(initialFrame, LiteMemberFieldOffset);

            var address = AddressCodec.Decode(iterator);
            var attributes = MapCodec.Decode(iterator, StringCodec.Decode, StringCodec.Decode);
            var version = MemberVersionCodec.Decode(iterator);

            iterator.SkipToStructEnd();

            return new Hazelcast.Data.MemberInfo(address, uuid, attributes, liteMember, version);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members