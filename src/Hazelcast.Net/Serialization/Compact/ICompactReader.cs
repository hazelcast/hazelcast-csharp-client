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

using System;
using Hazelcast.Models;

#nullable enable

// FIXME - eventually remove ReSharper disable when all methods are tested
// ReSharper disable UnusedMember.Global

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

        // FIXME - implement support for default value?
        // see Java DefaultCompactReader - default value for ?!

        /// <summary>Reads a <see cref="FieldKind.Boolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        bool ReadBoolean(string name);

        /// <summary>Reads a <see cref="FieldKind.BooleanRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// /// <returns>The value of the field.</returns>
        bool? ReadBooleanRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfBoolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        bool[]? ReadBooleans(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfBooleanRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        bool?[]? ReadBooleanRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.SignedInteger8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        sbyte ReadSByte(string name);

        /// <summary>Reads a <see cref="FieldKind.SignedInteger8Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        sbyte? ReadSByteRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfSignedInteger8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        sbyte[]? ReadSBytes(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfSignedInteger8Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        sbyte?[]? ReadSByteRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.SignedInteger16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        short ReadShort(string name);

        /// <summary>Reads a <see cref="FieldKind.SignedInteger16Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        short? ReadShortRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfSignedInteger16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        short[]? ReadShorts(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfSignedInteger16Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        short?[]? ReadShortRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.SignedInteger32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        int ReadInt(string name);

        /// <summary>Reads a <see cref="FieldKind.SignedInteger32Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        int? ReadIntRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfSignedInteger32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        int[]? ReadInts(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfSignedInteger32Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        int?[]? ReadIntRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.SignedInteger64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        long ReadLong(string name);

        /// <summary>Reads a <see cref="FieldKind.SignedInteger64Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        long? ReadLongRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfSignedInteger64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        long[]? ReadLongs(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfSignedInteger64Ref"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        long?[]? ReadLongRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.Float"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        float ReadFloat(string name);

        /// <summary>Reads a <see cref="FieldKind.FloatRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        float? ReadFloatRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfFloat"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        float[]? ReadFloats(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfFloatRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        float?[]? ReadFloatRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.Double"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        double ReadDouble(string name);

        /// <summary>Reads a <see cref="FieldKind.DoubleRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        double? ReadDoubleRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfDouble"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        double[]? ReadDoubles(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfDoubleRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        double?[]? ReadDoubleRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.StringRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        string? ReadStringRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfStringRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        string?[]? ReadStringRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.DecimalRef"/> field as a <see cref="decimal"/>.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <remarks>
        /// <para>This methods reads a <see cref="FieldKind.DecimalRef"/> primitive type. The range
        /// of this primitive type is different from the range of <see cref="decimal"/>. Refer to the
        /// primitive type documentation for details.</para>
        /// </remarks>
        /// <exception cref="SerializationException">A specified value is outside the range of the
        /// <see cref="decimal"/> type.</exception>
        decimal? ReadDecimalRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfDecimalRef"/> field as <see cref="decimal"/> values.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        /// <remarks>
        /// <para>This methods reads a <see cref="FieldKind.DecimalRef"/> primitive type. The range
        /// of this primitive type is different from the range of <see cref="decimal"/>. Refer to the
        /// primitive type documentation for details.</para>
        /// </remarks>
        /// <exception cref="SerializationException">A specified value is outside the range of the
        /// <see cref="decimal"/> type.</exception>
        decimal?[]? ReadDecimalRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.DecimalRef"/> field as a <see cref="HBigDecimal"/>.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        HBigDecimal? ReadBigDecimalRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfDecimalRef"/> field as <see cref="HBigDecimal"/> values.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        HBigDecimal?[]? ReadBigDecimalRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.TimeRef"/> field as a <see cref="TimeSpan"/>.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        TimeSpan? ReadTimeRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfTimeRef"/> field as <see cref="TimeSpan"/> values.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        TimeSpan?[]? ReadTimeRefs(string name);

#if NET6_0_OR_GREATER
        /// <summary>Reads a <see cref="FieldKind.TimeRef"/> field as a <see cref="TimeOnly"/>.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        TimeOnly? ReadTimeOnlyRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfTimeRef"/> field as <see cref="TimeOnly"/> values.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        TimeOnly?[]? ReadTimeOnlyRefs(string name);
#endif

        /// <summary>Reads a <see cref="FieldKind.DateRef"/> field as a <see cref="DateTime"/>.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateTime? ReadDateRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfDateRef"/> field as <see cref="DateTime"/> values.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateTime?[]? ReadDateRefs(string name);

#if NET6_0_OR_GREATER
        /// <summary>Reads a <see cref="FieldKind.DateRef"/> field as a <see cref="DateOnly"/>.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateOnly? ReadDateOnlyRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfDateRef"/> field as <see cref="DateOnly"/> values.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateOnly?[]? ReadDateOnlyRefs(string name);
#endif

        // FIXME - document date & time ranges when reading

        /// <summary>Reads a <see cref="FieldKind.TimeStampRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateTime? ReadTimeStampRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfTimeStampRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateTime?[]? ReadTimeStampRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.TimeStampWithTimeZoneRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateTimeOffset? ReadTimeStampWithTimeZoneRef(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfTimeStampWithTimeZoneRef"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateTimeOffset?[]? ReadTimeStampWithTimeZoneRefs(string name);

        /// <summary>Reads a <see cref="FieldKind.ObjectRef"/> field.</summary>
        /// <typeparam name="T">The expected type of the object.</typeparam>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        T? ReadObjectRef<T>(string name) where T: class;

        /// <summary>Reads a <see cref="FieldKind.ArrayOfObjectRef"/> field.</summary>
        /// <typeparam name="T">The expected type of the objects.</typeparam>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        T?[]? ReadObjectRefs<T>(string name) where T : class;
    }
}
