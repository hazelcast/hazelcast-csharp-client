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

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Defines methods for reading fields from a compact-serialized blob.
    /// </summary>
    public interface ICompactReader
    {
        // for types that support both a nullable and a non-nullable version, we define
        // the two methods, thus avoiding allocating an extra nullable struct and/or
        // boxing when it is not necessary.

        /// <summary>
        /// Gets the <see cref="FieldKind"/> of a field.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The <see cref="FieldKind"/> of the field, which can be <see cref="FieldKind.NotAvailable"/> if the field does not exist.</returns>
        FieldKind GetFieldKind(string name);

        // do NOT remove nor alter the <generated></generated> lines!
        // <generated>

        /// <summary>Reads a <see cref="FieldKind.Boolean"/> or <see cref="FieldKind.NullableBoolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <remarks>
        /// <para>Throws a <see cref="SerializationException" /> if the field is nullable and the value is <c>null</c>.</para>
        /// </remarks>
        bool ReadBoolean(string name);

        /// <summary>Reads a <see cref="FieldKind.Int8"/> or <see cref="FieldKind.NullableInt8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <remarks>
        /// <para>Throws a <see cref="SerializationException" /> if the field is nullable and the value is <c>null</c>.</para>
        /// </remarks>
        sbyte ReadInt8(string name);

        /// <summary>Reads a <see cref="FieldKind.Int16"/> or <see cref="FieldKind.NullableInt16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <remarks>
        /// <para>Throws a <see cref="SerializationException" /> if the field is nullable and the value is <c>null</c>.</para>
        /// </remarks>
        short ReadInt16(string name);

        /// <summary>Reads a <see cref="FieldKind.Int32"/> or <see cref="FieldKind.NullableInt32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <remarks>
        /// <para>Throws a <see cref="SerializationException" /> if the field is nullable and the value is <c>null</c>.</para>
        /// </remarks>
        int ReadInt32(string name);

        /// <summary>Reads a <see cref="FieldKind.Int64"/> or <see cref="FieldKind.NullableInt64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <remarks>
        /// <para>Throws a <see cref="SerializationException" /> if the field is nullable and the value is <c>null</c>.</para>
        /// </remarks>
        long ReadInt64(string name);

        /// <summary>Reads a <see cref="FieldKind.Float32"/> or <see cref="FieldKind.NullableFloat32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <remarks>
        /// <para>Throws a <see cref="SerializationException" /> if the field is nullable and the value is <c>null</c>.</para>
        /// </remarks>
        float ReadFloat32(string name);

        /// <summary>Reads a <see cref="FieldKind.Float64"/> or <see cref="FieldKind.NullableFloat64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <remarks>
        /// <para>Throws a <see cref="SerializationException" /> if the field is nullable and the value is <c>null</c>.</para>
        /// </remarks>
        double ReadFloat64(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfBoolean"/> or <see cref="FieldKind.ArrayOfNullableBoolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <remarks>
        /// <para>Throws a <see cref="SerializationException" /> if the field is nullable and a value is <c>null</c>.</para>
        /// </remarks>
        bool[]? ReadArrayOfBoolean(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfInt8"/> or <see cref="FieldKind.ArrayOfNullableInt8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <remarks>
        /// <para>Throws a <see cref="SerializationException" /> if the field is nullable and a value is <c>null</c>.</para>
        /// </remarks>
        sbyte[]? ReadArrayOfInt8(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfInt16"/> or <see cref="FieldKind.ArrayOfNullableInt16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <remarks>
        /// <para>Throws a <see cref="SerializationException" /> if the field is nullable and a value is <c>null</c>.</para>
        /// </remarks>
        short[]? ReadArrayOfInt16(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfInt32"/> or <see cref="FieldKind.ArrayOfNullableInt32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <remarks>
        /// <para>Throws a <see cref="SerializationException" /> if the field is nullable and a value is <c>null</c>.</para>
        /// </remarks>
        int[]? ReadArrayOfInt32(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfInt64"/> or <see cref="FieldKind.ArrayOfNullableInt64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <remarks>
        /// <para>Throws a <see cref="SerializationException" /> if the field is nullable and a value is <c>null</c>.</para>
        /// </remarks>
        long[]? ReadArrayOfInt64(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfFloat32"/> or <see cref="FieldKind.ArrayOfNullableFloat32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <remarks>
        /// <para>Throws a <see cref="SerializationException" /> if the field is nullable and a value is <c>null</c>.</para>
        /// </remarks>
        float[]? ReadArrayOfFloat32(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfFloat64"/> or <see cref="FieldKind.ArrayOfNullableFloat64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <remarks>
        /// <para>Throws a <see cref="SerializationException" /> if the field is nullable and a value is <c>null</c>.</para>
        /// </remarks>
        double[]? ReadArrayOfFloat64(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableBoolean"/> or <see cref="FieldKind.Boolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        bool? ReadNullableBoolean(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableInt8"/> or <see cref="FieldKind.Int8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        sbyte? ReadNullableInt8(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableInt16"/> or <see cref="FieldKind.Int16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        short? ReadNullableInt16(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableInt32"/> or <see cref="FieldKind.Int32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        int? ReadNullableInt32(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableInt64"/> or <see cref="FieldKind.Int64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        long? ReadNullableInt64(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableFloat32"/> or <see cref="FieldKind.Float32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        float? ReadNullableFloat32(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableFloat64"/> or <see cref="FieldKind.Float64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        double? ReadNullableFloat64(string name);

        /// <summary>Reads a <see cref="FieldKind.Decimal"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        HBigDecimal? ReadDecimal(string name);

        /// <summary>Reads a <see cref="FieldKind.String"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        string? ReadString(string name);

        /// <summary>Reads a <see cref="FieldKind.Time"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        HLocalTime? ReadTime(string name);

        /// <summary>Reads a <see cref="FieldKind.Date"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        HLocalDate? ReadDate(string name);

        /// <summary>Reads a <see cref="FieldKind.TimeStamp"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        HLocalDateTime? ReadTimeStamp(string name);

        /// <summary>Reads a <see cref="FieldKind.TimeStampWithTimeZone"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        HOffsetDateTime? ReadTimeStampWithTimeZone(string name);

        /// <summary>Reads a <see cref="FieldKind.Compact"/> field.</summary>
        /// <typeparam name="T">The expected type of the object.</typeparam>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        T? ReadCompact<T>(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableBoolean"/> or <see cref="FieldKind.ArrayOfBoolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        bool?[]? ReadArrayOfNullableBoolean(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableInt8"/> or <see cref="FieldKind.ArrayOfInt8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        sbyte?[]? ReadArrayOfNullableInt8(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableInt16"/> or <see cref="FieldKind.ArrayOfInt16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        short?[]? ReadArrayOfNullableInt16(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableInt32"/> or <see cref="FieldKind.ArrayOfInt32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        int?[]? ReadArrayOfNullableInt32(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableInt64"/> or <see cref="FieldKind.ArrayOfInt64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        long?[]? ReadArrayOfNullableInt64(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableFloat32"/> or <see cref="FieldKind.ArrayOfFloat32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        float?[]? ReadArrayOfNullableFloat32(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableFloat64"/> or <see cref="FieldKind.ArrayOfFloat64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        double?[]? ReadArrayOfNullableFloat64(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfDecimal"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        HBigDecimal?[]? ReadArrayOfDecimal(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfTime"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        HLocalTime?[]? ReadArrayOfTime(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfDate"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        HLocalDate?[]? ReadArrayOfDate(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfTimeStamp"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        HLocalDateTime?[]? ReadArrayOfTimeStamp(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfTimeStampWithTimeZone"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        HOffsetDateTime?[]? ReadArrayOfTimeStampWithTimeZone(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfString"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        string?[]? ReadArrayOfString(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfCompact"/> field.</summary>
        /// <typeparam name="T">The expected type of the objects.</typeparam>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        T?[]? ReadArrayOfCompact<T>(string name);

        // </generated>
    }
}
