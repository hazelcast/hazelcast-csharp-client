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

using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Sql;
using Hazelcast.Testing.Conditions;
using NUnit.Framework;

namespace Hazelcast.Tests.Sql
{
    [TestFixture]
    [ServerCondition("[5.0,)")] // only on server 5.0 and above
    public class SqlQueryTests : SqlTestBase
    {
        [Test]
        [TestCase(3, 1)]
        [TestCase(3, 3)]
        [TestCase(3, 5)]
        [TestCase(5, 2)]
        [TestCase(6, 3)]
        public async Task ExecuteQuery(int total, int pageSize)
        {
            var entries = Enumerable.Range(1, total).ToDictionary(i => i, i => i.ToString());

            await using var map = await CreateIntMapAsync(entries);

            var result = await Client.Sql.ExecuteQueryAsync($"SELECT * FROM {map.Name} ORDER BY __key",
                options: new SqlStatementOptions { CursorBufferSize = pageSize }
            );

            var resultValues = await result.ToDictionaryAsync(r => r.GetKey<int>(), r => r.GetValue<string>());

            CollectionAssert.AreEquivalent(entries, resultValues);
        }

        [Test]
        [TestCase(10, 0)]
        [TestCase(10, 1)]
        [TestCase(10, 5)]
        [TestCase(10, 100)]
        public async Task ExecuteQueryWithParameter(int total, int minValue)
        {
            var entries = Enumerable.Range(1, total).ToDictionary(i => i, i => i.ToString());

            await using var map = await CreateIntMapAsync(entries);

            var result = await Client.Sql.ExecuteQueryAsync($"SELECT * FROM {map.Name} WHERE __key >= ?", minValue);

            var expectedValues = entries.Where(p => p.Key >= minValue);
            var resultValues = await result.ToDictionaryAsync(r => r.GetKey<int>(), r => r.GetValue<string>());

            CollectionAssert.AreEquivalent(expectedValues, resultValues);
        }
    }
}