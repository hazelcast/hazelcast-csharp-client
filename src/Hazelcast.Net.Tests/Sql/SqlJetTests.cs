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
using NUnit.Framework;

namespace Hazelcast.Tests.Sql
{
    [TestFixture]
    public class SqlJetTests : SqlTestBase
    {
        protected override bool EnableJet => true;

        [Test]
        public void GenerateSeries()
        {
            var count = 10;
            var result = Client.Sql.ExecuteQuery($"SELECT v FROM TABLE(generate_series(1,{count}))");

            var expectedValues = Enumerable.Range(1, count);
            var resultValues = result.EnumerateOnce().Select(r => r.GetColumn<int>("v"));

            CollectionAssert.AreEquivalent(expectedValues, resultValues);
        }

        [Test]
        public void GenerateStream()
        {
            var (speed, take) = (10, 15);
            var result = Client.Sql.ExecuteQuery($"SELECT v FROM TABLE(generate_stream({speed}))");

            var expectedValues = Enumerable.Range(0, take).Select(i => (long)i).ToArray();
            var resultValues = result.EnumerateOnce().Take(take).Select(r => r.GetColumn<long>("v")).ToArray();

            CollectionAssert.AreEquivalent(expectedValues, resultValues);
        }

        [Test]
        public async Task CreateInsertMapping()
        {
            var mapName = GenerateMapName();
            var insertRow = (key: 1, value: -1);

            var createRowsCount = await Client.Sql.ExecuteCommand(
                $@"CREATE MAPPING {mapName} (__key INTEGER, this INTEGER) TYPE IMap OPTIONS (
                    'keyFormat'='integer',
                    'valueFormat'='integer')"
            ).Execution;
            Assert.AreEqual(expected: 0, createRowsCount);

            var insertRowsCount = await Client.Sql.ExecuteCommand(
                $@"INSERT INTO {mapName} VALUES ({insertRow.key}, {insertRow.value})"
            ).Execution;
            //Assert.AreEqual(expected: 1, insertRowsCount);

            await using var map = await Client.GetMapAsync<int, int>(mapName);
            var mapValue = await map.GetAsync(1);
            Assert.AreEqual(expected: insertRow.value, mapValue);
        }

        [Test]
        public async Task SelectCount()
        {
            var count = 10;
            await using var map = await CreateIntMapAsync(count);

            await using var result = Client.Sql.ExecuteQuery($"SELECT COUNT(*) FROM {map.Name}");
            var selectCount = result.EnumerateOnce().Select(r => r.GetColumn<long>(0)).Single();

            Assert.AreEqual(expected: count, selectCount);
        }

        [Test]
        public async Task SelectSum()
        {
            var count = 10;
            await using var map = await CreateIntMapAsync(count);

            await using var result = Client.Sql.ExecuteQuery($"SELECT SUM(__key) FROM {map.Name}");
            var selectSum = result.EnumerateOnce().Select(r => r.GetColumn<long>(0)).Single();

            var expectedSum = GenerateIntMapValues(count).Sum(p => p.Key);
            Assert.AreEqual(expectedSum, selectSum);
        }

        [Test]
        public async Task SelectMax()
        {
            var count = 10;
            await using var map = await CreateIntMapAsync(count);

            await using var result = Client.Sql.ExecuteQuery($"SELECT MAX(__key) FROM {map.Name}");
            var selectSum = result.EnumerateOnce().Select(r => r.GetColumn<int>(0)).Single();

            var expectedSum = GenerateIntMapValues(count).Max(p => p.Key);
            Assert.AreEqual(expectedSum, selectSum);
        }
    }
}