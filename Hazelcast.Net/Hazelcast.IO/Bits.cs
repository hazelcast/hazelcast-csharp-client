// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using System.IO;

namespace Hazelcast.IO
{
    /// <summary>Access and manipulate bits, bytes, primitives ...</summary>
    internal sealed class Bits
    {
        /// <summary>Byte size in bytes</summary>
        public const int ByteSizeInBytes = 1;

        /// <summary>Boolean size in bytes</summary>
        public const int BooleanSizeInBytes = 1;

        /// <summary>Short size in bytes</summary>
        public const int ShortSizeInBytes = 2;

        /// <summary>Char size in bytes</summary>
        public const int CharSizeInBytes = 2;

        /// <summary>Integer size in bytes</summary>
        public const int IntSizeInBytes = 4;

        /// <summary>Float size in bytes</summary>
        public const int FloatSizeInBytes = 4;

        /// <summary>Long size in bytes</summary>
        public const int LongSizeInBytes = 8;

        /// <summary>Double size in bytes</summary>
        public const int DoubleSizeInBytes = 8;

        /// <summary>Length of the data blocks used by the CPU cache sub-system in bytes.</summary>
        public const int CacheLineLength = 64;

        /// <summary>for null arrays, this value writen to stream to represent null array size.</summary>
        public const int NullArray = -1;

        /// <summary>Clears n-th bit of the byte value</summary>
        /// <param name="value">byte value</param>
        /// <param name="bit">n-th bit</param>
        /// <returns>value</returns>
        public static byte ClearBit(byte value, int bit)
        {
            return unchecked((byte) (value & ~(1 << bit)));
        }

        /// <summary>Clears n-th bit of the integer value</summary>
        /// <param name="value">integer value</param>
        /// <param name="bit">n-th bit</param>
        /// <returns>value</returns>
        public static int ClearBit(int value, int bit)
        {
            return unchecked(value & ~(1 << bit));
        }

        /// <summary>Combines two short integer values into an integer.</summary>
        public static int CombineToInt(short x, short y)
        {
            return unchecked((x << 16) | (y & 0xFFFF));
        }

        /// <summary>Combines two integer values into a long integer.</summary>
        public static long CombineToLong(int x, int y)
        {
            return unchecked(((long) x << 32) | (y & 0xFFFFFFFFL));
        }

        public static int ExtractInt(long value, bool lowerBits)
        {
            return unchecked((int) ((lowerBits) ? value : (value >> 32)));
        }

        public static short ExtractShort(int value, bool lowerBits)
        {
            return (short) ((lowerBits) ? value : (value >> 16));
        }

        /// <summary>Inverts n-th bit of the byte value</summary>
        /// <param name="value">byte value</param>
        /// <param name="bit">n-th bit</param>
        /// <returns>value</returns>
        public static byte InvertBit(byte value, int bit)
        {
            return unchecked((byte) (value ^ (1 << bit)));
        }

        /// <summary>Inverts n-th bit of the integer value</summary>
        /// <param name="value">integer value</param>
        /// <param name="bit">n-th bit</param>
        /// <returns>value</returns>
        public static int InvertBit(int value, int bit)
        {
            return unchecked(value ^ (1 << bit));
        }

        /// <summary>Returns true if n-th bit of the value is set, false otherwise</summary>
        public static bool IsBitSet(int value, int bit)
        {
            return unchecked((value & 1 << bit) != 0);
        }

        public static char ReadChar(byte[] buffer, int pos, bool bigEndian)
        {
            return bigEndian ? ReadCharB(buffer, pos) : ReadCharL(buffer, pos);
        }

        public static char ReadCharB(byte[] buffer, int pos)
        {
            return unchecked((char)(buffer[pos + 1] | buffer[pos] << 8));
        }

        public static char ReadCharL(byte[] buffer, int pos)
        {
            return unchecked((char)(buffer[pos] | buffer[pos + 1] << 8));
        }

        public static int ReadInt(byte[] buffer, int pos, bool bigEndian)
        {
            return bigEndian ? ReadIntB(buffer, pos) : ReadIntL(buffer, pos);
        }

        public static int ReadIntB(byte[] buffer, int pos)
        {
            unchecked
            {
                return buffer[pos] << 24 | 
                       buffer[pos + 1] << 16 | 
                       buffer[pos + 2] << 8 | 
                       buffer[pos + 3];
            }
        }

        public static int ReadIntL(byte[] buffer, int pos)
        {
            unchecked
            {
                return buffer[pos] | 
                       buffer[pos + 1] << 8 | 
                       buffer[pos + 2] << 16 | 
                       buffer[pos + 3] << 24;
            }
        }

        public static long ReadLong(byte[] buffer, int pos, bool bigEndian)
        {
            return bigEndian ? ReadLongB(buffer, pos) : ReadLongL(buffer, pos);
        }

        public static long ReadLongB(byte[] buffer, int pos)
        {
            unchecked
            {
                return (long)buffer[pos] << 56 |
                       (long)buffer[pos + 1] << 48 |
                       (long)buffer[pos + 2] << 40 |
                       (long)buffer[pos + 3] << 32 |
                       (long)buffer[pos + 4] << 24 |
                       (long)buffer[pos + 5] << 16 |
                       (long)buffer[pos + 6] << 8 |
                       buffer[pos + 7];
            }
        }

        public static long ReadLongL(byte[] buffer, int pos)
        {
            unchecked
            {
                return buffer[pos] |
                       (long)buffer[pos + 1] << 8 |
                       (long)buffer[pos + 2] << 16 |
                       (long)buffer[pos + 3] << 24 |
                       (long)buffer[pos + 4] << 32 |
                       (long)buffer[pos + 5] << 40 |
                       (long)buffer[pos + 6] << 48 |
                       (long)buffer[pos + 7] << 56;
            }
        }

        public static short ReadShort(byte[] buffer, int pos, bool bigEndian)
        {
            return bigEndian ? ReadShortB(buffer, pos) : ReadShortL(buffer, pos);
        }

        public static short ReadShortB(byte[] buffer, int pos)
        {
            return unchecked((short)(buffer[pos] << 8 | buffer[pos + 1]));
        }

        public static short ReadShortL(byte[] buffer, int pos)
        {
            return unchecked((short)(buffer[pos] | buffer[pos + 1] << 8));
        }

        public static char ReadUtf8Char(IDataInput input, byte firstByte)
        {
            var b = firstByte & 0xFF;
            switch (b >> 4)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    return (char) b;
                case 12:
                case 13:
                    var first = (b & 0x1F) << 6;
                    var second = input.ReadByte() & 0x3F;
                    return (char) (first | second);
                case 14:
                    var first2 = (b & 0x0F) << 12;
                    var second2 = (input.ReadByte() & 0x3F) << 6;
                    var third2 = input.ReadByte() & 0x3F;
                    return (char) (first2 | second2 | third2);
                default:
                    throw new InvalidDataException("Malformed byte sequence");
            }
        }

//        /// <summary>Sets n-th bit of the byte value</summary>
//        /// <param name="value">byte value</param>
//        /// <param name="bit">n-th bit</param>
//        /// <returns>value</returns>
//        public static byte SetBit(byte value, int bit)
//        {
//            return unchecked((byte) (value | (1 << bit)));
//        }
//
//        /// <summary>Sets n-th bit of the integer value</summary>
//        /// <param name="value">integer value</param>
//        /// <param name="bit">n-th bit</param>
//        /// <returns>value</returns>
//        public static int SetBit(int value, int bit)
//        {
//            return unchecked(value | (1 << bit));
//        }

        public static void WriteChar(byte[] buffer, int pos, char v, bool bigEndian)
        {
            if (bigEndian)
            {
                WriteCharB(buffer, pos, v);
            }
            else
            {
                WriteCharL(buffer, pos, v);
            }
        }

        public static void WriteCharB(byte[] buffer, int pos, char v)
        {
            unchecked
            {
                buffer[pos] = (byte) (v >> 8);
                buffer[pos + 1] = (byte) v;
            }
        }

        public static void WriteCharL(byte[] buffer, int pos, char v)
        {
            unchecked
            {
                buffer[pos] = (byte)v;
                buffer[pos + 1] = (byte)(v >> 8);
            }
        }

        public static void WriteInt(byte[] buffer, int pos, int v, bool bigEndian)
        {
            if (bigEndian)
            {
                WriteIntB(buffer, pos, v);
            }
            else
            {
                WriteIntL(buffer, pos, v);
            }
        }

        public static void WriteIntB(byte[] buffer, int pos, int v)
        {
            unchecked
            {
                uint unsignedValue = (uint)v;
                buffer[pos] = (byte)(unsignedValue >> 24);
                buffer[pos + 1] = (byte)(unsignedValue >> 16);
                buffer[pos + 2] = (byte)(unsignedValue >>8);
                buffer[pos + 3] = (byte)unsignedValue;
            }
        }

        public static void WriteIntL(byte[] buffer, int pos, int v)
        {
            
            unchecked
            {
                uint unsignedValue = (uint)v;
                buffer[pos] = (byte)unsignedValue;
                buffer[pos + 1] = (byte)(unsignedValue >> 8);
                buffer[pos + 2] = (byte)(unsignedValue >> 16);
                buffer[pos + 3] = (byte)(unsignedValue >> 24);
            }
        }

        public static void WriteLong(byte[] buffer, int pos, long v, bool bigEndian)
        {
            if (bigEndian)
            {
                WriteLongB(buffer, pos, v);
            }
            else
            {
                WriteLongL(buffer, pos, v);
            }
        }

        public static void WriteLongB(byte[] buffer, int pos, long v)
        {
            unchecked
            {
                ulong unsignedValue = (ulong)v;
                buffer[pos] = (byte)(unsignedValue >> 56);
                buffer[pos + 1] = (byte)(unsignedValue >> 48);
                buffer[pos + 2] = (byte)(unsignedValue >> 40);
                buffer[pos + 3] = (byte)(unsignedValue >> 32);
                buffer[pos + 4] = (byte)(unsignedValue >> 24);
                buffer[pos + 5] = (byte)(unsignedValue >> 16);
                buffer[pos + 6] = (byte)(unsignedValue >> 8);
                buffer[pos + 7] = (byte)unsignedValue;
            }
        }

        public static void WriteLongL(byte[] buffer, int pos, long v)
        {
            unchecked
            {
                ulong unsignedValue = (ulong)v;
                buffer[pos] = (byte)unsignedValue;
                buffer[pos + 1] = (byte)(unsignedValue >> 8);  
                buffer[pos + 2] = (byte)(unsignedValue >> 16);
                buffer[pos + 3] = (byte)(unsignedValue >> 24);
                buffer[pos + 4] = (byte)(unsignedValue >> 32);
                buffer[pos + 5] = (byte)(unsignedValue >> 40);
                buffer[pos + 6] = (byte)(unsignedValue >> 48);
                buffer[pos + 7] = (byte)(unsignedValue >> 56);
            }
        }

        public static void WriteShort(byte[] buffer, int pos, short v, bool bigEndian)
        {
            if (bigEndian)
            {
                WriteShortB(buffer, pos, v);
            }
            else
            {
                WriteShortL(buffer, pos, v);
            }
        }

        public static void WriteShortB(byte[] buffer, int pos, short v)
        {
            unchecked
            {
                buffer[pos] = (byte)((ushort)v >> 8);
                buffer[pos + 1] = (byte)v;
            }
        }

        public static void WriteShortL(byte[] buffer, int pos, short v)
        {
            unchecked
            {
                buffer[pos] = (byte)v;
                buffer[pos + 1] = (byte)((ushort)v >> 8);
            }
        }

        public static int WriteUtf8Char(byte[] buffer, int pos, int c)
        {
            if (c <= 0x007F)
            {
                buffer[pos] = (byte) c;
                return 1;
            }
            if (c > 0x07FF)
            {
                buffer[pos] = (byte) (0xE0 | c >> 12 & 0x0F);
                buffer[pos + 1] = (byte) (0x80 | c >> 6 & 0x3F);
                buffer[pos + 2] = (byte) (0x80 | c & 0x3F);
                return 3;
            }
            buffer[pos] = (byte) (0xC0 | c >> 6 & 0x1F);
            buffer[pos + 1] = (byte) (0x80 | c & 0x3F);
            return 2;
        }
    }
}