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
using Hazelcast.Core;
using Hazelcast.Messaging;

namespace Hazelcast.Protocol
{
    // FIXME MOVE THIS! it is a general utility also used outside of protocol!

    /// <summary>
    /// Defines constants and methods used in generated codecs to facilitate porting them,
    /// until their generator has been fully refactored and produced correct code.
    /// </summary>
    internal static class Portability
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

        public const int CharSizeInBytes = BytesExtensions.SizeOfChar;

        public const int ShortSizeInBytes = BytesExtensions.SizeOfInt16;

        public const int NullArray = -1;

        public static long CombineToLong(int x, int y)
        {
            return unchecked(((long)x << 32) | (y & 0xFFFFFFFFL));
        }

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

        public static IEnumerator<Frame> GetIterator(this ClientMessage message) => message.GetEnumerator();

        /// <summary>
        /// Takes the current frame and moves to the next frame.
        /// </summary>
        /// <returns>The current frame, or null if the end of the list has been reached.</returns>
        public static Frame Take(this IEnumerator<Frame> frames)
        {
            // if current is null maybe we haven't started yet - start
            // (if it's null because we've reached the end, nothing happens)
            if (frames.Current == null) frames.MoveNext();

            // capture and return the current frame, move to next
            var frame = frames.Current;
            frames.MoveNext();
            return frame;
        }

        /// <summary>
        /// Skips the current frame it is a "null frame".
        /// </summary>
        /// <returns></returns>
        public static bool SkipNull(this IEnumerator<Frame> frames)
        {
            var isNull = frames.Current != null && frames.Current.IsNull;
            if (isNull) frames.Take();
            return isNull;
        }

        /// <summary>
        /// Determines whether the current frame is an "end of structure" frame.
        /// </summary>
        public static bool AtStructEnd(this IEnumerator<Frame> frames) 
            => frames.Current != null && frames.Current.IsEndStruct;

        /// <summary>
        /// Advances the iterator by skipping all frames until the end of a structure.
        /// </summary>
        public static void SkipToStructEnd(this IEnumerator<Frame> frames)
        {
            // We are starting from 1 because of the BeginFrame we read
            // in the beginning of the Decode method
            var numberOfExpectedEndFrames = 1;

            while (numberOfExpectedEndFrames != 0)
            {
                var frame = frames.Take();
                if (frame == null)
                    throw new InvalidOperationException("Reached end of message.");

                if (frame.IsEndStruct)
                {
                    numberOfExpectedEndFrames--;
                }
                else if (frame.IsBeginStruct)
                {
                    numberOfExpectedEndFrames++;
                }
            }
        }
    }
}