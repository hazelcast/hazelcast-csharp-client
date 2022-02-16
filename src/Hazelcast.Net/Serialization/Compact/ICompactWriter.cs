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

// FIXME - eventually remove ReSharper disable when all methods are tested
// ReSharper disable UnusedMember.Global

using System;
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

        /// <summary>
        /// Determines whether the schema has a specified field.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="kind">The <see cref="FieldKind"/> of the field.</param>
        /// <returns><c>true</c> if the schema has a field with the specified <paramref name="name"/> and
        /// of the specified <paramref name="kind"/>; otherwise <c>false</c>.</returns>
        bool HasField(string name, FieldKind kind);

        /// <summary>Writes a <see cref="FieldKind.Boolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteBoolean(string name, bool value);

        /// <summary>Writes a <see cref="FieldKind.NullableBoolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteNullableBoolean(string name, bool? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfBoolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfBoolean(string name, bool[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableBoolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfNullableBoolean(string name, bool?[]? value);

        /// <summary>Writes a <see cref="FieldKind.Int8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInt8(string name, sbyte value);

        /// <summary>Writes a <see cref="FieldKind.NullableInt8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteNullableInt8(string name, sbyte? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfInt8(string name, sbyte[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableInt8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfNullableInt8(string name, sbyte?[]? value);

        /// <summary>Writes a <see cref="FieldKind.Int16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInt16(string name, short value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteNullableInt16(string name, short? value);

        /// <summary>Writes a <see cref="FieldKind.NullableInt16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfInt16(string name, short[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableInt16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfNullableInt16(string name, short?[]? value);

        /// <summary>Writes a <see cref="FieldKind.Int32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInt32(string name, int value);

        /// <summary>Writes a <see cref="FieldKind.NullableInt32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteNullableInt32(string name, int? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfInt32(string name, int[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableInt32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfNullableInt32(string name, int?[]? value);

        /// <summary>Writes a <see cref="FieldKind.Int64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInt64(string name, long value);

        /// <summary>Writes a <see cref="FieldKind.NullableInt64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteNullableInt64(string name, long? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfInt64(string name, long[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableInt64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfNullableInt64(string name, long?[]? value);

        /// <summary>Writes a <see cref="FieldKind.Float32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteFloat32(string name, float value);

        /// <summary>Writes a <see cref="FieldKind.NullableFloat32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteNullableFloat32(string name, float? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfFloat32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfFloat32(string name, float[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableFloat32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfNullableFloat32(string name, float?[]? value);

        /// <summary>Writes a <see cref="FieldKind.Float64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteFloat64(string name, double value);

        /// <summary>Writes a <see cref="FieldKind.NullableFloat64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteNullableFloat64(string name, double? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfFloat64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfFloat64(string name, double[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableFloat64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfNullableFloat64(string name, double?[]? value);

        /// <summary>Writes a <see cref="FieldKind.NullableString"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteNullableString(string name, string? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableString"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfNullableString(string name, string?[]? value);

        /// <summary>Writes a <see cref="FieldKind.NullableDecimal"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.NullableDecimal"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="decimal"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        void WriteNullableDecimal(string name, decimal? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableDecimal"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.ArrayOfNullableDecimal"/> primitive type. The range
        /// of this primitive type is different from the range of <see cref="decimal"/>. Refer to the
        /// primitive type documentation for details.</para>
        /// </remarks>
        void WriteArrayOfNullableDecimal(string name, decimal?[]? value);

        /// <summary>Writes a <see cref="FieldKind.NullableDecimal"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteNullableDecimal(string name, HBigDecimal? value); // FIXME and WTF are we supposed to do if we don't support overrides ?!

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableDecimal"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfNullableDecimal(string name, HBigDecimal?[]? value);

        /// <summary>Writes a <see cref="FieldKind.NullableTime"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.NullableTime"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="TimeSpan"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">A specified value is outside the range of the
        /// <see cref="FieldKind.NullableTime"/> primitive type.</exception>
        void WriteNullableTime(string name, TimeSpan? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableTime"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.NullableTime"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="TimeSpan"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">A specified value is outside the range of the
        /// <see cref="FieldKind.NullableTime"/> primitive type.</exception>
        void WriteArrayOfNullableTime(string name, TimeSpan?[]? value);

        // FIXME - discuss, we may not want it, rename it, whatever, I don't know anymore
#if NET6_0_OR_GREATER
        /// <summary>Writes a <see cref="FieldKind.TimeRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.TimeRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="TimeOnly"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        /// FIXME - document range of TimeOnly
        void WriteNullableTime(string name, TimeOnly? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfTimeRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.TimeRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="TimeOnly"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        /// FIXME - document range of TimeOnly
        void WriteArrayOfNullableTime(string name, TimeOnly?[]? value);
#endif

        /// <summary>Writes a <see cref="FieldKind.NullableDate"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.NullableDate"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="DateTime"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        void WriteNullableDate(string name, DateTime? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableDate"/> field</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.ArrayOfNullableDate"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="DateTime"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        void WriteArrayOfNullableDate(string name, DateTime?[]? value);

        // FIXME - see above
#if NET6_0_OR_GREATER
        /// <summary>Writes a <see cref="FieldKind.DateRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.DateRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="DateOnly"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        /// FIXME - document range of DateOnly
        void WriteNullableDate(string name, DateOnly? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfDateRef"/> field</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.ArrayOfDateRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="DateOnly"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        /// FIXME - document range of DateOnly
        void WriteArrayOfNullableDate(string name, DateOnly?[]? value);
#endif

        /// <summary>Writes a <see cref="FieldKind.NullableTimeStamp"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.NullableTimeStamp"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="DateTime"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        void WriteNullableTimeStamp(string name, DateTime? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableTimeStamp"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.NullableTimeStamp"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="DateTime"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        void WriteArrayOfNullableTimeStamp(string name, DateTime?[]? value);

        /// <summary>Writes a <see cref="FieldKind.NullableTimeStampWithTimeZone"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.NullableTimeStampWithTimeZone"/> primitive type.
        /// The range of this primitive type is different from the range of <see cref="DateTimeOffset"/>.
        /// Refer to the primitive type documentation for details.</para>
        /// </remarks>
        void WriteNullableTimeStampWithTimeZone(string name, DateTimeOffset? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableTimeStampWithTimeZone"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.ArrayOfNullableTimeStampWithTimeZone"/> primitive type.
        /// The range of this primitive type is different from the range of <see cref="DateTimeOffset"/>.
        /// Refer to the primitive type documentation for details.</para>
        /// </remarks>
        void WriteArrayOfNullableTimeStampWithTimeZone(string name, DateTimeOffset?[]? value);

        /// <summary>Writes a <see cref="FieldKind.NullableCompact"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteNullableCompact(string name, object? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfNullableCompact"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfNullableCompact(string name, object?[]? value);
    }
}
