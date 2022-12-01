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
using System.Reflection;
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;
using Hazelcast.Linq;
using Hazelcast.Testing;
using Hazelcast.Tests.Sql;
using NUnit.Framework;
using MemberInfo = Hazelcast.Models.MemberInfo;

namespace Hazelcast.Tests.Linq
{
    public class LinqSimpleTests : SqlTestBase
    {
        public class DummyType
        {
            public string ColumnString { get; set; }
            public double ColumnDouble { get; set; }
            public float ColumnFloat { get; set; }
            public int ColumnInt { get; set; }
            public long ColumnLong { get; set; }
            public bool ColumnBool { get; set; }
        }

        private IHMap<int, string> _map;

        [OneTimeSetUp]
        public async Task Up()
        {
            _map = await Client.GetMapAsync<int, string>("linqMap1");

            await Client.Sql.ExecuteCommandAsync(
                "CREATE MAPPING \"linqMap1\" TYPE IMap OPTIONS ('keyFormat' = 'int','valueFormat' = 'varchar')");
            
            for (var i = 0; i < 100; i++)
            {
                await _map.PutAsync(i, "VALUE: " + i);
            }
        }

        [Test]
        public async Task TestLinq()
        {
            var result = _map.AsAsyncQueryable().Where(p => p.Key > 90).Select(p=> p.Key);

            await foreach (var entry in result)
            {
                Console.WriteLine($"Value: {entry}");
                //Console.WriteLine($"Key: {entry.Key}, Value: {entry.Value}");
            }
        }

        [Test]
        public async Task T()
        {
            var result = await 
                Client.Sql.ExecuteQueryAsync("SELECT m0.__key, m0.this FROM linqMap1 m0 WHERE (m0.__key > ?)", new object[]{90});
            
            await  foreach(var row in result)
                Console.WriteLine(row.GetValue<string>());
            
        }
    }
}
