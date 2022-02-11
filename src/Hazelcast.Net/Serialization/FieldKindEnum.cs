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
                case FieldKind.BooleanRef:
                case FieldKind.Int8Ref:
                case FieldKind.Int16Ref:
                case FieldKind.Int32Ref:
                case FieldKind.Int64Ref:
                case FieldKind.Float32Ref:
                case FieldKind.Float64Ref:
                case FieldKind.DecimalRef:
                case FieldKind.StringRef:
                case FieldKind.TimeRef:
                case FieldKind.DateRef:
                case FieldKind.TimeStampRef:
                case FieldKind.TimeStampWithTimeZoneRef:
                case FieldKind.CompactRef:
                case FieldKind.PortableRef:
                case FieldKind.ArrayOfBooleanRef:
                case FieldKind.ArrayOfInt8Ref:
                case FieldKind.ArrayOfInt16Ref:
                case FieldKind.ArrayOfInt32Ref:
                case FieldKind.ArrayOfInt64Ref:
                case FieldKind.ArrayOfFloat32Ref:
                case FieldKind.ArrayOfFloat64Ref:
                case FieldKind.ArrayOfDecimalRef:
                case FieldKind.ArrayOfTimeRef:
                case FieldKind.ArrayOfDateRef:
                case FieldKind.ArrayOfTimeStampRef:
                case FieldKind.ArrayOfTimeStampWithTimeZoneRef:
                case FieldKind.ArrayOfStringRef:
                case FieldKind.ArrayOfCompactRef:
                case FieldKind.ArrayOfPortableRef:
                    return kind;
                default:
                    throw new ArgumentException($"Value {value} is not a valid FieldKind value.", nameof(value));
            }
        }
    }
}
