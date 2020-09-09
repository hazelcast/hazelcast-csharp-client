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
    /// Defines a source of data that can be read from, to deserialize an object.
    /// </summary>
    public interface IDataInput
    {
        // TODO: sbyte?

        /// <summary>
        /// Reads a <see cref="byte"/> value.
        /// </summary>
        /// <returns>The value.</returns>
        byte ReadByte();

        /// <summary>
        /// Tries to read a <see cref="byte"/> value.
        /// </summary>
        /// <returns>An attempt at reading a byte value, which may fail if the end of the buffer has been reached.</returns>
        Attempt<byte> TryReadByte();

        /// <summary>
        /// Reads an array of <see cref="byte"/> items.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The array.</returns>
        byte[] ReadByteArray(Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Reads a <see cref="short"/> value.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        short ReadShort(Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Reads an array of <see cref="short"/> items.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The array.</returns>
        short[] ReadShortArray(Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Reads an <see cref="ushort"/> value.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        ushort ReadUnsignedShort(Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Reads an array of <see cref="ushort"/> items.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The array.</returns>
        ushort[] ReadUnsignedShortArray(Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Reads an <see cref="int"/> value.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        int ReadInt(Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Reads an array of <see cref="int"/> items.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns></returns>
        int[] ReadIntArray(Endianness endianness = Endianness.Unspecified);

        // TODO: uint?

        /// <summary>
        /// Reads a <see cref="long"/> value.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        long ReadLong(Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Reads an array of <see cref="long"/> items.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns></returns>
        long[] ReadLongArray(Endianness endianness = Endianness.Unspecified);

        // TODO: ulong?



        /// <summary>
        /// Reads a <see cref="bool"/> value.
        /// </summary>
        /// <returns>The value.</returns>
        bool ReadBool();

        /// <summary>
        /// Reads an array of <see cref="bool"/> items.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The array.</returns>
        bool[] ReadBoolArray(Endianness endianness = Endianness.Unspecified);



        /// <summary>
        /// Reads a <see cref="char"/> value.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        char ReadChar(Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Reads an array of <see cref="char"/> items.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The array.</returns>
        char[] ReadCharArray(Endianness endianness = Endianness.Unspecified);



        /// <summary>
        /// Reads a <see cref="float"/> value.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        float ReadFloat(Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Reads an array of <see cref="float"/> items.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The array.</returns>
        float[] ReadFloatArray(Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Reads a <see cref="double"/> value.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        double ReadDouble(Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Reads an array of <see cref="double"/> items.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The array.</returns>
        double[] ReadDoubleArray(Endianness endianness = Endianness.Unspecified);

        // TODO: decimal?



        /// <summary>
        /// Reads an UTF8 <see cref="string"/> value.
        /// </summary>
        /// <returns>The value.</returns>
        string ReadString();

        /// <summary>
        /// Reads an array of UTF8 <see cref="string"/> items.
        /// </summary>
        /// <returns>The array.</returns>
        string[] ReadStringArray(Endianness endianness = Endianness.Unspecified);



        /// <summary>
        /// Reads bytes.
        /// </summary>
        /// <param name="bytes">The destination array of bytes.</param>
        int ReadBytes(byte[] bytes);

        /// <summary>
        /// Reads bytes.
        /// </summary>
        /// <param name="bytes">The destination array of bytes.</param>
        /// <param name="offset">The destination offset.</param>
        /// <param name="count">The number of bytes to read.</param>
        int ReadBytes(byte[] bytes, int offset, int count);

        /// <summary>
        /// Skips bytes.
        /// </summary>
        /// <param name="count">The number of bytes to skip.</param>
        /// <returns>The number of skipped bytes.</returns>
        /// <remarks>
        /// <para>The number of skipped bytes can be lower than <paramref name="count"/> it the end of the buffer is reached.</para>
        /// </remarks>
        int Skip(int count);
    }
}
