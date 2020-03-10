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
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal sealed class EmptyObjectDataOutput : IObjectDataOutput
    {
        public void WriteUTFArray(string[] values)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteObject(object @object)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteData(IData data)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Write(int b)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Write(byte[] b)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Write(byte[] b, int off, int len)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteBoolean(bool v)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteByte(int v)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteShort(int v)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteChar(int v)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteInt(int v)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteLong(long v)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteFloat(float v)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteDouble(double v)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteBytes(string s)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteChars(string s)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteUTF(string s)
        {
        }

        public void WriteBooleanArray(bool[] booleans)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteByteArray(byte[] value)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteCharArray(char[] chars)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteIntArray(int[] ints)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteLongArray(long[] longs)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteDoubleArray(double[] values)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteFloatArray(float[] values)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteShortArray(short[] values)
        {
        }

        public byte[] ToByteArray()
        {
            throw new NotSupportedException();
        }

        public ByteOrder GetByteOrder()
        {
            return ByteOrder.BigEndian;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Close()
        {
        }
    }
}