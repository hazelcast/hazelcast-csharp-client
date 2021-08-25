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

using System.Collections.Generic;
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
        public async Task GenerateSeries()
        {
            var count = 10;
            await using var result = Client.Sql.ExecuteQuery($"SELECT v FROM TABLE(generate_series(1,{count}))");

            var expectedValues = Enumerable.Range(1, count);
            var resultValues = await result.Select(r => r.GetColumn<int>("v")).ToListAsync();

            CollectionAssert.AreEquivalent(expectedValues, resultValues);
        }

        [Test]
        public async Task GenerateStream()
        {
            var (speed, take) = (10, 15);
            await using var result = Client.Sql.ExecuteQuery($"SELECT v FROM TABLE(generate_stream({speed}))");

            var expectedValues = Enumerable.Range(0, take).Select(i => (long)i).ToArray();
            var resultValues = await result.Take(take).Select(r => r.GetColumn<long>("v")).ToListAsync();

            CollectionAssert.AreEquivalent(expectedValues, resultValues);
        }

        [Test]
        public async Task CreateInsertMapping()
        {
            var mapName = GenerateMapName();
            var insertRow = (key: 1, value: -1);

            await using var createCommand= Client.Sql.ExecuteCommand(
                $@"CREATE MAPPING {mapName} (__key INTEGER, this INTEGER) TYPE IMap OPTIONS (
                    'keyFormat'='integer',
                    'valueFormat'='integer')"
            );
            var createRowsCount = await createCommand.Execution;
            Assert.AreEqual(expected: 0, createRowsCount);

            await using var insertCommand = Client.Sql.ExecuteCommand(
                $@"INSERT INTO {mapName} VALUES ({insertRow.key}, {insertRow.value})"
            );
            var insertRowsCount = await insertCommand.Execution;
            //Assert.AreEqual(expected: 1, insertRowsCount);

            await using var map = await Client.GetMapAsync<int, int>(mapName);
            var mapValue = await map.GetAsync(1);
            Assert.AreEqual(expected: insertRow.value, mapValue);
        }

        [Test]
        public async Task Update()
        {
            var values = new Dictionary<int, int>
            {
                { 1, 1 },
                { 2, 2 },
                { 3, 3 }
            };
            var update = (key: 2, value: -2);

            await using var map = await Client.GetMapAsync<int, int>(GenerateMapName());
            await map.SetAllAsync(values);

            await using var command = Client.Sql.ExecuteCommand(
                $@"UPDATE {map.Name} SET this = {update.value} WHERE __key = {update.key}"
            );
            var updateRowsCount = await command.Execution;
            //Assert.AreEqual(expected: 1, updateRowsCount);

            values[update.key] = update.value;

            var mapValues = await map.GetEntriesAsync();
            CollectionAssert.AreEquivalent(expected: values, mapValues);
        }

        [Test]
        public async Task Delete()
        {
            var values = new Dictionary<int, int>
            {
                { 1, 1 },
                { 2, 2 },
                { 3, 3 }
            };
            var deleteKey = 2;

            await using var map = await Client.GetMapAsync<int, int>(GenerateMapName());
            await map.SetAllAsync(values);

            await using var command = Client.Sql.ExecuteCommand(
                $@"DELETE FROM {map.Name} WHERE __key = {deleteKey}"
            );
            var deleteRowsCount = await command.Execution;
            //Assert.AreEqual(expected: 1, deleteRowsCount);

            values.Remove(deleteKey);

            var mapValues = await map.GetEntriesAsync();
            CollectionAssert.AreEquivalent(expected: values, mapValues);
        }

        [Test]
        public async Task SelectCount()
        {
            var count = 10;
            await using var map = await CreateIntMapAsync(count);

            await using var result = Client.Sql.ExecuteQuery($"SELECT COUNT(*) FROM {map.Name}");
            var selectCount = await result.Select(r => r.GetColumn<long>(0)).SingleAsync();

            Assert.AreEqual(expected: count, selectCount);
        }

        [Test]
        public async Task SelectSum()
        {
            var count = 10;
            await using var map = await CreateIntMapAsync(count);

            await using var result = Client.Sql.ExecuteQuery($"SELECT SUM(__key) FROM {map.Name}");
            var selectSum = await result.Select(r => r.GetColumn<long>(0)).SingleAsync();

            var expectedSum = GenerateIntMapValues(count).Sum(p => p.Key);
            Assert.AreEqual(expectedSum, selectSum);
        }

        [Test]
        public async Task SelectMax()
        {
            var count = 10;
            await using var map = await CreateIntMapAsync(count);

            await using var result = Client.Sql.ExecuteQuery($"SELECT MAX(__key) FROM {map.Name}");
            var selectSum = await result.Select(r => r.GetColumn<int>(0)).SingleAsync();

            var expectedSum = GenerateIntMapValues(count).Max(p => p.Key);
            Assert.AreEqual(expectedSum, selectSum);
        }
    }
}
