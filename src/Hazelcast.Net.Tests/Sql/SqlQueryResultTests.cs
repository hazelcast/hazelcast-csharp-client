﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Models;
using Hazelcast.Serialization;
using Hazelcast.Sql;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using Hazelcast.Testing.TestData;
using Hazelcast.Tests.TestObjects;
using NUnit.Framework;

namespace Hazelcast.Tests.Sql
{
    [TestFixture]
    [ServerCondition("[5.0,)")] // only on server 5.0 and above
    public class SqlQueryResultTests : SqlTestBase
    {
        // Needed to create long-running query
        protected override bool EnableJet => true;

        [Test]
        public async Task EnumerateAfterDisposeThrows()
        {
            await using var map = await CreateIntMapAsync(size: 10);

            var result = await Client.Sql.ExecuteQueryAsync($"SELECT * FROM {map.Name}");
            await result.DisposeAsync();

            Assert.Throws<ObjectDisposedException>(() => result.GetAsyncEnumerator());
        }

        [Test]
        public async Task CannotEnumerateResultMoreThanOnce()
        {
            await using var map = await CreateIntMapAsync(size: 10);

            await using var result = await Client.Sql.ExecuteQueryAsync($"SELECT * FROM {map.Name} ORDER BY 1");

            // enumerate once
            _ = await result.Take(5).ToListAsync();

            // cannot enumerate twice
            await AssertEx.ThrowsAsync<InvalidOperationException>(async () => await result.Take(5).ToListAsync());
        }

        [Test]
        public async Task CancelEnumeratorEnumerationThrows()
        {
            await using var result = await Client.Sql.ExecuteQueryAsync("SELECT * FROM TABLE(generate_stream(10))");
            using var cancellationSource = new CancellationTokenSource(50);

            await AssertEx.ThrowsAsync<OperationCanceledException>(async () =>
            {
                var enumerator = result.GetAsyncEnumerator(cancellationSource.Token);
                while (await enumerator.MoveNextAsync())
                {
                    var row = enumerator.Current;
                    if (row.GetColumn<long>(0) > 5)
                        break;
                }
            });
        }

        [Test]
        public async Task CancelEnumerableEnumerationThrows()
        {
            await using var result = await Client.Sql.ExecuteQueryAsync("SELECT * FROM TABLE(generate_stream(10))");
            using var cancellationSource = new CancellationTokenSource(50);

            await AssertEx.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await foreach (var row in result.WithCancellation(cancellationSource.Token))
                {
                    if (row.GetColumn<long>(0) > 5)
                        break;
                }
            });
        }

        [Test]
        public async Task CancelEnumerableEnumerationDoesNotThrow()
        {
            await using var result = await Client.Sql.ExecuteQueryAsync("SELECT * FROM TABLE(generate_stream(10))");
            using var cancellationSource = new CancellationTokenSource(50);

            await foreach (var row in result.WithCancellation(throwOnCancel: false, cancellationSource.Token))
            {
                if (row.GetColumn<long>(0) > 5)
                    break;
            }
        }

        [Test]
        public async Task EnumerateToListCancellation()
        {
            await using var result = await Client.Sql.ExecuteQueryAsync("SELECT * FROM TABLE(generate_stream(10))");

            await AssertEx.ThrowsAsync<OperationCanceledException>(async () =>
            {
                using var cancellationSource = new CancellationTokenSource(50);
                await result.Take(5).ToListAsync(cancellationSource.Token);
            });
        }

        [Test]
        public async Task CanDisposeResultMultipleTimes()
        {
            await using var map = await CreateIntMapAsync(size: 10);

            var result = await Client.Sql.ExecuteQueryAsync($"SELECT * FROM {map.Name}");

            await result.DisposeAsync();
            await result.DisposeAsync();
            await result.DisposeAsync();
        }

        [Test]
        public async Task DisposeDuringQuery()
        {
            var result = await Client.Sql.ExecuteQueryAsync("SELECT * FROM TABLE(generate_stream(10))", new SqlStatementOptions
            {
                CursorBufferSize = 1
            });

            var enumerator = result.GetAsyncEnumerator();
            var moveNextTask = enumerator.MoveNextAsync();

            await Task.Delay(millisecondsDelay: 10); // wait for query to reach the server
            await result.DisposeAsync();

            await AssertEx.ThrowsAsync<HazelcastSqlException>(async () => await moveNextTask);
        }

        [Test]
        public async Task TestSqlLazyDeserializationThrowsExceptionAtGetValue()
        {
            var map = await Client.GetMapAsync<int, DummyPortable>(nameof(SqlQueryResultTests));

            //DummyPortable implements IPortable but HZ client is not aware from deserialization,
            //so, client doesn't know how to deserialize. Hence, client can get sql results and can read keys
            //until trying to get value of the row -> throws SerializationException
            var myPortableObject = new DummyPortable();
            await map.PutAsync(1, myPortableObject);

            await Client.Sql.ExecuteCommandAsync(
                $"CREATE MAPPING {nameof(SqlQueryResultTests)} (name VARCHAR,id INT) TYPE IMap " +
                $"OPTIONS ('keyFormat'='int', 'valueFormat' = 'portable','valuePortableFactoryId' = '1','valuePortableClassId' = '1')");

            var result = await Client.Sql.ExecuteQueryAsync($"SELECT __key, this FROM {nameof(SqlQueryResultTests)}");

            await foreach (var row in result)
            {
                //this works, since key is int -> can be deserialized
                Assert.NotZero(row.GetKey<int>());

                //cause an exception since there is no deserializer for the value
                Assert.Throws<SerializationException>(() => row.GetValue<DummyPortable>());
            }
        }

        [Test]
        [ServerConditionAttribute("[5.0,)")] // only on 5.0 and above
        public async Task SqlErrorHasSuggestion()
        {
            var dummyMapName = "testingMap";
            var map = await Client.GetMapAsync<int, string>(dummyMapName);
            await map.PutAsync(0, "some value");

            //query the map without creating mapping to get exception with suggestion in it.
            var ex = Assert.ThrowsAsync<HazelcastSqlException>(async () => await Client.Sql.ExecuteQueryAsync($"SELECT * FROM {dummyMapName}"));

            Assert.IsFalse(string.IsNullOrEmpty(ex.Suggestion));
            Assert.IsFalse(string.IsNullOrEmpty(ex.Message));
        }

        //Put objects via sql.insert
        [TestCase(true)]
        //Put objects via map.put
        [TestCase(false)]
        [ServerConditionAttribute("5.1")]
        public async Task CanQueryComplexJsonValue(bool useSql)
        {
            var expectedObjects = EmployeeTestObjectTestData.EmployeeTestObjects.ToDictionary(p => p.Id, p => p);
            Assert.That(expectedObjects, Is.Not.Empty);

            var map = await CreateEmployeeTestObjectMapAsync(expectedObjects, useSql);

            char employeeTypeToQuery = expectedObjects.First().Value.Type;

            var queryResult = await Client.Sql.ExecuteQueryAsync($"SELECT * FROM {map.Name} WHERE JSON_VALUE(this,'$.Type')=?", employeeTypeToQuery);

            bool queryReturnedResult = false;
            int actualRowCount = 0;

            await foreach (var row in queryResult)
            {
                var jsonVal = row.GetValue<HazelcastJsonValue>();
                var actualObject = JsonSerializer.Deserialize<EmployeeTestObject>(jsonVal.ToString());
                var expectedObject = expectedObjects[row.GetKey<int>()];

                Assert.AreEqual(expectedObject.Id, actualObject.Id);
                Assert.AreEqual(expectedObject.Name, actualObject.Name);
                Assert.AreEqual(expectedObject.Salary, actualObject.Salary);
                Assert.AreEqual(expectedObject.Type, actualObject.Type);
                Assert.AreEqual(expectedObject.Started, actualObject.Started);
                Assert.AreEqual(expectedObject.StartedAtTimeStamp, actualObject.StartedAtTimeStamp);

                actualRowCount++;
                queryReturnedResult = true;
            }

            int expectedRowCount = expectedObjects.Where(p => p.Value.Type.Equals(employeeTypeToQuery)).Count();

            Assert.AreEqual(expectedRowCount, actualRowCount);
            // query result is async, getting count could be pricey. So, be sure there was result.
            Assert.True(queryReturnedResult, "Query result was empty!");

            await map.DestroyAsync();
        }

        [Test]
        public async Task NullColumnsDontThrowWhileReading()
        {
            // Reproduce Github Issue #854.

            var map = await Client.GetMapAsync<int, HazelcastJsonValue>("jsonFlatMap");
            await Client.Sql.ExecuteCommandAsync($"CREATE OR REPLACE MAPPING {map.Name}  " +
                                                 $"(__key INT," +
                                                 $"string_field VARCHAR," +
                                                 $"int_field INT," +
                                                 $"bool_field BOOLEAN," +
                                                 $"tint_field TINYINT," +
                                                 $"sint_field SMALLINT," +
                                                 $"bint_field BIGINT," +
                                                 $"decimal_field DECIMAL," +
                                                 $"float_field REAL," +
                                                 $"double_field double," +
                                                 $"date_field DATE," +
                                                 $"time_field TIME," +
                                                 $"times_field TIMESTAMP," +
                                                 $"times_zone_field TIMESTAMP WITH TIME ZONE) " +
                                                 $"TYPE IMap OPTIONS ('keyFormat'='int', 'valueFormat'='json-flat')");

            void AssertColumns(SqlRow row, string checkingFor)
            {
                if (checkingFor == "null")
                {
                    Assert.IsNull(row.GetColumn<string?>("string_field"));
                    Assert.IsNull(row.GetColumn<int?>("int_field"));
                    Assert.IsNull(row.GetColumn<bool?>("bool_field"));
                    Assert.IsNull(row.GetColumn<byte?>("tint_field"));
                    Assert.IsNull(row.GetColumn<short?>("sint_field"));
                    Assert.DoesNotThrow(() => row.GetColumn<HBigDecimal>("decimal_field"));
                    Assert.IsNull(row.GetColumn<float?>("float_field"));
                    Assert.IsNull(row.GetColumn<double?>("double_field"));
                    Assert.IsNull(row.GetColumn<HLocalDate?>("date_field"));
                    Assert.IsNull(row.GetColumn<HLocalTime?>("time_field"));
                    Assert.IsNull(row.GetColumn<HLocalDateTime?>("times_field"));
                    Assert.IsNull(row.GetColumn<HOffsetDateTime?>("times_zone_field"));
                }
                else if (checkingFor == "notnull")
                {
                    Assert.NotNull(row.GetColumn<string?>("string_field"));
                    Assert.NotNull(row.GetColumn<int?>("int_field"));
                    Assert.NotNull(row.GetColumn<bool?>("bool_field"));
                    Assert.NotNull(row.GetColumn<byte?>("tint_field"));
                    Assert.NotNull(row.GetColumn<short?>("sint_field"));
                    Assert.NotNull(row.GetColumn<long?>("bint_field"));
                    var decimalField = row.GetColumn<HBigDecimal>("decimal_field");
                    Assert.True(decimalField.TryToDecimal(out _));
                    Assert.NotNull(row.GetColumn<float?>("float_field"));
                    Assert.NotNull(row.GetColumn<double?>("double_field"));
                    Assert.NotNull(row.GetColumn<HLocalDate?>("date_field"));
                    Assert.NotNull(row.GetColumn<HLocalTime?>("time_field"));
                    Assert.NotNull(row.GetColumn<HLocalDateTime?>("times_field"));
                    Assert.NotNull(row.GetColumn<HOffsetDateTime?>("times_zone_field"));

                    Assert.NotNull(row.GetColumn<string>("string_field"));
                    Assert.NotNull(row.GetColumn<int>("int_field"));
                    Assert.NotNull(row.GetColumn<bool>("bool_field"));
                    Assert.NotNull(row.GetColumn<byte>("tint_field"));
                    Assert.NotNull(row.GetColumn<short>("sint_field"));
                    Assert.NotNull(row.GetColumn<long>("bint_field"));
                    decimalField = row.GetColumn<HBigDecimal>("decimal_field");
                    Assert.True(decimalField.TryToDecimal(out _));
                    Assert.NotNull(row.GetColumn<float>("float_field"));
                    Assert.NotNull(row.GetColumn<double>("double_field"));
                    Assert.NotNull(row.GetColumn<HLocalDate>("date_field"));
                    Assert.NotNull(row.GetColumn<HLocalTime>("time_field"));
                    Assert.NotNull(row.GetColumn<HLocalDateTime>("times_field"));
                    Assert.NotNull(row.GetColumn<HOffsetDateTime>("times_zone_field"));
                }
                else if (checkingFor == "mixed")
                {
                    Assert.DoesNotThrow(() => row.GetColumn<string?>("string_field"));
                    Assert.DoesNotThrow(() => row.GetColumn<int?>("int_field"));
                    Assert.DoesNotThrow(() => row.GetColumn<bool?>("bool_field"));
                    Assert.DoesNotThrow(() => row.GetColumn<byte?>("tint_field"));
                    Assert.DoesNotThrow(() => row.GetColumn<short?>("sint_field"));
                    Assert.DoesNotThrow(() => row.GetColumn<long?>("bint_field"));
                    Assert.DoesNotThrow(() => row.GetColumn<HBigDecimal>("decimal_field"));
                    Assert.DoesNotThrow(() => row.GetColumn<float?>("float_field"));
                    Assert.DoesNotThrow(() => row.GetColumn<double?>("double_field"));
                    Assert.DoesNotThrow(() => row.GetColumn<HLocalDate?>("date_field"));
                    Assert.DoesNotThrow(() => row.GetColumn<HLocalTime?>("time_field"));
                    Assert.DoesNotThrow(() => row.GetColumn<HLocalDateTime?>("times_field"));
                    Assert.DoesNotThrow(() => row.GetColumn<HOffsetDateTime?>("times_zone_field"));
                }
                else
                {
                    Assert.Fail("Unknown assertion request.");
                }
            }

            // record with values
            await Client.Sql.ExecuteCommandAsync($"INSERT INTO {map.Name} VALUES " +
                                                 $"(1, 'some  string', 10,false,11,12,13,14.1,15.1,16.1,'2024-01-29'," +
                                                 $" '10:15:30','2024-01-29 10:11:00','2024-01-29T10:11:00+03:00')");

            // record with null values
            await Client.Sql.ExecuteCommandAsync($"INSERT INTO {map.Name} VALUES " +
                                                 $"(2, null, null,null,null,null,null,null,null,null,null," +
                                                 $" null,null,null)");


            await using var notNullResult = await Client.Sql.ExecuteQueryAsync($"SELECT * FROM {map.Name} WHERE __key = 1");
            await foreach (var row in notNullResult)
                AssertColumns(row, "notnull");


            await using var nullResult = await Client.Sql.ExecuteQueryAsync($"SELECT * FROM {map.Name} WHERE __key = 2");
            await foreach (var row in nullResult)
                AssertColumns(row, "null");


            await using var mixedResult = await Client.Sql.ExecuteQueryAsync($"SELECT * FROM {map.Name} WHERE __key IN (1, 2)");
            await foreach (var row in mixedResult)
                AssertColumns(row, "mixed");
        }
    }
}
