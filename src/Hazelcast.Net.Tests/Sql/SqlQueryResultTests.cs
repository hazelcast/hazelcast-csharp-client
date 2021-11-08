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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Sql;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Testing;
using Hazelcast.Testing.Remote;
using Hazelcast.Tests.Networking;
using Hazelcast.Tests.TestObjects;
using NUnit.Framework;

namespace Hazelcast.Tests.Sql
{
    [TestFixture]
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
    }
}
