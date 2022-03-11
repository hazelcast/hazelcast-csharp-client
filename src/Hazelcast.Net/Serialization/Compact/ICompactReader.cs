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

        /// <summary>
        /// Determines whether the schema has a specified field.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="kind">The <see cref="FieldKind"/> of the field.</param>
        /// <returns><c>true</c> if the schema has a field with the specified <paramref name="name"/> and
        /// of the specified <paramref name="kind"/>; otherwise <c>false</c>.</returns>
        bool HasField(string name, FieldKind kind);

        /// <summary>Reads a <see cref="FieldKind.Boolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        bool ReadBoolean(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableBoolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// /// <returns>The value of the field.</returns>
        bool? ReadNullableBoolean(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfBoolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        bool[]? ReadArrayOfBoolean(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableBoolean"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        bool?[]? ReadArrayOfNullableBoolean(string name);

        /// <summary>Reads a <see cref="FieldKind.Int8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        sbyte ReadInt8(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableInt8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        sbyte? ReadNullableInt8(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfInt8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        sbyte[]? ReadArrayOfInt8(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableInt8"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        sbyte?[]? ReadArrayOfNullableInt8(string name);

        /// <summary>Reads a <see cref="FieldKind.Int16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        short ReadInt16(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableInt16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        short? ReadNullableInt16(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfInt16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        short[]? ReadArrayOfInt16(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableInt16"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        short?[]? ReadArrayOfNullableInt16(string name);

        /// <summary>Reads a <see cref="FieldKind.Int32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        int ReadInt32(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableInt32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        int? ReadNullableInt32(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfInt32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        int[]? ReadArrayOfInt32(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableInt32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        int?[]? ReadArrayOfNullableInt32(string name);

        /// <summary>Reads a <see cref="FieldKind.Int64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        long ReadInt64(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableInt64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        long? ReadNullableInt64(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfInt64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        long[]? ReadArrayOfInt64(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableInt64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        long?[]? ReadArrayOfNullableInt64(string name);

        /// <summary>Reads a <see cref="FieldKind.Float32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        float ReadFloat32(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableFloat32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        float? ReadNullableFloat32(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfFloat32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        float[]? ReadArrayOfFloat32(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableFloat32"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        float?[]? ReadArrayOfNullableFloat32(string name);

        /// <summary>Reads a <see cref="FieldKind.Float64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        double ReadFloat64(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableFloat64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        double? ReadNullableFloat64(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfFloat64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        double[]? ReadArrayOfFloat64(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableFloat64"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        double?[]? ReadArrayOfNullableFloat64(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableString"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        string? ReadNullableString(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableString"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        string?[]? ReadArrayOfNullableString(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableDecimal"/> field as a <see cref="HBigDecimal"/>.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        HBigDecimal? ReadNullableDecimal(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableDecimal"/> field as <see cref="HBigDecimal"/> values.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        HBigDecimal?[]? ReadArrayOfNullableDecimal(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableTime"/> field as a <see cref="TimeSpan"/>.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        TimeSpan? ReadNullableTime(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableTime"/> field as <see cref="TimeSpan"/> values.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        TimeSpan?[]? ReadArrayOfNullableTime(string name);

        // FIXME - discuss supporting this - of course we should, but then, method name?
        // ReadTimeAsTimeOnlyRef ?
#if NET6_0_OR_GREATER
        /// <summary>Reads a <see cref="FieldKind.TimeRef"/> field as a <see cref="TimeOnly"/>.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        TimeOnly? ReadNullableTimeOnly(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfTimeRef"/> field as <see cref="TimeOnly"/> values.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        TimeOnly?[]? ReadArrayOfNullableTimeOnly(string name);
#endif

        /// <summary>Reads a <see cref="FieldKind.NullableDate"/> field as a <see cref="DateTime"/>.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateTime? ReadNullableDate(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableDate"/> field as <see cref="DateTime"/> values.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateTime?[]? ReadArrayOfNullableDate(string name);

        // FIXME - discuss
#if NET6_0_OR_GREATER
        /// <summary>Reads a <see cref="FieldKind.DateRef"/> field as a <see cref="DateOnly"/>.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateOnly? ReadNullableDateOnly(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfDateRef"/> field as <see cref="DateOnly"/> values.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateOnly?[]? ReadNullableArrayOfDateOnly(string name);
#endif

        // FIXME - document date & time ranges when reading
        // and if we want the exact range as Java are we going to implement our OWN types ?!?!?!

        /// <summary>Reads a <see cref="FieldKind.NullableTimeStamp"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateTime? ReadNullableTimeStamp(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableTimeStamp"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateTime?[]? ReadArrayOfNullableTimeStamp(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableTimeStampWithTimeZone"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateTimeOffset? ReadNullableTimeStampWithTimeZone(string name);

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableTimeStampWithTimeZone"/> field.</summary>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        DateTimeOffset?[]? ReadArrayOfNullableTimeStampWithTimeZone(string name);

        /// <summary>Reads a <see cref="FieldKind.NullableCompact"/> field.</summary>
        /// <typeparam name="T">The expected type of the object.</typeparam>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        T? ReadNullableCompact<T>(string name) where T: class;

        /// <summary>Reads a <see cref="FieldKind.ArrayOfNullableCompact"/> field.</summary>
        /// <typeparam name="T">The expected type of the objects.</typeparam>
        /// <param name="name">The name of the field.</param>
        /// <returns>The value of the field.</returns>
        T?[]? ReadArrayOfNullableCompact<T>(string name) where T : class;
    }
}
