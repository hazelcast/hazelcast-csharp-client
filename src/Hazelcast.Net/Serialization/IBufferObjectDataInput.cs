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
    /// Extends <see cref="IObjectDataInput"/> with support for a buffer.
    /// </summary>
    public interface IBufferObjectDataInput : IObjectDataInput, IDisposable
    {
        // TODO: sbyte?

        /// <summary>
        /// Reads a <see cref="byte"/> value at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>The value.</returns>
        /// <remarks>
        /// <para>This method does not alter the current position of the buffer.</para>
        /// </remarks>
        byte ReadByte(int position);

        /// <summary>
        /// Reads a <see cref="short"/> value at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        /// <remarks>
        /// <para>This method does not alter the current position of the buffer.</para>
        /// </remarks>
        short ReadShort(int position, Endianness endianness = Endianness.Unspecified);

        // TODO: ushort?

        /// <summary>
        /// Reads an <see cref="int"/> value at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        /// <remarks>
        /// <para>This method does not alter the current position of the buffer.</para>
        /// </remarks>
        int ReadInt(int position, Endianness endianness = Endianness.Unspecified);

        // TODO: uint?

        /// <summary>
        /// Reads a <see cref="long"/> value at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        /// <remarks>
        /// <para>This method does not alter the current position of the buffer.</para>
        /// </remarks>
        long ReadLong(int position, Endianness endianness = Endianness.Unspecified);

        // TODO: ulong?



        /// <summary>
        /// Reads a <see cref="bool"/> value at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>The value.</returns>
        /// <remarks>
        /// <para>This method does not alter the current position of the buffer.</para>
        /// </remarks>
        bool ReadBool(int position);



        /// <summary>
        /// Reads a <see cref="char"/> value at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        /// <remarks>
        /// <para>This method does not alter the current position of the buffer.</para>
        /// </remarks>
        char ReadChar(int position, Endianness endianness = Endianness.Unspecified);



        /// <summary>
        /// Reads a <see cref="float"/> value at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        /// <remarks>
        /// <para>This method does not alter the current position of the buffer.</para>
        /// </remarks>
        float ReadFloat(int position, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Reads a <see cref="double"/> value at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="endianness">The endianness.</param>
        /// <returns>The value.</returns>
        /// <remarks>
        /// <para>This method does not alter the current position of the buffer.</para>
        /// </remarks>
        double ReadDouble(int position, Endianness endianness = Endianness.Unspecified);



        /// <summary>
        /// Clears the buffer after use (releases inner bytes).
        /// </summary>
        void Clear();

        /// <summary>
        /// Initializes the buffer for re-use.
        /// </summary>
        /// <param name="data">The buffer data.</param>
        /// <param name="offset">The buffer data offset.</param>
        void Initialize(byte[] data, int offset);

        /// <summary>
        /// Gets or sets the position in the buffer.
        /// </summary>
        int Position { get; set; }
    }
}
