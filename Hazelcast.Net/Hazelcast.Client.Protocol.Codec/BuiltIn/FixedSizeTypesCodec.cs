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

using System;
using Hazelcast.IO;
using static Hazelcast.IO.Bits;

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    internal static class FixedSizeTypesCodec
    {
        public const int GuidSizeInBytes = LongSizeInBytes * 2;

        public static void EncodeInt(byte[] buffer, int pos, int value)
        {
            Bits.WriteIntL(buffer, pos, value);
        }

        public static int DecodeInt(byte[] buffer, int pos)
        {
            return Bits.ReadIntL(buffer, pos);
        }

        public static void EncodeLong(byte[] buffer, int pos, long value)
        {
            Bits.WriteLongL(buffer, pos, value);
        }

        public static long DecodeLong(byte[] buffer, int pos)
        {
            return Bits.ReadLongL(buffer, pos);
        }

        public static void EncodeBool(byte[] buffer, int pos, bool value)
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
            throw new NotImplementedException("Guid is not implemented yet");

            //long mostSigBits = value.getMostSignificantBits();
            //long leastSigBits = value.getLeastSignificantBits();
            //EncodeLong(buffer, pos, mostSigBits);
            //EncodeLong(buffer, pos + LongSizeInBytes, leastSigBits);
        }

        public static Guid DecodeGuid(byte[] buffer, int pos)
        {
            throw new NotImplementedException("Guid is not implemented yet");

            //long mostSigBits = DecodeLong(buffer, pos);
            //long leastSigBits = DecodeLong(buffer, pos + LongSizeInBytes);
            //return new UUID(mostSigBits, leastSigBits);
        }
    }
}