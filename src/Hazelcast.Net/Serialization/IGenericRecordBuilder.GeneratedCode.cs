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

#nullable enable

using Hazelcast.Models;

namespace Hazelcast.Serialization;

public partial interface IGenericRecordBuilder
{
    // <generated>

    /// <summary>
    /// Adds a <see cref="FieldKind.Boolean"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetBoolean(string fieldname, bool value);

    /// <summary>
    /// Adds a <see cref="FieldKind.Int8"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetInt8(string fieldname, sbyte value);

    /// <summary>
    /// Adds a <see cref="FieldKind.Int16"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetInt16(string fieldname, short value);

    /// <summary>
    /// Adds a <see cref="FieldKind.Int32"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetInt32(string fieldname, int value);

    /// <summary>
    /// Adds a <see cref="FieldKind.Int64"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetInt64(string fieldname, long value);

    /// <summary>
    /// Adds a <see cref="FieldKind.Float32"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetFloat32(string fieldname, float value);

    /// <summary>
    /// Adds a <see cref="FieldKind.Float64"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetFloat64(string fieldname, double value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfBoolean"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfBoolean(string fieldname, bool[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfInt8"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfInt8(string fieldname, sbyte[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfInt16"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfInt16(string fieldname, short[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfInt32"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfInt32(string fieldname, int[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfInt64"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfInt64(string fieldname, long[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfFloat32"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfFloat32(string fieldname, float[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfFloat64"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfFloat64(string fieldname, double[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.NullableBoolean"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetNullableBoolean(string fieldname, bool? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.NullableInt8"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetNullableInt8(string fieldname, sbyte? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.NullableInt16"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetNullableInt16(string fieldname, short? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.NullableInt32"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetNullableInt32(string fieldname, int? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.NullableInt64"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetNullableInt64(string fieldname, long? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.NullableFloat32"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetNullableFloat32(string fieldname, float? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.NullableFloat64"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetNullableFloat64(string fieldname, double? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.Decimal"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetDecimal(string fieldname, HBigDecimal? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.String"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetString(string fieldname, string? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.Time"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetTime(string fieldname, HLocalTime? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.Date"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetDate(string fieldname, HLocalDate? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.TimeStamp"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetTimeStamp(string fieldname, HLocalDateTime? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.TimeStampWithTimeZone"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetTimeStampWithTimeZone(string fieldname, HOffsetDateTime? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfNullableBoolean"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfNullableBoolean(string fieldname, bool?[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfNullableInt8"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfNullableInt8(string fieldname, sbyte?[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfNullableInt16"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfNullableInt16(string fieldname, short?[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfNullableInt32"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfNullableInt32(string fieldname, int?[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfNullableInt64"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfNullableInt64(string fieldname, long?[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfNullableFloat32"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfNullableFloat32(string fieldname, float?[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfNullableFloat64"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfNullableFloat64(string fieldname, double?[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfDecimal"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfDecimal(string fieldname, HBigDecimal?[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfTime"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfTime(string fieldname, HLocalTime?[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfDate"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfDate(string fieldname, HLocalDate?[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfTimeStamp"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfTimeStamp(string fieldname, HLocalDateTime?[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfTimeStampWithTimeZone"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfTimeStampWithTimeZone(string fieldname, HOffsetDateTime?[]? value);

    /// <summary>
    /// Adds a <see cref="FieldKind.ArrayOfString"/> field to the record and sets its value.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <param name="value">The value of the field.</param>
    /// <returns>This <see cref="IGenericRecordBuilder"/>.</returns>
    IGenericRecordBuilder SetArrayOfString(string fieldname, string?[]? value);

    // </generated>
}
