// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Core
{
    internal static partial class BytesExtensions // Protocol
    {
        // use little endian where it makes sense
        // for some types (byte, bool...) it just does not make sense
        // for some types (guid...) endianness is just not an option

        public static void WriteLongL(this byte[] bytes, int position, long value)
            => bytes.WriteLong(position, value, Endianness.LittleEndian);

        public static void WriteIntL(this byte[] bytes, int position, int value)
            => bytes.WriteInt(position, value, Endianness.LittleEndian);

        public static void WriteIntL(this byte[] bytes, int position, Enum value)
            => bytes.WriteInt(position, (int) (object) value, Endianness.LittleEndian);

        public static void WriteBoolL(this byte[] bytes, int position, bool value)
            => bytes.WriteBool(position, value);

        public static void WriteGuidL(this byte[] bytes, int position, Guid value)
            => bytes.WriteGuid(position, value);

        public static void WriteByteL(this byte[] bytes, int position, byte value)
            => bytes.WriteByte(position, value);

        public static short ReadShortL(this byte[] bytes, int position)
            => bytes.ReadShort(position, Endianness.LittleEndian);

        public static long ReadLongL(this byte[] bytes, int position)
            => bytes.ReadLong(position, Endianness.LittleEndian);

        public static int ReadIntL(this byte[] bytes, int position)
            => bytes.ReadInt(position, Endianness.LittleEndian);

        public static float ReadFloatL(this byte[] bytes, int position)
            => bytes.ReadFloat(position, Endianness.LittleEndian);

        public static double ReadDoubleL(this byte[] bytes, int position)
            => bytes.ReadDouble(position, Endianness.LittleEndian);

        public static bool ReadBoolL(this byte[] bytes, int position)
            => bytes.ReadBool(position);

        public static Guid ReadGuidL(this byte[] bytes, int position)
            => bytes.ReadGuid(position);

        public static byte ReadByteL(this byte[] bytes, int position)
            => bytes.ReadByte(position);

        private static Guid ReadGuid(this byte[] bytes, int position)
        {
            // read the 'empty' bool
            if (bytes.ReadBool(position)) return Guid.Empty;

            // not 'empty', must be able to read a full guid
            if (bytes.Length < position + SizeOfGuid)
                throw new ArgumentOutOfRangeException(nameof(position));

            position += SizeOfByte;

            // ReSharper disable once UseObjectOrCollectionInitializer
#pragma warning disable IDE0017 // Simplify object initialization
            var v = new JavaUuidOrder();
#pragma warning restore IDE0017 // Simplify object initialization

            v.X0 = bytes[position]; position += SizeOfByte;
            v.X1 = bytes[position]; position += SizeOfByte;
            v.X2 = bytes[position]; position += SizeOfByte;
            v.X3 = bytes[position]; position += SizeOfByte;

            v.X4 = bytes[position]; position += SizeOfByte;
            v.X5 = bytes[position]; position += SizeOfByte;
            v.X6 = bytes[position]; position += SizeOfByte;
            v.X7 = bytes[position]; position += SizeOfByte;

            v.X8 = bytes[position]; position += SizeOfByte;
            v.X9 = bytes[position]; position += SizeOfByte;
            v.XA = bytes[position]; position += SizeOfByte;
            v.XB = bytes[position]; position += SizeOfByte;

            v.XC = bytes[position]; position += SizeOfByte;
            v.XD = bytes[position]; position += SizeOfByte;
            v.XE = bytes[position]; position += SizeOfByte;
            v.XF = bytes[position];

            return v.Value;
        }

        private static void WriteGuid(this byte[] bytes, int position, Guid value)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));

            // must at least be able to write a bool
            if (position < 0 || bytes.Length < position + SizeOfBool)
                throw new ArgumentOutOfRangeException(nameof(position));

            // write the 'empty' bool
            bytes.WriteBool(position, value == Guid.Empty);
            if (value == Guid.Empty) return;

            // if not empty, must be able to write the full guid
            if (bytes.Length < position + SizeOfGuid)
                throw new ArgumentOutOfRangeException(nameof(position));

            position += SizeOfByte;

            var v = new JavaUuidOrder {Value = value};

            bytes[position] = v.X0;
            position += SizeOfByte;
            bytes[position] = v.X1;
            position += SizeOfByte;
            bytes[position] = v.X2;
            position += SizeOfByte;
            bytes[position] = v.X3;
            position += SizeOfByte;

            bytes[position] = v.X4;
            position += SizeOfByte;
            bytes[position] = v.X5;
            position += SizeOfByte;
            bytes[position] = v.X6;
            position += SizeOfByte;
            bytes[position] = v.X7;
            position += SizeOfByte;

            bytes[position] = v.X8;
            position += SizeOfByte;
            bytes[position] = v.X9;
            position += SizeOfByte;
            bytes[position] = v.XA;
            position += SizeOfByte;
            bytes[position] = v.XB;
            position += SizeOfByte;

            bytes[position] = v.XC;
            position += SizeOfByte;
            bytes[position] = v.XD;
            position += SizeOfByte;
            bytes[position] = v.XE;
            position += SizeOfByte;
            bytes[position] = v.XF;
        }
    }
}
