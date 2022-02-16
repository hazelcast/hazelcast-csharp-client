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

using System;

namespace Hazelcast.Serialization
{
    internal static class FieldKindEnum
    {
        public static FieldKind Parse(int value)
        {
            var kind = (FieldKind)value; // this is the only place where this is acceptable
            switch (kind)
            {
                case FieldKind.Boolean:
                case FieldKind.Int8:
                case FieldKind.Char:
                case FieldKind.Int16:
                case FieldKind.Int32:
                case FieldKind.Int64:
                case FieldKind.Float32:
                case FieldKind.Float64:
                case FieldKind.ArrayOfBoolean:
                case FieldKind.ArrayOfInt8:
                case FieldKind.ArrayOfChar:
                case FieldKind.ArrayOfInt16:
                case FieldKind.ArrayOfInt32:
                case FieldKind.ArrayOfInt64:
                case FieldKind.ArrayOfFloat32:
                case FieldKind.ArrayOfFloat64:
                case FieldKind.NullableBoolean:
                case FieldKind.NullableInt8:
                case FieldKind.NullableInt16:
                case FieldKind.NullableInt32:
                case FieldKind.NullableInt64:
                case FieldKind.NullableFloat32:
                case FieldKind.NullableFloat64:
                case FieldKind.NullableDecimal:
                case FieldKind.NullableString:
                case FieldKind.NullableTime:
                case FieldKind.NullableDate:
                case FieldKind.NullableTimeStamp:
                case FieldKind.NullableTimeStampWithTimeZone:
                case FieldKind.NullableCompact:
                case FieldKind.NullablePortable:
                case FieldKind.ArrayOfNullableBoolean:
                case FieldKind.ArrayOfNullableInt8:
                case FieldKind.ArrayOfNullableInt16:
                case FieldKind.ArrayOfNullableInt32:
                case FieldKind.ArrayOfNullableInt64:
                case FieldKind.ArrayOfNullableFloat32:
                case FieldKind.ArrayOfNullableFloat64:
                case FieldKind.ArrayOfNullableDecimal:
                case FieldKind.ArrayOfNullableTime:
                case FieldKind.ArrayOfNullableDate:
                case FieldKind.ArrayOfNullableTimeStamp:
                case FieldKind.ArrayOfNullableTimeStampWithTimeZone:
                case FieldKind.ArrayOfNullableString:
                case FieldKind.ArrayOfNullableCompact:
                case FieldKind.ArrayOfNullablePortable:
                    return kind;
                default:
                    throw new ArgumentException($"Value {value} is not a valid FieldKind value.", nameof(value));
            }
        }
    }
}
