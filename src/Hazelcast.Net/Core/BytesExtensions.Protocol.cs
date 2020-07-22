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

namespace Hazelcast.Core
{
    public static partial class BytesExtensions // Protocol
    {
        // use little endian where it makes sense
        // for some types (byte, bool...) it just does not make sense
        // for some types (guid...) endianness is just not an option

        public static void WriteLongL(this byte[] bytes, int position, long value)
            => bytes.WriteLong(position, value, Endianness.LittleEndian);

        public static void WriteIntL(this byte[] bytes, int position, int value)
            => bytes.WriteInt(position, value, Endianness.LittleEndian);

        public static void WriteIntL(this byte[] bytes, int position, Enum value)
            => bytes.WriteInt(position, value, Endianness.LittleEndian);

        public static void WriteBoolL(this byte[] bytes, int position, bool value)
            => bytes.WriteBool(position, value);

        public static void WriteGuidL(this byte[] bytes, int position, Guid value)
            => bytes.WriteGuid(position, value);

        public static void WriteByteL(this byte[] bytes, int position, byte value)
            => bytes.WriteByte(position, value);

        public static long ReadLongL(this byte[] bytes, int position)
            => bytes.ReadLong(position, Endianness.LittleEndian);

        public static int ReadIntL(this byte[] bytes, int position)
            => bytes.ReadInt(position, Endianness.LittleEndian);

        public static bool ReadBoolL(this byte[] bytes, int position)
            => bytes.ReadBool(position);

        public static Guid ReadGuidL(this byte[] bytes, int position)
            => bytes.ReadGuid(position);

        public static byte ReadByteL(this byte[] bytes, int position)
            => bytes.ReadByte(position);
    }
}
