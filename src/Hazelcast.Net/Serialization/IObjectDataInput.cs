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
    /// Provides decoding methods for primitive and array of primitive types reading from the serialized byte array.
    /// </summary>
    public interface IObjectDataInput
    {
        /// <summary>
        /// The configured endianness via <see cref="SerializationOptions.Endianness"/>
        /// </summary>
        Endianness Endianness { get; }

        /// <summary>
        /// Reads and return a bool value.
        /// </summary>
        /// <returns>bool value</returns>
        bool ReadBoolean();

        /// <summary>
        /// Reads and return an unsigned byte.
        /// </summary>
        /// <returns>byte value</returns>
        byte ReadByte();

        /// <summary>
        /// Reads and return a signed byte value
        /// </summary>
        /// <returns>sbyte value</returns>
        sbyte ReadSByte();

        /// <summary>
        /// Reads and return a char value
        /// </summary>
        /// <returns>char value</returns>
        char ReadChar();

        /// <summary>
        /// Reads and return a signed short value.
        /// </summary>
        /// <returns>short value</returns>
        short ReadShort();

        /// <summary>
        /// Read and return am unsigned short value
        /// </summary>
        /// <returns>ushort value</returns>
        ushort ReadUShort();

        /// <summary>
        /// Read and return a signed integer value
        /// </summary>
        /// <returns>int value</returns>
        int ReadInt();

        /// <summary>
        /// Reads and return a signed long value
        /// </summary>
        /// <returns>long value</returns>
        long ReadLong();

        /// <summary>
        /// Reads and return a float value
        /// </summary>
        /// <returns>float value</returns>
        float ReadFloat();

        /// <summary>
        /// Reads and return double value
        /// </summary>
        /// <returns>double value</returns>
        double ReadDouble();

        /// <summary>
        /// Reads and return a UTF-8 encoded string
        /// </summary>
        /// <returns></returns>
        string ReadUTF();

        /// <summary>
        /// Reads and return a bool array
        /// </summary>
        /// <returns>bool array</returns>
        bool[] ReadBooleanArray();

        /// <summary>
        /// Reads and return byte array
        /// </summary>
        /// <returns>byte array</returns>
        byte[] ReadByteArray();

        /// <summary>
        /// Reads and return a char array
        /// </summary>
        /// <returns>char array</returns>
        char[] ReadCharArray();

        /// <summary>
        /// Reads and return a short array
        /// </summary>
        /// <returns>short array</returns>
        short[] ReadShortArray();

        /// <summary>
        /// Reads and return an int array
        /// </summary>
        /// <returns>int array</returns>
        int[] ReadIntArray();

        /// <summary>
        /// Reads and return long array
        /// </summary>
        /// <returns>long array</returns>
        long[] ReadLongArray();

        /// <summary>
        /// Reads and return a float array
        /// </summary>
        /// <returns>flaot array</returns>
        float[] ReadFloatArray();

        /// <summary>
        /// Reads and return double array
        /// </summary>
        /// <returns>double array</returns>
        double[] ReadDoubleArray();

        /// <summary>
        /// Reads and return utf-8 encoded string array
        /// </summary>
        /// <returns>string array</returns>
        string[] ReadUTFArray();

        /// <summary>
        /// Reads and return the object
        /// </summary>
        /// <typeparam name="T">The type of the return object. The internal object is cast to this type before returning</typeparam>
        /// <returns>T instance</returns>
        T ReadObject<T>();

        /// <summary>
        /// Reads into byte array <paramref name="bytes"/> as much as possible to fill the provided array.
        /// </summary>
        /// <param name="bytes">byte array to be filled</param>
        /// <returns>whether the number of bytes read, or -1 if end of internal serialized buffer is reached.</returns>
        int Read(byte[] bytes);

        /// <summary>
        /// Reads into byte array <paramref name="bytes"/> starting from the offset value. As much as count bytes will be written if there is available content.
        /// </summary>
        /// <param name="bytes">byte array to be filled</param>
        /// <param name="offset">fill starting offset of the provided array</param>
        /// <param name="count">the requested number of bytes to be filled.</param>
        /// <returns>whether the number of bytes read, or -1 if end of internal serialized buffer is reached.</returns>
        int Read(byte[] bytes, int offset, int count);

        /// <summary>
        /// Tries Skips <paramref name="count"/> number of bytes if possible.
        /// </summary>
        /// <param name="count">number of bytes to skip</param>
        /// <returns>The actual number of bytes skipped</returns>
        int SkipBytes(int count);
    }
}