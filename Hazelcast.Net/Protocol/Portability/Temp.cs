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
using Hazelcast.Core;
using Hazelcast.Messaging;

namespace Hazelcast.Protocol.Portability
{
    /// <summary>
    /// Defines constants and methods used in generated codecs to facilitate porting them,
    /// until their generator has been fully refactored and produced correct code.
    /// </summary>
    public static class Temp
    {
        public const FrameFlags UnfragmentedMessage = (FrameFlags) ClientMessageFlags.Unfragmented;

        public const int PartitionIdFieldOffset = Messaging.FrameFields.Offset.PartitionId;

        public const int TypeFieldOffset = Messaging.FrameFields.Offset.MessageType;

        public const int ResponseBackupAcksFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks;

        public const int LongSizeInBytes = BytesExtensions.SizeOfInt64;

        public const int IntSizeInBytes = BytesExtensions.SizeOfInt32;

        public const int ByteSizeInBytes = BytesExtensions.SizeOfByte;

        public const int GuidSizeInBytes = BytesExtensions.SizeOfGuid;

        public const int BoolSizeInBytes = BytesExtensions.SizeOfBool;

        public static void EncodeGuid(Frame frame, int position, Guid value)
            => frame.Bytes.WriteGuid(position, value);

        public static Guid DecodeGuid(Frame frame, int position)
            => frame.Bytes.ReadGuid(position);

        public static void EncodeBool(Frame frame, int position, bool value)
            => frame.Bytes.WriteBool(position, value);

        public static bool DecodeBool(Frame frame, int position)
            => frame.Bytes.ReadBool(position);

        public static void EncodeInt(Frame frame, int position, int value)
            => frame.Bytes.WriteInt32(position, value);

        public static void EncodeInt(Frame frame, int position, Enum value)
        {
            var type = Enum.GetUnderlyingType(value.GetType());
            if (type != typeof(int)) throw new NotSupportedException();
            frame.Bytes.WriteInt32(position, (int)(object)value);
        }

        public static int DecodeInt(Frame frame, int position)
            => frame.Bytes.ReadInt32(position);

        public static void EncodeLong(Frame frame, int position, long value)
            => frame.Bytes.WriteInt64(position, value);

        public static long DecodeLong(Frame frame, int position)
            => frame.Bytes.ReadInt64(position);

        public static void EncodeByte(Frame frame, int position, byte value)
            => frame.Bytes.WriteByte(position, value);

        public static byte DecodeByte(Frame frame, int position)
            => frame.Bytes.ReadByte(position);

        public static ClientMessage CreateForEncode() => new ClientMessage();

        public static void Add(this ClientMessage message, Frame frame) => message.Append(frame);

        // can we avoid allocating iterators?
        public static FrameIterator GetIterator(this ClientMessage message) => new FrameIterator(message);
    }
}