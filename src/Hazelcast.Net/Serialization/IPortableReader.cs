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

using System.Collections.Generic;

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Provides a mean of reading portable fields from a binary in form of primitives, arrays of primitives , nested portable fields and array of portable fields.
    /// </summary>
    /// <remarks>
    /// Provides a mean of reading portable fields from a binary in form of primitives, arrays of primitives , nested portable fields and array of portable fields.
    /// </remarks>
    public interface IPortableReader
    {
        /// <param name="fieldName">name of the field</param>
        /// <returns>classId of given field</returns>
        int GetFieldClassId(string fieldName);

        /// <returns>set of field names on this portable class</returns>
        ICollection<string> GetFieldNames();

        /// <param name="fieldName">name of the field</param>
        /// <returns>field type of given fieldName</returns>
        FieldType GetFieldType(string fieldName);

        /// <summary>
        /// <see cref="IPortableWriter.GetRawDataOutput()">IPortableWriter.GetRawDataOutput()</see>
        /// .
        /// <p/>
        /// Note that portable fields can not read after GetRawDataInput() is called. In case this happens,
        /// IOException will be thrown.
        /// </summary>
        /// <returns>rawDataInput</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        IObjectDataInput GetRawDataInput();

        /// <returns>version global version of portable classes</returns>
        int Version { get; }

        /// <param name="fieldName">name of the field</param>
        /// <returns>true if field exist in this class.</returns>
        bool HasField(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the boolean value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        bool ReadBoolean(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the boolean array value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        bool[] ReadBooleanArray(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the byte value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        byte ReadByte(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the byte array value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        byte[] ReadByteArray(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the char value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        char ReadChar(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the char array value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        char[] ReadCharArray(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the double value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        double ReadDouble(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the double array value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        double[] ReadDoubleArray(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the float value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        float ReadFloat(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the float array value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        float[] ReadFloatArray(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the int value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        int ReadInt(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the int array value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        int[] ReadIntArray(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the long value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        long ReadLong(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the long array value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        long[] ReadLongArray(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the portable value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        TPortable ReadPortable<TPortable>(string fieldName) where TPortable : IPortable;

        /// <param name="fieldName">name of the field</param>
        /// <returns>the portabke value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        TPortable[] ReadPortableArray<TPortable>(string fieldName) where TPortable : IPortable;

        /// <param name="fieldName">name of the field</param>
        /// <returns>the short value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        short ReadShort(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the short array value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        short[] ReadShortArray(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the utf string value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        string ReadUTF(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>the string array value read</returns>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        string[] ReadUTFArray(string fieldName);
    }
}
