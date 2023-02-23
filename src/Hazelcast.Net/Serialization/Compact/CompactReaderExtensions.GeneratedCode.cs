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

#nullable enable

using System;

namespace Hazelcast.Serialization.Compact;

internal static class CompactReaderExtensions
{
    public static object? ReadAny(this ICompactReader reader, string name, FieldKind kind)
    {
        switch (kind)
        {
            // <generated>

            case FieldKind.Boolean: return reader.ReadBoolean(name);
            case FieldKind.Int8: return reader.ReadInt8(name);
            case FieldKind.Int16: return reader.ReadInt16(name);
            case FieldKind.Int32: return reader.ReadInt32(name);
            case FieldKind.Int64: return reader.ReadInt64(name);
            case FieldKind.Float32: return reader.ReadFloat32(name);
            case FieldKind.Float64: return reader.ReadFloat64(name);
            case FieldKind.ArrayOfBoolean: return reader.ReadArrayOfBoolean(name);
            case FieldKind.ArrayOfInt8: return reader.ReadArrayOfInt8(name);
            case FieldKind.ArrayOfInt16: return reader.ReadArrayOfInt16(name);
            case FieldKind.ArrayOfInt32: return reader.ReadArrayOfInt32(name);
            case FieldKind.ArrayOfInt64: return reader.ReadArrayOfInt64(name);
            case FieldKind.ArrayOfFloat32: return reader.ReadArrayOfFloat32(name);
            case FieldKind.ArrayOfFloat64: return reader.ReadArrayOfFloat64(name);
            case FieldKind.NullableBoolean: return reader.ReadNullableBoolean(name);
            case FieldKind.NullableInt8: return reader.ReadNullableInt8(name);
            case FieldKind.NullableInt16: return reader.ReadNullableInt16(name);
            case FieldKind.NullableInt32: return reader.ReadNullableInt32(name);
            case FieldKind.NullableInt64: return reader.ReadNullableInt64(name);
            case FieldKind.NullableFloat32: return reader.ReadNullableFloat32(name);
            case FieldKind.NullableFloat64: return reader.ReadNullableFloat64(name);
            case FieldKind.Decimal: return reader.ReadDecimal(name);
            case FieldKind.String: return reader.ReadString(name);
            case FieldKind.Time: return reader.ReadTime(name);
            case FieldKind.Date: return reader.ReadDate(name);
            case FieldKind.TimeStamp: return reader.ReadTimeStamp(name);
            case FieldKind.TimeStampWithTimeZone: return reader.ReadTimeStampWithTimeZone(name);
            case FieldKind.ArrayOfNullableBoolean: return reader.ReadArrayOfNullableBoolean(name);
            case FieldKind.ArrayOfNullableInt8: return reader.ReadArrayOfNullableInt8(name);
            case FieldKind.ArrayOfNullableInt16: return reader.ReadArrayOfNullableInt16(name);
            case FieldKind.ArrayOfNullableInt32: return reader.ReadArrayOfNullableInt32(name);
            case FieldKind.ArrayOfNullableInt64: return reader.ReadArrayOfNullableInt64(name);
            case FieldKind.ArrayOfNullableFloat32: return reader.ReadArrayOfNullableFloat32(name);
            case FieldKind.ArrayOfNullableFloat64: return reader.ReadArrayOfNullableFloat64(name);
            case FieldKind.ArrayOfDecimal: return reader.ReadArrayOfDecimal(name);
            case FieldKind.ArrayOfTime: return reader.ReadArrayOfTime(name);
            case FieldKind.ArrayOfDate: return reader.ReadArrayOfDate(name);
            case FieldKind.ArrayOfTimeStamp: return reader.ReadArrayOfTimeStamp(name);
            case FieldKind.ArrayOfTimeStampWithTimeZone: return reader.ReadArrayOfTimeStampWithTimeZone(name);
            case FieldKind.ArrayOfString: return reader.ReadArrayOfString(name);

            // </generated>

            default: throw new NotSupportedException();
        }
    }
}
