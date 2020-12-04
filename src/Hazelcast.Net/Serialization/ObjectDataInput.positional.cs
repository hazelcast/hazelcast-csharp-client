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
using Hazelcast.Core;

namespace Hazelcast.Serialization
{
    internal partial class ObjectDataInput //public api
    {
        public bool ReadBoolean(int position)
        {
            CheckAvailable(position, BytesExtensions.SizeOfBool);
            var value = _buffer.ReadBool(position);
            return value;
        }

        public byte ReadByte(int position)
        {
            CheckAvailable(position, BytesExtensions.SizeOfByte);
            var value = _buffer.ReadByte(position);
            return value;
        }

        public char ReadChar(int position)
        {
            CheckAvailable(position, BytesExtensions.SizeOfChar);
            var value = _buffer.ReadChar(position, Endianness);
            return value;
        }

        public short ReadShort(int position)
        {
            CheckAvailable(position, BytesExtensions.SizeOfShort);
            var value = _buffer.ReadShort(position, Endianness);
            return value;
        }

        public int ReadInt(int position)
        {
            CheckAvailable(position, BytesExtensions.SizeOfInt);
            var value = _buffer.ReadInt(position, Endianness);
            return value;
        }

        public long ReadLong(int position)
        {
            CheckAvailable(position, BytesExtensions.SizeOfLong);
            var value = _buffer.ReadLong(position, Endianness);
            return value;
        }

        public float ReadFloat(int position)
        {
            CheckAvailable(position, BytesExtensions.SizeOfFloat);
            var value = _buffer.ReadFloat(position, Endianness);
            return value;
        }

        public double ReadDouble(int position)
        {
            CheckAvailable(position, BytesExtensions.SizeOfDouble);
            var value = _buffer.ReadDouble(position, Endianness);
            return value;
        }

    }
}