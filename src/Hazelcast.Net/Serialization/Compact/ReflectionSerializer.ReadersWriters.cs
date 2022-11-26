// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using Hazelcast.Models;

namespace Hazelcast.Serialization.Compact;

internal partial class ReflectionSerializer
{
    // map the CLR types to their corresponding ICompactWriter write method
    private static readonly Dictionary<Type, Action<ICompactWriter, string, object?>> Writers
        = new()
        {
            // there is no typeof nullable reference type (e.g. string?) since they are not
            // actual CLR types, so we have to register writers here against the actual types
            // (e.g. string) even though the value we write may be null.

            // first, register some non-generated writers for convenient .NET types

            { typeof (HBigDecimal), (w, n, o) => w.WriteDecimal(n, UnboxNonNull<HBigDecimal>(o)) },
            { typeof (HBigDecimal[]), (w, n, o) => w.WriteArrayOfDecimal(n, ToArray(o, UnboxNonNull<HBigDecimal>)) },
            { typeof (HLocalTime), (w, n, o) => w.WriteTime(n, UnboxNonNull<HLocalTime>(o)) },
            { typeof (HLocalTime[]), (w, n, o) => w.WriteArrayOfTime(n, ToArray(o, UnboxNonNull<HLocalTime>)) },
            { typeof (HLocalDate), (w, n, o) => w.WriteDate(n, UnboxNonNull<HLocalDate>(o)) },
            { typeof (HLocalDate[]), (w, n, o) => w.WriteArrayOfDate(n, ToArray(o, UnboxNonNull<HLocalDate>)) },
            { typeof (HLocalDateTime), (w, n, o) => w.WriteTimeStamp(n, UnboxNonNull<HLocalDateTime>(o)) },
            { typeof (HLocalDateTime[]), (w, n, o) => w.WriteArrayOfTimeStamp(n, ToArray(o, UnboxNonNull<HLocalDateTime>)) },
            { typeof (HOffsetDateTime), (w, n, o) => w.WriteTimeStampWithTimeZone(n, UnboxNonNull<HOffsetDateTime>(o)) },
            { typeof (HOffsetDateTime[]), (w, n, o) => w.WriteArrayOfTimeStampWithTimeZone(n, ToArray(o, UnboxNonNull<HOffsetDateTime>)) },

            { typeof (decimal), (w, n, o) => w.WriteDecimal(n, DecimalToBigDecimal(o)) },
            { typeof (decimal?), (w, n, o) => w.WriteDecimal(n, DecimalToBigDecimal(o)) },
            { typeof (decimal[]), (w, n, o) => w.WriteArrayOfDecimal(n, ToArray(o, DecimalToBigDecimal)) },
            { typeof (decimal?[]), (w, n, o) => w.WriteArrayOfDecimal(n, ToArray(o, DecimalToBigDecimal)) },

            { typeof (TimeSpan), (w, n, o) => w.WriteTime(n, TimeSpanToTime(o)) },
            { typeof (TimeSpan?), (w, n, o) => w.WriteTime(n, TimeSpanToTime(o)) },
            { typeof (TimeSpan[]), (w, n, o) => w.WriteArrayOfTime(n, ToArray(o, TimeSpanToTime)) },
            { typeof (TimeSpan?[]), (w, n, o) => w.WriteArrayOfTime(n, ToArray(o, TimeSpanToTime)) },

            { typeof (DateTime), (w, n, o) => w.WriteTimeStamp(n, DateTimeToTimeStamp(o)) },
            { typeof (DateTime?), (w, n, o) => w.WriteTimeStamp(n, DateTimeToTimeStamp(o)) },
            { typeof (DateTime[]), (w, n, o) => w.WriteArrayOfTimeStamp(n, ToArray(o, DateTimeToTimeStamp)) },
            { typeof (DateTime?[]), (w, n, o) => w.WriteArrayOfTimeStamp(n, ToArray(o, DateTimeToTimeStamp)) },

            { typeof (DateTimeOffset), (w, n, o) => w.WriteTimeStampWithTimeZone(n, DateTimeOffsetToTimeStampWithTimeZone(o)) },
            { typeof (DateTimeOffset?), (w, n, o) => w.WriteTimeStampWithTimeZone(n, DateTimeOffsetToTimeStampWithTimeZone(o)) },
            { typeof (DateTimeOffset[]), (w, n, o) => w.WriteArrayOfTimeStampWithTimeZone(n, ToArray(o, DateTimeOffsetToTimeStampWithTimeZone)) },
            { typeof (DateTimeOffset?[]), (w, n, o) => w.WriteArrayOfTimeStampWithTimeZone(n, ToArray(o, DateTimeOffsetToTimeStampWithTimeZone)) },

            { typeof (char), (w, n, o) => w.WriteInt16(n, CharToShort(o)) },
            { typeof (char?), (w, n, o) => w.WriteNullableInt16(n, NullableCharToNullableShort(o)) },
            { typeof (char[]), (w, n, o) => w.WriteArrayOfInt16(n, CharsToShorts(o)) },
            { typeof (char?[]), (w, n, o) => w.WriteArrayOfNullableInt16(n, NullableCharsToNullableShorts(o)) },

#if NET6_0_OR_GREATER
                { typeof (TimeOnly), (w, n, o) => w.WriteTime(n, TimeOnlyToTime(o)) },
                { typeof (TimeOnly?), (w, n, o) => w.WriteTime(n, TimeOnlyToTime(o)) },
                { typeof (TimeOnly[]), (w, n, o) => w.WriteArrayOfTime(n, ToArray(o, TimeOnlyToTime)) },
                { typeof (TimeOnly?[]), (w, n, o) => w.WriteArrayOfTime(n, ToArray(o, TimeOnlyToTime)) },

                { typeof (DateOnly), (w, n, o) => w.WriteDate(n, DateOnlyToDate(o)) },
                { typeof (DateOnly?), (w, n, o) => w.WriteDate(n, DateOnlyToDate(o)) },
                { typeof (DateOnly[]), (w, n, o) => w.WriteArrayOfDate(n, ToArray(o, DateOnlyToDate)) },
                { typeof (DateOnly?[]), (w, n, o) => w.WriteArrayOfDate(n, ToArray(o, DateOnlyToDate)) },
#endif

            // then generate the default types

            // do NOT remove nor alter the <generated></generated> lines!
            // <generated>

            { typeof (bool), (w, n, o) => w.WriteBoolean(n, UnboxNonNull<bool>(o)) },
            { typeof (sbyte), (w, n, o) => w.WriteInt8(n, UnboxNonNull<sbyte>(o)) },
            { typeof (short), (w, n, o) => w.WriteInt16(n, UnboxNonNull<short>(o)) },
            { typeof (int), (w, n, o) => w.WriteInt32(n, UnboxNonNull<int>(o)) },
            { typeof (long), (w, n, o) => w.WriteInt64(n, UnboxNonNull<long>(o)) },
            { typeof (float), (w, n, o) => w.WriteFloat32(n, UnboxNonNull<float>(o)) },
            { typeof (double), (w, n, o) => w.WriteFloat64(n, UnboxNonNull<double>(o)) },
            { typeof (bool[]), (w, n, o) => w.WriteArrayOfBoolean(n, (bool[]?)o) },
            { typeof (sbyte[]), (w, n, o) => w.WriteArrayOfInt8(n, (sbyte[]?)o) },
            { typeof (short[]), (w, n, o) => w.WriteArrayOfInt16(n, (short[]?)o) },
            { typeof (int[]), (w, n, o) => w.WriteArrayOfInt32(n, (int[]?)o) },
            { typeof (long[]), (w, n, o) => w.WriteArrayOfInt64(n, (long[]?)o) },
            { typeof (float[]), (w, n, o) => w.WriteArrayOfFloat32(n, (float[]?)o) },
            { typeof (double[]), (w, n, o) => w.WriteArrayOfFloat64(n, (double[]?)o) },
            { typeof (bool?), (w, n, o) => w.WriteNullableBoolean(n, (bool?)o) },
            { typeof (sbyte?), (w, n, o) => w.WriteNullableInt8(n, (sbyte?)o) },
            { typeof (short?), (w, n, o) => w.WriteNullableInt16(n, (short?)o) },
            { typeof (int?), (w, n, o) => w.WriteNullableInt32(n, (int?)o) },
            { typeof (long?), (w, n, o) => w.WriteNullableInt64(n, (long?)o) },
            { typeof (float?), (w, n, o) => w.WriteNullableFloat32(n, (float?)o) },
            { typeof (double?), (w, n, o) => w.WriteNullableFloat64(n, (double?)o) },
            { typeof (HBigDecimal?), (w, n, o) => w.WriteDecimal(n, (HBigDecimal?)o) },
            { typeof (string), (w, n, o) => w.WriteString(n, (string?)o) },
            { typeof (HLocalTime?), (w, n, o) => w.WriteTime(n, (HLocalTime?)o) },
            { typeof (HLocalDate?), (w, n, o) => w.WriteDate(n, (HLocalDate?)o) },
            { typeof (HLocalDateTime?), (w, n, o) => w.WriteTimeStamp(n, (HLocalDateTime?)o) },
            { typeof (HOffsetDateTime?), (w, n, o) => w.WriteTimeStampWithTimeZone(n, (HOffsetDateTime?)o) },
            { typeof (bool?[]), (w, n, o) => w.WriteArrayOfNullableBoolean(n, (bool?[]?)o) },
            { typeof (sbyte?[]), (w, n, o) => w.WriteArrayOfNullableInt8(n, (sbyte?[]?)o) },
            { typeof (short?[]), (w, n, o) => w.WriteArrayOfNullableInt16(n, (short?[]?)o) },
            { typeof (int?[]), (w, n, o) => w.WriteArrayOfNullableInt32(n, (int?[]?)o) },
            { typeof (long?[]), (w, n, o) => w.WriteArrayOfNullableInt64(n, (long?[]?)o) },
            { typeof (float?[]), (w, n, o) => w.WriteArrayOfNullableFloat32(n, (float?[]?)o) },
            { typeof (double?[]), (w, n, o) => w.WriteArrayOfNullableFloat64(n, (double?[]?)o) },
            { typeof (HBigDecimal?[]), (w, n, o) => w.WriteArrayOfDecimal(n, (HBigDecimal?[]?)o) },
            { typeof (HLocalTime?[]), (w, n, o) => w.WriteArrayOfTime(n, (HLocalTime?[]?)o) },
            { typeof (HLocalDate?[]), (w, n, o) => w.WriteArrayOfDate(n, (HLocalDate?[]?)o) },
            { typeof (HLocalDateTime?[]), (w, n, o) => w.WriteArrayOfTimeStamp(n, (HLocalDateTime?[]?)o) },
            { typeof (HOffsetDateTime?[]), (w, n, o) => w.WriteArrayOfTimeStampWithTimeZone(n, (HOffsetDateTime?[]?)o) },
            { typeof (string[]), (w, n, o) => w.WriteArrayOfString(n, (string?[]?)o) },

            // </generated>
        };

    // map the CLR types to their corresponding ICompactReader read method
    private static readonly Dictionary<Type, Func<ICompactReader, string, object?>> Readers
        = new()
        {
// ReSharper disable RedundantCast
#pragma warning disable IDE0004

                // some casts are redundant, but let's force ourselves to cast everywhere,
                // so that we are 100% we detect potential type mismatch errors

                // there is no typeof nullable reference type (e.g. string?) since they are not
                // actual CLR types, so we have to register readers here against the actual types
                // (e.g. string) even though the value we read may be null.

                // first, register some non-generated readers for convenient .NET types

                { typeof (HBigDecimal), (r, n) => (HBigDecimal)ValueNonNull(r.ReadDecimal(n)) },
                { typeof (HBigDecimal[]), (r, n) => ToArray(r.ReadArrayOfDecimal(n), x => (HBigDecimal)ValueNonNull(x)) },
                { typeof (HLocalTime), (r, n) => (HLocalTime)ValueNonNull(r.ReadTime(n)) },
                { typeof (HLocalTime[]), (r, n) => ToArray(r.ReadArrayOfTime(n), x => (HLocalTime)ValueNonNull(x)) },
                { typeof (HLocalDate), (r, n) => (HLocalDate)ValueNonNull(r.ReadDate(n)) },
                { typeof (HLocalDate[]), (r, n) => ToArray(r.ReadArrayOfDate(n), x => (HLocalDate)ValueNonNull(x)) },
                { typeof (HLocalDateTime), (r, n) => (HLocalDateTime)ValueNonNull(r.ReadTimeStamp(n)) },
                { typeof (HLocalDateTime[]), (r, n) => ToArray(r.ReadArrayOfTimeStamp(n), x => (HLocalDateTime)ValueNonNull(x)) },
                { typeof (HOffsetDateTime), (r, n) => (HOffsetDateTime)ValueNonNull(r.ReadTimeStampWithTimeZone(n)) },
                { typeof (HOffsetDateTime[]), (r, n) => ToArray(r.ReadArrayOfTimeStampWithTimeZone(n), x => (HOffsetDateTime)ValueNonNull(x)) },

                { typeof (decimal), (r, n) => (decimal)ValueNonNull(r.ReadDecimal(n)) },
                { typeof (decimal?), (r, n) => (decimal?)r.ReadDecimal(n) },
                { typeof (decimal[]), (r, n) => ToArray(r.ReadArrayOfDecimal(n), x => (decimal)ValueNonNull(x)) },
                { typeof (decimal?[]), (r, n) => ToArrayOfNullable(r.ReadArrayOfDecimal(n), x => (decimal?)x) },

                { typeof (TimeSpan), (r, n) => (TimeSpan)ValueNonNull(r.ReadTime(n)) },
                { typeof (TimeSpan?), (r, n) => (TimeSpan?)r.ReadTime(n) },
                { typeof (TimeSpan[]), (r, n) => ToArray(r.ReadArrayOfTime(n), x => (TimeSpan)ValueNonNull(x)) },
                { typeof (TimeSpan?[]), (r, n) => ToArrayOfNullable(r.ReadArrayOfTime(n), x => (TimeSpan?)x) },

                { typeof (DateTime), (r, n) => (DateTime)ValueNonNull(r.ReadTimeStamp(n)) },
                { typeof (DateTime?), (r, n) => (DateTime?)r.ReadTimeStamp(n) },
                { typeof (DateTime[]), (r, n) => ToArray(r.ReadArrayOfTimeStamp(n), x => (DateTime)ValueNonNull(x)) },
                { typeof (DateTime?[]), (r, n) => ToArrayOfNullable(r.ReadArrayOfTimeStamp(n), x => (DateTime?)x) },

                { typeof (DateTimeOffset), (r, n) => (DateTimeOffset)ValueNonNull(r.ReadTimeStampWithTimeZone(n)) },
                { typeof (DateTimeOffset?), (r, n) => (DateTimeOffset?)r.ReadTimeStampWithTimeZone(n) },
                { typeof (DateTimeOffset[]), (r, n) => ToArray(r.ReadArrayOfTimeStampWithTimeZone(n), x => (DateTimeOffset)ValueNonNull(x)) },
                { typeof (DateTimeOffset?[]), (r, n) => ToArrayOfNullable(r.ReadArrayOfTimeStampWithTimeZone(n), x => (DateTimeOffset?)x) },

                { typeof (char), (r, n) => (char)r.ReadInt16(n) },
                { typeof (char?), (r, n) => (char?)r.ReadNullableInt16(n) },
                { typeof (char[]), (r, n) => ToArray(r.ReadArrayOfInt16(n), x => (char)(short)x) },
                { typeof (char?[]), (r, n) => ToArrayOfNullable(r.ReadArrayOfNullableInt16(n), x => (char?)(short?)x) },

#if NET6_0_OR_GREATER
                { typeof (TimeOnly), (r, n) => (TimeOnly)ValueNonNull(r.ReadTime(n)) },
                { typeof (TimeOnly?), (r, n) => (TimeOnly?)r.ReadTime(n) },
                { typeof (TimeOnly[]), (r, n) => ToArray(r.ReadArrayOfTime(n), x => (TimeOnly)ValueNonNull(x)) },
                { typeof (TimeOnly?[]), (r, n) => ToArrayOfNullable(r.ReadArrayOfTime(n), x => (TimeOnly?)x) },

                { typeof (DateOnly), (r, n) => (DateOnly)ValueNonNull(r.ReadDate(n)) },
                { typeof (DateOnly?), (r, n) => (DateOnly?)r.ReadDate(n) },
                { typeof (DateOnly[]), (r, n) => ToArray(r.ReadArrayOfDate(n), x => (DateOnly)ValueNonNull(x)) },
                { typeof (DateOnly?[]), (r, n) => ToArrayOfNullable(r.ReadArrayOfDate(n), x => (DateOnly?)x) },
#endif

                // do NOT remove nor alter the <generated></generated> lines!
                // <generated>

                { typeof (bool), (r, n) => (bool)r.ReadBoolean(n) },
                { typeof (sbyte), (r, n) => (sbyte)r.ReadInt8(n) },
                { typeof (short), (r, n) => (short)r.ReadInt16(n) },
                { typeof (int), (r, n) => (int)r.ReadInt32(n) },
                { typeof (long), (r, n) => (long)r.ReadInt64(n) },
                { typeof (float), (r, n) => (float)r.ReadFloat32(n) },
                { typeof (double), (r, n) => (double)r.ReadFloat64(n) },
                { typeof (bool[]), (r, n) => (bool[]?)r.ReadArrayOfBoolean(n) },
                { typeof (sbyte[]), (r, n) => (sbyte[]?)r.ReadArrayOfInt8(n) },
                { typeof (short[]), (r, n) => (short[]?)r.ReadArrayOfInt16(n) },
                { typeof (int[]), (r, n) => (int[]?)r.ReadArrayOfInt32(n) },
                { typeof (long[]), (r, n) => (long[]?)r.ReadArrayOfInt64(n) },
                { typeof (float[]), (r, n) => (float[]?)r.ReadArrayOfFloat32(n) },
                { typeof (double[]), (r, n) => (double[]?)r.ReadArrayOfFloat64(n) },
                { typeof (bool?), (r, n) => (bool?)r.ReadNullableBoolean(n) },
                { typeof (sbyte?), (r, n) => (sbyte?)r.ReadNullableInt8(n) },
                { typeof (short?), (r, n) => (short?)r.ReadNullableInt16(n) },
                { typeof (int?), (r, n) => (int?)r.ReadNullableInt32(n) },
                { typeof (long?), (r, n) => (long?)r.ReadNullableInt64(n) },
                { typeof (float?), (r, n) => (float?)r.ReadNullableFloat32(n) },
                { typeof (double?), (r, n) => (double?)r.ReadNullableFloat64(n) },
                { typeof (HBigDecimal?), (r, n) => (HBigDecimal?)r.ReadDecimal(n) },
                { typeof (string), (r, n) => (string?)r.ReadString(n) },
                { typeof (HLocalTime?), (r, n) => (HLocalTime?)r.ReadTime(n) },
                { typeof (HLocalDate?), (r, n) => (HLocalDate?)r.ReadDate(n) },
                { typeof (HLocalDateTime?), (r, n) => (HLocalDateTime?)r.ReadTimeStamp(n) },
                { typeof (HOffsetDateTime?), (r, n) => (HOffsetDateTime?)r.ReadTimeStampWithTimeZone(n) },
                { typeof (bool?[]), (r, n) => (bool?[]?)r.ReadArrayOfNullableBoolean(n) },
                { typeof (sbyte?[]), (r, n) => (sbyte?[]?)r.ReadArrayOfNullableInt8(n) },
                { typeof (short?[]), (r, n) => (short?[]?)r.ReadArrayOfNullableInt16(n) },
                { typeof (int?[]), (r, n) => (int?[]?)r.ReadArrayOfNullableInt32(n) },
                { typeof (long?[]), (r, n) => (long?[]?)r.ReadArrayOfNullableInt64(n) },
                { typeof (float?[]), (r, n) => (float?[]?)r.ReadArrayOfNullableFloat32(n) },
                { typeof (double?[]), (r, n) => (double?[]?)r.ReadArrayOfNullableFloat64(n) },
                { typeof (HBigDecimal?[]), (r, n) => (HBigDecimal?[]?)r.ReadArrayOfDecimal(n) },
                { typeof (HLocalTime?[]), (r, n) => (HLocalTime?[]?)r.ReadArrayOfTime(n) },
                { typeof (HLocalDate?[]), (r, n) => (HLocalDate?[]?)r.ReadArrayOfDate(n) },
                { typeof (HLocalDateTime?[]), (r, n) => (HLocalDateTime?[]?)r.ReadArrayOfTimeStamp(n) },
                { typeof (HOffsetDateTime?[]), (r, n) => (HOffsetDateTime?[]?)r.ReadArrayOfTimeStampWithTimeZone(n) },
                { typeof (string[]), (r, n) => (string?[]?)r.ReadArrayOfString(n) },

                // </generated>

// ReSharper restore RedundantCast
#pragma warning restore IDE0004

        };
}
