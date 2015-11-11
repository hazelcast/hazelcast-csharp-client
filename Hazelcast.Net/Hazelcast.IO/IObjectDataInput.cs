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

using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
    /// <summary>Provides serialization methods for arrays of primitive types</summary>
    public interface IObjectDataInput : IDataInput
    {
        /// <returns>ByteOrder BIG_ENDIAN or LITTLE_ENDIAN</returns>
        ByteOrder GetByteOrder();

        /// <returns>the boolean array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        bool[] ReadBooleanArray();

        /// <returns>the byte array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        byte[] ReadByteArray();

        /// <returns>the char array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        char[] ReadCharArray();

        /// <returns>data read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        IData ReadData();

        /// <returns>double array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        double[] ReadDoubleArray();

        /// <returns>float array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        float[] ReadFloatArray();

        /// <returns>int array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        int[] ReadIntArray();

        /// <returns>long array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        long[] ReadLongArray();

        /// <returns>object array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        T ReadObject<T>();

        /// <returns>short array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        short[] ReadShortArray();

        /// <returns>String array read</returns>
        /// <exception cref="System.IO.IOException">
        ///     if it reaches end of file before finish reading
        /// </exception>
        string[] ReadUTFArray();
    }
}