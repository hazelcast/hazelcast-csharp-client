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
    /// Defines a more complete source of data that can be read to deserialize an object from a buffer.
    /// </summary>
    /// <remarks>
    /// <para>The basic <see cref="IDataInput"/> supports primitive types, and <see cref="IObjectDataOutput"/>
    /// adds support for arrays of primitive types. This class adds support for reading from a buffer,
    /// at specified positions.</para>
    /// FIXME: why support for endianness here?
    /// TODO: rename + do the same to output
    /// </remarks>
    public interface IBufferObjectDataInput : IObjectDataInput, IDisposable
    {
        #region Reads

        bool ReadBoolean(int position);

        byte ReadByte(int position);

        char ReadChar(int position);

        // FIXME could the Endianness = native collide with parent ReadDouble?

        double ReadDouble(Endianness endianness = Endianness.Unspecified);

        double ReadDouble(int position, Endianness endianness = Endianness.Unspecified);

        float ReadFloat(Endianness endianness = Endianness.Unspecified);

        float ReadFloat(int position, Endianness endianness = Endianness.Unspecified);

        int ReadInt(Endianness endianness = Endianness.Unspecified);

        int ReadInt(int position, Endianness endianness = Endianness.Unspecified);

        long ReadLong(Endianness endianness = Endianness.Unspecified);

        long ReadLong(int position, Endianness endianness = Endianness.Unspecified);

        short ReadShort(Endianness endianness = Endianness.Unspecified);

        short ReadShort(int position, Endianness endianness = Endianness.Unspecified);

        #endregion

        #region Special Reads

        int Read(int position);

        #endregion

        #region Buffer Management

        void Clear();

        void Init(byte[] data, int offset);

        int Position();

        void Position(int newPos);

        void Reset();

        #endregion
    }
}
