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
            var isNull = value == Guid.Empty;
            EncodeBool(buffer, pos, isNull);
            if (isNull)
            {
                return;
            }

            // TODO: perf improvements can me made in here to do not alloc, https://github.com/kevin-montrose/Jil/blob/0631e89ca667bccea353f1e7394663bedf91d9f8/Jil/Serialize/Methods.cs#L48-L220
            // TODO: correctness - GUIDs are not written in Java format
            var bytes = value.ToByteArray();

            Array.Copy(bytes, 0, buffer, pos + BoolSizeInBytes, 16);
        }

        public static Guid DecodeGuid(byte[] buffer, int pos)
        {
            var isNull = DecodeBool(buffer, pos);
            if (isNull)
            {
                return Guid.Empty;
            }

            // TODO: perf improvements can me made in here to do not alloc, https://github.com/kevin-montrose/Jil/blob/49e8a84ba9719e94de58d3d75ab8bb6eddf4183b/Jil/Deserialize/Methods.cs#L36-L77
            // TODO: correctness - GUIDs are not written in Java format

            var bytes = new byte[16];
            Array.Copy(buffer, pos + BoolSizeInBytes, bytes, 0, 16);
            return new Guid(bytes);
        }
    }
}