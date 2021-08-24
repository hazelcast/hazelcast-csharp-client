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

using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Sql;
using NUnit.Framework;

namespace Hazelcast.Tests.Sql
{
    [TestFixture]
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
            await using var map = await CreateIntMapAsync(total);

            var result = Client.Sql.ExecuteQuery($"SELECT * FROM {map.Name} ORDER BY __key",
                options: new SqlStatementOptions { CursorBufferSize = pageSize }
            );

            var expectedValues = GenerateIntMapValues(total);
            var resultValues = await result.EnumerateOnceAsync().ToDictionaryAsync(r => r.GetKey<int>(), r => r.GetValue<string>());

            CollectionAssert.AreEquivalent(expectedValues, resultValues);
        }

        [Test]
        [TestCase(10, 0)]
        [TestCase(10, 1)]
        [TestCase(10, 5)]
        [TestCase(10, 100)]
        public async Task ExecuteQueryWithParameter(int total, int minValue)
        {
            await using var map = await CreateIntMapAsync(total);

            var result = Client.Sql.ExecuteQuery($"SELECT * FROM {map.Name} WHERE __key >= ?", minValue);

            var expectedValues = GenerateIntMapValues(total).Where(p => p.Key >= minValue);
            var resultValues = await result.EnumerateOnceAsync().ToDictionaryAsync(r => r.GetKey<int>(), r => r.GetValue<string>());

            CollectionAssert.AreEquivalent(expectedValues, resultValues);
        }
    }
}