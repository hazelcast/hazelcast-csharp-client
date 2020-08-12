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
    // TODO: what is this?!
    internal sealed class EmptyObjectDataOutput : IObjectDataOutput
    {
        public byte[] ToByteArray() => Array.Empty<byte>();

        public void WriteData(IData value)
        {
            // nothing
        }

        public void WriteObject(object value)
        {
            // nothing
        }

        public void Write(byte value)
        {
            // nothing
        }

        public void WriteArray(byte[] values, Endianness endianness = Endianness.Unspecified)
        {
            // nothing
        }

        public void Write(short value, Endianness endianness = Endianness.Unspecified)
        {
            // nothing
        }

        public void WriteArray(short[] values, Endianness endianness = Endianness.Unspecified)
        {
            // nothing
        }

        public void Write(int value, Endianness endianness = Endianness.Unspecified)
        {
            // nothing
        }

        public void WriteArray(int[] values, Endianness endianness = Endianness.Unspecified)
        {
            // nothing
        }

        public void Write(long value, Endianness endianness = Endianness.Unspecified)
        {
            // nothing
        }

        public void WriteArray(long[] values, Endianness endianness = Endianness.Unspecified)
        {
            // nothing
        }

        public void Write(bool value)
        {
            // nothing
        }

        public void WriteArray(bool[] values, Endianness endianness = Endianness.Unspecified)
        {
            // nothing
        }

        public void Write(char value, Endianness endianness = Endianness.Unspecified)
        {
            // nothing
        }

        public void WriteArray(char[] values, Endianness endianness = Endianness.Unspecified)
        {
            // nothing
        }

        public void Write(float value, Endianness endianness = Endianness.Unspecified)
        {
            // nothing
        }

        public void WriteArray(float[] values, Endianness endianness = Endianness.Unspecified)
        {
            // nothing
        }

        public void Write(double value, Endianness endianness = Endianness.Unspecified)
        {
            // nothing
        }

        public void WriteArray(double[] values, Endianness endianness = Endianness.Unspecified)
        {
            // nothing
        }

        public void Write(string value, Endianness endianness = Endianness.Unspecified)
        {
            // nothing
        }

        public void WriteAsChars(string value, Endianness endianness = Endianness.Unspecified)
        {
            // nothing
        }

        public void WriteAsBytes(string value)
        {
            // nothing
        }

        public void WriteArray(string[] values, Endianness endianness = Endianness.Unspecified)
        {
            // nothing
        }

        public void WriteBytes(byte[] bytes)
        {
            // nothing
        }

        public void WriteBytes(byte[] bytes, int offset, int count)
        {
            // nothing
        }
    }
}
