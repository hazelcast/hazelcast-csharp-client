// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Serialization.Compact;
using Hazelcast.Sql;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using NUnit.Framework;

namespace Hazelcast.Tests.Sql;

[ServerCondition("[5.3,)")] // only on server 5.3 and above
public class SqlPartitionAwareTests : SqlTestBase
{
    protected override bool EnableJet => true;

    [TestCase("SELECT * FROM ${MAP_NAME} WHERE __key=?", 0, new object[] {10}, false)]
    [TestCase("DELETE FROM ${MAP_NAME} WHERE __key=?", 0, new object[] {10}, true)]
    [TestCase("UPDATE ${MAP_NAME} SET this = ? WHERE __key = ?", 1, new object[] {"111", 111}, true)]
    [TestCase("INSERT INTO ${MAP_NAME} (__key, this) VALUES (?, ?)", 0, new object[] {10, "10"}, true)]
    [TestCase("INSERT INTO ${MAP_NAME} (this, __key) VALUES (?, ?)", 1, new object[] {"10", 10}, true)]
    [TestCase("INSERT INTO ${MAP_NAME} (__key, this) VALUES (101, '101')", -1, null, true)]
    [TestCase("INSERT INTO ${MAP_NAME} (this, __key) VALUES ('102', 102)", -1, null, true)]
    public async Task TestArgumentIndex(string query, int expectedArgumentIndex, object[] args, bool noResult)
    {
        var entries = Enumerable.Range(1, 100).ToDictionary(i => i, i => i.ToString());

        await using var map = await Client.GetMapAsync<int, string>("partition_map");

        query = query.Replace("${MAP_NAME}", map.Name);
        await Client.Sql.ExecuteCommandAsync($"CREATE OR REPLACE MAPPING {map.Name} TYPE IMap OPTIONS ('keyFormat'='int', 'valueFormat'='varchar')");

        var sqlService = (SqlService) Client.Sql;

        // No argument index at the beginning. 
        Assert.False(sqlService._queryPartitionArgumentCache.TryGetValue(query, out var argIndex));
        Assert.AreEqual(0, argIndex);

        if (noResult)
            await Client.Sql.ExecuteCommandAsync(query, args);
        else
            await Client.Sql.ExecuteQueryAsync(query, args);

        // Means query is not appropriate for argument indexing
        if (expectedArgumentIndex != -1)
        {
            Assert.Greater(sqlService._queryPartitionArgumentCache.Cache.Count, 0);
            Assert.True(sqlService._queryPartitionArgumentCache.TryGetValue(query, out argIndex));
            // Argument index is received with query result.
            Assert.AreEqual(expectedArgumentIndex, argIndex);
        }
        else
        {
            // Cache returns default value of int which is 0 if no record found.
            Assert.False(sqlService._queryPartitionArgumentCache.TryGetValue(query, out argIndex));
            Assert.AreEqual(0, argIndex);
        }

        await map.DestroyAsync();
    }

    [TestCase("SELECT * FROM ${MAP_NAME} WHERE __key=?", 0, new object[] {10}, false)]
    [TestCase("DELETE FROM ${MAP_NAME} WHERE __key=?", 0, new object[] {10}, true)]
    [TestCase("UPDATE ${MAP_NAME} SET number = ? WHERE __key = ?", 1, new object[] {111, 111}, true)]
    [TestCase("INSERT INTO ${MAP_NAME} (__key, number, text) VALUES (?, ?,?)", 0, new object[] {10, 10,"10"}, true)]
    [TestCase("INSERT INTO ${MAP_NAME} (number, text, __key) VALUES (?, ?,?)", 2, new object[] {10, "10", 10}, true)]
    [TestCase("INSERT INTO ${MAP_NAME} (__key, text) VALUES (101, '101')", -1, null, true)]
    [TestCase("INSERT INTO ${MAP_NAME} (text, __key) VALUES ('102', 102)", -1, null, true)]
    public async Task TestArgumentIndexWithComplexType(string query, int expectedArgumentIndex, object[] args, bool noResult)
    {
        var _client = await CreateAndStartClientAsync((conf) =>
        {
            conf.Serialization.Compact.AddSerializer<MyClass>(new MyClassSerializer());
        });

        await using var map = await _client.GetMapAsync<int, MyClass>("complex_partition_map");

        query = query.Replace("${MAP_NAME}", map.Name);
        await Client.Sql.ExecuteCommandAsync($@"CREATE OR REPLACE MAPPING {map.Name} (
            number INT,
            text VARCHAR)
            TYPE IMap
            OPTIONS (
                'keyFormat' = 'int',
                'valueFormat' = 'compact',
                'valueCompactTypeName' = 'myclass'
        )");

        var sqlService = (SqlService) Client.Sql;

        // No argument index at the beginning. 
        Assert.False(sqlService._queryPartitionArgumentCache.TryGetValue(query, out var argIndex));
        Assert.AreEqual(0, argIndex);

        if (noResult)
            await Client.Sql.ExecuteCommandAsync(query, args);
        else
            await Client.Sql.ExecuteQueryAsync(query, args);

        // Means query is not appropriate for argument indexing
        if (expectedArgumentIndex != -1)
        {
            Assert.Greater(sqlService._queryPartitionArgumentCache.Cache.Count, 0);
            Assert.True(sqlService._queryPartitionArgumentCache.TryGetValue(query, out argIndex));
            // Argument index is received with query result.
            Assert.AreEqual(expectedArgumentIndex, argIndex);
        }
        else
        {
            // Cache returns default value of int which is 0 if no record found.
            Assert.False(sqlService._queryPartitionArgumentCache.TryGetValue(query, out argIndex));
            Assert.AreEqual(0, argIndex);
        }

        await map.DestroyAsync();
    }

    private class MyClass
    {
        public int Number { get; set; }
        public string Text { get; set; }
    }

    private class MyClassSerializer : ICompactSerializer<MyClass>
    {
        public string TypeName => "myclass";

        public MyClass Read(ICompactReader reader)
        {
            return new MyClass()
            {
                Number = reader.ReadInt32("number"),
                Text = reader.ReadString("text")
            };
        }

        public void Write(ICompactWriter writer, MyClass value)
        {
            writer.WriteString("text", value.Text);
            writer.WriteInt32("number", value.Number);
        }
    }
}
