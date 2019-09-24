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

internal static class FixedSizeTypesCodec {

    public static final int BYTE_SIZE_IN_BYTES = Bits.BYTE_SIZE_IN_BYTES;
    public static final int LONG_SIZE_IN_BYTES = Bits.LONG_SIZE_IN_BYTES;
    public static final int IntSizeInBytes = Bits.IntSizeInBytes;
    public static final int BOOLEAN_SIZE_IN_BYTES = Bits.BOOLEAN_SIZE_IN_BYTES;
    public static final int UUID_SIZE_IN_BYTES = Bits.LONG_SIZE_IN_BYTES * 2;

    private FixedSizeTypesCodec() {
    }

    public static void EncodeInt(byte[] buffer, int pos, int value) {
        Bits.writeIntL(buffer, pos, value);
    }

    public static int DecodeInt(byte[] buffer, int pos) {
        return Bits.readIntL(buffer, pos);
    }

    public static void EncodeInteger(byte[] buffer, int pos, Integer value) {
        Bits.writeIntL(buffer, pos, value);
    }

    public static Integer DecodeInteger(byte[] buffer, int pos) {
        return Bits.readIntL(buffer, pos);
    }

    public static void EncodeLong(byte[] buffer, int pos, long value) {
        Bits.writeLongL(buffer, pos, value);
    }

    public static long DecodeLong(byte[] buffer, int pos) {
        return Bits.readLongL(buffer, pos);
    }

    public static void EncodeBoolean(byte[] buffer, int pos, boolean value) {
        buffer[pos] = (byte) (value ? 1 : 0);
    }

    public static boolean DecodeBoolean(byte[] buffer, int pos) {
        return buffer[pos] == (byte) 1;
    }

    public static void EncodeByte(byte[] buffer, int pos, byte value) {
        buffer[pos] = value;
    }

    public static byte DecodeByte(byte[] buffer, int pos) {
        return buffer[pos];
    }

    public static void EncodeUUID(byte[] buffer, int pos, UUID value) {
        long mostSigBits = value.getMostSignificantBits();
        long leastSigBits = value.getLeastSignificantBits();
        EncodeLong(buffer, pos, mostSigBits);
        EncodeLong(buffer, pos + LONG_SIZE_IN_BYTES, leastSigBits);
    }

    public static UUID DecodeUUID(byte[] buffer, int pos) {
        long mostSigBits = DecodeLong(buffer, pos);
        long leastSigBits = DecodeLong(buffer, pos + LONG_SIZE_IN_BYTES);
        return new UUID(mostSigBits, leastSigBits);
    }

}
