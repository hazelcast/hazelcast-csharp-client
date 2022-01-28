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
                case FieldKind.SignedInteger8:
                case FieldKind.SignedInteger16:
                case FieldKind.SignedInteger32:
                case FieldKind.SignedInteger64:
                case FieldKind.Float:
                case FieldKind.Double:
                case FieldKind.ArrayOfBoolean:
                case FieldKind.ArrayOfSignedInteger8:
                case FieldKind.ArrayOfSignedInteger16:
                case FieldKind.ArrayOfSignedInteger32:
                case FieldKind.ArrayOfSignedInteger64:
                case FieldKind.ArrayOfFloat:
                case FieldKind.ArrayOfDouble:
                case FieldKind.BooleanRef:
                case FieldKind.SignedInteger8Ref:
                case FieldKind.SignedInteger16Ref:
                case FieldKind.SignedInteger32Ref:
                case FieldKind.SignedInteger64Ref:
                case FieldKind.FloatRef:
                case FieldKind.DoubleRef:
                case FieldKind.DecimalRef:
                case FieldKind.String:
                case FieldKind.TimeRef:
                case FieldKind.DateRef:
                case FieldKind.TimeStampRef:
                case FieldKind.TimeStampWithTimeZoneRef:
                case FieldKind.Object:
                case FieldKind.ArrayOfBooleanRef:
                case FieldKind.ArrayOfSignedInteger8Ref:
                case FieldKind.ArrayOfSignedInteger16Ref:
                case FieldKind.ArrayOfSignedInteger32Ref:
                case FieldKind.ArrayOfSignedInteger64Ref:
                case FieldKind.ArrayOfFloatRef:
                case FieldKind.ArrayOfDoubleRef:
                case FieldKind.ArrayOfDecimalRef:
                case FieldKind.ArrayOfTimeRef:
                case FieldKind.ArrayOfDateRef:
                case FieldKind.ArrayOfTimeStampRef:
                case FieldKind.ArrayOfTimeStampWithTimeZoneRef:
                case FieldKind.ArrayOfString:
                case FieldKind.ArrayOfObject:
                    return kind;
                default:
                    throw new ArgumentException($"Value {value} is not a valid FieldKind value.", nameof(value));
            }
        }
    }
}
