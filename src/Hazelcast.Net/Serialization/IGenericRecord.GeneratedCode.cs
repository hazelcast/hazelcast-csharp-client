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

using Hazelcast.Models;

namespace Hazelcast.Serialization;

public partial interface IGenericRecord
{
    // <generated>

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.Boolean"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public bool GetBoolean(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.Int8"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public sbyte GetInt8(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.Int16"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public short GetInt16(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.Int32"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public int GetInt32(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.Int64"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public long GetInt64(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.Float32"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public float GetFloat32(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.Float64"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public double GetFloat64(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfBoolean"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public bool[]? GetArrayOfBoolean(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfInt8"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public sbyte[]? GetArrayOfInt8(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfInt16"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public short[]? GetArrayOfInt16(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfInt32"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public int[]? GetArrayOfInt32(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfInt64"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public long[]? GetArrayOfInt64(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfFloat32"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public float[]? GetArrayOfFloat32(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfFloat64"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public double[]? GetArrayOfFloat64(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.NullableBoolean"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public bool? GetNullableBoolean(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.NullableInt8"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public sbyte? GetNullableInt8(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.NullableInt16"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public short? GetNullableInt16(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.NullableInt32"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public int? GetNullableInt32(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.NullableInt64"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public long? GetNullableInt64(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.NullableFloat32"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public float? GetNullableFloat32(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.NullableFloat64"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public double? GetNullableFloat64(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.Decimal"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public HBigDecimal? GetDecimal(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.String"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public string? GetString(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.Time"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public HLocalTime? GetTime(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.Date"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public HLocalDate? GetDate(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.TimeStamp"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public HLocalDateTime? GetTimeStamp(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.TimeStampWithTimeZone"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public HOffsetDateTime? GetTimeStampWithTimeZone(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfNullableBoolean"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public bool?[]? GetArrayOfNullableBoolean(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfNullableInt8"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public sbyte?[]? GetArrayOfNullableInt8(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfNullableInt16"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public short?[]? GetArrayOfNullableInt16(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfNullableInt32"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public int?[]? GetArrayOfNullableInt32(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfNullableInt64"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public long?[]? GetArrayOfNullableInt64(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfNullableFloat32"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public float?[]? GetArrayOfNullableFloat32(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfNullableFloat64"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public double?[]? GetArrayOfNullableFloat64(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfDecimal"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public HBigDecimal?[]? GetArrayOfDecimal(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfTime"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public HLocalTime?[]? GetArrayOfTime(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfDate"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public HLocalDate?[]? GetArrayOfDate(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfTimeStamp"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public HLocalDateTime?[]? GetArrayOfTimeStamp(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfTimeStampWithTimeZone"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public HOffsetDateTime?[]? GetArrayOfTimeStampWithTimeZone(string fieldname);

    /// <summary>
    /// Gets the value of a <see cref="FieldKind.ArrayOfString"/> field.
    /// </summary>
    /// <param name="fieldname">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    /// <exception cref="SerializationException">No field exists with the specified
    /// name in the record, or the type of the field does not match.</exception>
    public string?[]? GetArrayOfString(string fieldname);

    // </generated>
}
