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

using Hazelcast.Models;

namespace Hazelcast.Serialization.Compact;

internal partial class CompactGenericRecordBuilder
{
    // <generated>

    /// <inheritdoc />
    public IGenericRecordBuilder SetBoolean(string fieldname, bool value) => SetField(fieldname, value, FieldKind.Boolean);

    /// <inheritdoc />
    public IGenericRecordBuilder SetInt8(string fieldname, sbyte value) => SetField(fieldname, value, FieldKind.Int8);

    /// <inheritdoc />
    public IGenericRecordBuilder SetInt16(string fieldname, short value) => SetField(fieldname, value, FieldKind.Int16);

    /// <inheritdoc />
    public IGenericRecordBuilder SetInt32(string fieldname, int value) => SetField(fieldname, value, FieldKind.Int32);

    /// <inheritdoc />
    public IGenericRecordBuilder SetInt64(string fieldname, long value) => SetField(fieldname, value, FieldKind.Int64);

    /// <inheritdoc />
    public IGenericRecordBuilder SetFloat32(string fieldname, float value) => SetField(fieldname, value, FieldKind.Float32);

    /// <inheritdoc />
    public IGenericRecordBuilder SetFloat64(string fieldname, double value) => SetField(fieldname, value, FieldKind.Float64);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfBoolean(string fieldname, bool[]? value) => SetField(fieldname, value, FieldKind.ArrayOfBoolean);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfInt8(string fieldname, sbyte[]? value) => SetField(fieldname, value, FieldKind.ArrayOfInt8);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfInt16(string fieldname, short[]? value) => SetField(fieldname, value, FieldKind.ArrayOfInt16);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfInt32(string fieldname, int[]? value) => SetField(fieldname, value, FieldKind.ArrayOfInt32);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfInt64(string fieldname, long[]? value) => SetField(fieldname, value, FieldKind.ArrayOfInt64);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfFloat32(string fieldname, float[]? value) => SetField(fieldname, value, FieldKind.ArrayOfFloat32);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfFloat64(string fieldname, double[]? value) => SetField(fieldname, value, FieldKind.ArrayOfFloat64);

    /// <inheritdoc />
    public IGenericRecordBuilder SetNullableBoolean(string fieldname, bool? value) => SetField(fieldname, value, FieldKind.NullableBoolean);

    /// <inheritdoc />
    public IGenericRecordBuilder SetNullableInt8(string fieldname, sbyte? value) => SetField(fieldname, value, FieldKind.NullableInt8);

    /// <inheritdoc />
    public IGenericRecordBuilder SetNullableInt16(string fieldname, short? value) => SetField(fieldname, value, FieldKind.NullableInt16);

    /// <inheritdoc />
    public IGenericRecordBuilder SetNullableInt32(string fieldname, int? value) => SetField(fieldname, value, FieldKind.NullableInt32);

    /// <inheritdoc />
    public IGenericRecordBuilder SetNullableInt64(string fieldname, long? value) => SetField(fieldname, value, FieldKind.NullableInt64);

    /// <inheritdoc />
    public IGenericRecordBuilder SetNullableFloat32(string fieldname, float? value) => SetField(fieldname, value, FieldKind.NullableFloat32);

    /// <inheritdoc />
    public IGenericRecordBuilder SetNullableFloat64(string fieldname, double? value) => SetField(fieldname, value, FieldKind.NullableFloat64);

    /// <inheritdoc />
    public IGenericRecordBuilder SetDecimal(string fieldname, HBigDecimal? value) => SetField(fieldname, value, FieldKind.Decimal);

    /// <inheritdoc />
    public IGenericRecordBuilder SetString(string fieldname, string? value) => SetField(fieldname, value, FieldKind.String);

    /// <inheritdoc />
    public IGenericRecordBuilder SetTime(string fieldname, HLocalTime? value) => SetField(fieldname, value, FieldKind.Time);

    /// <inheritdoc />
    public IGenericRecordBuilder SetDate(string fieldname, HLocalDate? value) => SetField(fieldname, value, FieldKind.Date);

    /// <inheritdoc />
    public IGenericRecordBuilder SetTimeStamp(string fieldname, HLocalDateTime? value) => SetField(fieldname, value, FieldKind.TimeStamp);

    /// <inheritdoc />
    public IGenericRecordBuilder SetTimeStampWithTimeZone(string fieldname, HOffsetDateTime? value) => SetField(fieldname, value, FieldKind.TimeStampWithTimeZone);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfNullableBoolean(string fieldname, bool?[]? value) => SetField(fieldname, value, FieldKind.ArrayOfNullableBoolean);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfNullableInt8(string fieldname, sbyte?[]? value) => SetField(fieldname, value, FieldKind.ArrayOfNullableInt8);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfNullableInt16(string fieldname, short?[]? value) => SetField(fieldname, value, FieldKind.ArrayOfNullableInt16);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfNullableInt32(string fieldname, int?[]? value) => SetField(fieldname, value, FieldKind.ArrayOfNullableInt32);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfNullableInt64(string fieldname, long?[]? value) => SetField(fieldname, value, FieldKind.ArrayOfNullableInt64);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfNullableFloat32(string fieldname, float?[]? value) => SetField(fieldname, value, FieldKind.ArrayOfNullableFloat32);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfNullableFloat64(string fieldname, double?[]? value) => SetField(fieldname, value, FieldKind.ArrayOfNullableFloat64);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfDecimal(string fieldname, HBigDecimal?[]? value) => SetField(fieldname, value, FieldKind.ArrayOfDecimal);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfTime(string fieldname, HLocalTime?[]? value) => SetField(fieldname, value, FieldKind.ArrayOfTime);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfDate(string fieldname, HLocalDate?[]? value) => SetField(fieldname, value, FieldKind.ArrayOfDate);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfTimeStamp(string fieldname, HLocalDateTime?[]? value) => SetField(fieldname, value, FieldKind.ArrayOfTimeStamp);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfTimeStampWithTimeZone(string fieldname, HOffsetDateTime?[]? value) => SetField(fieldname, value, FieldKind.ArrayOfTimeStampWithTimeZone);

    /// <inheritdoc />
    public IGenericRecordBuilder SetArrayOfString(string fieldname, string?[]? value) => SetField(fieldname, value, FieldKind.ArrayOfString);

    // </generated>
}
