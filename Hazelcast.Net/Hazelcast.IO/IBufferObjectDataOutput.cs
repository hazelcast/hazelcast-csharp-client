// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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
    public interface IBufferObjectDataOutput : IObjectDataOutput, IDisposable
    {
        void Clear();

        int Position();

        void Position(int newPos);
        void Write(int position, int b);

        /// <exception cref="System.IO.IOException"/>
        void WriteBoolean(int position, bool v);

        /// <exception cref="System.IO.IOException"/>
        void WriteByte(int position, int v);

        /// <exception cref="System.IO.IOException"/>
        void WriteChar(int position, int v);

        /// <exception cref="System.IO.IOException"/>
        void WriteDouble(int position, double v);

        /// <exception cref="System.IO.IOException"/>
        void WriteDouble(double v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteDouble(int position, double v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteFloat(int position, float v);

        /// <exception cref="System.IO.IOException"/>
        void WriteFloat(float v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteFloat(int position, float v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteInt(int position, int v);

        /// <exception cref="System.IO.IOException"/>
        void WriteInt(int v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteInt(int position, int v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteLong(int position, long v);

        /// <exception cref="System.IO.IOException"/>
        void WriteLong(long v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteLong(int position, long v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteShort(int position, int v);

        /// <exception cref="System.IO.IOException"/>
        void WriteShort(int v, ByteOrder byteOrder);

        /// <exception cref="System.IO.IOException"/>
        void WriteShort(int position, int v, ByteOrder byteOrder);

        void WriteZeroBytes(int count);
    }

    public static class BufferObjectDataOutputConstants
    {
        public const int UtfBufferSize = 1024;
    }
}