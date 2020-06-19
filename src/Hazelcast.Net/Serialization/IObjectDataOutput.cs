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
    /// Defines a more complete destination of data that can be written to to serialize an object.
    /// </summary>
    public interface IObjectDataOutput : IDataOutput
    {
        /// <summary>
        /// Gets the endianness to use when serializing.
        /// </summary>
        Endianness Endianness { get; } // fixme why not on IDataOutput?

        /// <summary>
        /// Gets a byte array representation of the object.
        /// </summary>
        /// <returns>An array of byte representing the object.</returns>
        byte[] ToByteArray();

        /// <summary>
        /// Writes an array of <see cref="bool"/> (bool).
        /// </summary>
        /// <param name="values">The array.</param>
        void WriteBooleanArray(bool[] values);

        /// <summary>
        /// Writes an array of <see cref="byte"/> (byte).
        /// </summary>
        /// <param name="values">The array.</param>
        void WriteByteArray(byte[] values);

        /// <summary>
        /// Writes an array of <see cref="char"/> (char).
        /// </summary>
        /// <param name="values"></param>
        void WriteCharArray(char[] values);

        /// <summary>
        /// Writes an array of <see cref="double"/> double.
        /// </summary>
        /// <param name="values">The array.</param>
        void WriteDoubleArray(double[] values);

        /// <summary>
        /// Writes an array of <see cref="float"/> (float).
        /// </summary>
        /// <param name="values">The array.</param>
        void WriteFloatArray(float[] values); // fixme name

        /// <summary>
        /// Writes an array of <see cref="int"/> (int).
        /// </summary>
        /// <param name="values">The array.</param>
        void WriteIntArray(int[] values); // fixme name

        /// <summary>
        /// Writes an array of <see cref="long"/> (long).
        /// </summary>
        /// <param name="values">The array.</param>
        void WriteLongArray(long[] values); // fixme name

        /// <summary>
        /// Writes an array of <see cref="short"/> (short).
        /// </summary>
        /// <param name="values">The array.</param>
        void WriteShortArray(short[] values); // fixme name

        /// <summary>
        /// Writes an array of <see cref="string"/> (string).
        /// </summary>
        /// <param name="values">The array.</param>
        void WriteUtfArray(string[] values); // fixme name

        /// <summary>
        /// Writes an <see cref="IData"/> instance.
        /// </summary>
        /// <param name="value">The <see cref="IData"/> instance.</param>
        void WriteData(IData value);

        /// <summary>
        /// Writes an object.
        /// </summary>
        /// <param name="value">The object.</param>
        void WriteObject(object value);
    }
}
