// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
        public const char FourBytesH = (char) 0xd83d;
        public const char FourBytesL = (char) 0xde01;

        // can only be expressed as a string
        // '\u1f601' GRINNING FACE WITH SMILING EYES f09f9881
        public const string FourBytes = "😁"; // "\u1f601" - d83d + de01

        // can only be expressed as a string
        // '\u2070e' CJK UNIFIED IDEOGRAPH-2070E f0a09c8e
        //public const string FourBytes = "𠜎"; // "\u2070e" - d841 + df0e
    }
}
