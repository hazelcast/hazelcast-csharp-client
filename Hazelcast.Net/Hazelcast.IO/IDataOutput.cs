// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.IO
{
    public interface IDataOutput
    {
        /// <exception cref="System.IO.IOException"></exception>
        void Write(int b);

        /// <exception cref="System.IO.IOException"></exception>
        void Write(byte[] b);

        /// <exception cref="System.IO.IOException"></exception>
        void Write(byte[] b, int off, int len);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteBoolean(bool v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteByte(int v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteBytes(string s);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteChar(int v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteChars(string s);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteDouble(double v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteFloat(float v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteInt(int v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteLong(long v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteShort(int v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteUTF(string s);
    }
}