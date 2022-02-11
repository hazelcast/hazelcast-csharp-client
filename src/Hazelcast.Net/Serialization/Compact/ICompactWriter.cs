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

        /// <summary>Writes a <see cref="FieldKind.Boolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteBoolean(string name, bool value);

        /// <summary>Writes a <see cref="FieldKind.BooleanRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteBooleanRef(string name, bool? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfBoolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfBoolean(string name, bool[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfBooleanRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfBooleanRef(string name, bool?[]? value);

        /// <summary>Writes a <see cref="FieldKind.Int8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInt8(string name, sbyte value);

        /// <summary>Writes a <see cref="FieldKind.Int8Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInt8Ref(string name, sbyte? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfInt8(string name, sbyte[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt8Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfInt8Ref(string name, sbyte?[]? value);

        /// <summary>Writes a <see cref="FieldKind.Int16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInt16(string name, short value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInt16Ref(string name, short? value);

        /// <summary>Writes a <see cref="FieldKind.Int16Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfInt16(string name, short[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt16Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfInt16Ref(string name, short?[]? value);

        /// <summary>Writes a <see cref="FieldKind.Int32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInt32(string name, int value);

        /// <summary>Writes a <see cref="FieldKind.Int32Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInt32Ref(string name, int? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfInt32(string name, int[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt32Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfInt32Ref(string name, int?[]? value);

        /// <summary>Writes a <see cref="FieldKind.Int64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInt64(string name, long value);

        /// <summary>Writes a <see cref="FieldKind.Int64Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInt64Ref(string name, long? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfInt64(string name, long[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfInt64Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfInt64Ref(string name, long?[]? value);

        /// <summary>Writes a <see cref="FieldKind.Float32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteFloat32(string name, float value);

        /// <summary>Writes a <see cref="FieldKind.Float32Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteFloat32Ref(string name, float? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfFloat32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfFloat32(string name, float[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfFloat32Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfFloat32Ref(string name, float?[]? value);

        /// <summary>Writes a <see cref="FieldKind.Float64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteFloat64(string name, double value);

        /// <summary>Writes a <see cref="FieldKind.Float64Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteFloat64Ref(string name, double? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfFloat64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfFloat64(string name, double[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfFloat64Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfFloat64Ref(string name, double?[]? value);

        /// <summary>Writes a <see cref="FieldKind.StringRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteStringRef(string name, string? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfStringRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfStringRef(string name, string?[]? value);

        /// <summary>Writes a <see cref="FieldKind.DecimalRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.DecimalRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="decimal"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        void WriteDecimalRef(string name, decimal? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfDecimalRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.ArrayOfDecimalRef"/> primitive type. The range
        /// of this primitive type is different from the range of <see cref="decimal"/>. Refer to the
        /// primitive type documentation for details.</para>
        /// </remarks>
        void WriteArrayOfDecimalRef(string name, decimal?[]? value);

        /// <summary>Writes a <see cref="FieldKind.DecimalRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteDecimalRef(string name, HBigDecimal? value); // FIXME and WTF are we supposed to do if we don't support overrides ?!

        /// <summary>Writes a <see cref="FieldKind.ArrayOfDecimalRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfDecimalRef(string name, HBigDecimal?[]? value);

        /// <summary>Writes a <see cref="FieldKind.TimeRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.TimeRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="TimeSpan"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">A specified value is outside the range of the
        /// <see cref="FieldKind.TimeRef"/> primitive type.</exception>
        void WriteTimeRef(string name, TimeSpan? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfTimeRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.TimeRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="TimeSpan"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">A specified value is outside the range of the
        /// <see cref="FieldKind.TimeRef"/> primitive type.</exception>
        void WriteArrayOfTimeRef(string name, TimeSpan?[]? value);

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
        void WriteTimeRef(string name, TimeOnly? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfTimeRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.TimeRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="TimeOnly"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        /// FIXME - document range of TimeOnly
        void WriteArrayOfTimeRef(string name, TimeOnly?[]? value);
#endif

        /// <summary>Writes a <see cref="FieldKind.DateRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.DateRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="DateTime"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        void WriteDateRef(string name, DateTime? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfDateRef"/> field</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.ArrayOfDateRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="DateTime"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        void WriteArrayOfDateRef(string name, DateTime?[]? value);

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
        void WriteDateRef(string name, DateOnly? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfDateRef"/> field</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.ArrayOfDateRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="DateOnly"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        /// FIXME - document range of DateOnly
        void WriteDateRefs(string name, DateOnly?[]? value);
#endif

        /// <summary>Writes a <see cref="FieldKind.TimeStampRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.TimeStampRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="DateTime"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        void WriteTimeStampRef(string name, DateTime? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfTimeStampRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.TimeStampRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="DateTime"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        void WriteArrayOfTimeStampRef(string name, DateTime?[]? value);

        /// <summary>Writes a <see cref="FieldKind.TimeStampWithTimeZoneRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.TimeStampWithTimeZoneRef"/> primitive type.
        /// The range of this primitive type is different from the range of <see cref="DateTimeOffset"/>.
        /// Refer to the primitive type documentation for details.</para>
        /// </remarks>
        void WriteTimeStampWithTimeZoneRef(string name, DateTimeOffset? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfTimeStampWithTimeZoneRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.ArrayOfTimeStampWithTimeZoneRef"/> primitive type.
        /// The range of this primitive type is different from the range of <see cref="DateTimeOffset"/>.
        /// Refer to the primitive type documentation for details.</para>
        /// </remarks>
        void WriteArrayOfTimeStampWithTimeZoneRef(string name, DateTimeOffset?[]? value);

        /// <summary>Writes a <see cref="FieldKind.CompactRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteCompactRef(string name, object? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfCompactRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteArrayOfCompactRef(string name, object?[]? value);
    }
}
