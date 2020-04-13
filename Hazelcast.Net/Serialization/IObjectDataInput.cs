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
using Hazelcast.Core;

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Defines a more complete source of data that can be read to deserialize an object.
    /// </summary>
    public interface IObjectDataInput : IDataInput
    {
        /// <summary>
        /// Gets the endianness to use when deserializing.
        /// </summary>
        Endianness Endianness { get; } // fixme why not on IDataInput?

        /// <summary>
        /// Reads an array of <see cref="Boolean"/> (bool).
        /// </summary>
        /// <returns>The array.</returns>
        bool[] ReadBooleanArray();

        /// <summary>
        /// Reads an array of <see cref="Byte"/> (byte).
        /// </summary>
        /// <returns>The array.</returns>
        byte[] ReadByteArray();

        /// <summary>
        /// Reads an array of <see cref="Char"/> (char).
        /// </summary>
        /// <returns>The array.</returns>
        char[] ReadCharArray();

        /// <summary>
        /// Reads an array of <see cref="Double"/> (double).
        /// </summary>
        /// <returns>The array.</returns>
        double[] ReadDoubleArray();

        /// <summary>
        /// Reads an array of <see cref="Single"/> (float).
        /// </summary>
        /// <returns>The array.</returns>
        float[] ReadFloatArray(); // fixme name

        /// <summary>
        /// Reads an array of <see cref="Int32"/> (int).
        /// </summary>
        /// <returns></returns>
        int[] ReadIntArray(); // fixme name

        /// <summary>
        /// Reads an array of <see cref="Int64"/> (long).
        /// </summary>
        /// <returns></returns>
        long[] ReadLongArray(); // fixme name

        /// <summary>
        /// Reads an array of <see cref="Int16"/> (short).
        /// </summary>
        /// <returns>The array.</returns>
        short[] ReadShortArray(); // fixme name

        /// <summary>
        /// Reads an array of <see cref="String"/> (string).
        /// </summary>
        /// <returns>The array.</returns>
        string[] ReadUtfArray(); // fixme name

        /// <summary>
        /// Reads an <see cref="IData"/> instance.
        /// </summary>
        /// <returns>The <see cref="IData"/> instance.</returns>
        IData ReadData();

        /// <summary>
        /// Reads an object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>The object.</returns>
        T ReadObject<T>();
    }
}