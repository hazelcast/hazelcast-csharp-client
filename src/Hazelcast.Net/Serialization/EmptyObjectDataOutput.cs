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
using Hazelcast.Core;

namespace Hazelcast.Serialization
{
    // TODO: what is this?!
    internal sealed class EmptyObjectDataOutput : IObjectDataOutput
    {
        public void WriteObject(object @object)
        { }

        public void WriteUtfArray(string[] values)
        { }

        public void WriteData(IData data)
        { }

        public void Write(int b)
        { }

        public void WriteUtf(string value)
        { }

        public void Write(byte[] b)
        { }

        public void Write(byte[] b, int off, int len)
        { }

        public void WriteBoolean(bool v)
        { }

        public void WriteByte(int v)
        { }

        public void WriteShort(int v)
        { }

        public void WriteChar(int v)
        { }

        public void WriteInt(int v)
        { }

        public void WriteLong(long v)
        { }

        public void WriteFloat(float v)
        { }

        public void WriteDouble(double v)
        { }

        public void WriteBytes(string s)
        { }

        public void WriteChars(string s)
        { }

        public void WriteBooleanArray(bool[] booleans)
        { }

        public void WriteByteArray(byte[] value)
        { }

        public void WriteCharArray(char[] chars)
        { }

        public void WriteIntArray(int[] ints)
        { }

        public void WriteLongArray(long[] longs)
        { }

        public void WriteDoubleArray(double[] values)
        { }

        public void WriteFloatArray(float[] values)
        { }

        public void WriteShortArray(short[] values)
        { }

        public Endianness Endianness => Endianness.BigEndian;

        public byte[] ToByteArray() => Array.Empty<byte>();
    }
}
