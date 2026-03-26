// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Runtime.InteropServices;
using System.Threading;
using Hazelcast.Core;

namespace Hazelcast.Messaging
{
    /// <summary>
    /// Represents a message frame.
    /// </summary>
    /// <remarks>
    /// <para>A frame is composed of its length (int), its flags (ushort), and its payload (byte[]). What
    /// the payload contains depend on the type of the frame and of the <see cref="ClientMessage"/> containing
    /// the frame.</para>
    /// <para>Frames form a linked list through their <see cref="Next"/> property, with the
    /// <see cref="ClientMessage"/> keeping track of the first and last frame of the list.</para>
    /// </remarks>
    internal class Frame : IDisposable
    {
        private IDisposable _owner;

        /// <summary>
        /// Gets the owner whose lifetime is tied to this frame, or <c>null</c> if none.
        /// Used by the send path to detect pre-framed <see cref="Hazelcast.Serialization.HeapData"/>
        /// and route to the zero-copy send path.
        /// </summary>
        internal IDisposable Owner => _owner;
        /// <summary>
        /// Initializes a new instance of the <see cref="Frame"/> class representing an empty frame.
        /// </summary>
        /// <param name="flags">The frame flags.</param>
        public Frame(FrameFlags flags = FrameFlags.Default)
        {
            Flags = flags;
            Bytes = Memory<byte>.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Frame"/> class.
        /// </summary>
        /// <param name="bytes">The frame bytes.</param>
        /// <param name="flags">The frame flags.</param>
        public Frame(byte[] bytes, FrameFlags flags = FrameFlags.Default)
        {
            Flags = flags;
            Bytes = new Memory<byte>(bytes);
        }
        
        public Frame(Memory<byte> memoryBytes, FrameFlags flags = FrameFlags.Default)
        {
            Flags = flags;
            Bytes = memoryBytes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Frame"/> class from read-only memory.
        /// </summary>
        /// <param name="bytes">The frame bytes. Must be array-backed.</param>
        /// <param name="flags">The frame flags.</param>
        /// <param name="owner">An optional owner whose lifetime is tied to this frame; disposed when the frame is disposed.</param>
        public Frame(ReadOnlyMemory<byte> bytes, FrameFlags flags = FrameFlags.Default, IDisposable owner = null)
        {
            Flags = flags;
            Bytes = MemoryMarshal.AsMemory(bytes);
            _owner = owner;
        }

        /// <summary>
        /// Creates a new null frame.
        /// </summary>
        /// <returns></returns>
        public static Frame CreateNull() => new Frame(FrameFlags.Null);

        /// <summary>
        /// Creates a new structure begin frame.
        /// </summary>
        public static Frame CreateBeginStruct() => new Frame(FrameFlags.BeginStruct);

        /// <summary>
        /// Creates a new structure end frame.
        /// </summary>
        public static Frame CreateEndStruct() => new Frame(FrameFlags.EndStruct);

        /// <summary>
        /// Reads the length of the frame from a sequence of bytes, and slice the sequence accordingly.
        /// </summary>
        /// <param name="bytes">The sequence of bytes.</param>
        /// <returns>The length of the frame.</returns>
        public static int ReadLength(ref ReadOnlySequence<byte> bytes)
            => BytesExtensions.ReadInt(ref bytes, Endianness.LittleEndian);

        /// <summary>
        /// Reads the flags of the frame from a sequence of bytes, and slice the sequence accordingly.
        /// </summary>
        /// <param name="bytes">The sequence of bytes.</param>
        /// <returns>The flags of the frame.</returns>
        public static FrameFlags ReadFlags(ref ReadOnlySequence<byte> bytes)
            => (FrameFlags) BytesExtensions.ReadUShort(ref bytes, Endianness.LittleEndian);

        /// <summary>
        /// Writes the length and flags of the frame to an array of bytes.
        /// </summary>
        /// <param name="bytes">An array of bytes.</param>
        /// <param name="position">The position.</param>
        public void WriteLengthAndFlags(byte[] bytes, int position = 0)
        {
            bytes.WriteInt(position, Length, Endianness.LittleEndian);
            bytes.WriteUShort(position + FrameFields.SizeOf.Length, (ushort) Flags, Endianness.LittleEndian);
        }

        /// <summary>
        /// Gets or sets the frame flags.
        /// </summary>
        /// <remarks>
        /// <para>Message flags and Frame flags are carried by the same field.</para>
        /// </remarks>
        public FrameFlags Flags { get; set;  }

        /// <summary>
        /// Gets the frame bytes.
        /// </summary>
        public Memory<byte> Bytes { get; }


        /// <summary>
        /// Gets the next frame.
        /// </summary>
        public Frame Next { get; set; }

        /// <summary>
        /// Gets the frame length (including length and flags fields).
        /// </summary>
        public int Length => FrameFields.SizeOf.Length + FrameFields.SizeOf.Flags + Bytes.Length;

        /// <summary>
        /// Determines whether the frame is a structure end frame.
        /// </summary>
        public bool IsEndStruct => Flags.HasAll(FrameFlags.EndStruct);

        /// <summary>
        /// Determines whether the frame is a structure begin frame.
        /// </summary>
        public bool IsBeginStruct => Flags.HasAll(FrameFlags.BeginStruct);

        /// <summary>
        /// Determines whether the frame is a null frame.
        /// </summary>
        public bool IsNull => Flags.HasAll(FrameFlags.Null);

        /// <summary>
        /// Determines whether the frame is a final frame.
        /// </summary>
        public bool IsFinal => Flags.HasAll(FrameFlags.Final);

        /// <summary>
        /// Shallow clone the frame.
        /// </summary>
        /// <returns>A shallow clone of the frame.</returns>
        /// <remarks>
        /// <para>The shallow clone of a frame shares the byte array.</para>
        /// </remarks>
        public Frame ShallowClone() => new Frame(Bytes, Flags);

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
            Bytes.CopyTo(bytes);
            return new Frame(bytes, Flags);
        }

        /// <summary>
        /// Disposes the frame, releasing any owned resource.
        /// </summary>
        public void Dispose()
        {
            Interlocked.Exchange(ref _owner, null)?.Dispose();
        }

        /// <inheritdoc />
        public override string ToString()
            => $"{{Frame: {Length} bytes, {Flags.ToBetterString()} (0x{Flags:x})}}";
    }
}
