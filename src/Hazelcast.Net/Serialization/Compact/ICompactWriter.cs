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

using System;

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
        void WriteBooleans(string name, bool[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfBooleanRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteBooleanRefs(string name, bool?[]? value);

        /// <summary>Writes a <see cref="FieldKind.SignedInteger8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteSignedByte(string name, sbyte value);

        /// <summary>Writes a <see cref="FieldKind.SignedInteger8Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteSignedByteRef(string name, sbyte? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfSignedInteger8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteSignedBytes(string name, sbyte[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfSignedInteger8Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteSignedByteRefs(string name, sbyte?[]? value);

        /// <summary>Writes a <see cref="FieldKind.SignedInteger16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteShort(string name, short value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfSignedInteger16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteShortRef(string name, short? value);

        /// <summary>Writes a <see cref="FieldKind.SignedInteger16Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteShorts(string name, short[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfSignedInteger16Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteShortRefs(string name, short?[]? value);

        /// <summary>Writes a <see cref="FieldKind.SignedInteger32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInt(string name, int value);

        /// <summary>Writes a <see cref="FieldKind.SignedInteger32Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteIntRef(string name, int? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfSignedInteger32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteInts(string name, int[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfSignedInteger32Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteIntRefs(string name, int?[]? value);

        /// <summary>Writes a <see cref="FieldKind.SignedInteger64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteLong(string name, long value);

        /// <summary>Writes a <see cref="FieldKind.SignedInteger64Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteLongRef(string name, long? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfSignedInteger64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteLongs(string name, long[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfSignedInteger64Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteLongRefs(string name, long?[]? value);

        /// <summary>Writes a <see cref="FieldKind.Float"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteFloat(string name, float value);

        /// <summary>Writes a <see cref="FieldKind.FloatRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteFloatRef(string name, float? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfFloat"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteFloats(string name, float[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfFloatRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteFloatRefs(string name, float?[]? value);

        /// <summary>Writes a <see cref="FieldKind.Double"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteDouble(string name, double value);

        /// <summary>Writes a <see cref="FieldKind.DoubleRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteDoubleRef(string name, double? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfDouble"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteDoubles(string name, double[]? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfDoubleRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteDoubleRefs(string name, double?[]? value);

        /// <summary>Writes a <see cref="FieldKind.String"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteString(string name, string? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfString"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteStrings(string name, string?[]? value);

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
        void WriteDecimalRefs(string name, decimal?[]? value);

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
        void WriteTime(string name, TimeSpan? value);

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
        void WriteTimes(string name, TimeSpan?[]? value);

        /// <summary>Writes a <see cref="FieldKind.DateRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.DateRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="DateTime"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        void WriteDate(string name, DateTime? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfDateRef"/> field</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.ArrayOfDateRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="DateTime"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        void WriteDates(string name, DateTime?[]? value);

        /// <summary>Writes a <see cref="FieldKind.TimeStampRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.TimeStampRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="DateTime"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        void WriteDateTime(string name, DateTime? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfTimeStampRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.TimeStampRef"/> primitive type. The range of this
        /// primitive type is different from the range of <see cref="DateTime"/>. Refer to the primitive
        /// type documentation for details.</para>
        /// </remarks>
        void WriteDateTimes(string name, DateTime?[]? value);

        /// <summary>Writes a <see cref="FieldKind.TimeStampWithTimeZoneRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.TimeStampWithTimeZoneRef"/> primitive type.
        /// The range of this primitive type is different from the range of <see cref="DateTimeOffset"/>.
        /// Refer to the primitive type documentation for details.</para>
        /// </remarks>
        void WriteDateTimeOffset(string name, DateTimeOffset? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfTimeStampWithTimeZoneRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <remarks>
        /// <para>This method writes a <see cref="FieldKind.ArrayOfTimeStampWithTimeZoneRef"/> primitive type.
        /// The range of this primitive type is different from the range of <see cref="DateTimeOffset"/>.
        /// Refer to the primitive type documentation for details.</para>
        /// </remarks>
        void WriteDateTimeOffsets(string name, DateTimeOffset?[]? value);

        /// <summary>Writes a <see cref="FieldKind.Object"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteObject(string name, object? value);

        /// <summary>Writes a <see cref="FieldKind.ArrayOfObject"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="value">The value of the field.</param>
        void WriteObjects(string name, object?[]? value);
    }
}
