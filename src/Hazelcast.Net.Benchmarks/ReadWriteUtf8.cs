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
using System.Text;
using BenchmarkDotNet.Attributes;
using Hazelcast.Core;

namespace Hazelcast.Benchmarks
{
    public class ReadWriteUtf8
    {
        private readonly byte[] _bytes = new byte[256];

        // the integer we write first is 4 bytes
        // Utf8Char.Mix is 5 chars and 10 bytes long

        // yields
        //
        // |            Method |     Mean |    Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
        // |------------------ |---------:|---------:|---------:|-------:|------:|------:|----------:|
        // |  BuiltinWriteUtf8 | 58.87 ns | 1.148 ns | 1.074 ns | 0.0095 |     - |     - |      40 B |
        // | EncodingWriteUtf8 | 55.90 ns | 0.671 ns | 0.659 ns |      - |     - |     - |         - |
        //
        // so I guess our own stuff is not that fast eh?
        // ok going to kill it

        [Benchmark]
        public void BuiltinWriteUtf8()
        {
            var position = 0;
            var s = Utf8Char.Mix;

            var a = s.ToCharArray();
            var byteCount = BytesExtensions.CountUtf8Bytes(a);
            _bytes.WriteInt(position, byteCount);
            position += BytesExtensions.SizeOfInt;

            _bytes.WriteUtf8String(ref position, a);
            if (position != 14) throw new Exception($"position is {position}");
        }

        [Benchmark]
        public void EncodingWriteUtf8()
        {
            var position = 0;
            var s = Utf8Char.Mix;

            var byteCount = Encoding.UTF8.GetByteCount(s);
            _bytes.WriteInt(position, byteCount);
            position += BytesExtensions.SizeOfInt;

            var count = Encoding.UTF8.GetBytes(s, 0, s.Length, _bytes, position);
            position += count;
            if (position != 14) throw new Exception($"position is {position}");
        }

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
            public const char FourBytesH = (char)0xd83d;
            public const char FourBytesL = (char)0xde01;

            // can only be expressed as a string
            // '\u1f601' GRINNING FACE WITH SMILING EYES f09f9881
            public const string FourBytes = "😁"; // "\u1f601" - d83d + de01

            // can only be expressed as a string
            // '\u2070e' CJK UNIFIED IDEOGRAPH-2070E f0a09c8e
            //public const string FourBytes = "𠜎"; // "\u2070e" - d841 + df0e

            // all of them in one string
            //public static readonly string Mix = OneByte + TwoBytes + ThreeBytes + FourBytes;
            public static readonly string Mix = new string(new[] { OneByte , TwoBytes , ThreeBytes }) +  FourBytes;
        }
    }
}
