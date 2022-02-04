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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    [TestFixture]
    public class RemoteTests : SingleMemberRemoteTestBase
    {
        protected override string RcClusterConfiguration => Resources.jet_enabled;

        [Test]
        public async Task RemoteTest()
        {
            var thingSchema = SchemaBuilder
                .For("thing")
                .WithField(Thing.FieldNameOf.Name, FieldKind.StringRef)
                .WithField(Thing.FieldNameOf.Value, FieldKind.SignedInteger32)
                .Build();

            var options = new HazelcastOptionsBuilder().Build();

            options.ClusterName = RcCluster?.Id ?? options.ClusterName;
            options.Networking.Addresses.Add("127.0.0.1:5701");

            options.Serialization.Compact.Enabled = true;
            options.Serialization.Compact.Register(thingSchema, new ThingCompactSerializer());

            IHazelcastClient client = null;
            IHMap<string, Thing> map = null;

            await using var dispose = new AsyncDisposable(async () =>
            {
                // ReSharper disable AccessToModifiedClosure
                if (client != null)
                {
                    if (map != null)
                    {
                        await client.DestroyAsync(map);
                        await map.DisposeAsync();
                    }
                    await client.DisposeAsync();
                }
                // ReSharper restore AccessToModifiedClosure
            });

            client = await HazelcastClientFactory.StartNewClientAsync(options);

            // we are sending a "thing" object to the server, and the server does not know about the schema
            // because the schema was registered in the configuration, it *is* in the schemas already, but
            // unpublished - we need to publish it first - we have no mechanism to send them on-demand.
            var schemas = ((HazelcastClient)client).SerializationService.CompactSerializer.Schemas;
            Assert.That(schemas.TryGet(thingSchema.Id, out _), Is.True); // make sure it's here (local)
            await schemas.PublishAsync(); // publish

            // validate that the schema is on the server now
            var schemaControl = await ((Schemas)schemas).FetchAsync(thingSchema.Id);
            Assert.That(schemaControl, Is.Not.Null);
            Console.WriteLine($"FOUND SCHEMA ID: {schemaControl.Id} ON CLUSTER.");

            var thing1 = new Thing { Name = "thing1", Value = 1 };
            var thing2 = new Thing { Name = "thing2", Value = 2 };
            var thing3 = new Thing { Name = "thing3", Value = 3 };
            var thing4 = new Thing { Name = "thing4", Value = 4 };

            map = await client.GetMapAsync<string, Thing>("map_" + Guid.NewGuid().ToString("N"));

            await map.SetAsync(thing1.Name, thing1);
            await map.SetAsync(thing2.Name, thing2);
            await map.SetAsync(thing3.Name, thing3);
            await map.SetAsync(thing4.Name, thing4);

            var thing = await map.GetAsync("thing1");
            Assert.That(thing, Is.Not.Null);
            Assert.That(thing.Name, Is.EqualTo(thing1.Name));
            Assert.That(thing.Value, Is.EqualTo(thing1.Value));

            // just to be sure that the server actually & properly handles serialization
            // let's run a SQL query on our map with a WHERE condition on the actual columns

            // SQL requires a mapping
            // unfortunately, SqlServiceExtensions.CreateMapping only support IPortable and
            // primitive types, so the following does not work
            //await client.Sql.CreateMapping(map, x => x.Name, x => x.Value);
            // TODO: extend with compact somehow? how do you know a type is 'compact'?

            // FIXME
            // at that point,
            // 'value' is a reserved word and cannot be a schema field name
            // 'value' is a reserved word and cannot be a SQL mapping column
            const string columnNameOfName = "name";
            const string columnNameOfValue = "vvalue";

            // creating our mapping manually
            var mappingCommand = 
                $"CREATE MAPPING {map.Name} " +
                "(" + // a column list is required for compact
                $"  __key VARCHAR," +
                $"  {columnNameOfName} VARCHAR," +
                $"  {columnNameOfValue} INT EXTERNAL NAME \"{Thing.FieldNameOf.Value}\"" + // 'value' is a keyword?!
                ") " +
                "TYPE IMAP " + 
                "OPTIONS (" +
                "  'keyFormat'='varchar', " +
                "  'valueFormat'='compact', 'valueCompactTypeName'='thing'" +
                ")";
            Console.WriteLine(mappingCommand);
            await client.Sql.ExecuteCommandAsync(mappingCommand);

            // and then, run the query
            // which returns all columns from the mapping
            // and, the WHERE clause requires that the compact object be correctly de-serialized
            var rows = await client.Sql.ExecuteQueryAsync($"SELECT * FROM {map.Name} WHERE {columnNameOfValue} > 2");

            var rowCount = 0;
            var result = new List<(string, int)>();

            await foreach (var row in rows)
            {
                rowCount++;
                
                Console.WriteLine($"ROW {rowCount}");
                foreach (var column in row.Metadata.Columns)
                    Console.WriteLine($"  {column.Name} {column.Type}");

                var key = row.GetKey<string>();
                var name = row.GetColumn<string>(columnNameOfName);
                var value = row.GetColumn<int>(columnNameOfValue);

                Console.WriteLine($"= {key} {name} {value}");

                result.Add((name, value));
            }

            Assert.That(rowCount, Is.EqualTo(2));
            var min = result.Min(x => x.Item2);
            var max = result.Max(x => x.Item2);
            Assert.That(min, Is.EqualTo(3));
            Assert.That(max, Is.EqualTo(4));
        }

        private class Thing
        {
            public static class FieldNameOf
            {
                public const string Name = "name";
                public const string Value = "mehalue";
            }

            public string Name { get; set; }

            public int Value { get; set; }
        }

        private class ThingCompactSerializer : ICompactSerializer<Thing>
        {
            public Thing Read(ICompactReader reader)
            {
                return new Thing
                {
                    Name = reader.ReadStringRef(Thing.FieldNameOf.Name),
                    Value = reader.ReadInt(Thing.FieldNameOf.Value)
                };
            }

            public void Write(ICompactWriter writer, Thing obj)
            {
                writer.WriteStringRef(Thing.FieldNameOf.Name, obj.Name);
                writer.WriteInt(Thing.FieldNameOf.Value, obj.Value);
            }
        }
    }
}
