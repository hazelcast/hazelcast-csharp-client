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
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Linq;
using Hazelcast.Serialization.Compact;
using Hazelcast.Sql;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact;

[TestFixture]
public class CompactSqlTests : SingleMemberRemoteTestBase
{
    protected override string RcClusterConfiguration => Resources.jet_enabled;

    [TestCase(null, null)]
    [TestCase("aaaa", "bbbb")]
    [TestCase("select", "value")]
    public async Task CompactSqlTest(string nameFieldName, string valueFieldName)
    {
        nameFieldName ??= nameof(Thing.Name).ToLowerInvariant();
        valueFieldName ??= nameof(Thing.Value).ToLowerInvariant();

        await using var client1 = await CreateAndStartClientAsync(options =>
        {
            options.Serialization.Compact.AddSerializer(new ThingSerializer(nameFieldName, valueFieldName));
        });

        await using var map1 = await client1.GetMapAsync<string, Thing>(CreateUniqueName());

        for (var i = 0; i < 32; i++)
            await map1.SetAsync($"entry-{i}", new Thing { Name = $"thing-{i}", Value = i });

        // that does not work yet for compact objects
        //await client1.Sql.CreateMapping(map1);

        // note that we underscore the Thing.Value property as Value is an illegal name here
        // 

        var mappingCommand = $@"
            CREATE MAPPING {map1.Name}
            (
              {nameof(Thing.Name)} VARCHAR EXTERNAL NAME ""{nameFieldName}"",
              _{nameof(Thing.Value)} INT EXTERNAL NAME ""{valueFieldName}""
            )
            TYPE IMAP
            OPTIONS (
              'keyFormat'='varchar',
              'valueFormat'='compact', 'valueCompactTypeName'='thing'
            )";

        await client1.Sql.ExecuteCommandAsync(mappingCommand);

        await using var result1 = await client1.Sql.ExecuteQueryAsync($"SELECT __key, this FROM {map1.Name}");
        await foreach (var x in result1)
        {
            var key = x.GetKey<string>();
            var thing = x.GetValue<Thing>();
            Console.WriteLine($"{key}: ( {nameof(Thing.Name)}='{thing.Name}', {nameof(Thing.Value)}={thing.Value} )");
        }

        Console.WriteLine("--");

        var linqResult = map1.AsAsyncQueryable();
        await foreach (var (key, thing) in linqResult)
        {
            Console.WriteLine($"{key}: ( {nameof(Thing.Name)}='{thing.Name}', {nameof(Thing.Value)}={thing.Value} )");
        }

        Console.WriteLine("--");

        // now switch to another client which does not have the Thing schema yet,
        // and run a bare SQL query... we'll receive compact objects and must ensure
        // that they are properly deserialized.

        await using var client2 = await CreateAndStartClientAsync(options =>
        {
            options.Serialization.Compact.AddSerializer(new ThingSerializer(nameFieldName, valueFieldName));
        });

        await using var result2 = await client2.Sql.ExecuteQueryAsync($"SELECT __key, this FROM {map1.Name}");
        await foreach (var x in result2)
        {
            var key = x.GetKey<string>();
            var thing = x.GetValue<Thing>();
            Console.WriteLine($"{key}: ( {nameof(Thing.Name)}='{thing.Name}', {nameof(Thing.Value)}={thing.Value} )");
        }

        Console.WriteLine("--");

        // now switch to another client which does not have the Thing schema yet,
        // and run a LINQ SQL query... we'll receive compact objects and must ensure
        // that they are properly deserialized.

        await using var client3 = await CreateAndStartClientAsync(options =>
        {
            options.Serialization.Compact.AddSerializer(new ThingSerializer(nameFieldName, valueFieldName));
        });

        await using var map3 = await client3.GetMapAsync<string, Thing>(map1.Name);
        await foreach (var (key, thing) in map3.AsAsyncQueryable())
        {
            Console.WriteLine($"{key}: ( {nameof(Thing.Name)}='{thing.Name}', {nameof(Thing.Value)}={thing.Value} )");
        }
    }

    private class Thing
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    private class ThingSerializer : ICompactSerializer<Thing>
    {
        private readonly string _nameFieldName;
        private readonly string _valueFieldName;

        public ThingSerializer(string nameFieldName, string valueFieldName)
        {
            _nameFieldName = nameFieldName;
            _valueFieldName = valueFieldName;
        }

        public string TypeName => "thing";

        public Thing Read(ICompactReader reader)
        {
            return new Thing
            {
                Name = reader.ReadString(_nameFieldName),
                Value = reader.ReadInt32(_valueFieldName)
            };
        }

        public void Write(ICompactWriter writer, Thing value)
        {
            writer.WriteString(_nameFieldName, value.Name);
            writer.WriteInt32(_valueFieldName, value.Value);
        }
    }
}