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
    internal class RemoteTests : ClusterRemoteTestBase
    {
        // needed for SQL
        protected override string RcClusterConfiguration => Resources.jet_enabled;

        // we have to have 1 member per test else the schemas may end up being cached
        private Hazelcast.Testing.Remote.Member _rcMember;

        [SetUp]
        public async Task SetUp()
        {
            _rcMember = await RcClient.StartMemberAsync(RcCluster);
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_rcMember != null)
            {
                await RcClient.StopMemberAsync(RcCluster, _rcMember);
                _rcMember = null;
            }
        }

        // register a type + type name + serializer, but not the schema
        // so the type can be a plain POCO, we write the serializer independently, and the
        // schema will be derived from the serializer (which must plainly write all fields
        // without any clever optimization that would skip fields) and pushed to the cluster.
        //
        [Test]
        public async Task RegisterWithoutSchema()
        {
            var options = new HazelcastOptionsBuilder().Build();

            options.ClusterName = RcCluster?.Id ?? options.ClusterName;
            options.Networking.Addresses.Add("127.0.0.1:5701");

            options.Serialization.Compact.Enabled = true;
            options.Serialization.Compact.Register(Thing.TypeName, new ThingCompactSerializer<Thing>(), false);

            var things = new Thing[4];
            for (var i = 0; i < 4; i++) things[i] = new Thing { Name = $"thing{i}", Value = i };

            static void AssertIdentical(Thing t1, Thing t2)
            {
                Assert.That(t1.Name, Is.EqualTo(t2.Name));
                Assert.That(t1.Value, Is.EqualTo(t2.Value));
            }

            await AssertCompact(options, things, t => t.Name, Thing.TypeName, Thing.FieldNames.Value, AssertIdentical);
        }

        // register a type + schema + serializer
        // so the type can be a plain POCO, we write the serializer independently, and we
        // also provide the schema, which will be pushed to the cluster.
        //
        [Test]
        public async Task RegisterWithSchema()
        {
            var schema = SchemaBuilder
                .For("thing")
                .WithField(Thing.FieldNames.Name, FieldKind.StringRef)
                .WithField(Thing.FieldNames.Value, FieldKind.SignedInteger32)
                .Build();

            var options = new HazelcastOptionsBuilder().Build();

            options.ClusterName = RcCluster?.Id ?? options.ClusterName;
            options.Networking.Addresses.Add("127.0.0.1:5701");

            options.Serialization.Compact.Enabled = true;
            options.Serialization.Compact.Register(schema, new ThingCompactSerializer<Thing>(), false);

            var things = new Thing[4];
            for (var i = 0; i < 4; i++) things[i] = new Thing { Name = $"thing{i}", Value = i };

            static void AssertIdentical(Thing t1, Thing t2)
            {
                Assert.That(t1.Name, Is.EqualTo(t2.Name));
                Assert.That(t1.Value, Is.EqualTo(t2.Value));
            }

            await AssertCompact(options, things, t => t.Name, Thing.TypeName, Thing.FieldNames.Value, AssertIdentical);
        }

        // register nothing, compactable (from ICompactable) will provide its serializer,
        // and the schema will be derived from the serializer (which must plainly write
        // all fields without any clever optimization that would skip fields) and pushed
        // to the cluster.
        // type name will derive from the class name.
        [Test]
        public async Task CompactableFromInterface()
        {
            var options = new HazelcastOptionsBuilder().Build();

            options.ClusterName = RcCluster?.Id ?? options.ClusterName;
            options.Networking.Addresses.Add("127.0.0.1:5701");

            options.Serialization.Compact.Enabled = true;

            var things = new ThingCompactableInterface[4];
            for (var i = 0; i < 4; i++) things[i] = new ThingCompactableInterface { Name = $"thing{i}", Value = i };

            static void AssertIdentical(ThingCompactableInterface t1, ThingCompactableInterface t2)
            {
                Assert.That(t1.Name, Is.EqualTo(t2.Name));
                Assert.That(t1.Value, Is.EqualTo(t2.Value));
            }

            await AssertCompact(options, things, t => t.Name, ThingCompactableInterface.TypeName, Thing.FieldNames.Value, AssertIdentical);
        }

        // register nothing, compactable (from ICompactable) will provide its serializer,
        // and the schema will be derived from the serializer (which must plainly write
        // all fields without any clever optimization that would skip fields) and pushed
        // to the cluster.
        // type name is provided by the compactable.
        [Test]
        public async Task CompactableFromInterfaceWithTypeName()
        {
            var options = new HazelcastOptionsBuilder().Build();

            options.ClusterName = RcCluster?.Id ?? options.ClusterName;
            options.Networking.Addresses.Add("127.0.0.1:5701");

            options.Serialization.Compact.Enabled = true;

            var things = new ThingCompactableInterfaceWithTypeName[4];
            for (var i = 0; i < 4; i++) things[i] = new ThingCompactableInterfaceWithTypeName { Name = $"thing{i}", Value = i };

            static void AssertIdentical(ThingCompactableInterfaceWithTypeName t1, ThingCompactableInterfaceWithTypeName t2)
            {
                Assert.That(t1.Name, Is.EqualTo(t2.Name));
                Assert.That(t1.Value, Is.EqualTo(t2.Value));
            }

            await AssertCompact(options, things, t => t.Name, ThingCompactableInterfaceWithTypeName.TypeName, Thing.FieldNames.Value, AssertIdentical);
        }

        // same but with attribute instead of interface
        [Test]
        public async Task CompactableFromAttribute()
        {
            var options = new HazelcastOptionsBuilder().Build();

            options.ClusterName = RcCluster?.Id ?? options.ClusterName;
            options.Networking.Addresses.Add("127.0.0.1:5701");

            options.Serialization.Compact.Enabled = true;

            var things = new ThingCompactableAttribute[4];
            for (var i = 0; i < 4; i++) things[i] = new ThingCompactableAttribute { Name = $"thing{i}", Value = i };

            static void AssertIdentical(ThingCompactableAttribute t1, ThingCompactableAttribute t2)
            {
                Assert.That(t1.Name, Is.EqualTo(t2.Name));
                Assert.That(t1.Value, Is.EqualTo(t2.Value));
            }

            await AssertCompact(options, things, t => t.Name, ThingCompactableAttribute.TypeName, Thing.FieldNames.Value, AssertIdentical);
        }

        // same but with attribute instead of interface
        [Test]
        public async Task CompactableFromAttributeWithTypeName()
        {
            var options = new HazelcastOptionsBuilder().Build();

            options.ClusterName = RcCluster?.Id ?? options.ClusterName;
            options.Networking.Addresses.Add("127.0.0.1:5701");

            options.Serialization.Compact.Enabled = true;

            var things = new ThingCompactableAttributeWithTypeName[4];
            for (var i = 0; i < 4; i++) things[i] = new ThingCompactableAttributeWithTypeName { Name = $"thing{i}", Value = i };

            static void AssertIdentical(ThingCompactableAttributeWithTypeName t1, ThingCompactableAttributeWithTypeName t2)
            {
                Assert.That(t1.Name, Is.EqualTo(t2.Name));
                Assert.That(t1.Value, Is.EqualTo(t2.Value));
            }

            await AssertCompact(options, things, t => t.Name, ThingCompactableAttributeWithTypeName.TypeName, Thing.FieldNames.Value, AssertIdentical);
        }

        private static async Task AssertCompact<T>(
            HazelcastOptions options,
            T[] things, Func<T, string> getKey, 
            string typeName, string valueFieldName,
            Action<T, T> assertIdentical)
        {
            IHazelcastClient client = null;
            IHMap<string, T> map = null;

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

            map = await client.GetMapAsync<string, T>("map_" + Guid.NewGuid().ToString("N"));

            foreach (var x in things) await map.SetAsync(getKey(x), x);

            var thing = await map.GetAsync(getKey(things[0]));
            Assert.That(thing, Is.Not.Null);
            assertIdentical(thing, things[0]);

            const string valueColumnName = "vvalue"; // 'value' is a keyword?!

            // creating our mapping manually
            var mappingCommand =
                $"CREATE MAPPING {map.Name} " +
                "(" + // a column list is required for compact
                $"  __key VARCHAR," +
                $"  name VARCHAR," +
                $"  {valueColumnName} INT EXTERNAL NAME \"{valueFieldName}\"" +
                ") " +
                "TYPE IMAP " +
                "OPTIONS (" +
                "  'keyFormat'='varchar', " +
                $"  'valueFormat'='compact', 'valueCompactTypeName'='{typeName}'" +
                ")";
            Console.WriteLine(mappingCommand);
            await client.Sql.ExecuteCommandAsync(mappingCommand);

            var rows = await client.Sql.ExecuteQueryAsync($"SELECT * FROM {map.Name} WHERE {valueColumnName} > 1");
            var rowCount = 0;
            var result = new List<(string, int)>();

            await foreach (var row in rows)
            {
                rowCount++;

                Console.WriteLine($"ROW {rowCount}");
                foreach (var column in row.Metadata.Columns)
                    Console.WriteLine($"  {column.Name} {column.Type}");

                var key = row.GetKey<string>();
                var name = row.GetColumn<string>("name");
                var value = row.GetColumn<int>(valueColumnName);

                Console.WriteLine($"= {key} {name} {value}");

                result.Add((name, value));
            }

            Assert.That(rowCount, Is.EqualTo(2));
            var min = result.Min(x => x.Item2);
            var max = result.Max(x => x.Item2);
            Assert.That(min, Is.EqualTo(2));
            Assert.That(max, Is.EqualTo(3));
        }
    }
}
