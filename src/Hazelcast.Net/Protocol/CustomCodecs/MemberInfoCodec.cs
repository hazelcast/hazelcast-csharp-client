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
//   Hazelcast Client Protocol Code Generator @5655fe9be
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
    internal static class MemberInfoCodec
    {
        private const int UuidFieldOffset = 0;
        private const int LiteMemberFieldOffset = UuidFieldOffset + BytesExtensions.SizeOfCodecGuid;
        private const int InitialFrameSize = LiteMemberFieldOffset + BytesExtensions.SizeOfBool;

        public static void Encode(ClientMessage clientMessage, Hazelcast.Models.MemberInfo memberInfo)
        {
            clientMessage.Append(Frame.CreateBeginStruct());

            var initialFrame = new Frame(new byte[InitialFrameSize]);
            initialFrame.Bytes.WriteGuidL(UuidFieldOffset, memberInfo.Uuid);
            initialFrame.Bytes.WriteBoolL(LiteMemberFieldOffset, memberInfo.IsLiteMember);
            clientMessage.Append(initialFrame);

            AddressCodec.Encode(clientMessage, memberInfo.Address);
            MapCodec.Encode(clientMessage, memberInfo.Attributes, StringCodec.Encode, StringCodec.Encode);
            MemberVersionCodec.Encode(clientMessage, memberInfo.Version);
            MapCodec.Encode(clientMessage, memberInfo.AddressMap, EndpointQualifierCodec.Encode, AddressCodec.Encode);

            clientMessage.Append(Frame.CreateEndStruct());
        }

        public static Hazelcast.Models.MemberInfo Decode(IEnumerator<Frame> iterator)
        {
            // begin frame
            iterator.Take();

            var initialFrame = iterator.Take();
            var uuid = initialFrame.Bytes.ReadGuidL(UuidFieldOffset);

            var liteMember = initialFrame.Bytes.ReadBoolL(LiteMemberFieldOffset);
            var address = AddressCodec.Decode(iterator);
            var attributes = MapCodec.Decode(iterator, StringCodec.Decode, StringCodec.Decode);
            var version = MemberVersionCodec.Decode(iterator);
            var isAddressMapExists = false;
            Dictionary<Hazelcast.Models.EndpointQualifier, Hazelcast.Networking.NetworkAddress> addressMap = default;
            if (iterator.NextIsNotTheEnd())
            {
                addressMap = MapCodec.Decode(iterator, EndpointQualifierCodec.Decode, AddressCodec.Decode);
                isAddressMapExists = true;
            }

            iterator.SkipToStructEnd();
            return new Hazelcast.Models.MemberInfo(address, uuid, attributes, liteMember, version, isAddressMapExists, addressMap);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members
