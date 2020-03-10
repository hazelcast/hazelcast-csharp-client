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

using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
    /// <summary>Provides serialization methods for arrays by extending DataOutput</summary>
    public interface IObjectDataOutput : IDataOutput
    {
        /// <returns>ByteOrder BIG_ENDIAN or LITTLE_ENDIAN</returns>
        ByteOrder GetByteOrder();

        /// <returns>copy of internal byte array</returns>
        byte[] ToByteArray();

        /// <param name="booleans">boolean array to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteBooleanArray(bool[] booleans);

        /// <param name="bytes">byte array to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteByteArray(byte[] bytes);

        /// <param name="chars">char array to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteCharArray(char[] chars);

        /// <param name="data">data to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteData(IData data);

        /// <param name="values">double to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteDoubleArray(double[] values);

        /// <param name="values">float to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteFloatArray(float[] values);

        /// <param name="ints">int array to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteIntArray(int[] ints);

        /// <param name="longs">long array to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteLongArray(long[] longs);

        /// <param name="object">object to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteObject(object @object);

        /// <param name="values">short to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteShortArray(short[] values);

        /// <param name="values">string array to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteUTFArray(string[] values);
    }
}