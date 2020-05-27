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

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Defines a source of data that can be read to deserialize an object.
    /// </summary>
    public interface IDataInput
    {
        #region Reads

        /// <summary>
        /// Reads a <see cref="bool"/> (bool) value.
        /// </summary>
        /// <returns>The value.</returns>
        bool ReadBoolean();

        /// <summary>
        /// Reads a <see cref="byte"/> (byte) value.
        /// </summary>
        /// <returns>The value.</returns>
        byte ReadByte();

        /// <summary>
        /// Reads a <see cref="char"/> (char) value.
        /// </summary>
        /// <returns>The value.</returns>
        char ReadChar();

        /// <summary>
        /// Reads a <see cref="double"/> (double) value.
        /// </summary>
        /// <returns>The value.</returns>
        double ReadDouble();

        /// <summary>
        /// Reads a <see cref="float"/> (float) value.
        /// </summary>
        /// <returns>The value.</returns>
        float ReadFloat(); // fixme name

        /// <summary>
        /// Reads an <see cref="int"/> (int) value.
        /// </summary>
        /// <returns>The value.</returns>
        int ReadInt();

        /// <summary>
        /// Reads an <see cref="long"/> (long) value.
        /// </summary>
        /// <returns>The value.</returns>
        long ReadLong(); // fixme name

        /// <summary>
        /// Reads an <see cref="short"/> (short) value.
        /// </summary>
        /// <returns>The value.</returns>
        short ReadShort();

        /// <summary>
        /// Reads an <see cref="Unsi"/> () value.
        /// </summary>
        /// <returns></returns>
        int ReadUnsignedByte(); // fixme name, return type?

        /// <summary>
        /// Reads an <see cref="ushort"/> (ushort) value.
        /// </summary>
        /// <returns>The value.</returns>
        int ReadUnsignedShort(); // fixme name, return type?

        /// <summary>
        /// Reads a <see cref="string"/> (string) value.
        /// </summary>
        /// <returns>The value.</returns>
        string ReadUtf(); // fixme name

        #endregion

        #region Special Reads

        /// <summary>
        /// Reads the whole object.
        /// </summary>
        /// <param name="bytes"></param>
        void ReadFully(byte[] bytes); // fixme what is this doing?

        /// <summary>
        /// Reads the whole object.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        void ReadFully(byte[] bytes, int offset, int length); // fixme what is this doing?

        #endregion

        #region Buffer Management

        /// <summary>
        /// Skips bytes.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        int SkipBytes(int count); // fixme what? return?

        #endregion
    }
}