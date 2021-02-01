// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
    /// Provides encoding methods for primitive and array of primitive types writing to the serialized byte array.
    /// </summary>
    public interface IObjectDataOutput
    {
        /// <summary>
        /// The configured endianness via <see cref="SerializationOptions.Endianness"/>
        /// </summary>
        Endianness Endianness { get; }

        /// <summary>
        /// Writes a <c>bool</c> value to this serialization output
        /// </summary>
        /// <param name="value">the value to be encoded</param>
        void WriteBoolean(bool value);

        /// <summary>
        /// Writes a <c>byte</c> value to this serialization output
        /// </summary>
        /// <param name="value">the value to be encoded</param>
        void WriteByte(byte value);

        /// <summary>
        /// Writes a <c>sbyte</c> value to this serialization output
        /// </summary>
        /// <param name="value">the value to be encoded</param>
        void WriteSByte(sbyte value);

        /// <summary>
        /// Writes a <c>char</c> value to this serialization output
        /// </summary>
        /// <param name="value">the value to be encoded</param>
        void WriteChar(char value);

        /// <summary>
        /// Writes a <c>short</c> value to this serialization output
        /// </summary>
        /// <param name="value">the value to be encoded</param>
        void WriteShort(short value);

        /// <summary>
        /// Writes a <c>ushort</c> value to this serialization output
        /// </summary>
        /// <param name="value">the value to be encoded</param>
        void WriteUShort(ushort value);

        /// <summary>
        /// Writes a <c>int</c> value to this serialization output
        /// </summary>
        /// <param name="value">the value to be encoded</param>
        void WriteInt(int value);

        /// <summary>
        /// Writes a <c>long</c> value to this serialization output
        /// </summary>
        /// <param name="value">the value to be encoded</param>
        void WriteLong(long value);

        /// <summary>
        /// Writes a <c>float</c> value to this serialization output
        /// </summary>
        /// <param name="value">the value to be encoded</param>
        void WriteFloat(float value);

        /// <summary>
        /// Writes a <c>double</c> value to this serialization output
        /// </summary>
        /// <param name="value">the value to be encoded</param>
        void WriteDouble(double value);

        /// <summary>
        /// Writes a <c>string</c> value in UTF-8 encoding to this serialization output
        /// </summary>
        /// <param name="value">the value to be encoded</param>
        void WriteString(string value);

        /// <summary>
        /// Writes each <c>char</c> of the <paramref name="value"/> with <see cref="WriteChar"/> method to this serialization output
        /// </summary>
        /// <param name="value">the value to be encoded</param>
        /// <remarks>
        /// This method has the same effect with the following code;
        /// <code>
        /// foreach (var ch in value)
        /// {
        ///    WriteChar(ch);
        /// }
        /// </code>
        /// </remarks>
        void WriteChars(string value);

        /// <summary>
        /// Writes each <c>char</c> of the <paramref name="value"/> with <see cref="WriteByte"/> method to this serialization output        /// </summary>
        /// <param name="value">the value to be encoded</param>
        /// <remarks>
        /// Each char value of the string will be cast to byte then written to output.
        /// </remarks>
        void WriteBytes(string value);

        /// <summary>
        /// Writes the <c>bool[]</c> to this serialization output
        /// </summary>
        /// <param name="values">the value to be encoded</param>
        void WriteBooleanArray(bool[] values);

        /// <summary>
        /// Writes the <c>byte[]</c> to this serialization output
        /// </summary>
        /// <param name="values">the value to be encoded</param>
        void WriteByteArray(byte[] values);

        /// <summary>
        /// Writes the <c>char[]</c> to this serialization output
        /// </summary>
        /// <param name="values">the value to be encoded</param>
        void WriteCharArray(char[] values);

        /// <summary>
        /// Writes the <c>short[]</c> to this serialization output
        /// </summary>
        /// <param name="values">the value to be encoded</param>
        void WriteShortArray(short[] values);

        /// <summary>
        /// Writes the <c>int[]</c> to this serialization output
        /// </summary>
        /// <param name="values">the value to be encoded</param>
        void WriteIntArray(int[] values);

        /// <summary>
        /// Writes the <c>long[]</c> to this serialization output
        /// </summary>
        /// <param name="values">the value to be encoded</param>
        void WriteLongArray(long[] values);

        /// <summary>
        /// Writes the <c>float[]</c> to this serialization output
        /// </summary>
        /// <param name="values">the value to be encoded</param>
        void WriteFloatArray(float[] values);

        /// <summary>
        /// Writes the <c>double[]</c> to this serialization output
        /// </summary>
        /// <param name="values">the value to be encoded</param>
        void WriteDoubleArray(double[] values);

        /// <summary>
        /// Writes the <c>string[]</c> to this serialization output
        /// </summary>
        /// <param name="values">the value to be encoded</param>
        void WriteStringArray(string[] values);

        /// <summary>
        /// Writes an object to this serialization output using hazelcast serialization
        /// </summary>
        /// <param name="value"></param>
        void WriteObject(object value);

        /// <summary>
        /// Writes the content of the provided byte array to this serialization output
        /// </summary>
        /// <param name="bytes">the byte array to be written</param>
        /// <remarks>
        /// Please note that this method only writes the content of the array to the output
        /// whereas <see cref="WriteByteArray"/> methods also take care of the size of the array.
        /// </remarks>
        void Write(byte[] bytes);

        /// <summary>
        /// Writes the content of the provided byte array to this serialization output
        /// </summary>
        /// <param name="bytes">the byte array to be written</param>
        /// <param name="offset">the offset of the bytes array for start reading from</param>
        /// <param name="count">total number of bytes to be written from the bytes array</param>
        void Write(byte[] bytes, int offset, int count);

        /// <summary>
        /// Creates a new byte array with the internal content of the serialized data.
        /// </summary>
        /// <param name="padding">the number of zero byte(s) to add to beginning of the returned array</param>
        /// <returns>clone of internal byte content with optional padding bytes</returns>
        byte[] ToByteArray(int padding = 0);
    }
}
