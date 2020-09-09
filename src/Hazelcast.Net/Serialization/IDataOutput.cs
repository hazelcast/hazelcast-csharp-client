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
    /// Defines a destination of data that can be written to, to serialize an object.
    /// </summary>
    public interface IDataOutput
    {
        /// <summary>
        /// Writes a <see cref="byte"/> value.
        /// </summary>
        /// <param name="value">The value.</param>
        void Write(byte value);

        /// <summary>
        /// Writes an array of <see cref="byte"/> items.
        /// </summary>
        /// <param name="values">The array.</param>
        /// <param name="endianness">The endianness.</param>
        void WriteArray(byte[] values, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Writes a <see cref="short"/> value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The optional endianness.</param>
        void Write(short value, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Writes an array of <see cref="short"/> items.
        /// </summary>
        /// <param name="values">The array.</param>
        /// <param name="endianness">The endianness.</param>
        void WriteArray(short[] values, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Writes an <see cref="int"/> value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The optional endianness.</param>
        void Write(int value, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Writes an array of <see cref="int"/> items.
        /// </summary>
        /// <param name="values">The array.</param>
        /// <param name="endianness">The endianness.</param>
        void WriteArray(int[] values, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Writes an <see cref="long"/> value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The optional endianness.</param>
        void Write(long value, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Writes an array of <see cref="long"/> items.
        /// </summary>
        /// <param name="values">The array.</param>
        /// <param name="endianness">The endianness.</param>
        void WriteArray(long[] values, Endianness endianness = Endianness.Unspecified);



        /// <summary>
        /// Writes a <see cref="bool"/> value.
        /// </summary>
        /// <param name="value">The value.</param>
        void Write(bool value);

        /// <summary>
        /// Writes an array of <see cref="bool"/> items.
        /// </summary>
        /// <param name="values">The array.</param>
        /// <param name="endianness">The endianness.</param>
        void WriteArray(bool[] values, Endianness endianness = Endianness.Unspecified);



        /// <summary>
        /// Writes a <see cref="char"/> value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The endianness.</param>
        void Write(char value, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Writes an array of <see cref="char"/> items.
        /// </summary>
        /// <param name="values">The array.</param>
        /// <param name="endianness">The endianness.</param>
        void WriteArray(char[] values, Endianness endianness = Endianness.Unspecified);




        /// <summary>
        /// Writes a <see cref="float"/> value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The optional endianness.</param>
        void Write(float value, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Writes an array of <see cref="float"/> items.
        /// </summary>
        /// <param name="values">The array.</param>
        /// <param name="endianness">The endianness.</param>
        void WriteArray(float[] values, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Writes a <see cref="double"/> value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The optional endianness.</param>
        void Write(double value, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Writes an array of <see cref="double"/> items.
        /// </summary>
        /// <param name="values">The array.</param>
        /// <param name="endianness">The endianness.</param>
        void WriteArray(double[] values, Endianness endianness = Endianness.Unspecified);


        /// <summary>
        /// Writes an UTF8 <see cref="string"/> value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The endianness.</param>
        void Write(string value, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Writes a <see cref="string"/> value as chars.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The endianness.</param>
        void WriteAsCharArray(string value, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Writes a <see cref="string"/> value as bytes, assuming each char is 1-byte.
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteAsBytes(string value);

        /// <summary>
        /// Writes an array of UTF8 <see cref="string"/> items.
        /// </summary>
        /// <param name="values">The array.</param>
        /// <param name="endianness">The endianness.</param>
        void WriteArray(string[] values, Endianness endianness = Endianness.Unspecified);



        /// <summary>
        /// Writes bytes.
        /// </summary>
        /// <param name="bytes">The bytes to write.</param>
        void WriteBytes(byte[] bytes);

        /// <summary>
        /// Writes bytes.
        /// </summary>
        /// <param name="bytes">The source bytes to write.</param>
        /// <param name="offset">The source offset.</param>
        /// <param name="count">The number of bytes to write.</param>
        void WriteBytes(byte[] bytes, int offset, int count);
    }
}
