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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Models;
using Hazelcast.Serialization;
using Hazelcast.Sql;
using Hazelcast.Testing;
using Hazelcast.Testing.Remote;
using NUnit.Framework;

namespace Hazelcast.Tests.Sql
{
    /// <summary>
    /// Tests deserialization of SQL query result for all <see cref="SqlColumnType"/>s.
    /// </summary>
    [TestFixture]
    public class SqlDeserializationTests : SingleMemberClientRemoteTestBase
    {
        [Test]
        [TestCase(
            "",
            "test-string",
            "😊 Hello Приве́т नमस्ते שָׁלוֹם"
        )]
        public async Task Varchar(params string[] expectedValues)
        {
            await using var map = await CreateNewMap(expectedValues);
            await AssertSqlResultMatch(map.Name, expectedValues);
        }

        [Test]
        [TestCase(
            false,
            true
        )]
        public async Task Boolean(params bool[] expectedValues)
        {
            await using var map = await CreateNewMap(expectedValues);
            await AssertSqlResultMatch(map.Name, expectedValues);
        }

        [Test]
        [TestCase(new byte[]
        {
            0,
            1,
            byte.MaxValue
        })]
        public async Task TinyInt(byte[] expectedValues)
        {
            await using var map = await CreateNewMap(expectedValues);
            await AssertSqlResultMatch(map.Name, expectedValues);
        }

        [Test]
        [TestCase(new short[]
        {
            0,
            1,
            -1,
            short.MaxValue,
            short.MinValue
        })]
        public async Task SmallInt(short[] expectedValues)
        {
            await using var map = await CreateNewMap(expectedValues);
            await AssertSqlResultMatch(map.Name, expectedValues);
        }

        [Test]
        [TestCase(new[]
        {
            0,
            1,
            -1,
            int.MaxValue,
            int.MinValue
        })]
        public async Task Integer(int[] expectedValues)
        {
            await using var map = await CreateNewMap(expectedValues);
            await AssertSqlResultMatch(map.Name, expectedValues);
        }

        [Test]
        [TestCase(new[]
        {
            0,
            1,
            -1,
            long.MaxValue,
            long.MinValue
        })]
        public async Task BigInt(long[] expectedValues)
        {
            await using var map = await CreateNewMap(expectedValues);
            await AssertSqlResultMatch(map.Name, expectedValues);
        }

        [Test]
        [TestCase(
            "0",
            "1",
            "-1",
            "255",
            "-255",
            "0.1234567890",
            "-0.12345678901234567890",
            "1234567890",
            "-1234567890",
            "123456789" + "00000000000000000000000000000000",
            "-123456789" + "00000000000000000000000000000000",
            "0." + "00000000000000000000000000000000" + "123456789",
            "-0." + "00000000000000000000000000000000" + "123456789"
        )]
        public async Task Decimal(params string[] expectedValues)
        {
            await using var map = await CreateNewMap<object>();

            var populateScript = $@"var map = instance_0.getMap(""{map.Name}"");" +
                string.Join(";", expectedValues.Select((val, index) =>
                    $"map.set(new java.lang.Integer({index}), new java.math.BigDecimal('{val}'))"
                )) + ";";

            await RcClient.ExecuteOnControllerAsync(RcCluster.Id, populateScript, Lang.JAVASCRIPT);

            await AssertSqlResultMatch(map.Name, expectedValues);
        }

        [Test]
        [TestCase(new[]
        {
            0,
            1,
            -1,
            (float)0.123456,
            (float)-0.123456,
            123456,
            -123456,
            float.Epsilon,
            -float.Epsilon,
            float.MaxValue,
            float.MinValue
        })]
        public async Task Real(float[] expectedValues)
        {
            await using var map = await CreateNewMap(expectedValues);
            await AssertSqlResultMatch(map.Name, expectedValues);
        }

        [Test]
        [TestCase(new[]
        {
            0,
            1,
            -1,
            0.1234567890123456,
            -0.1234567890123456,
            1234567890123456,
            -1234567890123456,
            double.Epsilon,
            -double.Epsilon,
            double.MaxValue,
            double.MinValue
        })]
        public async Task Double(double[] expectedValues)
        {
            await using var map = await CreateNewMap(expectedValues);
            await AssertSqlResultMatch(map.Name, expectedValues);
        }

        // FIXME [Oleksii] discuss year range in HZ SQL and Java
        [Test]
        [TestCase(
            "1970-02-02",
            "2021-07-15",
            "0000-01-01",
            "32767-12-31",
            "-32768-01-01",
            "-999999999-01-01",
            "999999999-12-31"
        )]
        public async Task Date(params string[] expectedValues)
        {
            await using var map = await CreateNewMap<HLocalDate>();

            var expectedDates = expectedValues.Select(HLocalDate.Parse).ToList();

            var populateScript = $@"var map = instance_0.getMap(""{map.Name}"");" +
                string.Join(";", expectedDates.Select((val, index) =>
                    $"map.set(new java.lang.Integer({index}), java.time.LocalDate.of({val.Year},{val.Month},{val.Day}))"
                )) + ";";

            await RcClient.ExecuteOnControllerAsync(RcCluster.Id, populateScript, Lang.JAVASCRIPT);

            await AssertSqlResultMatch(map.Name, expectedDates);
        }

        [Test]
        [TestCase(
            "00:00:00",
            "12:00:00",
            "13:12:11.123456000",
            "01:02:03.000456789",
            "23:59:59.999999999"
        )]
        public async Task Time(params string[] expectedValues)
        {
            await using var map = await CreateNewMap<object>();

            var expectedTimes = expectedValues.Select(HLocalTime.Parse).ToList();

            var populateScript = $@"var map = instance_0.getMap(""{map.Name}"");" +
                string.Join(";", expectedTimes.Select((val, index) =>
                    $"map.set(new java.lang.Integer({index}), java.time.LocalTime.of({val.Hour},{val.Minute},{val.Second},{val.Nanosecond}))"
                )) + ";";

            await RcClient.ExecuteOnControllerAsync(RcCluster.Id, populateScript, Lang.JAVASCRIPT);

            await AssertSqlResultMatch(map.Name, expectedTimes);
        }

        [Test]
        [TestCase(
            "0000-01-01T00:00:00",
            "-32768-01-01T12:00:00",
            "1970-01-01T13:12:11.123456000",
            "2021-07-15T01:02:03.000456789",
            "32767-12-31T23:59:59.999999999"
        )]
        public async Task Timestamp(params string[] expectedValues)
        {
            await using var map = await CreateNewMap<HLocalDateTime>();

            var expectedTimestamps = expectedValues.Select(HLocalDateTime.Parse).ToList();

            var populateScript = $@"var map = instance_0.getMap(""{map.Name}"");" +
                string.Join(";", expectedTimestamps.Select((val, index) =>
                    $"map.set(new java.lang.Integer({index}), java.time.LocalDateTime.of({val.Year},{val.Month},{val.Day},{val.Hour},{val.Minute},{val.Second},{val.Nanosecond}))"
                )) + ";";

            await RcClient.ExecuteOnControllerAsync(RcCluster.Id, populateScript, Lang.JAVASCRIPT);

            await AssertSqlResultMatch(map.Name, expectedTimestamps);
        }

        [Test]
        [TestCase(
            "0000-01-01T00:00:00Z",
            "-32768-01-01T12:00:00+01:02",
            "1970-01-01T13:12:11.123456000-02:01",
            "2021-07-15T01:02:03.000456789+12:34",
            "32767-12-31T23:59:59.999999999-18:00"
        )]
        public async Task TimestampWithTimeZone(params string[] expectedValues)
        {
            await using var map = await CreateNewMap<HOffsetDateTime>();

            var expectedTimestamps = expectedValues.Select(HOffsetDateTime.Parse).ToList();

            var populateScript = $@"var map = instance_0.getMap(""{map.Name}"");" +
                string.Join(";", expectedTimestamps.Select((val, index) =>
                {
                    var local = val.LocalDateTime;
                    return "map.set(" +
                        $"new java.lang.Integer({index}), " +
                        "java.time.OffsetDateTime.of(" +
                        $"{local.Year},{local.Month},{local.Day},{local.Hour},{local.Minute},{local.Second},{local.Nanosecond}," +
                        $"java.time.ZoneOffset.ofTotalSeconds({val.Offset.TotalSeconds})" +
                        "))";
                })) + ";";

            await RcClient.ExecuteOnControllerAsync(RcCluster.Id, populateScript, Lang.JAVASCRIPT);

            await AssertSqlResultMatch(map.Name, expectedTimestamps);
        }

        [Test]
        [TestCase(new[]
        {
            0,
            1,
            -1,
            int.MaxValue,
            int.MinValue
        })]
        public async Task Object(params int[] expectedValues)
        {
            var expectedObjects = expectedValues.Select(i => new PortableObject(i, $"{i}", i % 2 == 0)).ToArray();
            await using var map = await CreateNewMap(expectedObjects);
            await AssertSqlResultMatch(map.Name, expectedObjects);
        }

        private async Task<IHMap<int, TValue>> CreateNewMap<TValue>(TValue[] values = null,
            [CallerMemberName] string methodName = null)
        {
            await using var map = await Client.GetMapAsync<int, TValue>($"{GetType().Name}_{methodName}_{Guid.NewGuid():N}");
            await map.AddIndexAsync(IndexType.Sorted, "__key");

            if (values?.Any() ?? false)
            {
                await map.SetAllAsync(
                    values.Select((val, index) => new KeyValuePair<int, TValue>(index, val)).ToDictionary()
                );
            }

            return map;
        }

        private async Task AssertSqlResultMatch<TValue>(string mapName, IEnumerable<TValue> expectedValues)
        {
            var result = await Client.Sql.ExecuteQueryAsync($"SELECT this FROM {mapName} ORDER BY __key");
            var resultValues = result.EnumerateOnce().Select(r => r.GetValue<TValue>()).ToList();

            CollectionAssert.AreEqual(expectedValues, resultValues);
        }

        protected override HazelcastOptions CreateHazelcastOptions()
        {
            var options = base.CreateHazelcastOptions();
            options.Serialization.AddPortableFactory(PortableObject.FactoryId, PortableObject.Factory);
            return options;
        }

        private class PortableObject : IPortable, IPortableFactory, IEquatable<PortableObject>
        {
            public const int FactoryId = 1;
            public static readonly IPortableFactory Factory = new PortableObject();

            public int IntValue { get; private set; }
            public string StringValue { get; private set; }
            public bool BoolValue { get; private set; }

            private PortableObject()
            { }

            public PortableObject(int intValue, string stringValue, bool boolValue)
            {
                IntValue = intValue;
                StringValue = stringValue;
                BoolValue = boolValue;
            }

            public void ReadPortable(IPortableReader reader)
            {
                IntValue = reader.ReadInt(nameof(IntValue));
                StringValue = reader.ReadString(nameof(StringValue));
                BoolValue = reader.ReadBoolean(nameof(BoolValue));
            }

            public void WritePortable(IPortableWriter writer)
            {
                writer.WriteInt(nameof(IntValue), IntValue);
                writer.WriteString(nameof(StringValue), StringValue);
                writer.WriteBoolean(nameof(BoolValue), BoolValue);
            }

            int IPortable.ClassId => 1;
            int IPortable.FactoryId => FactoryId;
            IPortable IPortableFactory.Create(int classId) => new PortableObject();

            #region Equality members

            public bool Equals(PortableObject other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return IntValue == other.IntValue && StringValue == other.StringValue && BoolValue == other.BoolValue;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((PortableObject)obj);
            }

            public override int GetHashCode()
            {
                return IntValue.GetHashCode() |
                    (StringValue?.GetHashCode() ?? 0) |
                    BoolValue.GetHashCode();
            }

            public static bool operator ==(PortableObject left, PortableObject right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(PortableObject left, PortableObject right)
            {
                return !Equals(left, right);
            }

            #endregion

        }
    }
}
