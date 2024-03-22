// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Models;

namespace Hazelcast.Serialization.Compact;

internal static class CompactWriterExtensions
{
    public static void WriteAny(this ICompactWriter writer, string fieldname, FieldKind kind, object? value)
    {
        switch (kind)
        {
            // <generated>

            case FieldKind.Boolean: writer.WriteBoolean(fieldname, ConvertEx.UnboxNonNull<bool>(value)); break;
            case FieldKind.Int8: writer.WriteInt8(fieldname, ConvertEx.UnboxNonNull<sbyte>(value)); break;
            case FieldKind.Int16: writer.WriteInt16(fieldname, ConvertEx.UnboxNonNull<short>(value)); break;
            case FieldKind.Int32: writer.WriteInt32(fieldname, ConvertEx.UnboxNonNull<int>(value)); break;
            case FieldKind.Int64: writer.WriteInt64(fieldname, ConvertEx.UnboxNonNull<long>(value)); break;
            case FieldKind.Float32: writer.WriteFloat32(fieldname, ConvertEx.UnboxNonNull<float>(value)); break;
            case FieldKind.Float64: writer.WriteFloat64(fieldname, ConvertEx.UnboxNonNull<double>(value)); break;
            case FieldKind.ArrayOfBoolean: writer.WriteArrayOfBoolean(fieldname, (bool[]?) value); break;
            case FieldKind.ArrayOfInt8: writer.WriteArrayOfInt8(fieldname, (sbyte[]?) value); break;
            case FieldKind.ArrayOfInt16: writer.WriteArrayOfInt16(fieldname, (short[]?) value); break;
            case FieldKind.ArrayOfInt32: writer.WriteArrayOfInt32(fieldname, (int[]?) value); break;
            case FieldKind.ArrayOfInt64: writer.WriteArrayOfInt64(fieldname, (long[]?) value); break;
            case FieldKind.ArrayOfFloat32: writer.WriteArrayOfFloat32(fieldname, (float[]?) value); break;
            case FieldKind.ArrayOfFloat64: writer.WriteArrayOfFloat64(fieldname, (double[]?) value); break;
            case FieldKind.NullableBoolean: writer.WriteNullableBoolean(fieldname, (bool?) value); break;
            case FieldKind.NullableInt8: writer.WriteNullableInt8(fieldname, (sbyte?) value); break;
            case FieldKind.NullableInt16: writer.WriteNullableInt16(fieldname, (short?) value); break;
            case FieldKind.NullableInt32: writer.WriteNullableInt32(fieldname, (int?) value); break;
            case FieldKind.NullableInt64: writer.WriteNullableInt64(fieldname, (long?) value); break;
            case FieldKind.NullableFloat32: writer.WriteNullableFloat32(fieldname, (float?) value); break;
            case FieldKind.NullableFloat64: writer.WriteNullableFloat64(fieldname, (double?) value); break;
            case FieldKind.Decimal: writer.WriteDecimal(fieldname, (HBigDecimal?) value); break;
            case FieldKind.String: writer.WriteString(fieldname, (string?) value); break;
            case FieldKind.Time: writer.WriteTime(fieldname, (HLocalTime?) value); break;
            case FieldKind.Date: writer.WriteDate(fieldname, (HLocalDate?) value); break;
            case FieldKind.TimeStamp: writer.WriteTimeStamp(fieldname, (HLocalDateTime?) value); break;
            case FieldKind.TimeStampWithTimeZone: writer.WriteTimeStampWithTimeZone(fieldname, (HOffsetDateTime?) value); break;
            case FieldKind.ArrayOfNullableBoolean: writer.WriteArrayOfNullableBoolean(fieldname, (bool?[]?) value); break;
            case FieldKind.ArrayOfNullableInt8: writer.WriteArrayOfNullableInt8(fieldname, (sbyte?[]?) value); break;
            case FieldKind.ArrayOfNullableInt16: writer.WriteArrayOfNullableInt16(fieldname, (short?[]?) value); break;
            case FieldKind.ArrayOfNullableInt32: writer.WriteArrayOfNullableInt32(fieldname, (int?[]?) value); break;
            case FieldKind.ArrayOfNullableInt64: writer.WriteArrayOfNullableInt64(fieldname, (long?[]?) value); break;
            case FieldKind.ArrayOfNullableFloat32: writer.WriteArrayOfNullableFloat32(fieldname, (float?[]?) value); break;
            case FieldKind.ArrayOfNullableFloat64: writer.WriteArrayOfNullableFloat64(fieldname, (double?[]?) value); break;
            case FieldKind.ArrayOfDecimal: writer.WriteArrayOfDecimal(fieldname, (HBigDecimal?[]?) value); break;
            case FieldKind.ArrayOfTime: writer.WriteArrayOfTime(fieldname, (HLocalTime?[]?) value); break;
            case FieldKind.ArrayOfDate: writer.WriteArrayOfDate(fieldname, (HLocalDate?[]?) value); break;
            case FieldKind.ArrayOfTimeStamp: writer.WriteArrayOfTimeStamp(fieldname, (HLocalDateTime?[]?) value); break;
            case FieldKind.ArrayOfTimeStampWithTimeZone: writer.WriteArrayOfTimeStampWithTimeZone(fieldname, (HOffsetDateTime?[]?) value); break;
            case FieldKind.ArrayOfString: writer.WriteArrayOfString(fieldname, (string?[]?) value); break;

            // </generated>

            default: throw new NotSupportedException($"Not supported field kind: {kind}.");
        }
    }
}
