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
using Hazelcast.Exceptions;

namespace Hazelcast.Core
{
    internal static partial class BytesExtensions // Read from ReadOnlySpan
    {
        /// <summary>
        /// Reads a <see cref="short"/> value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static short ReadShort(this ReadOnlySpan<byte> bytes, Endianness endianness)
        {
            const byte length = sizeof(ushort);

            if (bytes.Length < length)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            unchecked
            {
                return (short)(endianness.IsBigEndian()

                    ? bytes[0] << 8 | bytes[1]
                    : bytes[0]      | bytes[1] << 8);
            }
        }

        /// <summary>
        /// Reads an <see cref="ushort"/> value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static ushort ReadUShort(this ReadOnlySpan<byte> bytes, Endianness endianness)
        {
            const byte length = sizeof(ushort);

            if (bytes.Length < length)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            unchecked
            {
                return (ushort)(endianness.IsBigEndian()

                    ? bytes[0] << 8 | bytes[1]
                    : bytes[0]      | bytes[1] << 8);
            }
        }

        /// <summary>
        /// Reads an <see cref="int"/> value from a span of bytes.
        /// </summary>
        /// <param name="bytes">The span of bytes to read from.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        public static int ReadInt(this ReadOnlySpan<byte> bytes, Endianness endianness)
        {
            const byte length = sizeof(int);

            if (bytes.Length < length)
                throw new ArgumentException(ExceptionMessages.NotEnoughBytes, nameof(bytes));

            return endianness.IsBigEndian()

                ? bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8  | bytes[3]
                : bytes[0]       | bytes[1] << 8  | bytes[2] << 16 | bytes[3] << 24;
        }
    }
}
