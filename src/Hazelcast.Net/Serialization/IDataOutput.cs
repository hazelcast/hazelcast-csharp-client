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
    /// <summary>
    /// Defines a destination of data that can be written to to serialize an object.
    /// </summary>
    public interface IDataOutput
    {
        /// <summary>
        /// Writes a <see cref="bool"/> (bool) value.
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteBoolean(bool value);

        /// <summary>
        /// Writes a <see cref="char"/> (char) value.
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteChar(int value);

        /// <summary>
        /// Writes a <see cref="double"/> (double) value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The optional endianness.</param>
        void WriteDouble(double value, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Writes a <see cref="float"/> (float) value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The optional endianness.</param>
        void WriteFloat(float value, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Writes an <see cref="int"/> (int) value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The optional endianness.</param>
        void WriteInt(int value, Endianness endianness = Endianness.Unspecified); // fixme name

        /// <summary>
        /// Writes an <see cref="long"/> (long) value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The optional endianness.</param>
        void WriteLong(long value, Endianness endianness = Endianness.Unspecified); // fixme name

        /// <summary>
        /// Writes a <see cref="short"/> (short) value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The optional endianness.</param>
        void WriteShort(int value, Endianness endianness = Endianness.Unspecified); // fixme name

        /// <summary>
        /// Writes a <see cref="string"/> (string) value.
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteUtf(string value); // fixme name




        void Write(byte[] b);

        /// <exception cref="System.IO.IOException"></exception>
        void Write(byte[] b, int off, int len);


        /// <exception cref="System.IO.IOException"></exception>
        void WriteByte(int v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteBytes(string s);


        /// <exception cref="System.IO.IOException"></exception>
        void WriteChars(string s);

    }
}
