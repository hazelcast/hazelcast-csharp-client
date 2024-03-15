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

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Models;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;

namespace Hazelcast.Tests.Serialization.Compact;

public static class ReflectionDataSource
{
    private static Dictionary<Type, object?[]>? _typeValueMap;
    private static Dictionary<FieldKind, object?>? _kindValueMap;

    private static IGenericRecord _rec = new CompactDictionaryGenericRecord(SchemaBuilder.For("generic-record").Build(),
        new Dictionary<string, object?>());

    public static object? GetValueOfType(Type type)
    {
        if (type == typeof(IGenericRecord)) return _rec;
        if (type == typeof(IGenericRecord[])) return new[] { _rec };
        return TypeValueMap[type][0];
    }

    public static object? GetValueOfKind(FieldKind kind)
    {
        if (_kindValueMap == null)
        {
            _kindValueMap = new Dictionary<FieldKind, object?>();
            foreach (var (t, k) in TypeKindMap)
                if (!_kindValueMap.ContainsKey(k) && t != typeof(object) && t != typeof(object[]))
                    _kindValueMap[k] = GetValueOfType(t);
        }

        return _kindValueMap[kind];
    }

    public static Dictionary<Type, object?[]> TypeValueMap
        => _typeValueMap ??= TypeValueList
            .GroupBy(x => x.Item1)
            .ToDictionary(
                x => x.Key,
                x => x.Select(y => y.Item2).ToArray());

    public static readonly (Type, object?)[] TypeValueList =
    {
// ReSharper disable RedundantCast
#pragma warning disable IDE0004

        (typeof(bool), true),
        (typeof(bool), false),
        (typeof(bool?), true),
        (typeof(bool?), false),
        (typeof(bool?), null),
        (typeof(bool[]), new[] { true, false, false, true, false }),
        (typeof(bool[]), new[] { true }),
        (typeof(bool[]), new[] { false }),
        (typeof(bool[]), Array.Empty<bool>()),
        (typeof(bool[]), null),
        (typeof(bool?[]), new bool?[] { true, null, false, null, true }),
        (typeof(bool?[]), new bool?[] { true }),
        (typeof(bool?[]), new bool?[] { null }),
        (typeof(bool?[]), Array.Empty<bool?>()),
        (typeof(bool?[]), null),

        (typeof(sbyte), (sbyte) 64),
        (typeof(sbyte?), (sbyte) 64),
        (typeof(sbyte?), null),
        (typeof(sbyte[]), new sbyte[] { 1, 2, 3, 4 }),
        (typeof(sbyte[]), new sbyte[] { 1 }),
        (typeof(sbyte[]), Array.Empty<sbyte>()),
        (typeof(sbyte[]), null),
        (typeof(sbyte?[]), new sbyte?[] { 1, null, 3, null }),
        (typeof(sbyte?[]), new sbyte?[] { 1 }),
        (typeof(sbyte?[]), new sbyte?[] { null }),
        (typeof(sbyte?[]), Array.Empty<sbyte?>()),
        (typeof(sbyte?[]), null),

        (typeof(short), (short) 1234),
        (typeof(short?), (short) 1234),
        (typeof(short?), null),
        (typeof(short[]), new short[] { 1, 2, 3, 4 }),
        (typeof(short[]), new short[] { 1 }),
        (typeof(short[]), Array.Empty<short>()),
        (typeof(short[]), null),
        (typeof(short?[]), new short?[] { 1, null, 3, null }),
        (typeof(short?[]), new short?[] { 1 }),
        (typeof(short?[]), new short?[] { null }),
        (typeof(short?[]), Array.Empty<short?>()),
        (typeof(short?[]), null),

        (typeof(int), 123456),
        (typeof(int?), 123456),
        (typeof(int?), null),
        (typeof(int[]), new[] { 1, 2, 3, 4 }),
        (typeof(int[]), new[] { 1 }),
        (typeof(int[]), Array.Empty<int>()),
        (typeof(int[]), null),
        (typeof(int?[]), new int?[] { 1, null, 3, null }),
        (typeof(int?[]), new int?[] { 1 }),
        (typeof(int?[]), new int?[] { null }),
        (typeof(int?[]), Array.Empty<int?>()),
        (typeof(int?[]), null),

        (typeof(long), (long) int.MaxValue),
        (typeof(long?), 123456L),
        (typeof(long?), null),
        (typeof(long[]), new long[] { 1, 2, 3, 4 }),
        (typeof(long[]), new long[] { 1 }),
        (typeof(long[]), Array.Empty<long>()),
        (typeof(long[]), null),
        (typeof(long?[]), new long?[] { 1, null, 3, null }),
        (typeof(long?[]), new long?[] { 1 }),
        (typeof(long?[]), new long?[] { null }),
        (typeof(long?[]), Array.Empty<long?>()),
        (typeof(long?[]), null),

        (typeof(float), 1.2345f),
        (typeof(float?), 1.2345f),
        (typeof(float?), null),
        (typeof(float[]), new[] { 1.2f, 2.3f, 3.4f, 4.5f }),
        (typeof(float[]), new[] { 1.2f }),
        (typeof(float[]), Array.Empty<float>()),
        (typeof(float[]), null),
        (typeof(float?[]), new float?[] { 1.2f, null, 3.4f, null }),
        (typeof(float?[]), new float?[] { 1.2f }),
        (typeof(float?[]), new float?[] { null }),
        (typeof(float?[]), Array.Empty<float?>()),
        (typeof(float?[]), null),

        (typeof(double), 1.2345d),
        (typeof(double?), 1.2345d),
        (typeof(double?), null),
        (typeof(double[]), new[] { 1.2d, 2.3d, 3.4d, 4.5d }),
        (typeof(double[]), new[] { 1.2d }),
        (typeof(double[]), Array.Empty<double>()),
        (typeof(double[]), null),
        (typeof(double?[]), new double?[] { 1.2d, null, 3.4d, null }),
        (typeof(double?[]), new double?[] { 1.2d }),
        (typeof(double?[]), new double?[] { null }),
        (typeof(double?[]), Array.Empty<double?>()),
        (typeof(double?[]), null),

        (typeof(string), "hello"),
        (typeof(string), null),
        (typeof(string[]), new[] { "hello", null, "world", null }),
        (typeof(string[]), new[] { "hello" }),
        (typeof(string[]), Array.Empty<string>()),
        (typeof(string[]), null),

        (typeof(HBigDecimal), new HBigDecimal(1.2m)),
        (typeof(HBigDecimal?), new HBigDecimal(1.2m)),
        (typeof(HBigDecimal?), null),
        (typeof(HBigDecimal[]), new[] { new HBigDecimal(1.2m), new HBigDecimal(2.3m), new HBigDecimal(3.4m), new HBigDecimal(4.5m) }),
        (typeof(HBigDecimal[]), new[] { new HBigDecimal(1.2m) }),
        (typeof(HBigDecimal[]), Array.Empty<HBigDecimal>()),
        (typeof(HBigDecimal[]), null),
        (typeof(HBigDecimal?[]), new HBigDecimal?[] { new HBigDecimal(1.2m), null, new HBigDecimal(3.4m), null }),
        (typeof(HBigDecimal?[]), new HBigDecimal?[] { new HBigDecimal(1.2m) }),
        (typeof(HBigDecimal?[]), new HBigDecimal?[] { null }),
        (typeof(HBigDecimal?[]), Array.Empty<HBigDecimal?>()),
        (typeof(HBigDecimal?[]), null),

        (typeof(HLocalTime), new HLocalTime(1, 2, 3, 4)),
        (typeof(HLocalTime?), new HLocalTime(1, 2, 3, 4)),
        (typeof(HLocalTime?), null),
        (typeof(HLocalTime[]), new[] { new HLocalTime(1, 2, 3, 4), new HLocalTime(5, 6, 7, 8), new HLocalTime(1, 2, 3, 4), new HLocalTime(5, 6, 7, 8) }),
        (typeof(HLocalTime[]), new[] { new HLocalTime(1, 2, 3, 4) }),
        (typeof(HLocalTime[]), Array.Empty<HLocalTime>()),
        (typeof(HLocalTime[]), null),
        (typeof(HLocalTime?[]), new HLocalTime?[] { new HLocalTime(1, 2, 3, 4), null, new HLocalTime(5, 6, 7, 8), null }),
        (typeof(HLocalTime?[]), new HLocalTime?[] { new HLocalTime(1, 2, 3, 4) }),
        (typeof(HLocalTime?[]), new HLocalTime?[] { null }),
        (typeof(HLocalTime?[]), Array.Empty<HLocalTime?>()),
        (typeof(HLocalTime?[]), null),

        (typeof(HLocalDate), new HLocalDate(1, 2, 3)),
        (typeof(HLocalDate?), new HLocalDate(1, 2, 3)),
        (typeof(HLocalDate?), null),
        (typeof(HLocalDate[]), new[] { new HLocalDate(1, 2, 3), new HLocalDate(5, 6, 7), new HLocalDate(1, 2, 3), new HLocalDate(5, 6, 7) }),
        (typeof(HLocalDate[]), new[] { new HLocalDate(1, 2, 3) }),
        (typeof(HLocalDate[]), Array.Empty<HLocalDate>()),
        (typeof(HLocalDate[]), null),
        (typeof(HLocalDate?[]), new HLocalDate?[] { new HLocalDate(1, 2, 3), null, new HLocalDate(5, 6, 7), null }),
        (typeof(HLocalDate?[]), new HLocalDate?[] { new HLocalDate(1, 2, 3) }),
        (typeof(HLocalDate?[]), new HLocalDate?[] { null }),
        (typeof(HLocalDate?[]), Array.Empty<HLocalDate?>()),
        (typeof(HLocalDate?[]), null),

        (typeof(HLocalDateTime), new HLocalDateTime(1, 2, 3, 4, 5, 6, 7)),
        (typeof(HLocalDateTime?), new HLocalDateTime(1, 2, 3, 4, 5, 6, 7)),
        (typeof(HLocalDateTime?), null),
        (typeof(HLocalDateTime[]), new[] { new HLocalDateTime(1, 2, 3, 4, 5, 6, 7), new HLocalDateTime(5, 6, 7), new HLocalDateTime(1, 2, 3), new HLocalDateTime(5, 6, 7) }),
        (typeof(HLocalDateTime[]), new[] { new HLocalDateTime(1, 2, 3, 4, 5, 6, 7) }),
        (typeof(HLocalDateTime[]), Array.Empty<HLocalDateTime>()),
        (typeof(HLocalDateTime[]), null),
        (typeof(HLocalDateTime?[]), new HLocalDateTime?[] { new HLocalDateTime(1, 2, 3, 4, 5, 6, 7), null, new HLocalDateTime(5, 6, 7), null }),
        (typeof(HLocalDateTime?[]), new HLocalDateTime?[] { new HLocalDateTime(1, 2, 3, 4, 5, 6, 7) }),
        (typeof(HLocalDateTime?[]), new HLocalDateTime?[] { null }),
        (typeof(HLocalDateTime?[]), Array.Empty<HLocalDateTime?>()),
        (typeof(HLocalDateTime?[]), null),

        (typeof(HOffsetDateTime), new HLocalDateTime(1, 2, 3, 4, 5, 6, 7).Offset(1234)),
        (typeof(HOffsetDateTime?), new HLocalDateTime(1, 2, 3, 4, 5, 6, 7).Offset(1234)),
        (typeof(HOffsetDateTime?), null),
        (typeof(HOffsetDateTime[]), new[] { new HLocalDateTime(1, 2, 3, 4, 5, 6, 7).Offset(1234), new HLocalDateTime(5, 6, 7).Offset(5678), new HLocalDateTime(1, 2, 3).Offset(1234), new HLocalDateTime(5, 6, 7).Offset(5678) }),
        (typeof(HOffsetDateTime[]), new[] { new HLocalDateTime(1, 2, 3, 4, 5, 6, 7).Offset(1234) }),
        (typeof(HOffsetDateTime[]), Array.Empty<HOffsetDateTime>()),
        (typeof(HOffsetDateTime[]), null),
        (typeof(HOffsetDateTime?[]), new HOffsetDateTime?[] { new HLocalDateTime(1, 2, 3, 4, 5, 6, 7).Offset(1234), null, new HLocalDateTime(5, 6, 7).Offset(5678), null }),
        (typeof(HOffsetDateTime?[]), new HOffsetDateTime?[] { new HLocalDateTime(1, 2, 3, 4, 5, 6, 7).Offset(1234) }),
        (typeof(HOffsetDateTime?[]), new HOffsetDateTime?[] { null }),
        (typeof(HOffsetDateTime?[]), Array.Empty<HOffsetDateTime?>()),
        (typeof(HOffsetDateTime?[]), null),

        (typeof(decimal), 1.2345m),
        (typeof(decimal?), 1.2345m),
        (typeof(decimal?), null),
        (typeof(decimal[]), new[] { 1.2m, 2.3m, 3.4m, 4.5m }),
        (typeof(decimal[]), new[] { 1.2m }),
        (typeof(decimal[]), Array.Empty<decimal>()),
        (typeof(decimal[]), null),
        (typeof(decimal?[]), new decimal?[] { 1.2m, null, 3.4m, null }),
        (typeof(decimal?[]), new decimal?[] { 1.2m }),
        (typeof(decimal?[]), new decimal?[] { null }),
        (typeof(decimal?[]), Array.Empty<decimal?>()),
        (typeof(decimal?[]), null),

        (typeof(TimeSpan), new TimeSpan(0, 2, 3, 4)),
        (typeof(TimeSpan?), new TimeSpan(0, 2, 3, 4)),
        (typeof(TimeSpan?), null),
        (typeof(TimeSpan[]), new[] { new TimeSpan(0, 2, 3, 4), new TimeSpan(0, 6, 7, 8), new TimeSpan(0, 2, 3, 4), new TimeSpan(0, 6, 7, 8) }),
        (typeof(TimeSpan[]), new[] { new TimeSpan(0, 2, 3, 4) }),
        (typeof(TimeSpan[]), Array.Empty<TimeSpan>()),
        (typeof(TimeSpan[]), null),
        (typeof(TimeSpan?[]), new TimeSpan?[] { new TimeSpan(0, 2, 3, 4), null, new TimeSpan(0, 6, 7, 8), null }),
        (typeof(TimeSpan?[]), new TimeSpan?[] { new TimeSpan(0, 2, 3, 4) }),
        (typeof(TimeSpan?[]), new TimeSpan?[] { null }),
        (typeof(TimeSpan?[]), Array.Empty<TimeSpan?>()),
        (typeof(TimeSpan?[]), null),

        (typeof(DateTime), new DateTime(1, 2, 3)),
        (typeof(DateTime?), new DateTime(1, 2, 3)),
        (typeof(DateTime?), null),
        (typeof(DateTime[]), new[] { new DateTime(1, 2, 3), new DateTime(5, 6, 7), new DateTime(1, 2, 3), new DateTime(5, 6, 7) }),
        (typeof(DateTime[]), new[] { new DateTime(1, 2, 3) }),
        (typeof(DateTime[]), Array.Empty<DateTime>()),
        (typeof(DateTime[]), null),
        (typeof(DateTime?[]), new DateTime?[] { new DateTime(1, 2, 3), null, new DateTime(5, 6, 7), null }),
        (typeof(DateTime?[]), new DateTime?[] { new DateTime(1, 2, 3) }),
        (typeof(DateTime?[]), new DateTime?[] { null }),
        (typeof(DateTime?[]), Array.Empty<DateTime?>()),
        (typeof(DateTime?[]), null),

        (typeof(DateTimeOffset), new DateTimeOffset(1, 2, 3, 1, 2, 3, TimeSpan.Zero)),
        (typeof(DateTimeOffset?), new DateTimeOffset(1, 2, 3, 1, 2, 3, TimeSpan.FromMinutes(4))),
        (typeof(DateTimeOffset?), null),
        (typeof(DateTimeOffset[]), new[] { new DateTimeOffset(1, 2, 3, 1, 2, 3, TimeSpan.FromMinutes(4)), new DateTimeOffset(5, 6, 7, 1, 2, 3, TimeSpan.FromMinutes(4)), new DateTimeOffset(1, 2, 3, 1, 2, 3, TimeSpan.FromMinutes(4)), new DateTimeOffset(5, 6, 7, 1, 2, 3, TimeSpan.FromMinutes(4)) }),
        (typeof(DateTimeOffset[]), new[] { new DateTimeOffset(1, 2, 3, 1, 2, 3, TimeSpan.FromMinutes(789)) }),
        (typeof(DateTimeOffset[]), Array.Empty<DateTimeOffset>()),
        (typeof(DateTimeOffset[]), null),
        (typeof(DateTimeOffset?[]), new DateTimeOffset?[] { new DateTimeOffset(1, 2, 3, 1, 2, 3, TimeSpan.FromMinutes(4)), null, new DateTimeOffset(5, 6, 7, 1, 2, 3, TimeSpan.FromMinutes(4)), null }),
        (typeof(DateTimeOffset?[]), new DateTimeOffset?[] { new DateTimeOffset(1, 2, 3, 1, 2, 3, TimeSpan.FromMinutes(4)) }),
        (typeof(DateTimeOffset?[]), new DateTimeOffset?[] { null }),
        (typeof(DateTimeOffset?[]), Array.Empty<DateTimeOffset?>()),
        (typeof(DateTimeOffset?[]), null),

#if NET6_0_OR_GREATER
            (typeof(TimeOnly), new TimeOnly(1, 2, 3, 4)),
            (typeof(TimeOnly?), new TimeOnly(1, 2, 3, 4)),
            (typeof(TimeOnly?), null),
            (typeof(TimeOnly[]), new[] { new TimeOnly(1, 2, 3, 4), new TimeOnly(5, 6, 7, 8), new TimeOnly(1, 2, 3, 4), new TimeOnly(5, 6, 7, 8) }),
            (typeof(TimeOnly[]), new[] { new TimeOnly(1, 2, 3, 4) }),
            (typeof(TimeOnly[]), Array.Empty<TimeOnly>()),
            (typeof(TimeOnly[]), null),
            (typeof(TimeOnly?[]), new TimeOnly?[] { new TimeOnly(1, 2, 3, 4), null, new TimeOnly(5, 6, 7, 8), null }),
            (typeof(TimeOnly?[]), new TimeOnly?[] { new TimeOnly(1, 2, 3, 4) }),
            (typeof(TimeOnly?[]), new TimeOnly?[] { null }),
            (typeof(TimeOnly?[]), Array.Empty<TimeOnly?>()),
            (typeof(TimeOnly?[]), null),

            (typeof(DateOnly), new DateOnly(1, 2, 3)),
            (typeof(DateOnly?), new DateOnly(1, 2, 3)),
            (typeof(DateOnly?), null),
            (typeof(DateOnly[]), new[] { new DateOnly(1, 2, 3), new DateOnly(5, 6, 7), new DateOnly(1, 2, 3), new DateOnly(5, 6, 7) }),
            (typeof(DateOnly[]), new[] { new DateOnly(1, 2, 3) }),
            (typeof(DateOnly[]), Array.Empty<DateOnly>()),
            (typeof(DateOnly[]), null),
            (typeof(DateOnly?[]), new DateOnly?[] { new DateOnly(1, 2, 3), null, new DateOnly(5, 6, 7), null }),
            (typeof(DateOnly?[]), new DateOnly?[] { new DateOnly(1, 2, 3) }),
            (typeof(DateOnly?[]), new DateOnly?[] { null }),
            (typeof(DateOnly?[]), Array.Empty<DateOnly?>()),
            (typeof(DateOnly?[]), null),
#endif

        (typeof(SomeClass), new SomeClass { Value = 42 }),
        (typeof(SomeClass), null),
        (typeof(SomeClass[]), new[] { new SomeClass { Value = 42 }, null, new SomeClass { Value = 24 }, null }),
        (typeof(SomeClass[]), new[] { new SomeClass { Value = 42 } }),
        (typeof(SomeClass[]), Array.Empty<SomeClass>()),
        (typeof(SomeClass[]), null),

        (typeof(SomeClass2), new SomeClass2 { Value = new SomeClass { Value = 42 } }),
        (typeof(SomeClass2), new SomeClass2 { Value = null }),

        (typeof(SomeStruct), new SomeStruct { Value = 42 }),
        (typeof(SomeStruct?), new SomeStruct { Value = 42 }),
        (typeof(SomeStruct?), null),
        (typeof(SomeStruct?[]), new SomeStruct?[] { new SomeStruct { Value = 42 }, null, new SomeStruct { Value = 24 }, null }),
        (typeof(SomeStruct[]), new[] { new SomeStruct { Value = 42 } }),
        (typeof(SomeStruct[]), Array.Empty<SomeStruct>()),
        (typeof(SomeStruct?[]), new SomeStruct?[] { new SomeStruct { Value = 42 } }),
        (typeof(SomeStruct?[]), Array.Empty<SomeStruct?>()),
        (typeof(SomeStruct[]), null),
        (typeof(SomeStruct?[]), null),

        (typeof(SomeStruct2N), new SomeStruct2N { Value = new SomeStruct { Value = 42 } }),
        (typeof(SomeStruct2N), new SomeStruct2N { Value = null }),

        (typeof (char), 'a'),
        (typeof (char?), 'a'),
        (typeof (char?), null),
        (typeof (char[]), new[] { 'a', 'b' }),
        (typeof (char[]), new[] { 'a' }),
        (typeof (char[]), Array.Empty<char>()),
        (typeof (char[]), null),
        (typeof (char?[]), new char?[] { 'a', 'b' }),
        (typeof (char?[]), new char?[] { 'a' }),
        (typeof (char?[]), new char?[] { null }),
        (typeof (char?[]), new char?[] { 'a', null }),
        (typeof (char?[]), Array.Empty<char?>()),
        (typeof (char?[]), null),

        (typeof (SByteEnum), SByteEnum.A),
        (typeof (SByteEnum?), SByteEnum.A),
        (typeof (SByteEnum?), null),
        (typeof (SByteEnum[]), new[] { SByteEnum.A, SByteEnum.B }),
        (typeof (SByteEnum[]), new[] { SByteEnum.A }),
        (typeof (SByteEnum[]), Array.Empty<SByteEnum>()),
        (typeof (SByteEnum[]), null),
        (typeof (SByteEnum?[]), new SByteEnum?[] { SByteEnum.A, SByteEnum.B }),
        (typeof (SByteEnum?[]), new SByteEnum?[] { SByteEnum.A, null }),
        (typeof (SByteEnum?[]), new SByteEnum?[] { SByteEnum.A }),
        (typeof (SByteEnum?[]), Array.Empty<SByteEnum?>()),
        (typeof (SByteEnum?[]), null),

        (typeof (ByteEnum), ByteEnum.A),
        (typeof (ShortEnum), ShortEnum.A),
        (typeof (UShortEnum), UShortEnum.A),
        (typeof (IntEnum), IntEnum.A),
        (typeof (UIntEnum), UIntEnum.A),
        (typeof (LongEnum), LongEnum.A),
        (typeof (ULongEnum), ULongEnum.A),

        // ReSharper enable RedundantCast
#pragma warning restore IDE0004
    };

    public static readonly (Type, FieldKind)[] TypeKindMap =
    {
        (typeof (bool), FieldKind.Boolean),
        (typeof (sbyte), FieldKind.Int8),
        (typeof (short), FieldKind.Int16),
        (typeof (int), FieldKind.Int32),
        (typeof (long), FieldKind.Int64),
        (typeof (float), FieldKind.Float32),
        (typeof (double), FieldKind.Float64),

        (typeof (bool[]), FieldKind.ArrayOfBoolean),
        (typeof (sbyte[]), FieldKind.ArrayOfInt8),
        (typeof (short[]), FieldKind.ArrayOfInt16),
        (typeof (int[]), FieldKind.ArrayOfInt32),
        (typeof (long[]), FieldKind.ArrayOfInt64),
        (typeof (float[]), FieldKind.ArrayOfFloat32),
        (typeof (double[]), FieldKind.ArrayOfFloat64),

        (typeof (bool?), FieldKind.NullableBoolean),
        (typeof (sbyte?), FieldKind.NullableInt8),
        (typeof (short?), FieldKind.NullableInt16),
        (typeof (int?), FieldKind.NullableInt32),
        (typeof (long?), FieldKind.NullableInt64),
        (typeof (float?), FieldKind.NullableFloat32),
        (typeof (double?), FieldKind.NullableFloat64),

        (typeof (bool?[]), FieldKind.ArrayOfNullableBoolean),
        (typeof (sbyte?[]), FieldKind.ArrayOfNullableInt8),
        (typeof (short?[]), FieldKind.ArrayOfNullableInt16),
        (typeof (int?[]), FieldKind.ArrayOfNullableInt32),
        (typeof (long?[]), FieldKind.ArrayOfNullableInt64),
        (typeof (float?[]), FieldKind.ArrayOfNullableFloat32),
        (typeof (double?[]), FieldKind.ArrayOfNullableFloat64),

        (typeof (HBigDecimal?), FieldKind.Decimal),
        (typeof (HLocalTime?), FieldKind.Time),
        (typeof (HLocalDate?), FieldKind.Date),
        (typeof (HLocalDateTime?), FieldKind.TimeStamp),
        (typeof (HOffsetDateTime?), FieldKind.TimeStampWithTimeZone),

        (typeof (HBigDecimal?[]), FieldKind.ArrayOfDecimal),
        (typeof (HLocalTime?[]), FieldKind.ArrayOfTime),
        (typeof (HLocalDate?[]), FieldKind.ArrayOfDate),
        (typeof (HLocalDateTime?[]), FieldKind.ArrayOfTimeStamp),
        (typeof (HOffsetDateTime?[]), FieldKind.ArrayOfTimeStampWithTimeZone),

        (typeof (string), FieldKind.String),
        (typeof (string[]), FieldKind.ArrayOfString),

        (typeof (object), FieldKind.Compact),
        (typeof (object[]), FieldKind.ArrayOfCompact),

        // a few types are special

        (typeof (HBigDecimal), FieldKind.Decimal),
        (typeof (HLocalTime), FieldKind.Time),
        (typeof (HLocalDate), FieldKind.Date),
        (typeof (HLocalDateTime), FieldKind.TimeStamp),
        (typeof (HOffsetDateTime), FieldKind.TimeStampWithTimeZone),

        (typeof (HBigDecimal[]), FieldKind.ArrayOfDecimal),
        (typeof (HLocalTime[]), FieldKind.ArrayOfTime),
        (typeof (HLocalDate[]), FieldKind.ArrayOfDate),
        (typeof (HLocalDateTime[]), FieldKind.ArrayOfTimeStamp),
        (typeof (HOffsetDateTime[]), FieldKind.ArrayOfTimeStampWithTimeZone),

        (typeof (decimal), FieldKind.Decimal),
        (typeof (decimal[]), FieldKind.ArrayOfDecimal),
        (typeof (decimal?), FieldKind.Decimal),
        (typeof (decimal?[]), FieldKind.ArrayOfDecimal),

        (typeof (TimeSpan), FieldKind.Time),
        (typeof (TimeSpan[]), FieldKind.ArrayOfTime),
        (typeof (TimeSpan?), FieldKind.Time),
        (typeof (TimeSpan?[]), FieldKind.ArrayOfTime),

        (typeof (DateTime), FieldKind.TimeStamp),
        (typeof (DateTime[]), FieldKind.ArrayOfTimeStamp),
        (typeof (DateTime?), FieldKind.TimeStamp),
        (typeof (DateTime?[]), FieldKind.ArrayOfTimeStamp),

        (typeof (DateTimeOffset), FieldKind.TimeStampWithTimeZone),
        (typeof (DateTimeOffset[]), FieldKind.ArrayOfTimeStampWithTimeZone),
        (typeof (DateTimeOffset?), FieldKind.TimeStampWithTimeZone),
        (typeof (DateTimeOffset?[]), FieldKind.ArrayOfTimeStampWithTimeZone),

        (typeof (char), FieldKind.Int16),
        (typeof (char?), FieldKind.NullableInt16),
        (typeof (char[]), FieldKind.ArrayOfInt16),
        (typeof (char?[]), FieldKind.ArrayOfNullableInt16),

        (typeof (SByteEnum), FieldKind.String),
        (typeof (ByteEnum), FieldKind.String),
        (typeof (ShortEnum), FieldKind.String),
        (typeof (UShortEnum), FieldKind.String),
        (typeof (IntEnum), FieldKind.String),
        (typeof (UIntEnum), FieldKind.String),
        (typeof (LongEnum), FieldKind.String),
        (typeof (ULongEnum), FieldKind.String),

        (typeof (SByteEnum?), FieldKind.String),
        (typeof (ByteEnum?), FieldKind.String),
        (typeof (ShortEnum?), FieldKind.String),
        (typeof (UShortEnum?), FieldKind.String),
        (typeof (IntEnum?), FieldKind.String),
        (typeof (UIntEnum?), FieldKind.String),
        (typeof (LongEnum?), FieldKind.String),
        (typeof (ULongEnum?), FieldKind.String),

        (typeof (SByteEnum[]), FieldKind.ArrayOfString),
        (typeof (ByteEnum[]), FieldKind.ArrayOfString),
        (typeof (ShortEnum[]), FieldKind.ArrayOfString),
        (typeof (UShortEnum[]), FieldKind.ArrayOfString),
        (typeof (IntEnum[]), FieldKind.ArrayOfString),
        (typeof (UIntEnum[]), FieldKind.ArrayOfString),
        (typeof (LongEnum[]), FieldKind.ArrayOfString),
        (typeof (ULongEnum[]), FieldKind.ArrayOfString),

        (typeof (SByteEnum?[]), FieldKind.ArrayOfString),
        (typeof (ByteEnum?[]), FieldKind.ArrayOfString),
        (typeof (ShortEnum?[]), FieldKind.ArrayOfString),
        (typeof (UShortEnum?[]), FieldKind.ArrayOfString),
        (typeof (IntEnum?[]), FieldKind.ArrayOfString),
        (typeof (UIntEnum?[]), FieldKind.ArrayOfString),
        (typeof (LongEnum?[]), FieldKind.ArrayOfString),
        (typeof (ULongEnum?[]), FieldKind.ArrayOfString),

#if NET6_0_OR_GREATER
        (typeof (TimeOnly), FieldKind.Time),
        (typeof (TimeOnly[]), FieldKind.ArrayOfTime),
        (typeof (TimeOnly?), FieldKind.Time),
        (typeof (TimeOnly?[]), FieldKind.ArrayOfTime),
        (typeof (DateOnly), FieldKind.Date),
        (typeof (DateOnly[]), FieldKind.ArrayOfDate),
        (typeof (DateOnly?), FieldKind.Date),
        (typeof (DateOnly?[]), FieldKind.ArrayOfDate),
#endif

        // all other types (including value types) all are NullableCompact
        // including value types (structs) - and their nullable equivalents

        (typeof (SomeClass), FieldKind.Compact),
        (typeof (SomeClass[]), FieldKind.ArrayOfCompact),
        (typeof (SomeStruct), FieldKind.Compact),
        (typeof (SomeStruct[]), FieldKind.ArrayOfCompact),
        (typeof (SomeStruct?), FieldKind.Compact),
        (typeof (SomeStruct?[]), FieldKind.ArrayOfCompact),
        (typeof (ISomeInterface), FieldKind.Compact),
        (typeof (ISomeInterface[]), FieldKind.ArrayOfCompact),
    };

    public class SomeClass
    {
        public int Value { get; set; }

        public override string ToString() => $"SomeClass(Value={Value})";
    }

    public class SomeClass2
    {
        public SomeClass? Value { get; set; }

        public override string ToString() => $"SomeClass2(Value={Value})";
    }

    public class SomeExtend : SomeClass
    {
        public int Other { get; set; }

        public override string ToString() => $"SomeExtend(Value={Value}, Other={Other})";
    }

    public struct SomeStruct
    {
        public int Value { get; set; }

        public override string ToString() => $"SomeStruct(Value={Value})";
    }

    public struct SomeStruct2
    {
        public SomeStruct Value { get; set; }

        public override string ToString() => $"SomeStruct2(Value={Value})";
    }

    public struct SomeStruct2N
    {
        public SomeStruct? Value { get; set; }

        public override string ToString() => $"SomeStruct2(Value={Value})";
    }

    public class PoisonClass1
    {
        private static bool _throw = true;

        public PoisonClass1()
        {
            if (_throw) throw new Exception("bang");
        }

        public string? Value { get; set; }

        public static PoisonClass1 CreateInstance(string value)
        {
            _throw = false;
            var instance = new PoisonClass1();
            instance.Value = value;
            _throw = true;
            return instance;
        }
    }

    public class PoisonClass2
    {
        private static bool _throw = true;

        // ReSharper disable once InconsistentNaming
        public PoisonClass2(string Value)
        {
            if (_throw) throw new Exception("bang");
            this.Value = Value;
        }

        public string? Value { get; set; }

        public static PoisonClass2 CreateInstance(string value)
        {
            _throw = false;
            var instance = new PoisonClass2(value);
            _throw = true;
            return instance;
        }
    }

    public interface ISomeInterface { }

    public enum SByteEnum : sbyte { A = 1, B = 2 }
    public enum ByteEnum : byte { A = 1, B = 2 }
    public enum ShortEnum : short { A = 1, B = 2 }
    public enum UShortEnum : ushort { A = 1, B = 2 }
    public enum IntEnum : int { A = 1, B = 2 }
    public enum UIntEnum : uint { A = 1, B = 2 }
    public enum LongEnum : long { A = 1, B = 2 }
    public enum ULongEnum : ulong { A = 1, B = 2 }
}
