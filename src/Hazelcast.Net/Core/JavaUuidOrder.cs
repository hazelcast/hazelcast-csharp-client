// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Runtime.InteropServices;

namespace Hazelcast.Core;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
internal struct JavaUuidOrder
{
    [FieldOffset(0)] public Guid Value;

    // the offsets are so that X0, X1, ... are the guid bytes, MSB-first
    // ie in the same order they would appear when converting the guid to a string
    // so if the guid is 694fa83a-a282-449c-b2c0-9cc1f9f550c1, then
    // X0 is 69, X1 is 4f, X2 is a8, X3 is 3a, etc.

    [FieldOffset(3)] public byte X0;
    [FieldOffset(2)] public byte X1;
    [FieldOffset(1)] public byte X2;
    [FieldOffset(0)] public byte X3;

    [FieldOffset(5)] public byte X4;
    [FieldOffset(4)] public byte X5;
    [FieldOffset(7)] public byte X6;
    [FieldOffset(6)] public byte X7;

    [FieldOffset(8)] public byte X8;
    [FieldOffset(9)] public byte X9;
    [FieldOffset(10)] public byte XA;
    [FieldOffset(11)] public byte XB;

    [FieldOffset(12)] public byte XC;
    [FieldOffset(13)] public byte XD;
    [FieldOffset(14)] public byte XE;
    [FieldOffset(15)] public byte XF;

    // now, Java has *another* definition of the byte order, and
    // this order is the polyglot order we use for serialization

    public JavaUuidOrder ReadBytes(byte[] bytes, int position, Endianness endianess)
    {
        // assume BytesExtensions.SizeOfByte is 1
        var i = endianess == Endianness.LittleEndian ? 7 : 0;
        var s = endianess == Endianness.LittleEndian ? -1 : +1;

        int Index()
        {
            var i0 = i;
            i += s;
            return i0;
        }

        X0 = bytes[position + Index()];
        X1 = bytes[position + Index()];
        X2 = bytes[position + Index()];
        X3 = bytes[position + Index()];

        X4 = bytes[position + Index()];
        X5 = bytes[position + Index()];
        X6 = bytes[position + Index()];
        X7 = bytes[position + Index()];

        i = endianess == Endianness.LittleEndian ? 15 : 8;

        X8 = bytes[position + Index()];
        X9 = bytes[position + Index()];
        XA = bytes[position + Index()];
        XB = bytes[position + Index()];

        XC = bytes[position + Index()];
        XD = bytes[position + Index()];
        XE = bytes[position + Index()];
        XF = bytes[position + Index()];

        return this;
    }

    public void WriteBytes(byte[] bytes, int position, Endianness endianess)
    {
        // assume BytesExtensions.SizeOfByte is 1
        var i = endianess == Endianness.LittleEndian ? 7 : 0;
        var s = endianess == Endianness.LittleEndian ? -1 : +1;

        int Index()
        {
            var i0 = i;
            i += s;
            return i0;
        }

        bytes[position + Index()] = X0;
        bytes[position + Index()] = X1;
        bytes[position + Index()] = X2;
        bytes[position + Index()] = X3;

        bytes[position + Index()] = X4;
        bytes[position + Index()] = X5;
        bytes[position + Index()] = X6;
        bytes[position + Index()] = X7;

        i = endianess == Endianness.LittleEndian ? 15 : 8;

        bytes[position + Index()] = X8;
        bytes[position + Index()] = X9;
        bytes[position + Index()] = XA;
        bytes[position + Index()] = XB;

        bytes[position + Index()] = XC;
        bytes[position + Index()] = XD;
        bytes[position + Index()] = XE;
        bytes[position + Index()] = XF;
    }
}