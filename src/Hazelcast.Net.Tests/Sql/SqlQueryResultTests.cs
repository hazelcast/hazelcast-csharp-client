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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Sql;
using NUnit.Framework;

namespace Hazelcast.Tests.Sql
{
    [TestFixture]
    public class SqlQueryResultTests : SqlTestBase
    {
        // Needed to create long-running query
        protected override bool EnableJet => true;

        [Test]
        public async Task EnumerateAfterDispose()
        {
            await using var map = await CreateIntMapAsync(size: 10);

            var result = Client.Sql.ExecuteQuery($"SELECT * FROM {map.Name}");
            await result.DisposeAsync();

            Assert.Throws<ObjectDisposedException>(() => result.GetAsyncEnumerator());
        }

        [Test]
        public async Task EnumerateMultipleTimes()
        {
            await using var map = await CreateIntMapAsync(size: 10);

            await using var result = Client.Sql.ExecuteQuery($"SELECT * FROM {map.Name} ORDER BY 1");

            var values1 = await result.Take(5).ToListAsync();
            var values2 = await result.Take(5).ToListAsync();

            CollectionAssert.AreEqual(
                expected: GenerateIntMapValues(size: 10).Keys.OrderBy(v => v).ToList(),
                actual: values1.Concat(values2).Select(r => r.GetColumn<int>(0))
            );

            var values3 = await result.ToListAsync();

            CollectionAssert.IsEmpty(values3);
        }

        [Test]
        public async Task EnumeratorCancellation()
        {
            ISqlQueryResult result;
            await using (result = Client.Sql.ExecuteQuery("SELECT * FROM TABLE(generate_stream(10))"))
            {
                Assert.ThrowsAsync<OperationCanceledException>(async () =>
                {
                    using var cancellationSource = new CancellationTokenSource(50);
                    var enumerator = result.GetAsyncEnumerator(cancellationSource.Token);
                    while (await enumerator.MoveNextAsync())
                    {
                        var row = enumerator.Current;
                        if (row.GetColumn<long>(0) > 5)
                            break;
                    }
                });
            }
        }

        [Test]
        public async Task EnumerateWithCancellation()
        {
            ISqlQueryResult result;
            await using (result = Client.Sql.ExecuteQuery("SELECT * FROM TABLE(generate_stream(10))"))
            {
                Assert.ThrowsAsync<OperationCanceledException>(async () =>
                {
                    using var cancellationSource = new CancellationTokenSource(50);
                    await foreach (var row in result.WithCancellation(cancellationSource.Token))
                    {
                        if (row.GetColumn<long>(0) > 5)
                            break;
                    }
                });
            }
        }

        [Test]
        public async Task EnumerateToListCancellation()
        {
            ISqlQueryResult result;
            await using (result = Client.Sql.ExecuteQuery("SELECT * FROM TABLE(generate_stream(10))"))
            {
                Assert.ThrowsAsync<OperationCanceledException>(async () =>
                {
                    using var cancellationSource = new CancellationTokenSource(50);
                    await result.Take(5).ToListAsync(cancellationSource.Token);
                });
            }
        }

        [Test]
        public async Task DisposeMultipleTimes()
        {
            await using var map = await CreateIntMapAsync(size: 10);

            var result = Client.Sql.ExecuteQuery($"SELECT * FROM {map.Name}");
            await result.DisposeAsync();

            Assert.DoesNotThrowAsync(async () =>
            {
                await result.DisposeAsync();
                await result.DisposeAsync();
            });
        }

        [Test]
        public async Task DisposeDuringQuery()
        {
            var result = Client.Sql.ExecuteQuery("SELECT * FROM TABLE(generate_stream(10))", new SqlStatementOptions
            {
                CursorBufferSize = 1
            });

            var enumerator = result.GetAsyncEnumerator();
            var moveNextTask = enumerator.MoveNextAsync();

            await Task.Delay(millisecondsDelay: 10); // wait for query to reach the server
            await result.DisposeAsync();

            Assert.ThrowsAsync<HazelcastSqlException>(() => moveNextTask.AsTask());
        }
    }
}
