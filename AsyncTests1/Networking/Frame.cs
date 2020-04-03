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
using System.Buffers;

namespace AsyncTests1.Networking
{
    /// <summary>
    /// Represents a message frame.
    /// </summary>
    /// <remarks>
    /// <para>There are different type of frames...
    /// TODO properly document frames
    /// </para>
    /// <para>length (int) | flags (ushort) | ...</para>
    /// <para>Frames are a linked list.</para>
    /// </remarks>
    public class Frame
    {
        /// <summary>
        /// Defines constants representing the size of frame elements.
        /// </summary>
        public static class SizeOf
        {
            /// <summary>
            /// Gets the size of the length field.
            /// </summary>
            public const int Length = sizeof(int);

            /// <summary>
            /// Gets the size of the flags field.
            /// </summary>
            public const int Flags = sizeof(ushort);

            /// <summary>
            /// Gets the size of the length+flags fields.
            /// </summary>
            public const int LengthAndFlags = Length + Flags;
        }

        /// <summary>
        /// Defines constants representing the offset of frame elements.
        /// </summary>
        public static class Offset
        {
            /// <summary>
            /// Gets the offset of the length field.
            /// </summary>
            public const int Length = 0;

            /// <summary>
            /// Gets the offset of the flags field.
            /// </summary>
            public const int Flags = SizeOf.Length;

            /// <summary>
            /// Gets the offset of the bytes array.
            /// </summary>
            public const int Bytes = Flags + SizeOf.Flags;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Frame"/> class.
        /// </summary>
        /// <param name="flags">The frame flags.</param>
        /// <param name="bytes">The frame bytes.</param>
        public Frame(FrameFlags flags, byte[] bytes)
        {
            Flags = flags;
            Bytes = bytes ?? Array.Empty<byte>();
        }

        /// <summary>
        /// Gets the null frame.
        /// </summary>
        public static readonly Frame Null = new Frame(FrameFlags.Null, Array.Empty<byte>());

        /// <summary>
        /// Gets the begin frame.
        /// </summary>
        public static readonly Frame Begin = new Frame(FrameFlags.Begin, Array.Empty<byte>());

        /// <summary>
        /// Gets the end frame.
        /// </summary>
        public static readonly Frame End = new Frame(FrameFlags.End, Array.Empty<byte>());

        /// <summary>
        /// Reads the length of the frame from a sequence of bytes, and slice the sequence accordingly.
        /// </summary>
        /// <param name="bytes">The sequence of bytes.</param>
        /// <param name="bigEndian">Whether to write using big-endian or not.</param>
        /// <returns>The length of the frame.</returns>
        public static int ReadLength(ref ReadOnlySequence<byte> bytes, bool bigEndian = false)
            => BytesExtensions.ReadInt32(ref bytes, bigEndian);

        /// <summary>
        /// Reads the flags of the frame from a sequence of bytes, and slice the sequence accordingly.
        /// </summary>
        /// <param name="bytes">The sequence of bytes.</param>
        /// <param name="bigEndian">Whether to write using big-endian or not.</param>
        /// <returns>The flags of the frame.</returns>
        public static FrameFlags ReadFlags(ref ReadOnlySequence<byte> bytes, bool bigEndian = false)
            => (FrameFlags) BytesExtensions.ReadUInt16(ref bytes, bigEndian);

        /// <summary>
        /// Writes the length and flags of the frame to an array of bytes.
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <param name="bigEndian">Whether to write using big-endian or not.</param>
        public void WriteLengthAndFlags(byte[] bytes, bool bigEndian = false)
        {
            bytes.WriteInt32(0, Length, bigEndian);
            bytes.WriteUInt16(SizeOf.Length, (ushort) Flags, bigEndian);
        }

        /// <summary>
        /// Gets the frame flags.
        /// </summary>
        public FrameFlags Flags { get; }

        /// <summary>
        /// Gets the frame bytes.
        /// </summary>
        public byte[] Bytes { get; }

        /// <summary>
        /// Gets the next frame.
        /// </summary>
        public Frame Next { get; set; }

        /// <summary>
        /// Gets the frame length (including length and flags fields).
        /// </summary>
        public int Length => SizeOf.Length + SizeOf.Flags + (Bytes?.Length ?? 0);

        /// <summary>
        /// Determines whether the frame is an end frame.
        /// </summary>
        public bool IsEnd => Flags.Has(FrameFlags.End);

        /// <summary>
        /// Determines whether the frame is a begin frame.
        /// </summary>
        public bool IsBegin => Flags.Has(FrameFlags.Begin);

        /// <summary>
        /// Determines whether the frame is a null frame.
        /// </summary>
        public bool IsNull => Flags.Has(FrameFlags.Null);

        /// <summary>
        /// Determines whether the frame is a final frame.
        /// </summary>
        public bool IsFinal => Flags.Has(FrameFlags.Final);

        /// <summary>
        /// Shallow clone the frame.
        /// </summary>
        /// <returns>A shallow clone of the frame.</returns>
        /// <remarks>
        /// <para>The shallow clone of a frame shares the byte array.</para>
        /// </remarks>
        public Frame ShallowClone() => new Frame(Flags, Bytes);

        /// <summary>
        /// Deep clone the frame.
        /// </summary>
        /// <returns>A deep clone of the frame.</returns>
        /// <remarks>
        /// <para>THe deep clone of a frame contains a copy of the original byte array.</para>
        /// </remarks>
        public Frame DeepClone()
        {
            var bytes = new byte[Bytes.Length];
            Buffer.BlockCopy(Bytes, 0, bytes, 0, bytes.Length);
            return new Frame(Flags, bytes);
        }
    }
}