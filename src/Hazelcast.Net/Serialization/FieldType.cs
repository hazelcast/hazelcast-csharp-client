// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
    public enum FieldType
    {
#pragma warning disable CA1720 // Identifier contains type name - yes, happy with it
        Portable = 0,
        Byte = 1,
        Boolean = 2,
        Char = 3,
        Short = 4,
        Int = 5,
        Long = 6,
        Float = 7,
        Double = 8,
        Utf = 9,
        PortableArray = 10,
        ByteArray = 11,
        BooleanArray = 12,
        CharArray = 13,
        ShortArray = 14,
        IntArray = 15,
        LongArray = 16,
        FloatArray = 17,
        DoubleArray = 18,
        UtfArray = 19
#pragma warning restore CA1720
    }
}
