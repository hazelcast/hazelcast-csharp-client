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
    internal static partial class CompactReaderExtensions
    {
        // notes
        // the strict minimum was decided to be ICompactReader.GetFieldKind(name) which can return
        // FieldKind.NoField in case the field does not exist. then, everything can be provided on
        // top of it through the extension methods exposed here.

        /// <summary>
        /// Determines whether a field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="kind">The <see cref="FieldKind"/> of the field.</param>
        /// <returns><c>true</c> if the schema has a field with the specified <paramref name="name"/> and
        /// of the specified <paramref name="kind"/>; otherwise <c>false</c>.</returns>
        public static bool HasField(this ICompactReader reader, string name, FieldKind kind)
            => reader.NotNull().GetFieldKind(name) == kind;

        // FIXME document - the nice thing is that we could do without FieldKind.NotAvailable entirely
        public static bool HasField(this ICompactReader reader, string name, out FieldKind kind)
        {
            kind = reader.NotNull().GetFieldKind(name);
            return kind != FieldKind.NotAvailable;
        }

        /// <summary>
        /// Determines whether a field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a field with the specified <paramref name="name"/>;
        /// otherwise <c>false</c>.</returns>
        public static bool HasField(this ICompactReader reader, string name)
            => reader.NotNull().GetFieldKind(name) != FieldKind.NotAvailable;

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
        /// Determines whether a <see cref="FieldKind.Decimal"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.Decimal"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasDecimal(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.Decimal);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.String"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.String"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasString(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.String);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.Time"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.Time"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasTime(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.Time);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.Date"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.Date"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasDate(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.Date);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.TimeStamp"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.TimeStamp"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasTimeStamp(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.TimeStamp);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.TimeStampWithTimeZone"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.TimeStampWithTimeZone"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasTimeStampWithTimeZone(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.TimeStampWithTimeZone);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.Compact"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.Compact"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasCompact(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.Compact);

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
        /// Determines whether a <see cref="FieldKind.ArrayOfDecimal"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfDecimal"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfDecimal(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfDecimal);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfTime"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfTime"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfTime(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfTime);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfDate"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfDate"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfDate(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfDate);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfTimeStamp"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfTimeStamp"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfTimeStamp(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfTimeStamp);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfTimeStampWithTimeZone"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfTimeStampWithTimeZone"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfTimeStampWithTimeZone(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfTimeStampWithTimeZone);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfString"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfString"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfString(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfString);

        /// <summary>
        /// Determines whether a <see cref="FieldKind.ArrayOfCompact"/> field is available.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="name">The name of the field.</param>
        /// <returns><c>true</c> if the schema has a <see cref="FieldKind.ArrayOfCompact"/> field with the
        /// specified <paramref name="name"/>; otherwise <c>false</c>.</returns>
        public static bool HasArrayOfCompact(this ICompactReader reader, string name)
            => reader.NotNull().HasField(name, FieldKind.ArrayOfCompact);

        // </generated>
    }
}
