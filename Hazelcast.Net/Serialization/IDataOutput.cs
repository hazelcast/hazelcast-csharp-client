﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Defines a destination of data that can be written to to serialize an object.
    /// </summary>
    public interface IDataOutput
    {
        /// <summary>
        /// Writes an <see cref="Int32"/> (int) value.
        /// </summary>
        /// <param name="value">The value.</param>
        void Write(int value); // fixme duplicate (see below)?

        /// <summary>
        /// Writes a <see cref="Boolean"/> (bool) value.
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteBoolean(bool value);

        /// <summary>
        /// Writes a <see cref="Char"/> (char) value.
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteChar(int value);

        /// <summary>
        /// Writes a <see cref="Double"/> (double) value.
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteDouble(double value);

        /// <summary>
        /// Writes a <see cref="Single"/> (float) value.
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteFloat(float value); // fixme name

        /// <summary>
        /// Writes an <see cref="Int32"/> (int) value.
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteInt(int value); // fixme name

        /// <summary>
        /// Writes an <see cref="Int64"/> (long) value.
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteLong(long value); // fixme name

        /// <summary>
        /// Writes a <see cref="Int16"/> (short) value.
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteShort(int value); // fixme name

        /// <summary>
        /// Writes a <see cref="String"/> (string) value.
        /// </summary>
        /// <param name="value">The value.</param>
        void WriteUtf(string value); // fixme name




        void Write(byte[] b);

        /// <exception cref="System.IO.IOException"></exception>
        void Write(byte[] b, int off, int len);


        /// <exception cref="System.IO.IOException"></exception>
        void WriteByte(int v);

        /// <exception cref="System.IO.IOException"></exception>
        void WriteBytes(string s);


        /// <exception cref="System.IO.IOException"></exception>
        void WriteChars(string s);

    }
}