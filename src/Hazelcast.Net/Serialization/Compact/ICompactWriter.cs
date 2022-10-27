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

#nullable enable

using Hazelcast.Models;

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Defines methods for writing fields to a compact-serialized blob.
    /// </summary>
    public interface ICompactWriter
    {
        // for types that support both a nullable and a non-nullable version, we define
        // the two methods, thus avoiding allocating an extra nullable struct and/or
        // boxing when it is not necessary.

        // do NOT remove nor alter the <generated></generated> lines!
        // <generated>

        /// <summary>Writes a <see cref="FieldKind.Boolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteBoolean(string name, bool value);

        /// <summary>Writes a <see cref="FieldKind.Int8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInt8(string name, sbyte value);

        /// <summary>Writes a <see cref="FieldKind.Int16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInt16(string name, short value);

        /// <summary>Writes a <see cref="FieldKind.Int32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInt32(string name, int value);

        /// <summary>Writes a <see cref="FieldKind.Int64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInt64(string name, long value);

        /// <summary>Writes a <see cref="FieldKind.Float32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteFloat32(string name, float value);

        /// <summary>Writes a <see cref="FieldKind.Float64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteFloat64(string name, double value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfBoolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfBoolean(string name, bool[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfInt8(string name, sbyte[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfInt16(string name, short[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfInt32(string name, int[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfInt64(string name, long[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfFloat32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfFloat32(string name, float[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfFloat64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfFloat64(string name, double[]? value);

        /// <summary>Writes a <see cref="FieldKind.NullableBoolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteNullableBoolean(string name, bool? value);

        /// <summary>Writes a <see cref="FieldKind.NullableInt8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteNullableInt8(string name, sbyte? value);

        /// <summary>Writes a <see cref="FieldKind.NullableInt16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteNullableInt16(string name, short? value);

        /// <summary>Writes a <see cref="FieldKind.NullableInt32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteNullableInt32(string name, int? value);

        /// <summary>Writes a <see cref="FieldKind.NullableInt64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteNullableInt64(string name, long? value);

        /// <summary>Writes a <see cref="FieldKind.NullableFloat32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteNullableFloat32(string name, float? value);

        /// <summary>Writes a <see cref="FieldKind.NullableFloat64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteNullableFloat64(string name, double? value);

        /// <summary>Writes a <see cref="FieldKind.Decimal"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteDecimal(string name, HBigDecimal? value);

        /// <summary>Writes a <see cref="FieldKind.String"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteString(string name, string? value);

        /// <summary>Writes a <see cref="FieldKind.Time"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteTime(string name, HLocalTime? value);

        /// <summary>Writes a <see cref="FieldKind.Date"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteDate(string name, HLocalDate? value);

        /// <summary>Writes a <see cref="FieldKind.TimeStamp"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteTimeStamp(string name, HLocalDateTime? value);

        /// <summary>Writes a <see cref="FieldKind.TimeStampWithTimeZone"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteTimeStampWithTimeZone(string name, HOffsetDateTime? value);

        /// <summary>Writes a <see cref="FieldKind.Compact"/> field.</summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteCompact<T>(string name, T? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableBoolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfNullableBoolean(string name, bool?[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableInt8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfNullableInt8(string name, sbyte?[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableInt16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfNullableInt16(string name, short?[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableInt32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfNullableInt32(string name, int?[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableInt64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfNullableInt64(string name, long?[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableFloat32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfNullableFloat32(string name, float?[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableFloat64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfNullableFloat64(string name, double?[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfDecimal"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfDecimal(string name, HBigDecimal?[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfTime"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfTime(string name, HLocalTime?[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfDate"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfDate(string name, HLocalDate?[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfTimeStamp"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfTimeStamp(string name, HLocalDateTime?[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfTimeStampWithTimeZone"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfTimeStampWithTimeZone(string name, HOffsetDateTime?[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfString"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfString(string name, string?[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfCompact"/> field.</summary>
        /// <typeparam name="T">The type of the objects.</typeparam>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfCompact<T>(string name, T?[]? value);

        // </generated>
    }
}
