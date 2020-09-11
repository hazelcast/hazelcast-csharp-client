namespace Hazelcast.Testing
{
    public static class Utf8Char
    {
        // http://www.i18nguy.com/unicode/supplementary-test.html

        // '\u0078' ('x') LATIN SMALL LETTER X (U+0078) 78
        public const char OneByte = '\u0078';

        // '\u00E3' ('ã') LATIN SMALL LETTER A WITH TILDE (U+00E3) c3a3
        public const char TwoBytes = '\u00e3';

        // '\u08DF' ARABIC SMALL HIGH WORD WAQFA (U+08DF) e0a39f
        public const char ThreeBytes = '\u08df';

        // there are no '4 bytes' chars in C#, surrogate pairs are 2 chars
        // '\u2070e' CJK UNIFIED IDEOGRAPH-2070E f0a09c8e
        public const char FourBytesH = '\uf0a0';
        public const char FourBytesL = '\u8c8e';
    }
}
