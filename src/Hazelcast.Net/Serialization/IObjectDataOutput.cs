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

using Hazelcast.Core;

namespace Hazelcast.Serialization
{
    public interface IObjectDataOutput
    {
        Endianness Endianness { get; }

        void WriteBoolean(bool v);

        void WriteByte(byte b);
        
        void WriteSbyte(sbyte b);

        void WriteChar(char v);

        void WriteShort(short v);
        
        void WriteUshort(ushort v);

        void WriteInt(int v);

        void WriteLong(long v);

        void WriteFloat(float v);

        void WriteDouble(double v);

        void WriteUTF(string s);
        
        void WriteChars(string s);

        void WriteBytes(string s);

        void WriteBooleanArray(bool[] booleans);

        void WriteByteArray(byte[] bytes);

        void WriteCharArray(char[] chars);

        void WriteShortArray(short[] values);

        void WriteIntArray(int[] ints);

        void WriteLongArray(long[] longs);

        void WriteFloatArray(float[] values);

        void WriteDoubleArray(double[] values);

        void WriteUTFArray(string[] values);

        void WriteObject(object value);

        void Write(byte[] b);

        void Write(byte[] b, int off, int len);

        byte[] ToByteArray(int padding = 0);
    }
}