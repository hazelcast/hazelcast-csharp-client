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

namespace Hazelcast.Serialization.Compact
{
    // FIXME - currently exposing all the HasXxx methods - should they be internal?

    public static partial class CompactReaderExtensions
    {
        // do NOT remove nor alter the <generated></generated> lines!
        // <generated>

        /// <summary>
        /// Determines whether a <see cref="FieldKind.Boolean"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.Boolean"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasBoolean(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.Boolean);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.Int8"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.Int8"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasInt8(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.Int8);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.Int16"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.Int16"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasInt16(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.Int16);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.Int32"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.Int32"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasInt32(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.Int32);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.Int64"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.Int64"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasInt64(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.Int64);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.Float32"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.Float32"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasFloat32(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.Float32);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.Float64"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.Float64"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasFloat64(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.Float64);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfBoolean"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfBoolean"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfBoolean(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfBoolean);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfInt8"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfInt8"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfInt8(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfInt8);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfInt16"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfInt16"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfInt16(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfInt16);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfInt32"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfInt32"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfInt32(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfInt32);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfInt64"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfInt64"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfInt64(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfInt64);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfFloat32"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfFloat32"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfFloat32(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfFloat32);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfFloat64"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfFloat64"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfFloat64(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfFloat64);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.NullableBoolean"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.NullableBoolean"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasNullableBoolean(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.NullableBoolean);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.NullableInt8"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.NullableInt8"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasNullableInt8(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.NullableInt8);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.NullableInt16"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.NullableInt16"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasNullableInt16(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.NullableInt16);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.NullableInt32"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.NullableInt32"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasNullableInt32(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.NullableInt32);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.NullableInt64"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.NullableInt64"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasNullableInt64(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.NullableInt64);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.NullableFloat32"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.NullableFloat32"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasNullableFloat32(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.NullableFloat32);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.NullableFloat64"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.NullableFloat64"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasNullableFloat64(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.NullableFloat64);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.NullableDecimal"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.NullableDecimal"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasNullableDecimal(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.NullableDecimal);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.NullableString"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.NullableString"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasNullableString(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.NullableString);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.NullableTime"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.NullableTime"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasNullableTime(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.NullableTime);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.NullableDate"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.NullableDate"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasNullableDate(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.NullableDate);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.NullableTimeStamp"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.NullableTimeStamp"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasNullableTimeStamp(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.NullableTimeStamp);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.NullableTimeStampWithTimeZone"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.NullableTimeStampWithTimeZone"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasNullableTimeStampWithTimeZone(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.NullableTimeStampWithTimeZone);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.NullableCompact"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.NullableCompact"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasNullableCompact(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.NullableCompact);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfNullableBoolean"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfNullableBoolean"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfNullableBoolean(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfNullableBoolean);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfNullableInt8"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfNullableInt8"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfNullableInt8(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfNullableInt8);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfNullableInt16"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfNullableInt16"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfNullableInt16(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfNullableInt16);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfNullableInt32"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfNullableInt32"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfNullableInt32(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfNullableInt32);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfNullableInt64"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfNullableInt64"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfNullableInt64(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfNullableInt64);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfNullableFloat32"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfNullableFloat32"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfNullableFloat32(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfNullableFloat32);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfNullableFloat64"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfNullableFloat64"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfNullableFloat64(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfNullableFloat64);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfNullableDecimal"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfNullableDecimal"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfNullableDecimal(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfNullableDecimal);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfNullableTime"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfNullableTime"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfNullableTime(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfNullableTime);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfNullableDate"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfNullableDate"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfNullableDate(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfNullableDate);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfNullableTimeStamp"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfNullableTimeStamp"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfNullableTimeStamp(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfNullableTimeStamp);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfNullableTimeStampWithTimeZone"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfNullableTimeStampWithTimeZone"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfNullableTimeStampWithTimeZone(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfNullableTimeStampWithTimeZone);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfNullableString"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfNullableString"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfNullableString(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfNullableString);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfNullableCompact"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfNullableCompact"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfNullableCompact(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfNullableCompact);

        // </generated>
    }
}
