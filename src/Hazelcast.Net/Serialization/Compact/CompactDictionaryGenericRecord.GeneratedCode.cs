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

internal partial class CompactDictionaryGenericRecord
{
    // <generated>

    /// <inheritdoc />
    public bool GetBoolean(string fieldname)
    {
        ValidateField(fieldname, FieldKind.Boolean, FieldKind.NullableBoolean);
        return GetValueOf<bool>(_fieldValues[fieldname], fieldname, FieldKind.Boolean, FieldKind.NullableBoolean);
    }

    /// <inheritdoc />
    public sbyte GetInt8(string fieldname)
    {
        ValidateField(fieldname, FieldKind.Int8, FieldKind.NullableInt8);
        return GetValueOf<sbyte>(_fieldValues[fieldname], fieldname, FieldKind.Int8, FieldKind.NullableInt8);
    }

    /// <inheritdoc />
    public short GetInt16(string fieldname)
    {
        ValidateField(fieldname, FieldKind.Int16, FieldKind.NullableInt16);
        return GetValueOf<short>(_fieldValues[fieldname], fieldname, FieldKind.Int16, FieldKind.NullableInt16);
    }

    /// <inheritdoc />
    public int GetInt32(string fieldname)
    {
        ValidateField(fieldname, FieldKind.Int32, FieldKind.NullableInt32);
        return GetValueOf<int>(_fieldValues[fieldname], fieldname, FieldKind.Int32, FieldKind.NullableInt32);
    }

    /// <inheritdoc />
    public long GetInt64(string fieldname)
    {
        ValidateField(fieldname, FieldKind.Int64, FieldKind.NullableInt64);
        return GetValueOf<long>(_fieldValues[fieldname], fieldname, FieldKind.Int64, FieldKind.NullableInt64);
    }

    /// <inheritdoc />
    public float GetFloat32(string fieldname)
    {
        ValidateField(fieldname, FieldKind.Float32, FieldKind.NullableFloat32);
        return GetValueOf<float>(_fieldValues[fieldname], fieldname, FieldKind.Float32, FieldKind.NullableFloat32);
    }

    /// <inheritdoc />
    public double GetFloat64(string fieldname)
    {
        ValidateField(fieldname, FieldKind.Float64, FieldKind.NullableFloat64);
        return GetValueOf<double>(_fieldValues[fieldname], fieldname, FieldKind.Float64, FieldKind.NullableFloat64);
    }

    /// <inheritdoc />
    public bool[]? GetArrayOfBoolean(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfBoolean, FieldKind.ArrayOfNullableBoolean);
        return GetArrayOf<bool>(_fieldValues[fieldname], fieldname, FieldKind.ArrayOfBoolean, FieldKind.ArrayOfNullableBoolean);
    }

    /// <inheritdoc />
    public sbyte[]? GetArrayOfInt8(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfInt8, FieldKind.ArrayOfNullableInt8);
        return GetArrayOf<sbyte>(_fieldValues[fieldname], fieldname, FieldKind.ArrayOfInt8, FieldKind.ArrayOfNullableInt8);
    }

    /// <inheritdoc />
    public short[]? GetArrayOfInt16(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfInt16, FieldKind.ArrayOfNullableInt16);
        return GetArrayOf<short>(_fieldValues[fieldname], fieldname, FieldKind.ArrayOfInt16, FieldKind.ArrayOfNullableInt16);
    }

    /// <inheritdoc />
    public int[]? GetArrayOfInt32(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfInt32, FieldKind.ArrayOfNullableInt32);
        return GetArrayOf<int>(_fieldValues[fieldname], fieldname, FieldKind.ArrayOfInt32, FieldKind.ArrayOfNullableInt32);
    }

    /// <inheritdoc />
    public long[]? GetArrayOfInt64(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfInt64, FieldKind.ArrayOfNullableInt64);
        return GetArrayOf<long>(_fieldValues[fieldname], fieldname, FieldKind.ArrayOfInt64, FieldKind.ArrayOfNullableInt64);
    }

    /// <inheritdoc />
    public float[]? GetArrayOfFloat32(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfFloat32, FieldKind.ArrayOfNullableFloat32);
        return GetArrayOf<float>(_fieldValues[fieldname], fieldname, FieldKind.ArrayOfFloat32, FieldKind.ArrayOfNullableFloat32);
    }

    /// <inheritdoc />
    public double[]? GetArrayOfFloat64(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfFloat64, FieldKind.ArrayOfNullableFloat64);
        return GetArrayOf<double>(_fieldValues[fieldname], fieldname, FieldKind.ArrayOfFloat64, FieldKind.ArrayOfNullableFloat64);
    }

    /// <inheritdoc />
    public bool? GetNullableBoolean(string fieldname)
    {
        ValidateField(fieldname, FieldKind.NullableBoolean, FieldKind.Boolean);
        return (bool?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public sbyte? GetNullableInt8(string fieldname)
    {
        ValidateField(fieldname, FieldKind.NullableInt8, FieldKind.Int8);
        return (sbyte?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public short? GetNullableInt16(string fieldname)
    {
        ValidateField(fieldname, FieldKind.NullableInt16, FieldKind.Int16);
        return (short?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public int? GetNullableInt32(string fieldname)
    {
        ValidateField(fieldname, FieldKind.NullableInt32, FieldKind.Int32);
        return (int?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public long? GetNullableInt64(string fieldname)
    {
        ValidateField(fieldname, FieldKind.NullableInt64, FieldKind.Int64);
        return (long?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public float? GetNullableFloat32(string fieldname)
    {
        ValidateField(fieldname, FieldKind.NullableFloat32, FieldKind.Float32);
        return (float?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public double? GetNullableFloat64(string fieldname)
    {
        ValidateField(fieldname, FieldKind.NullableFloat64, FieldKind.Float64);
        return (double?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public HBigDecimal? GetDecimal(string fieldname)
    {
        ValidateField(fieldname, FieldKind.Decimal);
        return (HBigDecimal?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public string? GetString(string fieldname)
    {
        ValidateField(fieldname, FieldKind.String);
        return (string?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public HLocalTime? GetTime(string fieldname)
    {
        ValidateField(fieldname, FieldKind.Time);
        return (HLocalTime?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public HLocalDate? GetDate(string fieldname)
    {
        ValidateField(fieldname, FieldKind.Date);
        return (HLocalDate?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public HLocalDateTime? GetTimeStamp(string fieldname)
    {
        ValidateField(fieldname, FieldKind.TimeStamp);
        return (HLocalDateTime?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public HOffsetDateTime? GetTimeStampWithTimeZone(string fieldname)
    {
        ValidateField(fieldname, FieldKind.TimeStampWithTimeZone);
        return (HOffsetDateTime?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public bool?[]? GetArrayOfNullableBoolean(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfNullableBoolean, FieldKind.ArrayOfBoolean);
        return GetArrayOfNullableOf<bool>(_fieldValues[fieldname]);
    }

    /// <inheritdoc />
    public sbyte?[]? GetArrayOfNullableInt8(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfNullableInt8, FieldKind.ArrayOfInt8);
        return GetArrayOfNullableOf<sbyte>(_fieldValues[fieldname]);
    }

    /// <inheritdoc />
    public short?[]? GetArrayOfNullableInt16(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfNullableInt16, FieldKind.ArrayOfInt16);
        return GetArrayOfNullableOf<short>(_fieldValues[fieldname]);
    }

    /// <inheritdoc />
    public int?[]? GetArrayOfNullableInt32(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfNullableInt32, FieldKind.ArrayOfInt32);
        return GetArrayOfNullableOf<int>(_fieldValues[fieldname]);
    }

    /// <inheritdoc />
    public long?[]? GetArrayOfNullableInt64(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfNullableInt64, FieldKind.ArrayOfInt64);
        return GetArrayOfNullableOf<long>(_fieldValues[fieldname]);
    }

    /// <inheritdoc />
    public float?[]? GetArrayOfNullableFloat32(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfNullableFloat32, FieldKind.ArrayOfFloat32);
        return GetArrayOfNullableOf<float>(_fieldValues[fieldname]);
    }

    /// <inheritdoc />
    public double?[]? GetArrayOfNullableFloat64(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfNullableFloat64, FieldKind.ArrayOfFloat64);
        return GetArrayOfNullableOf<double>(_fieldValues[fieldname]);
    }

    /// <inheritdoc />
    public HBigDecimal?[]? GetArrayOfDecimal(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfDecimal);
        return (HBigDecimal?[]?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public HLocalTime?[]? GetArrayOfTime(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfTime);
        return (HLocalTime?[]?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public HLocalDate?[]? GetArrayOfDate(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfDate);
        return (HLocalDate?[]?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public HLocalDateTime?[]? GetArrayOfTimeStamp(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfTimeStamp);
        return (HLocalDateTime?[]?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public HOffsetDateTime?[]? GetArrayOfTimeStampWithTimeZone(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfTimeStampWithTimeZone);
        return (HOffsetDateTime?[]?) _fieldValues[fieldname];
    }

    /// <inheritdoc />
    public string?[]? GetArrayOfString(string fieldname)
    {
        ValidateField(fieldname, FieldKind.ArrayOfString);
        return (string?[]?) _fieldValues[fieldname];
    }

    // </generated>
}
