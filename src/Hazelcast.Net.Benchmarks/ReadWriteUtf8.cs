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
using BenchmarkDotNet.Attributes;
using Hazelcast.Core;

namespace Hazelcast.Benchmarks
{
    public class ReadWriteUtf8
    {
        private readonly byte[] _bytes = new byte[256];

        // what about four chars?

        [Benchmark]
        public void BuiltinWriteUtf8()
        {
            var position = 0;

            _bytes.WriteUtf8Char(ref position, Utf8Char.OneByte);
            _bytes.WriteUtf8Char(ref position, Utf8Char.TwoBytes);
            _bytes.WriteUtf8Char(ref position, Utf8Char.ThreeBytes);

            if (position != 6) throw new Exception("position");
        }

        [Benchmark]
        public void EncodingWriteUtf8()
        {
            var position = 0;

            // that version yields
            //
            // |            Method |     Mean |    Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
            // |------------------ |---------:|---------:|---------:|-------:|------:|------:|----------:|
            // |  BuiltinWriteUtf8 | 12.80 ns | 0.272 ns | 0.255 ns |      - |     - |     - |         - |
            // | EncodingWriteUtf8 | 32.69 ns | 0.419 ns | 0.392 ns | 0.0076 |     - |     - |      32 B |

            //var chars = new[] { Utf8Char.OneByte, Utf8Char.TwoBytes, Utf8Char.ThreeBytes };
            //position += Encoding.UTF8.GetBytes(chars, 0, chars.Length, _bytes, 0);

            // that version yields
            //
            // |            Method |     Mean |    Error |   StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
            // |------------------ |---------:|---------:|---------:|-------:|------:|------:|----------:|
            // |  BuiltinWriteUtf8 | 12.62 ns | 0.236 ns | 0.375 ns |      - |     - |     - |         - |
            // | EncodingWriteUtf8 | 71.60 ns | 0.877 ns | 0.820 ns | 0.0229 |     - |     - |      96 B |

            //position += Encoding.UTF8.GetBytes(new []{ Utf8Char.OneByte }, 0, 1, _bytes, 0);
            //position += Encoding.UTF8.GetBytes(new[] { Utf8Char.TwoBytes }, 0, 1, _bytes, 0);
            //position += Encoding.UTF8.GetBytes(new[] { Utf8Char.ThreeBytes }, 0, 1, _bytes, 0);

            if (position != 6) throw new Exception("position");
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
            public const char FourBytesH = '\uf0a0';
            public const char FourBytesL = '\u8c8e';
        }
    }
}
