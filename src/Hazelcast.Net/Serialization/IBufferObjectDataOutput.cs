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
    /// Extends <see cref="IObjectDataOutput"/> with support for a buffer.
    /// </summary>
    public interface IBufferObjectDataOutput : IObjectDataOutput, IDisposable
    {
        /// <summary>
        /// Writes a <see cref="byte"/> value at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="value">The value.</param>
        void Write(int position, byte value);

        /// <summary>
        /// Writes a <see cref="short"/> value at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The endianness.</param>
        void Write(int position, short value, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Writes a <see cref="int"/> value at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The endianness.</param>
        void Write(int position, int value, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Writes a <see cref="long"/> value at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The endianness.</param>
        void Write(int position, long value, Endianness endianness = Endianness.Unspecified);



        /// <summary>
        /// Writes a <see cref="bool"/> value at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="value">The value.</param>
        void Write(int position, bool value);



        /// <summary>
        /// Writes a <see cref="char"/> value at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The endianness.</param>
        void Write(int position, char value, Endianness endianness = Endianness.Unspecified);



        /// <summary>
        /// Writes a <see cref="float"/> value at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The endianness.</param>
        void Write(int position, float value, Endianness endianness = Endianness.Unspecified);

        /// <summary>
        /// Writes a <see cref="double"/> value at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="value">The value.</param>
        /// <param name="endianness">The endianness.</param>
        void Write(int position, double value, Endianness endianness = Endianness.Unspecified);



        /// <summary>
        /// Writes zero bytes.
        /// </summary>
        /// <param name="count"></param>
        void WriteZeroBytes(int count);



        /// <summary>
        /// Clears the buffer.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the position in the buffer.
        /// </summary>
        int Position { get; }
    }
}
