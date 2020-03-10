/*
 * Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Runtime.InteropServices;
using Hazelcast.Config;
using static Hazelcast.IO.Bits;

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    internal static class FixedSizeTypesCodec
    {
        public const int GuidSizeInBytes = BoolSizeInBytes + LongSizeInBytes * 2;

        public static void EncodeInt(byte[] buffer, int pos, int value)
        {
            WriteIntL(buffer, pos, value);
        }

        public static void EncodeInt(byte[] buffer, int pos, IndexType value)
        {
            WriteIntL(buffer, pos, (int) value);
        }

        public static void EncodeInt(byte[] buffer, int pos, UniqueKeyTransformation value)
        {
            WriteIntL(buffer, pos, (int) value);
        }

        public static int DecodeInt(byte[] buffer, int pos)
        {
            return ReadIntL(buffer, pos);
        }

        public static void EncodeLong(byte[] buffer, int pos, long value)
        {
            WriteLongL(buffer, pos, value);
        }

        public static long DecodeLong(byte[] buffer, int pos)
        {
            return ReadLongL(buffer, pos);
        }

        public static void EncodeBool(byte[] buffer, int pos, bool value)
        {
            buffer[pos] = (byte) (value ? 1 : 0);
        }

        public static bool DecodeBool(byte[] buffer, int pos)
        {
            return buffer[pos] == 1;
        }

        public static void EncodeByte(byte[] buffer, int pos, byte value)
        {
            buffer[pos] = value;
        }

        public static byte DecodeByte(byte[] buffer, int pos)
        {
            return buffer[pos];
        }

        public static void EncodeGuid(byte[] buffer, int pos, Guid value)
        {
            var isNull = value == Guid.Empty;
            EncodeBool(buffer, pos, isNull);
            if (isNull)
            {
                return;
            }

            var order = default(JavaUUIDOrder);
            order.Value = value;

            pos++;

            buffer[pos++] = order.B00;
            buffer[pos++] = order.B01;
            buffer[pos++] = order.B02;
            buffer[pos++] = order.B03;

            buffer[pos++] = order.B04;
            buffer[pos++] = order.B05;
            buffer[pos++] = order.B06;
            buffer[pos++] = order.B07;

            buffer[pos++] = order.B08;
            buffer[pos++] = order.B09;
            buffer[pos++] = order.B10;
            buffer[pos++] = order.B11;

            buffer[pos++] = order.B12;
            buffer[pos++] = order.B13;
            buffer[pos++] = order.B14;
            buffer[pos++] = order.B15;
        }

        public static Guid DecodeGuid(byte[] buffer, int pos)
        {
            var isNull = DecodeBool(buffer, pos);
            if (isNull)
            {
                return Guid.Empty;
            }

            var order = default(JavaUUIDOrder);

            pos++;

            order.B00 = buffer[pos++];
            order.B01 = buffer[pos++];
            order.B02 = buffer[pos++];
            order.B03 = buffer[pos++];

            order.B04 = buffer[pos++];
            order.B05 = buffer[pos++];
            order.B06 = buffer[pos++];
            order.B07 = buffer[pos++];

            order.B08 = buffer[pos++];
            order.B09 = buffer[pos++];
            order.B10 = buffer[pos++];
            order.B11 = buffer[pos++];

            order.B12 = buffer[pos++];
            order.B13 = buffer[pos++];
            order.B14 = buffer[pos++];
            order.B15 = buffer[pos++];

            return order.Value;
        }

        // the following GUID: "00010203-0405-0607-0809-0a0b0c0d0e0f" is:
        // in .NET ToArray   as 3, 2, 1, 0,     5, 4, 7, 6,     8,   9, 10, 11,     12, 13, 14, 15
        // in Java UUIDCodec as 7, 6, 5, 4,     3, 2, 1, 0,     15, 14, 13, 12,     11, 10, 9,  8
        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        internal struct JavaUUIDOrder
        {
            [FieldOffset(0)] public Guid Value;

            [FieldOffset(6)] public byte B00;
            [FieldOffset(7)] public byte B01;
            [FieldOffset(4)] public byte B02;
            [FieldOffset(5)] public byte B03;

            [FieldOffset(0)] public byte B04;
            [FieldOffset(1)] public byte B05;
            [FieldOffset(2)] public byte B06;
            [FieldOffset(3)] public byte B07;

            [FieldOffset(15)] public byte B08;
            [FieldOffset(14)] public byte B09;
            [FieldOffset(13)] public byte B10;
            [FieldOffset(12)] public byte B11;

            [FieldOffset(11)] public byte B12;
            [FieldOffset(10)] public byte B13;
            [FieldOffset(9)] public byte B14;
            [FieldOffset(8)] public byte B15;
        }
    }
}