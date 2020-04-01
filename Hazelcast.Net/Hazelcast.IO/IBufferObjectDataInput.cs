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

namespace Hazelcast.IO
{
    public interface IBufferObjectDataInput : IObjectDataInput, IDisposable
    {
        void Clear();
        void Init(byte[] data, int offset);

        int Position();

        void Position(int newPos);

        /// <exception cref="System.IO.IOException"/>
        int Read(int position);

        /// <exception cref="System.IO.IOException"/>
        bool ReadBoolean(int position);

        /// <exception cref="System.IO.IOException"/>
        byte ReadByte(int position);

        /// <exception cref="System.IO.IOException"/>
        char ReadChar(int position);

        /// <exception cref="System.IO.IOException"/>
        double ReadDouble(int position);

        /// <exception cref="System.IO.IOException"/>
        double ReadDouble(ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        double ReadDouble(int position, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        float ReadFloat(int position);

        /// <exception cref="System.IO.IOException"/>
        float ReadFloat(ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        float ReadFloat(int position, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        int ReadInt(int position);

        /// <exception cref="System.IO.IOException"/>
        int ReadInt(ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        int ReadInt(int position, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        long ReadLong(int position);

        /// <exception cref="System.IO.IOException"/>
        long ReadLong(ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        long ReadLong(int position, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        short ReadShort(int position);

        /// <exception cref="System.IO.IOException"/>
        short ReadShort(ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        short ReadShort(int position, ByteOrder byteOrder);

        void Reset();
    }

    public static class BufferObjectDataInputConstants
    {
        public const int UtfBufferSize = 1024;
    }
}