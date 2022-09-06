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

using System;
using System.Buffers;
using System.Reflection;
using System.Reflection.Emit;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Moq;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    [TestFixture]
    public class CompactReaderExtensionsTests
    {
        private static readonly (Type, FieldKind, object, object)[] ReadOrDefaultReflCases = 
        {
            // do NOT remove nor alter the <generated></generated> lines!
            // <generated>

            (typeof (bool), FieldKind.Boolean, true, false),
            (typeof (sbyte), FieldKind.Int8, (sbyte)12, (sbyte)42),
            (typeof (short), FieldKind.Int16, (short)12, (short)42),
            (typeof (int), FieldKind.Int32, 12, 42),
            (typeof (long), FieldKind.Int64, (long)12, (long)42),
            (typeof (float), FieldKind.Float32, (float)12, (float)42),
            (typeof (double), FieldKind.Float64, (double)12, (double)42),
            (typeof (bool[]), FieldKind.ArrayOfBoolean, new bool[]{true}, new bool[]{false}),
            (typeof (bool[]), FieldKind.ArrayOfBoolean, new bool[]{true}, (bool[])null),
            (typeof (bool[]), FieldKind.ArrayOfBoolean, null, new bool[]{false}),
            (typeof (bool[]), FieldKind.ArrayOfBoolean, null, null),
            (typeof (sbyte[]), FieldKind.ArrayOfInt8, new sbyte[]{(sbyte)12}, new sbyte[]{(sbyte)42}),
            (typeof (sbyte[]), FieldKind.ArrayOfInt8, new sbyte[]{(sbyte)12}, (sbyte[])null),
            (typeof (sbyte[]), FieldKind.ArrayOfInt8, null, new sbyte[]{(sbyte)42}),
            (typeof (sbyte[]), FieldKind.ArrayOfInt8, null, null),
            (typeof (short[]), FieldKind.ArrayOfInt16, new short[]{(short)12}, new short[]{(short)42}),
            (typeof (short[]), FieldKind.ArrayOfInt16, new short[]{(short)12}, (short[])null),
            (typeof (short[]), FieldKind.ArrayOfInt16, null, new short[]{(short)42}),
            (typeof (short[]), FieldKind.ArrayOfInt16, null, null),
            (typeof (int[]), FieldKind.ArrayOfInt32, new int[]{12}, new int[]{42}),
            (typeof (int[]), FieldKind.ArrayOfInt32, new int[]{12}, (int[])null),
            (typeof (int[]), FieldKind.ArrayOfInt32, null, new int[]{42}),
            (typeof (int[]), FieldKind.ArrayOfInt32, null, null),
            (typeof (long[]), FieldKind.ArrayOfInt64, new long[]{(long)12}, new long[]{(long)42}),
            (typeof (long[]), FieldKind.ArrayOfInt64, new long[]{(long)12}, (long[])null),
            (typeof (long[]), FieldKind.ArrayOfInt64, null, new long[]{(long)42}),
            (typeof (long[]), FieldKind.ArrayOfInt64, null, null),
            (typeof (float[]), FieldKind.ArrayOfFloat32, new float[]{(float)12}, new float[]{(float)42}),
            (typeof (float[]), FieldKind.ArrayOfFloat32, new float[]{(float)12}, (float[])null),
            (typeof (float[]), FieldKind.ArrayOfFloat32, null, new float[]{(float)42}),
            (typeof (float[]), FieldKind.ArrayOfFloat32, null, null),
            (typeof (double[]), FieldKind.ArrayOfFloat64, new double[]{(double)12}, new double[]{(double)42}),
            (typeof (double[]), FieldKind.ArrayOfFloat64, new double[]{(double)12}, (double[])null),
            (typeof (double[]), FieldKind.ArrayOfFloat64, null, new double[]{(double)42}),
            (typeof (double[]), FieldKind.ArrayOfFloat64, null, null),
            (typeof (bool?), FieldKind.NullableBoolean, true, false),
            (typeof (bool?), FieldKind.NullableBoolean, true, null),
            (typeof (bool?), FieldKind.NullableBoolean, null, false),
            (typeof (bool?), FieldKind.NullableBoolean, null, null),
            (typeof (sbyte?), FieldKind.NullableInt8, (sbyte)12, (sbyte)42),
            (typeof (sbyte?), FieldKind.NullableInt8, (sbyte)12, null),
            (typeof (sbyte?), FieldKind.NullableInt8, null, (sbyte)42),
            (typeof (sbyte?), FieldKind.NullableInt8, null, null),
            (typeof (short?), FieldKind.NullableInt16, (short)12, (short)42),
            (typeof (short?), FieldKind.NullableInt16, (short)12, null),
            (typeof (short?), FieldKind.NullableInt16, null, (short)42),
            (typeof (short?), FieldKind.NullableInt16, null, null),
            (typeof (int?), FieldKind.NullableInt32, 12, 42),
            (typeof (int?), FieldKind.NullableInt32, 12, null),
            (typeof (int?), FieldKind.NullableInt32, null, 42),
            (typeof (int?), FieldKind.NullableInt32, null, null),
            (typeof (long?), FieldKind.NullableInt64, (long)12, (long)42),
            (typeof (long?), FieldKind.NullableInt64, (long)12, null),
            (typeof (long?), FieldKind.NullableInt64, null, (long)42),
            (typeof (long?), FieldKind.NullableInt64, null, null),
            (typeof (float?), FieldKind.NullableFloat32, (float)12, (float)42),
            (typeof (float?), FieldKind.NullableFloat32, (float)12, null),
            (typeof (float?), FieldKind.NullableFloat32, null, (float)42),
            (typeof (float?), FieldKind.NullableFloat32, null, null),
            (typeof (double?), FieldKind.NullableFloat64, (double)12, (double)42),
            (typeof (double?), FieldKind.NullableFloat64, (double)12, null),
            (typeof (double?), FieldKind.NullableFloat64, null, (double)42),
            (typeof (double?), FieldKind.NullableFloat64, null, null),
            (typeof (HBigDecimal?), FieldKind.Decimal, new HBigDecimal(12), new HBigDecimal(42)),
            (typeof (HBigDecimal?), FieldKind.Decimal, new HBigDecimal(12), null),
            (typeof (HBigDecimal?), FieldKind.Decimal, null, new HBigDecimal(42)),
            (typeof (HBigDecimal?), FieldKind.Decimal, null, null),
            (typeof (string), FieldKind.String, (string)"aaa", (string)"xxx"),
            (typeof (string), FieldKind.String, (string)"aaa", null),
            (typeof (string), FieldKind.String, null, (string)"xxx"),
            (typeof (string), FieldKind.String, null, null),
            (typeof (HLocalTime?), FieldKind.Time, new HLocalTime(1, 2, 3, 0), new HLocalTime(4, 5, 6, 0)),
            (typeof (HLocalTime?), FieldKind.Time, new HLocalTime(1, 2, 3, 0), null),
            (typeof (HLocalTime?), FieldKind.Time, null, new HLocalTime(4, 5, 6, 0)),
            (typeof (HLocalTime?), FieldKind.Time, null, null),
            (typeof (HLocalDate?), FieldKind.Date, new HLocalDate(1, 2, 3), new HLocalDate(4, 5, 6)),
            (typeof (HLocalDate?), FieldKind.Date, new HLocalDate(1, 2, 3), null),
            (typeof (HLocalDate?), FieldKind.Date, null, new HLocalDate(4, 5, 6)),
            (typeof (HLocalDate?), FieldKind.Date, null, null),
            (typeof (HLocalDateTime?), FieldKind.TimeStamp, new HLocalDateTime(1, 2, 3, 0, 4, 5, 6), new HLocalDateTime(4, 5, 6, 0, 1, 2, 3)),
            (typeof (HLocalDateTime?), FieldKind.TimeStamp, new HLocalDateTime(1, 2, 3, 0, 4, 5, 6), null),
            (typeof (HLocalDateTime?), FieldKind.TimeStamp, null, new HLocalDateTime(4, 5, 6, 0, 1, 2, 3)),
            (typeof (HLocalDateTime?), FieldKind.TimeStamp, null, null),
            (typeof (HOffsetDateTime?), FieldKind.TimeStampWithTimeZone, new HOffsetDateTime(new HLocalDateTime(1, 2, 3, 0, 4, 5, 6), 0), new HOffsetDateTime(new HLocalDateTime(4, 5, 6, 0, 1, 2, 3), 0)),
            (typeof (HOffsetDateTime?), FieldKind.TimeStampWithTimeZone, new HOffsetDateTime(new HLocalDateTime(1, 2, 3, 0, 4, 5, 6), 0), null),
            (typeof (HOffsetDateTime?), FieldKind.TimeStampWithTimeZone, null, new HOffsetDateTime(new HLocalDateTime(4, 5, 6, 0, 1, 2, 3), 0)),
            (typeof (HOffsetDateTime?), FieldKind.TimeStampWithTimeZone, null, null),
            (typeof (bool?[]), FieldKind.ArrayOfNullableBoolean, new bool?[]{true}, new bool?[]{false}),
            (typeof (bool?[]), FieldKind.ArrayOfNullableBoolean, new bool?[]{true}, (bool[])null),
            (typeof (bool?[]), FieldKind.ArrayOfNullableBoolean, null, new bool?[]{false}),
            (typeof (bool?[]), FieldKind.ArrayOfNullableBoolean, null, null),
            (typeof (bool?[]), FieldKind.ArrayOfNullableBoolean, new bool?[]{true}, new bool?[]{null}),
            (typeof (bool?[]), FieldKind.ArrayOfNullableBoolean, new bool?[]{null}, new bool?[]{false}),
            (typeof (bool?[]), FieldKind.ArrayOfNullableBoolean, new bool?[]{null}, new bool?[]{null}),
            (typeof (sbyte?[]), FieldKind.ArrayOfNullableInt8, new sbyte?[]{(sbyte)12}, new sbyte?[]{(sbyte)42}),
            (typeof (sbyte?[]), FieldKind.ArrayOfNullableInt8, new sbyte?[]{(sbyte)12}, (sbyte[])null),
            (typeof (sbyte?[]), FieldKind.ArrayOfNullableInt8, null, new sbyte?[]{(sbyte)42}),
            (typeof (sbyte?[]), FieldKind.ArrayOfNullableInt8, null, null),
            (typeof (sbyte?[]), FieldKind.ArrayOfNullableInt8, new sbyte?[]{(sbyte)12}, new sbyte?[]{null}),
            (typeof (sbyte?[]), FieldKind.ArrayOfNullableInt8, new sbyte?[]{null}, new sbyte?[]{(sbyte)42}),
            (typeof (sbyte?[]), FieldKind.ArrayOfNullableInt8, new sbyte?[]{null}, new sbyte?[]{null}),
            (typeof (short?[]), FieldKind.ArrayOfNullableInt16, new short?[]{(short)12}, new short?[]{(short)42}),
            (typeof (short?[]), FieldKind.ArrayOfNullableInt16, new short?[]{(short)12}, (short[])null),
            (typeof (short?[]), FieldKind.ArrayOfNullableInt16, null, new short?[]{(short)42}),
            (typeof (short?[]), FieldKind.ArrayOfNullableInt16, null, null),
            (typeof (short?[]), FieldKind.ArrayOfNullableInt16, new short?[]{(short)12}, new short?[]{null}),
            (typeof (short?[]), FieldKind.ArrayOfNullableInt16, new short?[]{null}, new short?[]{(short)42}),
            (typeof (short?[]), FieldKind.ArrayOfNullableInt16, new short?[]{null}, new short?[]{null}),
            (typeof (int?[]), FieldKind.ArrayOfNullableInt32, new int?[]{12}, new int?[]{42}),
            (typeof (int?[]), FieldKind.ArrayOfNullableInt32, new int?[]{12}, (int[])null),
            (typeof (int?[]), FieldKind.ArrayOfNullableInt32, null, new int?[]{42}),
            (typeof (int?[]), FieldKind.ArrayOfNullableInt32, null, null),
            (typeof (int?[]), FieldKind.ArrayOfNullableInt32, new int?[]{12}, new int?[]{null}),
            (typeof (int?[]), FieldKind.ArrayOfNullableInt32, new int?[]{null}, new int?[]{42}),
            (typeof (int?[]), FieldKind.ArrayOfNullableInt32, new int?[]{null}, new int?[]{null}),
            (typeof (long?[]), FieldKind.ArrayOfNullableInt64, new long?[]{(long)12}, new long?[]{(long)42}),
            (typeof (long?[]), FieldKind.ArrayOfNullableInt64, new long?[]{(long)12}, (long[])null),
            (typeof (long?[]), FieldKind.ArrayOfNullableInt64, null, new long?[]{(long)42}),
            (typeof (long?[]), FieldKind.ArrayOfNullableInt64, null, null),
            (typeof (long?[]), FieldKind.ArrayOfNullableInt64, new long?[]{(long)12}, new long?[]{null}),
            (typeof (long?[]), FieldKind.ArrayOfNullableInt64, new long?[]{null}, new long?[]{(long)42}),
            (typeof (long?[]), FieldKind.ArrayOfNullableInt64, new long?[]{null}, new long?[]{null}),
            (typeof (float?[]), FieldKind.ArrayOfNullableFloat32, new float?[]{(float)12}, new float?[]{(float)42}),
            (typeof (float?[]), FieldKind.ArrayOfNullableFloat32, new float?[]{(float)12}, (float[])null),
            (typeof (float?[]), FieldKind.ArrayOfNullableFloat32, null, new float?[]{(float)42}),
            (typeof (float?[]), FieldKind.ArrayOfNullableFloat32, null, null),
            (typeof (float?[]), FieldKind.ArrayOfNullableFloat32, new float?[]{(float)12}, new float?[]{null}),
            (typeof (float?[]), FieldKind.ArrayOfNullableFloat32, new float?[]{null}, new float?[]{(float)42}),
            (typeof (float?[]), FieldKind.ArrayOfNullableFloat32, new float?[]{null}, new float?[]{null}),
            (typeof (double?[]), FieldKind.ArrayOfNullableFloat64, new double?[]{(double)12}, new double?[]{(double)42}),
            (typeof (double?[]), FieldKind.ArrayOfNullableFloat64, new double?[]{(double)12}, (double[])null),
            (typeof (double?[]), FieldKind.ArrayOfNullableFloat64, null, new double?[]{(double)42}),
            (typeof (double?[]), FieldKind.ArrayOfNullableFloat64, null, null),
            (typeof (double?[]), FieldKind.ArrayOfNullableFloat64, new double?[]{(double)12}, new double?[]{null}),
            (typeof (double?[]), FieldKind.ArrayOfNullableFloat64, new double?[]{null}, new double?[]{(double)42}),
            (typeof (double?[]), FieldKind.ArrayOfNullableFloat64, new double?[]{null}, new double?[]{null}),
            (typeof (HBigDecimal?[]), FieldKind.ArrayOfDecimal, new HBigDecimal?[]{new HBigDecimal(12)}, new HBigDecimal?[]{new HBigDecimal(42)}),
            (typeof (HBigDecimal?[]), FieldKind.ArrayOfDecimal, new HBigDecimal?[]{new HBigDecimal(12)}, (HBigDecimal[])null),
            (typeof (HBigDecimal?[]), FieldKind.ArrayOfDecimal, null, new HBigDecimal?[]{new HBigDecimal(42)}),
            (typeof (HBigDecimal?[]), FieldKind.ArrayOfDecimal, null, null),
            (typeof (HBigDecimal?[]), FieldKind.ArrayOfDecimal, new HBigDecimal?[]{new HBigDecimal(12)}, new HBigDecimal?[]{null}),
            (typeof (HBigDecimal?[]), FieldKind.ArrayOfDecimal, new HBigDecimal?[]{null}, new HBigDecimal?[]{new HBigDecimal(42)}),
            (typeof (HBigDecimal?[]), FieldKind.ArrayOfDecimal, new HBigDecimal?[]{null}, new HBigDecimal?[]{null}),
            (typeof (HLocalTime?[]), FieldKind.ArrayOfTime, new HLocalTime?[]{new HLocalTime(1, 2, 3, 0)}, new HLocalTime?[]{new HLocalTime(4, 5, 6, 0)}),
            (typeof (HLocalTime?[]), FieldKind.ArrayOfTime, new HLocalTime?[]{new HLocalTime(1, 2, 3, 0)}, (HLocalTime[])null),
            (typeof (HLocalTime?[]), FieldKind.ArrayOfTime, null, new HLocalTime?[]{new HLocalTime(4, 5, 6, 0)}),
            (typeof (HLocalTime?[]), FieldKind.ArrayOfTime, null, null),
            (typeof (HLocalTime?[]), FieldKind.ArrayOfTime, new HLocalTime?[]{new HLocalTime(1, 2, 3, 0)}, new HLocalTime?[]{null}),
            (typeof (HLocalTime?[]), FieldKind.ArrayOfTime, new HLocalTime?[]{null}, new HLocalTime?[]{new HLocalTime(4, 5, 6, 0)}),
            (typeof (HLocalTime?[]), FieldKind.ArrayOfTime, new HLocalTime?[]{null}, new HLocalTime?[]{null}),
            (typeof (HLocalDate?[]), FieldKind.ArrayOfDate, new HLocalDate?[]{new HLocalDate(1, 2, 3)}, new HLocalDate?[]{new HLocalDate(4, 5, 6)}),
            (typeof (HLocalDate?[]), FieldKind.ArrayOfDate, new HLocalDate?[]{new HLocalDate(1, 2, 3)}, (HLocalDate[])null),
            (typeof (HLocalDate?[]), FieldKind.ArrayOfDate, null, new HLocalDate?[]{new HLocalDate(4, 5, 6)}),
            (typeof (HLocalDate?[]), FieldKind.ArrayOfDate, null, null),
            (typeof (HLocalDate?[]), FieldKind.ArrayOfDate, new HLocalDate?[]{new HLocalDate(1, 2, 3)}, new HLocalDate?[]{null}),
            (typeof (HLocalDate?[]), FieldKind.ArrayOfDate, new HLocalDate?[]{null}, new HLocalDate?[]{new HLocalDate(4, 5, 6)}),
            (typeof (HLocalDate?[]), FieldKind.ArrayOfDate, new HLocalDate?[]{null}, new HLocalDate?[]{null}),
            (typeof (HLocalDateTime?[]), FieldKind.ArrayOfTimeStamp, new HLocalDateTime?[]{new HLocalDateTime(1, 2, 3, 0, 4, 5, 6)}, new HLocalDateTime?[]{new HLocalDateTime(4, 5, 6, 0, 1, 2, 3)}),
            (typeof (HLocalDateTime?[]), FieldKind.ArrayOfTimeStamp, new HLocalDateTime?[]{new HLocalDateTime(1, 2, 3, 0, 4, 5, 6)}, (HLocalDateTime[])null),
            (typeof (HLocalDateTime?[]), FieldKind.ArrayOfTimeStamp, null, new HLocalDateTime?[]{new HLocalDateTime(4, 5, 6, 0, 1, 2, 3)}),
            (typeof (HLocalDateTime?[]), FieldKind.ArrayOfTimeStamp, null, null),
            (typeof (HLocalDateTime?[]), FieldKind.ArrayOfTimeStamp, new HLocalDateTime?[]{new HLocalDateTime(1, 2, 3, 0, 4, 5, 6)}, new HLocalDateTime?[]{null}),
            (typeof (HLocalDateTime?[]), FieldKind.ArrayOfTimeStamp, new HLocalDateTime?[]{null}, new HLocalDateTime?[]{new HLocalDateTime(4, 5, 6, 0, 1, 2, 3)}),
            (typeof (HLocalDateTime?[]), FieldKind.ArrayOfTimeStamp, new HLocalDateTime?[]{null}, new HLocalDateTime?[]{null}),
            (typeof (HOffsetDateTime?[]), FieldKind.ArrayOfTimeStampWithTimeZone, new HOffsetDateTime?[]{new HOffsetDateTime(new HLocalDateTime(1, 2, 3, 0, 4, 5, 6), 0)}, new HOffsetDateTime?[]{new HOffsetDateTime(new HLocalDateTime(4, 5, 6, 0, 1, 2, 3), 0)}),
            (typeof (HOffsetDateTime?[]), FieldKind.ArrayOfTimeStampWithTimeZone, new HOffsetDateTime?[]{new HOffsetDateTime(new HLocalDateTime(1, 2, 3, 0, 4, 5, 6), 0)}, (HOffsetDateTime[])null),
            (typeof (HOffsetDateTime?[]), FieldKind.ArrayOfTimeStampWithTimeZone, null, new HOffsetDateTime?[]{new HOffsetDateTime(new HLocalDateTime(4, 5, 6, 0, 1, 2, 3), 0)}),
            (typeof (HOffsetDateTime?[]), FieldKind.ArrayOfTimeStampWithTimeZone, null, null),
            (typeof (HOffsetDateTime?[]), FieldKind.ArrayOfTimeStampWithTimeZone, new HOffsetDateTime?[]{new HOffsetDateTime(new HLocalDateTime(1, 2, 3, 0, 4, 5, 6), 0)}, new HOffsetDateTime?[]{null}),
            (typeof (HOffsetDateTime?[]), FieldKind.ArrayOfTimeStampWithTimeZone, new HOffsetDateTime?[]{null}, new HOffsetDateTime?[]{new HOffsetDateTime(new HLocalDateTime(4, 5, 6, 0, 1, 2, 3), 0)}),
            (typeof (HOffsetDateTime?[]), FieldKind.ArrayOfTimeStampWithTimeZone, new HOffsetDateTime?[]{null}, new HOffsetDateTime?[]{null}),
            (typeof (string[]), FieldKind.ArrayOfString, new string[]{(string)"aaa"}, new string[]{(string)"xxx"}),
            (typeof (string[]), FieldKind.ArrayOfString, new string[]{(string)"aaa"}, (string[])null),
            (typeof (string[]), FieldKind.ArrayOfString, null, new string[]{(string)"xxx"}),
            (typeof (string[]), FieldKind.ArrayOfString, null, null),
            (typeof (string[]), FieldKind.ArrayOfString, new string[]{(string)"aaa"}, new string[]{null}),
            (typeof (string[]), FieldKind.ArrayOfString, new string[]{null}, new string[]{(string)"xxx"}),
            (typeof (string[]), FieldKind.ArrayOfString, new string[]{null}, new string[]{null}),

            // </generated>

            (typeof (Thing), FieldKind.Compact, new Thing { Value = 1 }, new Thing { Value = 2 }),
            (typeof (Thing), FieldKind.Compact, new Thing { Value = 1 }, null),
            (typeof (Thing), FieldKind.Compact, null, new Thing { Value = 2 }),
            (typeof (Thing), FieldKind.Compact, null, null),

            (typeof (Thing[]), FieldKind.ArrayOfCompact, new Thing[] { new Thing { Value = 1 } }, new Thing[] { new Thing { Value = 2 } }),
            (typeof (Thing[]), FieldKind.ArrayOfCompact, new Thing[] { new Thing { Value = 1 } }, null),
            (typeof (Thing[]), FieldKind.ArrayOfCompact, null, new Thing[] { new Thing { Value = 2 } }),
            (typeof (Thing[]), FieldKind.ArrayOfCompact, null, null),
            (typeof (Thing[]), FieldKind.ArrayOfCompact, new Thing[] { new Thing { Value = 1 } }, new Thing[] { null }),
            (typeof (Thing[]), FieldKind.ArrayOfCompact, new Thing[] { null }, new Thing[] { new Thing { Value = 2 } }),
            (typeof (Thing[]), FieldKind.ArrayOfCompact, new Thing[] { null }, new Thing[] { null }),
        };

        private static void MockReadWriteCompact(IReadWriteObjectsFromIObjectDataInputOutput orw)
        {
            var thingSchema = SchemaBuilder.For("thing").WithField("name", FieldKind.String).WithField("value", FieldKind.Int32).Build();

            Mock.Get(orw)
                .Setup(x => x.Write(It.IsAny<IObjectDataOutput>(), It.IsAny<object>()))
                .Callback<IObjectDataOutput, object>((o, x) =>
                {
                    if (x is not Thing t) throw new NotSupportedException();
                    var w = new CompactWriter(orw, (ObjectDataOutput)o, thingSchema);
                    new ThingCompactSerializer<Thing>().Write(w, t);
                    w.Complete();
                });
            Mock.Get(orw)
                .Setup(x => x.Read<Thing>(It.IsAny<IObjectDataInput>()))
                .Returns<IObjectDataInput>(i =>
                {
                    var r = new CompactReader(orw, (ObjectDataInput)i, thingSchema, typeof(Thing));
                    return new ThingCompactSerializer<Thing>().Read(r);
                });
        }

        private static void AssertValue(object expected, object value)
        {
            switch (expected)
            {
                case null:
                    Assert.That(value, Is.Null);
                    break;
                case Thing expectedThing:
                    Assert.That(value, Is.InstanceOf<Thing>());
                    var thingValue = (Thing) value;
                    Assert.That(thingValue.Name, Is.EqualTo(expectedThing.Name));
                    Assert.That(thingValue.Value, Is.EqualTo(expectedThing.Value));
                    break;
                case Thing[] expectedThings:
                    Assert.That(value, Is.InstanceOf<Thing[]>());
                    var thingValues = (Thing[])value;
                    Assert.That(thingValues.Length, Is.EqualTo(expectedThings.Length));
                    for (var i = 0; i < expectedThings.Length; i++) AssertValue(expectedThings[i], thingValues[i]);
                    break;
                default:
                    Assert.That(value, Is.EqualTo(expected));
                    break;
            }
        }
    }
}
