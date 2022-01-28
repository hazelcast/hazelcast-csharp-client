
//package com.hazelcast.internal.serialization.impl.compact;

//import javax.annotation.Nullable; - oh my, this is not a Java built-in thing
import java.nio.charset.StandardCharsets;

// this is a copy of the Java RabinFingerprint class, which is all private - so we can use it in our tests

/**
 * A very collision-resistant fingerprint method used to create automatic
 * schema ids for the Compact format.
 */
public final class RabinFingerprint {

    private static final long INIT = 0xc15d213aa4d7a795L;
    private static final long[] FP_TABLE = new long[256];
    private static final int NULL_ARRAY_LENGTH = -1; // from com.hazelcast.internal.nio.Bits.NULL_ARRAY_LENGTH

    static {
        for (int i = 0; i < 256; i++) {
            long fp = i;
            for (int j = 0; j < 8; j++) {
                fp = (fp >>> 1) ^ (INIT & -(fp & 1L));
            }
            FP_TABLE[i] = fp;
        }
    }

    private RabinFingerprint() {
    }

    // Package-private for tests
    public static long fingerprint64(byte[] buf) {
        long fp = INIT;
        for (byte b : buf) {
            fp = fingerprint64(fp, b);
        }
        return fp;
    }

    public static long fingerprint64(long fp, byte b) {
        return (fp >>> 8) ^ FP_TABLE[(int) (fp ^ b) & 0xff];
    }

    public static long fingerprint64(long fp, /*@Nullable*/ String value) {
        if (value == null) {
            return fingerprint64(fp, NULL_ARRAY_LENGTH);
        }
        byte[] utf8Bytes = value.getBytes(StandardCharsets.UTF_8);
        fp = fingerprint64(fp, utf8Bytes.length);
        for (byte utf8Byte : utf8Bytes) {
            fp = fingerprint64(fp, utf8Byte);
        }
        return fp;
    }

    /**
     * FingerPrint of a little endian representation of an integer.
     */
    public static long fingerprint64(long fp, int v) {
        fp = fingerprint64(fp, (byte) ((v) & 0xFF));
        fp = fingerprint64(fp, (byte) ((v >>> 8) & 0xFF));
        fp = fingerprint64(fp, (byte) ((v >>> 16) & 0xFF));
        fp = fingerprint64(fp, (byte) ((v >>> 24) & 0xFF));
        return fp;
    }
}
