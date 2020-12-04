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
    public interface IObjectDataInput
    {
        Endianness Endianness { get; }

        bool ReadBoolean();

        byte ReadByte();

        sbyte ReadSByte();

        char ReadChar();

        short ReadShort();

        ushort ReadUShort();

        int ReadInt();

        long ReadLong();

        float ReadFloat();

        double ReadDouble();

        string ReadUTF();

        bool[] ReadBooleanArray();

        byte[] ReadByteArray();

        char[] ReadCharArray();

        short[] ReadShortArray();

        int[] ReadIntArray();

        long[] ReadLongArray();

        float[] ReadFloatArray();

        double[] ReadDoubleArray();

        string[] ReadUTFArray();

        T ReadObject<T>();

        int Read(byte[] b);

        int Read(byte[] b, int off, int len);

        int SkipBytes(int n);
    }
}