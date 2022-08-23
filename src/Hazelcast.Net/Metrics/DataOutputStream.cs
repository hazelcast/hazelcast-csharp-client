// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.IO;

namespace Hazelcast.Metrics
{
    // a minimal port of Java's DataOutputStream to be used in MetricsCompressor,
    // because MetricsCompressor needs to send things in a Java-expected way
    internal class DataOutputStream
    {
        private readonly Stream _stream;

        public DataOutputStream(Stream stream)
        {
            _stream = stream;
        }

        public void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

        public void WriteInt(int value)
        {
            // this has to be in line with Java

            var uvalue = (uint) value;

            WriteByte((byte) (uvalue >> 0x18));
            WriteByte((byte) (uvalue >> 0x10));
            WriteByte((byte) (uvalue >> 8));
            WriteByte((byte) uvalue);
        }

        public void WriteLong(long value)
        {
            // this has to be in line with Java

            var uvalue = (ulong) value;

            WriteByte((byte) (uvalue >> 0x38));
            WriteByte((byte) (uvalue >> 0x30));
            WriteByte((byte) (uvalue >> 0x28));
            WriteByte((byte) (uvalue >> 0x20));

            WriteByte((byte) (uvalue >> 0x18));
            WriteByte((byte) (uvalue >> 0x10));
            WriteByte((byte) (uvalue >> 8));
            WriteByte((byte) uvalue);
        }

        public void WriteChar(char value)
        {
            // this has to be in line with Java

            var uvalue = (ushort) value;

            WriteByte((byte) (uvalue >> 8));
            WriteByte((byte) uvalue);
        }

        public void WriteDouble(double value)
        {
            // this has to be in line with Java

            WriteLong(BitConverter.DoubleToInt64Bits(value));
        }

        public void Write(byte[] bytes, int offset, int count)
        {
            _stream.Write(bytes, offset, count);
        }

        public void Write(byte[] bytes)
            => Write(bytes, 0, bytes.Length);

        public void WriteString(string s, int offset, int count)
        {
            var bound = offset + count;
            for (var i = offset; i < bound; i++)
                WriteChar(s[i]);
        }
    }
}
