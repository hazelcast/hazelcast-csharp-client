// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Core
{
    // the following GUID: "00010203-0405-0607-0809-0a0b0c0d0e0f" is:
    // in .NET ToByteArray as 3, 2, 1, 0,   5, 4, 7, 6,   8, 9, 10, 11,   12, 13, 14, 15
    // in Java UUIDCodec   as 0, 1, 2, 3,   4, 5, 6, 7,   8, 9, 10, 11,   12, 13, 14, 15
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    internal struct JavaUuidOrder
    {
        [FieldOffset(0)] public Guid Value;

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
    }
}
