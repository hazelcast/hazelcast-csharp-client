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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Moq;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    [TestFixture]
    public class ReflectionSerializerTests
    {
        private static readonly (Type, FieldKind)[] GenerateSchemaSource =
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

        // this test ensures that all FieldKind values are tested in GenerateSchema
        //
        [Test]
        public void ValidateGenerateSchemaSource()
        {
            // get all enum values
            var kindValues = new HashSet<FieldKind>(Enum.GetValues(typeof (FieldKind)).Cast<FieldKind>());

            // remove those that don't make sense here or are not supported by compact serialization
            kindValues.Remove(FieldKind.NotAvailable);

            // remove those that are tested
            foreach (var (_, kind) in GenerateSchemaSource) kindValues.Remove(kind);

            // nothing should remain
            Assert.That(kindValues.Count, Is.Zero, $"Untested values: {string.Join(", ", kindValues)}");
        }

        // this test ensures that ReflectionSerializer + SchemaBuilderWriter generate the correct schema
        //
        [TestCaseSource(nameof(GenerateSchemaSource))]
        public void GenerateSchema((Type PropertyType, FieldKind ExpectedFieldKind) testCase)
        {
            var type = ReflectionHelper.CreateObjectType(testCase.PropertyType);
            var obj = Activator.CreateInstance(type);
            if (obj == null) throw new Exception("panic: null object.");
            var serializer = new ReflectionSerializer();
            var sw = new SchemaBuilderWriter("thing");
            serializer.Write(sw, obj);
            var schema = sw.Build();

            Assert.That(schema.TypeName, Is.EqualTo("thing"));
            Assert.That(schema.Fields.Count, Is.EqualTo(1));
            Assert.That(schema.Fields[0].FieldName, Is.EqualTo("Value0"));
            Assert.That(schema.Fields[0].Kind, Is.EqualTo(testCase.ExpectedFieldKind));
        }

        // this test ensures that SchemaBuilderWriter throws the right exceptions.
        [Test]
        public void SchemaBuilderWriterExceptions()
        {
            var sw = new SchemaBuilderWriter("thing");
            sw.WriteBoolean("foo", false);
            Assert.Throws<SerializationException>(() => sw.WriteBoolean("foo", false));
            Assert.Throws<SerializationException>(() => sw.WriteInt8("foo", 0));
            sw.WriteBoolean("FOO", false); // is case-sensitive
        }

        private static readonly (Type, object?)[] SerializeSource = 
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

        // this ensures that ReflectionSerializer + SchemaBuilderWriter can write then read primitive types
        //
        [TestCaseSource(nameof(SerializeSource))]
        public void SerializeOne((Type PropertyType, object? PropertyValue) testCase)
        {
            Console.Write("Value: ");
            if (testCase.PropertyValue is Array arrayValue)
            {
                Console.Write("[");
                var first = true;
                foreach (var element in arrayValue)
                {
                    if (first) first = false; else Console.Write(",");
                    Console.Write(element?.ToString() ?? "<null>");
                }
                Console.Write("]");
            }
            else
            {
                Console.Write(testCase.PropertyValue?.ToString() ?? "<null>");
            }
            Console.WriteLine();

            var type = ReflectionHelper.CreateObjectType(testCase.PropertyType);
            var obj = Activator.CreateInstance(type);
            if (obj == null) throw new Exception("panic: null obj");
            ReflectionHelper.SetPropertyValue(obj, "Value0", testCase.PropertyValue);

            var serializer = new ReflectionSerializer();
            var sw = new SchemaBuilderWriter("thing");
            serializer.Write(sw, obj);
            var schema = sw.Build();

            var orw = new ObjectReaderWriter(serializer);
            
            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            serializer.Write(writer, obj);
            writer.Complete();

            var buffer = output.ToByteArray();
            Console.WriteLine(buffer.Dump());

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, obj.GetType());

            var obj2 = serializer.Read(reader);
            Assert.That(obj2, Is.Not.Null);
            Assert.That(obj2.GetType(), Is.EqualTo(type));

            AssertPropertyValue(testCase.PropertyValue, ReflectionHelper.GetPropertyValue(obj2, "Value0"));
        }

        private void AssertPropertyValue(object? value, object? value2)
        {
            switch (value)
            {
                case null:
                    Assert.That(value2, Is.Null);
                    break;

                case SomeClass o:
                    {
                        Assert.That(value2, Is.Not.Null);
                        Assert.That(value2, Is.InstanceOf<SomeClass>());
                        var o2 = (SomeClass)value2!;
                        Assert.That(o.Value, Is.EqualTo(o2.Value));
                        break;
                    }

                case SomeClass2 o:
                    {
                        Assert.That(value2, Is.Not.Null);
                        Assert.That(value2, Is.InstanceOf<SomeClass2>());
                        var o2 = (SomeClass2)value2!;
                        if (o.Value == null)
                        {
                            Assert.That(o2.Value, Is.Null);
                        }
                        else
                        {
                            Assert.That(o2.Value, Is.Not.Null);
                            Assert.That(o2.Value!.Value, Is.EqualTo(o.Value.Value));
                        }
                        break;
                    }

                case SomeStruct o:
                    {
                        Assert.That(value2, Is.Not.Null);
                        Assert.That(value2, Is.InstanceOf<SomeStruct>());
                        var o2 = (SomeStruct)value2!;
                        Assert.That(o.Value, Is.EqualTo(o2.Value));
                        break;
                    }

                case Array array when value2 is Array array2:
                    {
                        Assert.That(array2.Rank, Is.EqualTo(array.Rank));
                        Assert.That(array2.Length, Is.EqualTo(array.Length));
                        for (var i = 0; i < array.Length; i++)
                            AssertPropertyValue(array.GetValue(i), array2.GetValue(i));
                        break;
                    }

                case Array:
                    Assert.That(value2 is Array); // fail
                    break;

                default:
                    Assert.That(value2, Is.EqualTo(value));
                    break;
            }
        }

        [Test]
        public void SerializeBooleans()
        {
            var propertyTypes = new Type[32];
            for (var i = 0; i < propertyTypes.Length; i++) propertyTypes[i] = typeof (bool);
            var type = ReflectionHelper.CreateObjectType(propertyTypes);
            var obj = Activator.CreateInstance(type);
            if (obj == null) throw new Exception("panic: null obj");
            var properties = new PropertyInfo[propertyTypes.Length];
            for (var i = 0; i < propertyTypes.Length; i++)
            {
                var property = type.GetProperty($"Value{i}");
                Assert.That(property, Is.Not.Null);
                properties[i] = property!;
                try
                {
                    property!.SetValue(obj, i % 3 == 0);
                }
                catch
                {
                    Assert.Fail($"Failed to assign value to property.");
                }
            }

            var serializer = new ReflectionSerializer();
            var sw = new SchemaBuilderWriter("thing");
            serializer.Write(sw, obj);
            var schema = sw.Build();

            var orw = new ObjectReaderWriter(serializer);

            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            serializer.Write(writer, obj);
            writer.Complete();

            var buffer = output.ToByteArray();
            Console.WriteLine(buffer.Dump());

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, obj.GetType());

            var obj2 = serializer.Read(reader);
            Assert.That(obj2, Is.Not.Null);
            Assert.That(obj2.GetType(), Is.EqualTo(type));

            for (var i = 0; i < propertyTypes.Length; i++)
            {
                Assert.That(properties[i].GetValue(obj2), Is.EqualTo(properties[i].GetValue(obj)));
            }
        }

        [Test]
        public void SerializeMany()
        {
            const int repeat = 100;
            const int count = 10; // 10 properties

            var random = new Random();

            for (var i = 0; i < repeat; i++)
                SerializeMultiple(count, random, SerializeSource);

            // push to short offsets
            SerializeMultiple(byte.MaxValue * 2, random, SerializeSource);
            // actually *not* possible - see SerializeALot
            //SerializeMultiple(short.MaxValue * 2, random, SerializeSource);
        }

        [Test]
        public void SerializeALot()
        {
            const int repeat = 100;
            var random = new Random();

            // this is going to trigger the offset size thing
            // make sure to only use reference types
            var serializeSource = SerializeSource.Where(x => x.Item1.IsNullable()).ToArray();

            for (var i = 0; i < repeat; i++)
            {
                // byte offsets
                SerializeMultiple(100, random, serializeSource);
                SerializeMultiple(byte.MaxValue - 1, random, serializeSource);
                // push to short offsets
                SerializeMultiple(byte.MaxValue, random, serializeSource);
                SerializeMultiple(byte.MaxValue * 2, random, serializeSource);
            }

            // we cannot push to int offsets this way, as .NET will not accept
            // creating a type with enough properties, so we'll have to do it
            // differently (with large data volumes)
        }

        [Test]
        public void SerializeLargeVolume()
        {
            // each string is 4 bytes (length) + 1 byte per char (we're using [A-Z0-9] chars)
            // note that string.Length is an Int32 and a string cannot get bigger than that

            var random = new Random();

            // byte offsets (data length < byte.MaxValue)
            SerializeVolume(50, random);
            SerializeVolume(byte.MaxValue - 1, random);

            // short offsets (data length < ushort.MaxValue)
            SerializeVolume(byte.MaxValue, random);
            SerializeVolume(byte.MaxValue + 1, random);
            SerializeVolume(byte.MaxValue * 2, random);
            SerializeVolume(ushort.MaxValue - 1, random);

            // int offsets
            SerializeVolume(ushort.MaxValue, random);
            SerializeVolume(ushort.MaxValue + 1, random);
            SerializeVolume(ushort.MaxValue * 2, random);
        }

        private void SerializeVolume(long datalength, Random random)
        {
            const int count = 10;
            var sizeOther = (datalength - count * 4) / (count - 1);
            var sizeFirst = datalength - count * 4 - (count - 1) * sizeOther;

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var propertyTypes = new Type[count];
            for (var i = 0; i < count; i++) propertyTypes[i] = typeof(string);

            var type = ReflectionHelper.CreateObjectType(propertyTypes);
            var obj = Activator.CreateInstance(type);
            if (obj == null) throw new Exception("panic: null obj");
            var properties = new PropertyInfo[count];
            for (var i = 0; i < count; i++)
            {
                var property = type.GetProperty($"Value{i}");
                Assert.That(property, Is.Not.Null);
                properties[i] = property!;
                try
                {
                    var size = i == 0 ? sizeFirst : sizeOther;
                    var text = new char[size];
                    for (var j = 0; j < size; j++) text[j] = chars[random.Next(chars.Length)];
                    property!.SetValue(obj, new string(text));
                }
                catch
                {
                    Assert.Fail($"Failed to assign value to property.");
                }
            }

            var serializer = new ReflectionSerializer();
            var sw = new SchemaBuilderWriter("thing");
            serializer.Write(sw, obj);
            var schema = sw.Build();

            var orw = new ObjectReaderWriter(serializer);

            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            serializer.Write(writer, obj);
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, obj.GetType());

            var obj2 = serializer.Read(reader);
            Assert.That(obj2, Is.Not.Null);
            Assert.That(obj2.GetType(), Is.EqualTo(type));

            for (var i = 0; i < count; i++)
                Assert.That(properties[i].GetValue(obj2), Is.EqualTo(properties[i].GetValue(obj)));
        }

        private void SerializeMultiple(int count, Random random, (Type, object?)[] serializeSource)
        {
            var sources = new (Type, object?)[count];
            for (var i = 0; i < count; i++)
                sources[i] = serializeSource[random.Next(serializeSource.Length)];
            var propertyTypes = sources.Select(x => x.Item1).ToArray();
            var type = ReflectionHelper.CreateObjectType(propertyTypes);
            var obj = Activator.CreateInstance(type);
            if (obj == null) throw new Exception("panic: null obj");
            var properties = new PropertyInfo[count];
            for (var i = 0; i < count; i++)
            {
                var property = type.GetProperty($"Value{i}");
                Assert.That(property, Is.Not.Null);
                properties[i] = property!;
                try
                {
                    property!.SetValue(obj, sources[i].Item2);
                }
                catch
                {
                    Assert.Fail($"Failed to assign value to property.");
                }
            }

            var serializer = new ReflectionSerializer();
            var sw = new SchemaBuilderWriter("thing");
            serializer.Write(sw, obj);
            var schema = sw.Build();

            var orw = new ObjectReaderWriter(serializer);

            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            serializer.Write(writer, obj);
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, obj.GetType());

            var obj2 = serializer.Read(reader);
            Assert.That(obj2, Is.Not.Null);
            Assert.That(obj2.GetType(), Is.EqualTo(type));

            for (var i = 0; i < count; i++)
            {
                AssertPropertyValue(properties[i].GetValue(obj2), properties[i].GetValue(obj));
            }
        }

        // this test ensures that ReflectionSerializer throws the right exceptions.
        [Test]
        public void ReflectionSerializerExceptions()
        {
            var serializer = new ReflectionSerializer();

            Assert.Throws<NotSupportedException>(() =>
            {
                _ = serializer.TypeName;
            });

            Assert.Throws<ArgumentNullException>(() => serializer.Write(null!, null!));

            var notCompactReader = Mock.Of<ICompactReader>();
            Assert.Throws<ArgumentException>(() => serializer.Read(notCompactReader));

            var orw = Mock.Of<IReadWriteObjectsFromIObjectDataInputOutput>();
            var compactReader = new CompactReader(
                orw,
                new ObjectDataInput(Array.Empty<byte>(), orw, Endianness.BigEndian),
                SchemaBuilder.For("thing").Build(),
                typeof(ActivatorKiller)); // Activator.CreateInstance throws on that type (no public ctor)

            Assert.Throws<SerializationException>(() => serializer.Read(compactReader));

            compactReader = new CompactReader(
                orw,
                new ObjectDataInput(Array.Empty<byte>(), orw, Endianness.BigEndian),
                SchemaBuilder.For("thing").Build(),
                typeof(int?)); // Activator.CreateInstance returns null on that type

            Assert.Throws<SerializationException>(() => serializer.Read(compactReader));

            var bytes = new byte[BytesExtensions.SizeOfInt];
            for (var i = 0; i < bytes.Length; i++) bytes[i] = 0;
            compactReader = new CompactReader(
                orw,
                new ObjectDataInput(bytes, orw, Endianness.BigEndian),
                SchemaBuilder.For("thing").WithField("values", FieldKind.Compact).Build(),
                typeof(ClassWithInterfaceProperty)); // interfaces are not supported

            Assert.Throws<SerializationException>(() => serializer.Read(compactReader));

            compactReader = new CompactReader(
                orw,
                new ObjectDataInput(bytes, orw, Endianness.BigEndian),
                SchemaBuilder.For("thing").WithField("values", FieldKind.ArrayOfCompact).Build(),
                typeof(ClassWithInterfaceArrayProperty)); // arrays of interfaces are not supported

            Assert.Throws<SerializationException>(() => serializer.Read(compactReader));

            Assert.That(ReflectionSerializer.UnboxNonNull<int>(33), Is.EqualTo(33));
            Assert.Throws<InvalidOperationException>(() =>
            {
                // defensive coding, that should never happen in our code
                _ = ReflectionSerializer.UnboxNonNull<int>(null);
            });

            Assert.That(ReflectionSerializer.ValueNonNull<int>(33), Is.EqualTo(33));
            Assert.Throws<InvalidOperationException>(() =>
            {
                // defensive coding, that should never happen in our code
                _ = ReflectionSerializer.ValueNonNull<int>(null);
            });
        }

        [Test]
        public void SerializeNestedClass()
        {
            var obj = new SomeExtend { Value = 33, Other = 44 };

            Assert.That(obj.GetType(), Is.EqualTo(typeof(SomeExtend)));
            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            Assert.That(properties.Count, Is.EqualTo(2));
            foreach (var property in properties) Console.WriteLine($"PROPERTY: {property.DeclaringType}.{property.Name}");

            var serializer = new ReflectionSerializer();
            var sw = new SchemaBuilderWriter("thing");
            serializer.Write(sw, obj);
            var schema = sw.Build();

            var orw = new ObjectReaderWriter(serializer);

            var output = new ObjectDataOutput(1024, orw, Endianness.LittleEndian);
            var writer = new CompactWriter(orw, output, schema);
            serializer.Write(writer, obj);
            writer.Complete();

            var buffer = output.ToByteArray();

            var input = new ObjectDataInput(buffer, orw, Endianness.LittleEndian);
            var reader = new CompactReader(orw, input, schema, obj.GetType());

            var obj2 = serializer.Read(reader);
            Assert.That(obj2, Is.Not.Null);
            Assert.That(obj2.GetType(), Is.EqualTo(typeof(SomeExtend)));
            var obj2e = (SomeExtend)obj2;
            Assert.That(obj2e.Value, Is.EqualTo(obj.Value));
            Assert.That(obj2e.Other, Is.EqualTo(obj.Other));

        }

        [Test]
        public void CannotSerializeNotSupportedTypes()
        {
            var obj = new List<int>();
            Console.WriteLine(obj.GetType().FullName);

            var serializer = new ReflectionSerializer();
            var sw = new SchemaBuilderWriter("thing");
            var e = Assert.Throws<SerializationException>(() => serializer.Write(sw, obj))!;
            Console.WriteLine(e.Message);
        }

        [Test]
        public void CannotSerializeAnonymousTypes()
        {
            var obj = new { a = 2 };
            Console.WriteLine(obj.GetType().FullName);

            var serializer = new ReflectionSerializer();
            var sw = new SchemaBuilderWriter("thing");
            var e = Assert.Throws<SerializationException>(() => serializer.Write(sw, obj))!;
            Console.WriteLine(e.Message);
        }

        private class ActivatorKiller
        {
            private ActivatorKiller()
            { }
        }

        private class ClassWithInterfaceProperty
        {
            public IList<int> Values { get; set; } = new List<int>();
        }

        private class ClassWithInterfaceArrayProperty
        {
            public IList<int>[] Values { get; set; } = Array.Empty<IList<int>>();
        }

        private class ObjectReaderWriter : IReadObjectsFromObjectDataInput, IWriteObjectsToObjectDataOutput
        {
            private readonly ReflectionSerializer _serializer;
            private readonly Schema _someClassSchema;
            private readonly Schema _someClass2Schema;
            private readonly Schema _someStructSchema;
            private readonly Schema _someStruct2Schema;
            private readonly Schema _someStruct2NSchema;

            public ObjectReaderWriter(ReflectionSerializer serializer)
            {
                Schema BuildSchema(string name, object o)
                {
                    var nsw = new SchemaBuilderWriter(name);
                    serializer.Write(nsw, o);
                    return nsw.Build();
                }

                _serializer = serializer;
                _someClassSchema = BuildSchema("some-class", new SomeClass());
                _someClass2Schema = BuildSchema("some-class-2", new SomeClass2());
                _someStructSchema = BuildSchema("some-struct", new SomeStruct());
                _someStruct2Schema = BuildSchema("some-struct-2", new SomeStruct2());
                _someStruct2NSchema = BuildSchema("some-struct-2N", new SomeStruct2N());
            }

            public void Write(IObjectDataOutput output, object obj)
            {
                var schema = obj switch
                {
                    SomeClass => _someClassSchema,
                    SomeClass2 => _someClass2Schema,
                    SomeStruct => _someStructSchema,
                    SomeStruct2 => _someStruct2Schema,
                    SomeStruct2N => _someStruct2NSchema,
                    _ => throw new NotSupportedException($"Don't know how to write {obj.GetType()}.")
                };
                var w = new CompactWriter(this, (ObjectDataOutput) output, schema);
                _serializer.Write(w, obj);
                w.Complete();
            }

            public object Read(IObjectDataInput input, Type type)
            {
                var schema =
                    type == typeof(SomeClass) ? _someClassSchema :
                    type == typeof(SomeClass2) ? _someClass2Schema :
                    type == typeof(SomeStruct) ? _someStructSchema :
                    type == typeof(SomeStruct2) ? _someStruct2Schema :
                    type == typeof(SomeStruct2N) ? _someStruct2NSchema :
                    throw new NotSupportedException($"Don't know how to read {type}.");
                var r = new CompactReader(this, (ObjectDataInput)input, schema, type);
                return _serializer.Read(r);
            }

            public T Read<T>(IObjectDataInput input) => (T) Read(input, typeof (T));
        }

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
}
