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
    internal static partial class CompactReaderExtensions
    {
        // do NOT remove nor alter the <generated></generated> lines!
        // <generated>


        // NOTE
        // ReadInt8 can read a Int8 or NullableInt8 field, provided that the value is not null.
        // For consistency purposes, ReadInt8OrDefault *also* can read a Int8 or NullableInt8
        // field, and if the field is NullableInt8 and the value is null, it throws just as
        // ReadInt8 (does not return the default value).

        /// <summary>
        /// Reads a <see cref="FieldKind.Boolean"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static bool ReadBooleanOrDefault(this ICompactReader reader, string name, bool defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasBoolean(name) || safeReader.HasNullableBoolean(name) ? safeReader.ReadBoolean(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.Int8"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static sbyte ReadInt8OrDefault(this ICompactReader reader, string name, sbyte defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasInt8(name) || safeReader.HasNullableInt8(name) ? safeReader.ReadInt8(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.Int16"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static short ReadInt16OrDefault(this ICompactReader reader, string name, short defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasInt16(name) || safeReader.HasNullableInt16(name) ? safeReader.ReadInt16(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.Int32"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static int ReadInt32OrDefault(this ICompactReader reader, string name, int defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasInt32(name) || safeReader.HasNullableInt32(name) ? safeReader.ReadInt32(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.Int64"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static long ReadInt64OrDefault(this ICompactReader reader, string name, long defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasInt64(name) || safeReader.HasNullableInt64(name) ? safeReader.ReadInt64(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.Float32"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static float ReadFloat32OrDefault(this ICompactReader reader, string name, float defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasFloat32(name) || safeReader.HasNullableFloat32(name) ? safeReader.ReadFloat32(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.Float64"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static double ReadFloat64OrDefault(this ICompactReader reader, string name, double defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasFloat64(name) || safeReader.HasNullableFloat64(name) ? safeReader.ReadFloat64(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfBoolean"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static bool[]? ReadArrayOfBooleanOrDefault(this ICompactReader reader, string name, bool[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfBoolean(name) || safeReader.HasArrayOfNullableBoolean(name) ? safeReader.ReadArrayOfBoolean(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfInt8"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static sbyte[]? ReadArrayOfInt8OrDefault(this ICompactReader reader, string name, sbyte[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfInt8(name) || safeReader.HasArrayOfNullableInt8(name) ? safeReader.ReadArrayOfInt8(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfInt16"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static short[]? ReadArrayOfInt16OrDefault(this ICompactReader reader, string name, short[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfInt16(name) || safeReader.HasArrayOfNullableInt16(name) ? safeReader.ReadArrayOfInt16(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfInt32"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static int[]? ReadArrayOfInt32OrDefault(this ICompactReader reader, string name, int[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfInt32(name) || safeReader.HasArrayOfNullableInt32(name) ? safeReader.ReadArrayOfInt32(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfInt64"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static long[]? ReadArrayOfInt64OrDefault(this ICompactReader reader, string name, long[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfInt64(name) || safeReader.HasArrayOfNullableInt64(name) ? safeReader.ReadArrayOfInt64(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfFloat32"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static float[]? ReadArrayOfFloat32OrDefault(this ICompactReader reader, string name, float[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfFloat32(name) || safeReader.HasArrayOfNullableFloat32(name) ? safeReader.ReadArrayOfFloat32(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfFloat64"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static double[]? ReadArrayOfFloat64OrDefault(this ICompactReader reader, string name, double[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfFloat64(name) || safeReader.HasArrayOfNullableFloat64(name) ? safeReader.ReadArrayOfFloat64(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableBoolean"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static bool? ReadNullableBooleanOrDefault(this ICompactReader reader, string name, bool? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasNullableBoolean(name) || safeReader.HasBoolean(name) ? safeReader.ReadNullableBoolean(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableInt8"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static sbyte? ReadNullableInt8OrDefault(this ICompactReader reader, string name, sbyte? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasNullableInt8(name) || safeReader.HasInt8(name) ? safeReader.ReadNullableInt8(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableInt16"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static short? ReadNullableInt16OrDefault(this ICompactReader reader, string name, short? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasNullableInt16(name) || safeReader.HasInt16(name) ? safeReader.ReadNullableInt16(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableInt32"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static int? ReadNullableInt32OrDefault(this ICompactReader reader, string name, int? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasNullableInt32(name) || safeReader.HasInt32(name) ? safeReader.ReadNullableInt32(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableInt64"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static long? ReadNullableInt64OrDefault(this ICompactReader reader, string name, long? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasNullableInt64(name) || safeReader.HasInt64(name) ? safeReader.ReadNullableInt64(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableFloat32"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static float? ReadNullableFloat32OrDefault(this ICompactReader reader, string name, float? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasNullableFloat32(name) || safeReader.HasFloat32(name) ? safeReader.ReadNullableFloat32(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.NullableFloat64"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static double? ReadNullableFloat64OrDefault(this ICompactReader reader, string name, double? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasNullableFloat64(name) || safeReader.HasFloat64(name) ? safeReader.ReadNullableFloat64(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.Decimal"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HBigDecimal? ReadDecimalOrDefault(this ICompactReader reader, string name, HBigDecimal? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasDecimal(name) ? safeReader.ReadDecimal(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.String"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static string? ReadStringOrDefault(this ICompactReader reader, string name, string? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasString(name) ? safeReader.ReadString(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.Time"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HLocalTime? ReadTimeOrDefault(this ICompactReader reader, string name, HLocalTime? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasTime(name) ? safeReader.ReadTime(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.Date"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HLocalDate? ReadDateOrDefault(this ICompactReader reader, string name, HLocalDate? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasDate(name) ? safeReader.ReadDate(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.TimeStamp"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HLocalDateTime? ReadTimeStampOrDefault(this ICompactReader reader, string name, HLocalDateTime? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasTimeStamp(name) ? safeReader.ReadTimeStamp(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.TimeStampWithTimeZone"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HOffsetDateTime? ReadTimeStampWithTimeZoneOrDefault(this ICompactReader reader, string name, HOffsetDateTime? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasTimeStampWithTimeZone(name) ? safeReader.ReadTimeStampWithTimeZone(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.Compact"/> field.
        /// </summary>
        /// <typeparam name="T">The expected type of the object.</typeparam>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static T? ReadCompactOrDefault<T>(this ICompactReader reader, string name, T? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasCompact(name) ? safeReader.ReadCompact<T>(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableBoolean"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static bool?[]? ReadArrayOfNullableBooleanOrDefault(this ICompactReader reader, string name, bool?[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfNullableBoolean(name) || safeReader.HasArrayOfBoolean(name) ? safeReader.ReadArrayOfNullableBoolean(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableInt8"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static sbyte?[]? ReadArrayOfNullableInt8OrDefault(this ICompactReader reader, string name, sbyte?[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfNullableInt8(name) || safeReader.HasArrayOfInt8(name) ? safeReader.ReadArrayOfNullableInt8(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableInt16"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static short?[]? ReadArrayOfNullableInt16OrDefault(this ICompactReader reader, string name, short?[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfNullableInt16(name) || safeReader.HasArrayOfInt16(name) ? safeReader.ReadArrayOfNullableInt16(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableInt32"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static int?[]? ReadArrayOfNullableInt32OrDefault(this ICompactReader reader, string name, int?[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfNullableInt32(name) || safeReader.HasArrayOfInt32(name) ? safeReader.ReadArrayOfNullableInt32(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableInt64"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static long?[]? ReadArrayOfNullableInt64OrDefault(this ICompactReader reader, string name, long?[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfNullableInt64(name) || safeReader.HasArrayOfInt64(name) ? safeReader.ReadArrayOfNullableInt64(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableFloat32"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static float?[]? ReadArrayOfNullableFloat32OrDefault(this ICompactReader reader, string name, float?[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfNullableFloat32(name) || safeReader.HasArrayOfFloat32(name) ? safeReader.ReadArrayOfNullableFloat32(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfNullableFloat64"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static double?[]? ReadArrayOfNullableFloat64OrDefault(this ICompactReader reader, string name, double?[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfNullableFloat64(name) || safeReader.HasArrayOfFloat64(name) ? safeReader.ReadArrayOfNullableFloat64(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfDecimal"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HBigDecimal?[]? ReadArrayOfDecimalOrDefault(this ICompactReader reader, string name, HBigDecimal?[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfDecimal(name) ? safeReader.ReadArrayOfDecimal(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfTime"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HLocalTime?[]? ReadArrayOfTimeOrDefault(this ICompactReader reader, string name, HLocalTime?[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfTime(name) ? safeReader.ReadArrayOfTime(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfDate"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HLocalDate?[]? ReadArrayOfDateOrDefault(this ICompactReader reader, string name, HLocalDate?[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfDate(name) ? safeReader.ReadArrayOfDate(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfTimeStamp"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HLocalDateTime?[]? ReadArrayOfTimeStampOrDefault(this ICompactReader reader, string name, HLocalDateTime?[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfTimeStamp(name) ? safeReader.ReadArrayOfTimeStamp(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfTimeStampWithTimeZone"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static HOffsetDateTime?[]? ReadArrayOfTimeStampWithTimeZoneOrDefault(this ICompactReader reader, string name, HOffsetDateTime?[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfTimeStampWithTimeZone(name) ? safeReader.ReadArrayOfTimeStampWithTimeZone(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfString"/> field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static string?[]? ReadArrayOfStringOrDefault(this ICompactReader reader, string name, string?[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfString(name) ? safeReader.ReadArrayOfString(name) : defaultValue;
        }

        /// <summary>
        /// Reads a <see cref="FieldKind.ArrayOfCompact"/> field.
        /// </summary>
        /// <typeparam name="T">The expected type of the objects.</typeparam>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value of the field, if available; otherwise the <paramref name="defaultValue"/>.</returns>
        public static T?[]? ReadArrayOfCompactOrDefault<T>(this ICompactReader reader, string name, T?[]? defaultValue = default)
        {
            var safeReader = reader.NotNull();
            return safeReader.HasArrayOfCompact(name) ? safeReader.ReadArrayOfCompact<T>(name) : defaultValue;
        }

        // </generated>
    }
}
