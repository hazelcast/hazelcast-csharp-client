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

using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Sql;
using NUnit.Framework;

namespace Hazelcast.Tests.Sql
{
    [TestFixture]
    public class SqlServiceTests: SqlTestBase
    {
        [Test]
        [TestCase(3, 1)]
        [TestCase(3, 3)]
        [TestCase(3, 5)]
        [TestCase(5, 2)]
        [TestCase(6, 3)]
        public async Task ExecuteQueryMap(int total, int pageSize)
        {
            Debug.Assert(pageSize <= MapValues.Count);

            var sqlService = await Client.GetSqlServiceAsync();

            var result = await sqlService.ExecuteQueryAsync($"SELECT * FROM {MapName} ORDER BY __key LIMIT {total}",
                options: new SqlStatementOptions { CursorBufferSize = pageSize }
            );

            var expectedValues = MapValues.OrderBy(p => p.Key).Take(total).ToDictionary();
            var resultValues = result.EnumerateOnce().ToDictionary(r => r.GetKey<string>(), r => r.GetValue<int>());

            CollectionAssert.AreEquivalent(expectedValues, resultValues);
        }

        [Test]
        [TestCase(3, 1)]
        [TestCase(3, 3)]
        [TestCase(3, 5)]
        [TestCase(5, 2)]
        [TestCase(6, 3)]
        public async Task ExecuteQueryJet(int total, int pageSize)
        {
            var sqlService = await Client.GetSqlServiceAsync();

            var result = await sqlService.ExecuteQueryAsync($"SELECT v FROM TABLE(generate_series(1,{total}))",
                options: new SqlStatementOptions { CursorBufferSize = pageSize }
            );

            var expectedValues = Enumerable.Range(1, total);
            var resultValues = result.EnumerateOnce().Select(r => r.GetColumn<int>("v"));

            CollectionAssert.AreEquivalent(expectedValues, resultValues);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(100)]
        public async Task ExecuteQueryWithIntParameter(int minValue)
        {
            var sql = await Client.GetSqlServiceAsync();
            var result = await sql.ExecuteQueryAsync($"SELECT * FROM {MapName} WHERE this >= ?", new object[] { minValue });

            var expectedValues = MapValues.Where(p => p.Value >= minValue);
            var resultValues = result.EnumerateOnce().ToDictionary(r => r.GetKey<string>(), r => r.GetValue<int>());

            CollectionAssert.AreEquivalent(expectedValues, resultValues);
        }

        [Test]
        [TestCase("0")]
        [TestCase("1")]
        [TestCase("5")]
        [TestCase("100")]
        public async Task ExecuteQueryWithStringParameter(string key)
        {
            var sql = await Client.GetSqlServiceAsync();
            var result = await sql.ExecuteQueryAsync($"SELECT * FROM {MapName} WHERE __key = ?", new object[] { key });

            var expectedValues = MapValues.Where(p => p.Key == key);
            var resultValues = result.EnumerateOnce().ToDictionary(r => r.GetKey<string>(), r => r.GetValue<int>());

            CollectionAssert.AreEquivalent(expectedValues, resultValues);
        }
    }
}
