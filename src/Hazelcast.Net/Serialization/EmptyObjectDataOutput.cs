// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
    /// <summary>
    /// Empty implementation of <see cref="IObjectDataOutput"/> used by <see cref="ClassDefinitionWriter"/> to
    /// create a class definition by calling the WritePortable of the Portable instance.
    /// </summary>
    internal sealed class EmptyObjectDataOutput : IObjectDataOutput
    {
        public Endianness Endianness => Endianness.BigEndian;
        public void WriteBoolean(bool v) { }

        public void WriteByte(byte b) { }

        public void WriteSByte(sbyte b) { }

        public void WriteChar(char v) { }

        public void WriteShort(short v) { }

        public void WriteUShort(ushort v) { }

        public void WriteInt(int v) { }

        public void WriteLong(long v) { }

        public void WriteFloat(float v) { }

        public void WriteDouble(double v) { }

        public void WriteString(string s) { }

        public void WriteChars(string s) { }

        public void WriteBytes(string s) { }

        public void WriteBooleanArray(bool[] booleans) { }

        public void WriteByteArray(byte[] bytes) { }

        public void WriteCharArray(char[] chars) { }

        public void WriteShortArray(short[] values) { }

        public void WriteIntArray(int[] ints) { }

        public void WriteLongArray(long[] longs) { }

        public void WriteFloatArray(float[] values) { }

        public void WriteDoubleArray(double[] values) { }

        public void WriteStringArray(string[] values) { }

        public void WriteObject(object value) { }

        public void Write(byte[] b) { }

        public void Write(byte[] b, int off, int len) { }

        public byte[] ToByteArray(int padding = 0) => Array.Empty<byte>();
    }
}
