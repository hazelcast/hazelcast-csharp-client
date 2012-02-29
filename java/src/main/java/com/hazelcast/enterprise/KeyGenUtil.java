package com.hazelcast.enterprise;

import java.util.Random;

public class KeyGenUtil {

    private static final Random rand = new Random();
    static final char[] chars = "QRSTUVWXYZ6789ABCDEFGHIJKLMNOP".toCharArray();
    static final char[] digits = "0123456789".toCharArray();
    private static final int length = chars.length;
    private static final int reserved = 12;
    static final int yearBase = 2010;

    public static License extractLicense(char[] originalKey) {
        if (originalKey == null || length != originalKey.length) {
            throw new IllegalArgumentException("Invalid key!");
        }
        final char[] key = new char[length];
        System.arraycopy(originalKey, 0, key, 0, length);
        char fp = key[reserved - 1];
        key[reserved - 1] = 0;
        char lp = key[reserved];
        key[reserved] = 0;

        char[] hash = hash(key);
        if (hash[0] != fp || hash[hash.length - 1] != lp) {
            throw new IllegalArgumentException("Invalid key!");
        }

        int ix = 0;
        char r = key[ix++];
        boolean full = key[ix0(r)] == '1';

        char t = key[ix++];
        boolean enterprise = key[ix0(t)] == '1';

        char d0 = key[ix++];
        char d1 = key[ix++];
        int day = ix1(key[ix0(d0)]) * 10 + ix1(key[ix0(d1)]);

        char m0 = key[ix++];
        char m1 = key[ix++];
        int month = ix1(key[ix0(m0)]) * 10 + ix1(key[ix0(m1)]);

        char y = key[ix++];
        int year = yearBase + ix1(key[ix0(y)]);

        char n0 = key[ix++];
        char n1 = key[ix++];
        char n2 = key[ix++];
        char n3 = key[ix++];
        int nodes = ix1(key[ix0(n0)]) * 1000 + ix1(key[ix0(n1)]) * 100
                + ix1(key[ix0(n2)]) * 10 + ix1(key[ix0(n3)]);

        return new License(full, enterprise, day, month, year, nodes);
    }

    public static char[] generateKey(boolean full, boolean enterprise,
                                     int day, int month, int year, int nodes) {
        char[] key = new char[length];
        int ix = 0;
        int mode = pick(key);
        key[ix++] = chars[mode];
        key[mode] = full ? '1' : '0';

        int type = pick(key);
        key[ix++] = chars[type];
        key[type] = enterprise ? '1' : '0';

        int d0 = pick(key);
        key[ix++] = chars[d0];
        key[d0] = digits[day / 10];

        int d1 = pick(key);
        key[ix++] = chars[d1];
        key[d1] = digits[day % 10];

        int m0 = pick(key);
        key[ix++] = chars[m0];
        key[m0] = digits[month / 10];

        int m1 = pick(key);
        key[ix++] = chars[m1];
        key[m1] = digits[month % 10];

        int y = pick(key);
        key[ix++] = chars[y];
        key[y] = digits[year % 10];

        int n0 = pick(key);
        key[ix++] = chars[n0];
        key[n0] = digits[nodes / 1000];

        int n1 = pick(key);
        key[ix++] = chars[n1];
        key[n1] = digits[(nodes % 1000) / 100];

        int n2 = pick(key);
        key[ix++] = chars[n2];
        key[n2] = digits[(nodes % 100) / 10];

        int n3 = pick(key);
        key[ix++] = chars[n3];
        key[n3] = digits[nodes % 10];

        for (int i = reserved + 1; i < key.length; i++) {
            if (key[i] == 0) {
                key[i] = chars[rand.nextInt(reserved + 1)];
            }
        }
        char[] hash = hash(key);
        key[reserved - 1] = hash[0];
        key[reserved] = hash[hash.length - 1];
        return key;
    }

    private static int pick(char[] key) {
        int k = -1;
        boolean loop = true;
        while (loop) {
            k = rand.nextInt(length);
            if (k > reserved && key[k] == 0) {
                loop = false;
            }
        }
        return k;
    }

    private static int ix0(char c) {
        return ix(chars, c);
    }

    private static int ix1(char c) {
        return ix(digits, c);
    }

    private static int ix(char[] cc, char c) {
        for (int i = 0; i < cc.length; i++) {
            if (c == cc[i]) {
                return i;
            }
        }
        return -1;
    }

    private static char[] hash(char a[]) {
        if (a == null) {
            return new char[]{'0'};
        }
        int result = 1;
        for (char element : a) {
            result = 31 * result + element;
        }
        return Integer.toString(Math.abs(result)).toCharArray();
    }
}
