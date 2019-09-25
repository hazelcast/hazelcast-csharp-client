/*
 * Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.IO;

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    internal static class FixedSizeTypesCodec
    {
        public const int ByteSizeInBytes = Bits.ByteSizeInBytes;
        public const int LongSizeInBytes = Bits.LongSizeInBytes;
        public const int IntSizeInBytes = Bits.IntSizeInBytes;
        public const int BoolSizeInBytes = Bits.BooleanSizeInBytes;
        public const int GuidSizeInBytes = Bits.LongSizeInBytes * 2;

        public static void EncodeInt(byte[] buffer, int pos, int value)
        {
            Bits.writeIntL(buffer, pos, value);
        }

        public static int DecodeInt(byte[] buffer, int pos)
        {
            return Bits.readIntL(buffer, pos);
        }

        public static void EncodeLong(byte[] buffer, int pos, long value)
        {
            Bits.writeLongL(buffer, pos, value);
        }

        public static long DecodeLong(byte[] buffer, int pos)
        {
            return Bits.readLongL(buffer, pos);
        }

        public static void EncodeBool(byte[] buffer, int pos, boolean value)
        {
            buffer[pos] = (byte)(value ? 1 : 0);
        }

        public static bool DecodeBool(byte[] buffer, int pos)
        {
            return buffer[pos] == (byte)1;
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
            long mostSigBits = value.getMostSignificantBits();
            long leastSigBits = value.getLeastSignificantBits();
            EncodeLong(buffer, pos, mostSigBits);
            EncodeLong(buffer, pos + LONG_SIZE_IN_BYTES, leastSigBits);
        }

        public static Guid DecodeGuid(byte[] buffer, int pos)
        {
            long mostSigBits = DecodeLong(buffer, pos);
            long leastSigBits = DecodeLong(buffer, pos + LONG_SIZE_IN_BYTES);
            return new UUID(mostSigBits, leastSigBits);
        }
    }
}