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

using Hazelcast.Models;

#nullable enable

namespace Hazelcast.Serialization.Compact
{
    public static partial class CompactReaderExtensions
    {
        // do NOT remove nor alter the <generated></generated> lines!
        // <generated>

        /// <summary>
        /// Reads a <see cref="FieldKind.Boolean"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static bool ReadBooleanOrDefault(this ICompactReader reader, string name, bool defaultValue = default)
            => reader.NotNull().HasBoolean(name) ? reader.ReadBoolean(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.Int8"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static sbyte ReadInt8OrDefault(this ICompactReader reader, string name, sbyte defaultValue = default)
            => reader.NotNull().HasInt8(name) ? reader.ReadInt8(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.Int16"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static short ReadInt16OrDefault(this ICompactReader reader, string name, short defaultValue = default)
            => reader.NotNull().HasInt16(name) ? reader.ReadInt16(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.Int32"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static int ReadInt32OrDefault(this ICompactReader reader, string name, int defaultValue = default)
            => reader.NotNull().HasInt32(name) ? reader.ReadInt32(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.Int64"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static long ReadInt64OrDefault(this ICompactReader reader, string name, long defaultValue = default)
            => reader.NotNull().HasInt64(name) ? reader.ReadInt64(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.Float32"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static float ReadFloat32OrDefault(this ICompactReader reader, string name, float defaultValue = default)
            => reader.NotNull().HasFloat32(name) ? reader.ReadFloat32(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.Float64"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static double ReadFloat64OrDefault(this ICompactReader reader, string name, double defaultValue = default)
            => reader.NotNull().HasFloat64(name) ? reader.ReadFloat64(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfBoolean"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static bool[]? ReadArrayOfBooleanOrDefault(this ICompactReader reader, string name, bool[]? defaultValue = default)
            => reader.NotNull().HasArrayOfBoolean(name) ? reader.ReadArrayOfBoolean(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfInt8"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static sbyte[]? ReadArrayOfInt8OrDefault(this ICompactReader reader, string name, sbyte[]? defaultValue = default)
            => reader.NotNull().HasArrayOfInt8(name) ? reader.ReadArrayOfInt8(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfInt16"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static short[]? ReadArrayOfInt16OrDefault(this ICompactReader reader, string name, short[]? defaultValue = default)
            => reader.NotNull().HasArrayOfInt16(name) ? reader.ReadArrayOfInt16(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfInt32"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static int[]? ReadArrayOfInt32OrDefault(this ICompactReader reader, string name, int[]? defaultValue = default)
            => reader.NotNull().HasArrayOfInt32(name) ? reader.ReadArrayOfInt32(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfInt64"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static long[]? ReadArrayOfInt64OrDefault(this ICompactReader reader, string name, long[]? defaultValue = default)
            => reader.NotNull().HasArrayOfInt64(name) ? reader.ReadArrayOfInt64(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfFloat32"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static float[]? ReadArrayOfFloat32OrDefault(this ICompactReader reader, string name, float[]? defaultValue = default)
            => reader.NotNull().HasArrayOfFloat32(name) ? reader.ReadArrayOfFloat32(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfFloat64"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static double[]? ReadArrayOfFloat64OrDefault(this ICompactReader reader, string name, double[]? defaultValue = default)
            => reader.NotNull().HasArrayOfFloat64(name) ? reader.ReadArrayOfFloat64(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableBoolean"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static bool? ReadNullableBooleanOrDefault(this ICompactReader reader, string name, bool? defaultValue = default)
            => reader.NotNull().HasNullableBoolean(name) ? reader.ReadNullableBoolean(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableInt8"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static sbyte? ReadNullableInt8OrDefault(this ICompactReader reader, string name, sbyte? defaultValue = default)
            => reader.NotNull().HasNullableInt8(name) ? reader.ReadNullableInt8(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableInt16"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static short? ReadNullableInt16OrDefault(this ICompactReader reader, string name, short? defaultValue = default)
            => reader.NotNull().HasNullableInt16(name) ? reader.ReadNullableInt16(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableInt32"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static int? ReadNullableInt32OrDefault(this ICompactReader reader, string name, int? defaultValue = default)
            => reader.NotNull().HasNullableInt32(name) ? reader.ReadNullableInt32(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableInt64"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static long? ReadNullableInt64OrDefault(this ICompactReader reader, string name, long? defaultValue = default)
            => reader.NotNull().HasNullableInt64(name) ? reader.ReadNullableInt64(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableFloat32"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static float? ReadNullableFloat32OrDefault(this ICompactReader reader, string name, float? defaultValue = default)
            => reader.NotNull().HasNullableFloat32(name) ? reader.ReadNullableFloat32(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableFloat64"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static double? ReadNullableFloat64OrDefault(this ICompactReader reader, string name, double? defaultValue = default)
            => reader.NotNull().HasNullableFloat64(name) ? reader.ReadNullableFloat64(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableDecimal"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HBigDecimal? ReadNullableDecimalOrDefault(this ICompactReader reader, string name, HBigDecimal? defaultValue = default)
            => reader.NotNull().HasNullableDecimal(name) ? reader.ReadNullableDecimal(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableString"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static string? ReadNullableStringOrDefault(this ICompactReader reader, string name, string? defaultValue = default)
            => reader.NotNull().HasNullableString(name) ? reader.ReadNullableString(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableTime"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HLocalTime? ReadNullableTimeOrDefault(this ICompactReader reader, string name, HLocalTime? defaultValue = default)
            => reader.NotNull().HasNullableTime(name) ? reader.ReadNullableTime(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableDate"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HLocalDate? ReadNullableDateOrDefault(this ICompactReader reader, string name, HLocalDate? defaultValue = default)
            => reader.NotNull().HasNullableDate(name) ? reader.ReadNullableDate(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableTimeStamp"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HLocalDateTime? ReadNullableTimeStampOrDefault(this ICompactReader reader, string name, HLocalDateTime? defaultValue = default)
            => reader.NotNull().HasNullableTimeStamp(name) ? reader.ReadNullableTimeStamp(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableTimeStampWithTimeZone"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HOffsetDateTime? ReadNullableTimeStampWithTimeZoneOrDefault(this ICompactReader reader, string name, HOffsetDateTime? defaultValue = default)
            => reader.NotNull().HasNullableTimeStampWithTimeZone(name) ? reader.ReadNullableTimeStampWithTimeZone(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableCompact"/> field.
        /// </summary>
        /// <typeparam name="T">The expected type of the object.</typeparam>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static T? ReadNullableCompactOrDefault<T>(this ICompactReader reader, string name, T? defaultValue = default) where T : class
            => reader.NotNull().HasNullableCompact(name) ? reader.ReadNullableCompact<T>(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableBoolean"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static bool?[]? ReadArrayOfNullableBooleanOrDefault(this ICompactReader reader, string name, bool?[]? defaultValue = default)
            => reader.NotNull().HasArrayOfNullableBoolean(name) ? reader.ReadArrayOfNullableBoolean(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableInt8"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static sbyte?[]? ReadArrayOfNullableInt8OrDefault(this ICompactReader reader, string name, sbyte?[]? defaultValue = default)
            => reader.NotNull().HasArrayOfNullableInt8(name) ? reader.ReadArrayOfNullableInt8(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableInt16"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static short?[]? ReadArrayOfNullableInt16OrDefault(this ICompactReader reader, string name, short?[]? defaultValue = default)
            => reader.NotNull().HasArrayOfNullableInt16(name) ? reader.ReadArrayOfNullableInt16(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableInt32"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static int?[]? ReadArrayOfNullableInt32OrDefault(this ICompactReader reader, string name, int?[]? defaultValue = default)
            => reader.NotNull().HasArrayOfNullableInt32(name) ? reader.ReadArrayOfNullableInt32(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableInt64"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static long?[]? ReadArrayOfNullableInt64OrDefault(this ICompactReader reader, string name, long?[]? defaultValue = default)
            => reader.NotNull().HasArrayOfNullableInt64(name) ? reader.ReadArrayOfNullableInt64(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableFloat32"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static float?[]? ReadArrayOfNullableFloat32OrDefault(this ICompactReader reader, string name, float?[]? defaultValue = default)
            => reader.NotNull().HasArrayOfNullableFloat32(name) ? reader.ReadArrayOfNullableFloat32(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableFloat64"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static double?[]? ReadArrayOfNullableFloat64OrDefault(this ICompactReader reader, string name, double?[]? defaultValue = default)
            => reader.NotNull().HasArrayOfNullableFloat64(name) ? reader.ReadArrayOfNullableFloat64(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableDecimal"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HBigDecimal?[]? ReadArrayOfNullableDecimalOrDefault(this ICompactReader reader, string name, HBigDecimal?[]? defaultValue = default)
            => reader.NotNull().HasArrayOfNullableDecimal(name) ? reader.ReadArrayOfNullableDecimal(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableTime"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HLocalTime?[]? ReadArrayOfNullableTimeOrDefault(this ICompactReader reader, string name, HLocalTime?[]? defaultValue = default)
            => reader.NotNull().HasArrayOfNullableTime(name) ? reader.ReadArrayOfNullableTime(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableDate"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HLocalDate?[]? ReadArrayOfNullableDateOrDefault(this ICompactReader reader, string name, HLocalDate?[]? defaultValue = default)
            => reader.NotNull().HasArrayOfNullableDate(name) ? reader.ReadArrayOfNullableDate(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableTimeStamp"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HLocalDateTime?[]? ReadArrayOfNullableTimeStampOrDefault(this ICompactReader reader, string name, HLocalDateTime?[]? defaultValue = default)
            => reader.NotNull().HasArrayOfNullableTimeStamp(name) ? reader.ReadArrayOfNullableTimeStamp(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableTimeStampWithTimeZone"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HOffsetDateTime?[]? ReadArrayOfNullableTimeStampWithTimeZoneOrDefault(this ICompactReader reader, string name, HOffsetDateTime?[]? defaultValue = default)
            => reader.NotNull().HasArrayOfNullableTimeStampWithTimeZone(name) ? reader.ReadArrayOfNullableTimeStampWithTimeZone(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableString"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static string?[]? ReadArrayOfNullableStringOrDefault(this ICompactReader reader, string name, string?[]? defaultValue = default)
            => reader.NotNull().HasArrayOfNullableString(name) ? reader.ReadArrayOfNullableString(name) : defaultValue;

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableCompact"/> field.
        /// </summary>
        /// <typeparam name="T">The expected type of the objects.</typeparam>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static T?[]? ReadArrayOfNullableCompactOrDefault<T>(this ICompactReader reader, string name, T?[]? defaultValue = default) where T : class
            => reader.NotNull().HasArrayOfNullableCompact(name) ? reader.ReadArrayOfNullableCompact<T>(name) : defaultValue;

        // </generated>
    }
}
