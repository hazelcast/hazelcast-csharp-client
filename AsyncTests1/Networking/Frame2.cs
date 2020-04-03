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
    public class Frame2
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
            // structure is
            // length (int) | flags (ushort) | ...

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

        public Frame2(FrameFlags2 flags, byte[] bytes)
        {
            Flags = flags;
            Bytes = bytes;
        }

        public static readonly Frame2 Null = new Frame2(FrameFlags2.Null, Array.Empty<byte>());

        public static readonly Frame2 Begin = new Frame2(FrameFlags2.Begin, Array.Empty<byte>());

        public static readonly Frame2 End = new Frame2(FrameFlags2.End, Array.Empty<byte>());

        public static int ReadLength(ref ReadOnlySequence<byte> bytes)
            => BytesExtensions.ReadInt32(ref bytes);

        public static FrameFlags2 ReadFlags(ref ReadOnlySequence<byte> bytes)
            => (FrameFlags2) BytesExtensions.ReadUInt16(ref bytes);

        public void WriteLengthAndFlags(byte[] bytes, bool bigEndian = false)
        {
            bytes.WriteInt32(0, Length, bigEndian);
            bytes.WriteUInt16(SizeOf.Length, (ushort) Flags, bigEndian);
        }

        public FrameFlags2 Flags { get; }

        public byte[] Bytes { get; }

        public Frame2 Next { get; set; }

        public int Length => SizeOf.Length + SizeOf.Flags + (Bytes?.Length ?? 0);

        public bool IsEnd => Flags.Has(FrameFlags2.End);

        public bool IsBegin => Flags.Has(FrameFlags2.Begin);

        public bool IsNull => Flags.Has(FrameFlags2.Null);

        public bool IsFinal => Flags.Has(FrameFlags2.Final);

        public Frame2 ShallowClone() => new Frame2(Flags, Bytes); // next???

        public Frame2 DeepClone()
        {
            var bytes = new byte[Bytes.Length];
            Buffer.BlockCopy(Bytes, 0, bytes, 0, bytes.Length);
            return new Frame2(Flags, bytes); // next???
        }
    }
}