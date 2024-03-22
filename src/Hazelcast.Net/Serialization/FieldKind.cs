// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Defines the kind of a field for serialization.
    /// </summary>
    /// <remarks>
    /// <para>In order to stay close to C# nullable names, arrays and everything named "Nullable"
    /// is nullable, whereas everything else is non-nullable. Therefore, for instance,
    /// <see cref="String"/> indicates a reference (which may be null) to a string.</para>
    /// <para>This enumeration describe types that can be used by portable and/and compact
    /// serialization. Note that one serialization solution may not support all types.</para>
    /// </remarks>
    public enum FieldKind
    {
        // Note: following PR&QA reviews it has been decided that values that are not supported
        // by .NET / Compact would not be listed here. This is for Char, Portable, etc.

#pragma warning disable CA1720 // Identifier contains type name

        /// <summary>The type of a field that is not available.</summary>
        NotAvailable = 0,

        // ---- non-nullable types ----

        /// <summary>The boolean primitive type.</summary>
        Boolean = 1,

        /// <summary>The i8 primitive type.</summary>
        Int8 = 3,

        /*
        /// <summary>The char primitive type.</summary>
        /// <remarks><para>This type is not supported by compact serialization.</para></remarks>
        Char = 5,
        */

        /// <summary>The i16 primitive type.</summary>
        Int16 = 7,

        /// <summary>The i32 primitive type.</summary>
        Int32 = 9,

        /// <summary>The i64 primitive type.</summary>
        Int64 = 11,

        /// <summary>The f32 primitive type.</summary>
        /// <remarks>
        /// <para>The f32 primitive type is a 32-bits IEEE 754 floating-point number.</para>
        /// </remarks>
        Float32 = 13,

        /// <summary>The f64 primitive type.</summary>
        /// <remarks>
        /// <para>The f64 primitive type is a 64-bits IEEE 754 floating-point number.</para>
        /// </remarks>
        Float64 = 15,


        // ---- arrays of non-nullable types ----

        /// <summary>The array-of-boolean primitive type.</summary>
        ArrayOfBoolean = 2,

        /// <summary>The array-of-i8 primitive type.</summary>
        ArrayOfInt8 = 4,

        /*
        /// <summary>The array-of-char primitive type.</summary>
        /// <remarks><para>This type is not supported by compact serialization.</para></remarks>
        ArrayOfChar = 6,
        */

        /// <summary>The array-of-i16 primitive type.</summary>
        ArrayOfInt16 = 8,

        /// <summary>The array-of-i32 primitive type.</summary>
        ArrayOfInt32 = 10,

        /// <summary>The array-of-i64 primitive type.</summary>
        ArrayOfInt64 = 12,

        /// <summary>The array-of-f32 primitive type.</summary>
        ArrayOfFloat32 = 14,

        /// <summary>The array-of-f64 primitive type.</summary>
        ArrayOfFloat64 = 16,


        // ---- nullable types ----
        
        /// <summary>The nullable-boolean primitive type.</summary>
        NullableBoolean = 33,

        /// <summary>The nullable-i8 primitive type.</summary>
        NullableInt8 = 35,

        /// <summary>The nullable-i16 primitive type.</summary>
        NullableInt16 = 37,

        /// <summary>The nullable-i32 primitive type.</summary>
        NullableInt32 = 39,

        /// <summary>The nullable-i64 primitive type.</summary>
        NullableInt64 = 41,

        /// <summary>The nullable-f32 primitive type.</summary>
        /// <remarks>
        /// <para>The f32 primitive type is a 32-bits IEEE 754 floating-point number.</para>
        /// </remarks>
        NullableFloat32 = 43,

        /// <summary>The nullable-f64 primitive type.</summary>
        /// <remarks>
        /// <para>The f64 primitive type is a 64-bits IEEE 754 floating-point number.</para>
        /// </remarks>
        NullableFloat64 = 45,

        /// <summary>The nullable-decimal primitive type.</summary>
        /// <remarks>
        /// <para>The decimal primitive type is an arbitrary-precision and scale floating-point number.</para>
        /// </remarks>
        Decimal = 19,

        /// <summary>The nullable-string primitive type.</summary>
        String = 17,

        /// <summary>The nullable-time primitive type.</summary>
        /// <remarks>
        /// <para>The time primitive type represents a time expressed in hours (0-24), minutes, seconds
        /// and nanoseconds, with nanoseconds precision. Its best C# equivalent is <see cref="TimeSpan"/>,
        /// but note that <see cref="TimeSpan"/> only has 100ns tick precision, and can support negative
        /// values or values greater than 1 day. It is therefore not fully equivalent to the primitive
        /// type.</para>
        /// </remarks>
        Time = 21,

        /// <summary>The nullable-date primitive type.</summary>
        /// <remarks>
        /// <para>The date primitive type represents a date expressed in year, month, and day-of-month,
        /// with year within the -10^9 to 10^9 exclusive range. Its best C# equivalent is <see cref="DateTime"/>,
        /// but note that <see cref="DateTime"/> only has support for years within the 1 to 9999 range,
        /// and supports time. It is therefore not fully equivalent to the primitive type.</para>
        /// </remarks>
        Date = 23,

        /// <summary>The nullable-timestamp primitive type.</summary>
        /// <remarks>
        /// <para>The timestamp primitive type is a combination of a date and a time primitive type.</para>
        /// </remarks>
        TimeStamp = 25,

        /// <summary>The nullable-timestamp-with-timezone primitive type.</summary>
        /// <remarks>
        /// <para>The timestamp-with-timezone primitive type is a combination of a timestamp primitive type
        /// and a timezone offset within the -18h to +18h range and with seconds precision.</para>
        /// </remarks>
        TimeStampWithTimeZone = 27,

        /// <summary>The nullable compact object primitive type.</summary>
        /// <remarks>
        /// <para>The compact object primitive type represents any object which is, in turn, composed of fields
        /// with primitive type values.</para>
        /// <para>This type is not supported by portable serialization.</para>
        /// </remarks>
        Compact = 29,

        /*
        /// <summary>The nullable portable object primitive type.</summary>
        /// <remarks>
        /// <para>The portable object primitive type represents any object which is, in turn, composed of fields
        /// with primitive type values.</para>
        /// <para>This type is not supported by compact serialization.</para>
        /// </remarks>
        Portable = 31,
        */


        // ---- arrays of nullable types ----

        /// <summary>The array-of-nullable-boolean primitive type.</summary>
        ArrayOfNullableBoolean = 34,

        /// <summary>The array-of-nullable-i8 primitive type.</summary>
        ArrayOfNullableInt8 = 36,

        /// <summary>The array-of-nullable-i16 primitive type.</summary>
        ArrayOfNullableInt16 = 38,

        /// <summary>The array-of-nullable-i32 primitive type.</summary>
        ArrayOfNullableInt32 = 40,

        /// <summary>The array-of-nullable-i64 primitive type.</summary>
        ArrayOfNullableInt64 = 42,

        /// <summary>The array-of-nullable-f32 primitive type.</summary>
        ArrayOfNullableFloat32 = 44,

        /// <summary>The array-of-nullable-f24 primitive type.</summary>
        ArrayOfNullableFloat64 = 46,

        /// <summary>The array-of-nullable-decimal primitive type.</summary>
        ArrayOfDecimal = 20,

        /// <summary>The array-of-nullable-time primitive type.</summary>
        ArrayOfTime = 22,

        /// <summary>The array-of-nullable-date primitive type.</summary>
        ArrayOfDate = 24,

        /// <summary>The array-of-nullable-timestamp primitive type.</summary>
        ArrayOfTimeStamp = 26,

        /// <summary>The array-of-nullable-timestamp-with-timezone primitive type.</summary>
        ArrayOfTimeStampWithTimeZone = 28,

        /// <summary>The array-of-string primitive type.</summary>
        ArrayOfString = 18,

        /// <summary>The array-of-compact-object primitive type.</summary>
        /// <para>This type is not supported by portable serialization.</para>
        ArrayOfCompact = 30

        /*
        /// <summary>The array-of-portable-object primitive type.</summary>
        /// <para>This type is not supported by compact serialization.</para>
        ArrayOfPortable = 32
        */

#pragma warning restore CA1720 // Identifier contains type name
    }
}
