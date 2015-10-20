/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace Hazelcast.IO.Serialization
{
	/// <summary>
	/// Provides a mean of writing portable fields to a binary in form of primitives, arrays of  primitives , nested portable fields and array of portable fields.
	/// </summary>
	/// <remarks>
    /// Provides a mean of writing portable fields to a binary in form of primitives, arrays of  primitives , nested portable fields and array of portable fields.
	/// </remarks>
	public interface IPortableWriter
	{
		/// <summary>Writes a primitive int.</summary>
		/// <remarks>Writes a primitive int.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="value">int value to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteInt(string fieldName, int value);

		/// <summary>Writes a primitive long.</summary>
		/// <remarks>Writes a primitive long.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="value">long value to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteLong(string fieldName, long value);

		/// <summary>Writes an UTF string.</summary>
		/// <remarks>Writes an UTF string.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="value">utf string value to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteUTF(string fieldName, string value);

		/// <summary>Writes a primitive boolean.</summary>
		/// <remarks>Writes a primitive boolean.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="value">int value to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteBoolean(string fieldName, bool value);

		/// <summary>Writes a primitive byte.</summary>
		/// <remarks>Writes a primitive byte.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="value">int value to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteByte(string fieldName, byte value);

		/// <summary>Writes a primitive char.</summary>
		/// <remarks>Writes a primitive char.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="value">int value to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteChar(string fieldName, int value);

		/// <summary>Writes a primitive double.</summary>
		/// <remarks>Writes a primitive double.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="value">int value to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteDouble(string fieldName, double value);

		/// <summary>Writes a primitive float.</summary>
		/// <remarks>Writes a primitive float.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="value">int value to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteFloat(string fieldName, float value);

		/// <summary>Writes a primitive short.</summary>
		/// <remarks>Writes a primitive short.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="value">int value to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteShort(string fieldName, short value);

		/// <summary>Writes a Portable.</summary>
		/// <remarks>Writes a Portable.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="portable">Portable to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WritePortable(string fieldName, IPortable portable);

		/// <summary>To write a null portable value, user needs to provide class and factoryIds of related class.
		/// 	</summary>
		/// <remarks>To write a null portable value, user needs to provide class and factoryIds of related class.
		/// 	</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="factoryId">factory id of related portable class</param>
		/// <param name="classId">class id of related portable class</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteNullPortable(string fieldName, int factoryId, int classId);

        /// <summary>Writes a primitive boolean-array.</summary>
        /// <remarks>Writes a primitive boolean-array.</remarks>
        /// <param name="fieldName">name of the field</param>
        /// <param name="bools">boolean array to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteBooleanArray(string fieldName, bool[] bools);

		/// <summary>Writes a primitive byte-array.</summary>
		/// <remarks>Writes a primitive byte-array.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="bytes">byte array to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteByteArray(string fieldName, byte[] bytes);

		/// <summary>Writes a primitive char-array.</summary>
		/// <remarks>Writes a primitive char-array.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="chars">char array to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteCharArray(string fieldName, char[] chars);

		/// <summary>Writes a primitive int-array.</summary>
		/// <remarks>Writes a primitive int-array.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="ints">int array to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteIntArray(string fieldName, int[] ints);

		/// <summary>Writes a primitive long-array.</summary>
		/// <remarks>Writes a primitive long-array.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="longs">long array to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteLongArray(string fieldName, long[] longs);

		/// <summary>Writes a primitive double array.</summary>
		/// <remarks>Writes a primitive double array.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="values">double array to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteDoubleArray(string fieldName, double[] values);

		/// <summary>Writes a primitive float array.</summary>
		/// <remarks>Writes a primitive float array.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="values">float array to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteFloatArray(string fieldName, float[] values);

		/// <summary>Writes a primitive short-array.</summary>
		/// <remarks>Writes a primitive short-array.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="values">short array to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WriteShortArray(string fieldName, short[] values);

        /// <summary>Writes a primitive string-array.</summary>
        /// <remarks>Writes a primitive string-array.</remarks>
        /// <param name="fieldName">name of the field</param>
        /// <param name="strings">string array to be written</param>
        /// <exception cref="System.IO.IOException">System.IO.IOException</exception>
        void WriteUTFArray(string fieldName, string[] strings);

		/// <summary>Writes a an array of Portables.</summary>
		/// <remarks>Writes a an array of Portables.</remarks>
		/// <param name="fieldName">name of the field</param>
		/// <param name="portables">portable array to be written</param>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		void WritePortableArray(string fieldName, IPortable[] portables);

		/// <summary>
		/// After writing portable fields, one can write remaining fields in old fashioned way consecutively at the end
		/// of stream.
		/// </summary>
		/// <remarks>
		/// After writing portable fields, one can write remaining fields in old fashioned way consecutively at the end
		/// of stream. User should not that after getting raw dataoutput trying to write portable fields will result
		/// in IOException
		/// </remarks>
		/// <returns>ObjectDataOutput</returns>
		/// <exception cref="System.IO.IOException">System.IO.IOException</exception>
		IObjectDataOutput GetRawDataOutput();
	}
}
