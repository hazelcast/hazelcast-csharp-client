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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Linq;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using NUnit.Framework;

namespace Hazelcast.Tests.Linq
{
    /// <summary>
    /// Tests the IHMap using LINQ APIs. It focuses the results mainly.
    /// For query tests, please see other linq test suites.
    /// </summary>
    [ServerCondition("[5.0,)")] // only on server 5.0 and above
    public class LinqTests : SingleMemberClientRemoteTestBase
    {
        protected override string RcClusterConfiguration => Resources.Cluster_JetEnabled;

        public class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string LastName { get; set; }
        }

        public class Address
        {
            public int PostCode { get; set; }
            public string Country { get; set; }
        }

        private IHMap<int, Person> _map;
        private const int SizeOfMap = 30;

        [OneTimeSetUp]
        public async Task Up()
        {
            HConsole.Configure(x => x.Configure<LinqTests>().SetPrefix("LINQ"));

            _map = await Client.GetMapAsync<int, Person>("personMap");

            // Creating the mapping as compact type. As long as server can deserialize the fields,
            // it doesn't matter for us. Fields become columns, we interest with columns.
            // Note: Column names and fields names should match exactly. Naming is case sensitive.
            await Client.Sql.ExecuteCommandAsync(
                "CREATE MAPPING \"personMap\" (Id int, Name varchar, LastName varchar) " +
                "TYPE IMap " +
                "OPTIONS ('keyFormat' = 'int'," +
                "'keyCompactTypeName' = 'personId'," +
                "'valueFormat' = 'compact'," +
                "'valueCompactTypeName' = 'person')");

            for (var i = 0; i < SizeOfMap; i++)
            {
                await _map.PutAsync(i, new Person() {Id = i, Name = "PersonName " + i, LastName = "LastName " + i});
            }
        }

        [OneTimeTearDown]
        public async Task Down()
        {
            await _map.DestroyAsync();
        }

        [Test]
        public async Task TestLinqWhere()
        {
            var result = _map.AsAsyncQueryable()
                .Where(p => p.Key > SizeOfMap - 11);

            var count = 0;

            await foreach (var entry in result)
            {
                Assert.IsAssignableFrom<MapEntry<int, Person>>(entry);
                count++;
            }

            Assert.AreEqual(10, count);
        }

        [Test]
        public async Task TestLinqWithDefault()
        {
            var result = _map.AsAsyncQueryable();
            var count = 0;

            await foreach (var entry in result)
            {
                Assert.IsAssignableFrom<MapEntry<int, Person>>(entry);
                count++;
            }

            Assert.AreEqual(SizeOfMap, count);
        }

        [Test]
        public async Task TestLinqWithPrimitivesTypes()
        {
            var myMap = await Client.GetMapAsync<int, string>("myBasicMap");
            await myMap.PutAsync(1, "some string");
            var count = 0;

            await Client.Sql.ExecuteCommandAsync(
                "CREATE MAPPING \"myBasicMap\" " +
                "TYPE IMap " +
                "OPTIONS ('keyFormat' = 'int'," +
                "'valueFormat' = 'varchar')");

            await foreach (var entry in myMap.AsAsyncQueryable())
            {
                Assert.IsAssignableFrom<MapEntry<int, string>>(entry);
                count++;
            }

            Assert.AreEqual(1, count);
        }

        [Test]
        public async Task TestLinqByValueType()
        {
            var result = _map.AsAsyncQueryable()
                .Where(p => p.Value.Id < SizeOfMap - 10);

            var count = 0;

            await foreach (var entry in result)
            {
                Assert.IsAssignableFrom<MapEntry<int, Person>>(entry);
                Assert.AreEqual(entry.Key, entry.Value.Id);
                count++;
            }

            Assert.AreEqual(SizeOfMap - 10, count);
        }

        [Test]
        public async Task TestLinqByValueTypeWithString()
        {
            var result = _map.AsAsyncQueryable()
                .Where(p => p.Value.Name == "PersonName 1");

            var count = 0;

            await foreach (var entry in result)
            {
                Assert.IsAssignableFrom<MapEntry<int, Person>>(entry);
                Assert.AreEqual(entry.Key, entry.Value.Id);
                count++;
            }

            Assert.AreEqual(1, count);
        }

        [TestCase(true, ExpectedResult = 10)]
        [TestCase(false, ExpectedResult = SizeOfMap)]
        public async Task<int> TestLinqProjection(bool addCondition)
        {
            var query = _map.AsAsyncQueryable();

            if (addCondition)
                query = query.Where(p => p.Key >= SizeOfMap - 10);

            var queryWithNewType = query.Select(p => new Person() {Name = p.Value.Name});

            var count = 0;

            await foreach (var entry in queryWithNewType)
            {
                Assert.IsAssignableFrom<Person>(entry);
                Assert.That(entry.Id, Is.Zero);
                Assert.That(entry.Name, Is.Not.Empty);
                Assert.That(entry.LastName, Is.Empty.Or.Null);
                count++;
            }

            return count;
        }

        [TestCase(true, ExpectedResult = 10)]
        [TestCase(false, ExpectedResult = SizeOfMap)]
        public async Task<int> TestLinqProjectionDynamicType(bool addCondition)
        {
            var query = _map.AsAsyncQueryable();

            if (addCondition)
                query = query.Where(p => p.Key >= SizeOfMap - 10);

            var queryWithNewType = query.Select(p => new {nm = p.Value.Name});

            var count = 0;

            await foreach (var entry in queryWithNewType)
            {
                Assert.IsNotAssignableFrom<Person>(entry);
                Assert.That(entry.nm, Is.Not.Empty);
                count++;
            }

            return count;
        }

        [TestCase(true, ExpectedResult = 10)]
        [TestCase(false, ExpectedResult = SizeOfMap)]
        public async Task<int> TestLinqProjectionPrimitiveType(bool addCondition)
        {
            var query = _map.AsAsyncQueryable();

            if (addCondition)
                query = query.Where(p => p.Key > SizeOfMap - 11);

            var queryWithNewType = query.Select(p => p.Key);

            var count = 0;

            await foreach (var entry in queryWithNewType)
            {
                Assert.AreEqual(typeof(int), entry.GetType());
                Assert.That(entry, Is.GreaterThanOrEqualTo(0));
                count++;
            }

            return count;
        }

        [Test]
        public void TestCannotEnumerateTwice()
        {
            var query = _map.AsAsyncQueryable();
            var _ = query.GetAsyncEnumerator();
            Assert.Throws<InvalidOperationException>(() => _ = query.GetAsyncEnumerator());
        }

        [Test]
        public async Task TestFullComplexTypeMapping()
        {
            async Task AssertSqlResult(IAsyncEnumerable<MapEntry<Address, Person>> queryable)
            {
                await foreach (var e in queryable)
                    Assert.AreEqual(e.Key.PostCode, 33090);
            }

            var map = await Client.GetMapAsync<Address, Person>("addressPersonMap");

            await Client.Sql.ExecuteCommandAsync(
                "CREATE MAPPING \"addressPersonMap\" " +
                "(Id int, Name varchar, LastName varchar, Country varchar EXTERNAL NAME \"__key.Country\", PostCode int  EXTERNAL NAME \"__key.PostCode\") " +
                "TYPE IMap " +
                "OPTIONS ('keyFormat' = 'compact'," +
                "'keyCompactTypeName' = 'address'," +
                "'valueFormat' = 'compact'," +
                "'valueCompactTypeName' = 'person')");

            var key = new Address() {Country = "TR", PostCode = 33090};
            var val = new Person() {Id = 1, Name = "pName", LastName = "lName"};

            await map.PutAsync(key, val);

            // Case 1: Comparision between columns.
            await AssertSqlResult(map.AsAsyncQueryable().Where(p => p.Key.PostCode > p.Value.Id));

            // Case 2: Query by key fields.
            await AssertSqlResult(map.AsAsyncQueryable().Where(p => p.Key.PostCode == 33090));

            // Case 3: Query by value fields.
            await AssertSqlResult(map.AsAsyncQueryable().Where(p => p.Value.Id == 1));

            await map.DestroyAsync();
        }
    }
}
