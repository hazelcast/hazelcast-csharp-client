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

// ReSharper disable StringLiteralTypo

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Hazelcast.Testing;
using Hazelcast.Tests.Serialization.Compact;
using NUnit.Framework;

[assembly:CompactSerializer(typeof(Thing), typeof(ThingCompactSerializer<Thing>))]

namespace Hazelcast.Tests.Serialization.Compact
{
    // a collection of tests that validate the different ways to configure compact
    // serialization with a schema that is not known by the cluster, and therefore
    // must be automatically published, during the first operation which involves
    // ToData (i.e. we ToData before ToObject and have to publish the schema to the
    // cluster).

    [TestFixture]
    internal class RemoteToDataFirstTests : ClusterRemoteTestBase
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

        private HazelcastOptions GetHazelcastOptions()
            => new HazelcastOptionsBuilder()
                .With(options =>
                {
                    options.ClusterName = RcCluster?.Id ?? options.ClusterName;
                    options.Networking.Addresses.Add("127.0.0.1:5701");
                    options.Serialization.Compact.Enabled = true;
                })
                .Build();

        // the constant values, ie values that the user specifies
        // either when registering, when providing a schema, or a serializer
        private const string ConstantTypeName = Thing.TypeName;
        private const string ConstantValueFieldName = Thing.FieldNames.Value;

        // the computed values, when not specified by the user
        // here, we'll generate the schema from the type
        // ReSharper disable once InconsistentNaming
        private readonly string ComputedTypeName = CompactSerializer.GetTypeName(typeof (Thing));
        private const string ComputedValueFieldName = nameof(Thing.Value);

        [Test]
        public async Task RegisterNothing()
        {
            var options = GetHazelcastOptions();

            // writing a totally unregistered type works
            // uses the reflection serializer + derives the schema & type name from the serializer

            // type name a,d field names derived from type = computed
            await AssertCompact(options, ComputedTypeName, ComputedValueFieldName, true);
        }

        [Test]
        public async Task RegisterAssembly()
        {
            var options = GetHazelcastOptions();

            // register the assembly, discover the [assembly:CompactSerializer(...)] attribute
            // above and register the corresponding type and serializer - in real-life, the
            // attribute will be part of generated code.
            options.Serialization.Compact.Register(GetType().Assembly);

            // type name derived from type = computed
            // field name obtained from the serializer = constant
            await AssertCompact(options, ComputedTypeName, ConstantValueFieldName, false);
        }

        [Test]
        public async Task RegisterAssemblyAndType()
        {
            var options = GetHazelcastOptions();

            // register the assembly, discover the [assembly:CompactSerializer(...)] attribute
            // above and register the corresponding type and serializer - in real-life, the
            // attribute will be part of generated code. in addition, register the type so we
            // can specify a typeName + isClusterSchema. both will be merged.
            options.Serialization.Compact.Register(GetType().Assembly);
            options.Serialization.Compact.Register<Thing>(ConstantTypeName, false);

            // type name and field name obtained from the registration + serializer = constant
            await AssertCompact(options, ConstantTypeName, ConstantValueFieldName, false);
        }

        [Test]
        public async Task RegisterTypeWithSerializer()
        {
            var options = GetHazelcastOptions();

            // register a type + serializer, but not the schema so the type can be a plain POCO,
            // nor the type name which will derive from the CLR name. we write the serializer
            // independently, and the schema will be derived from the serializer (which must plainly
            // write all fields without any clever optimization that would skip fields) and pushed
            // to the cluster.
            options.Serialization.Compact.Register(new ThingCompactSerializer<Thing>(), false);

            // type name derived from type = computed
            // field name obtained from the serializer = constant
            await AssertCompact(options, ComputedTypeName, ConstantValueFieldName, false);
        }

        [Test]
        public async Task RegisterTypeWithSerializerAndTypeName()
        {
            var options = GetHazelcastOptions();

            // register a type + type name + serializer, but not the schema
            // so the type can be a plain POCO, we write the serializer independently, and the
            // schema will be derived from the serializer (which must plainly write all fields
            // without any clever optimization that would skip fields) and pushed to the cluster.
            options.Serialization.Compact.Register(new ThingCompactSerializer<Thing>(), ConstantTypeName, false);

            // type name and field name obtained from the registration + serializer = constant
            await AssertCompact(options, ConstantTypeName, ConstantValueFieldName, false);
        }

        [Test]
        public async Task RegisterTypeWithSerializerAndSchema()
        {
            var options = GetHazelcastOptions();

            var schema = SchemaBuilder
                .For("thing")
                .WithField(Thing.FieldNames.Name, FieldKind.StringRef)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build();

            // register a type + schema + serializer
            // so the type can be a plain POCO, we write the serializer independently, and we
            // also provide the schema, which will be pushed to the cluster.
            options.Serialization.Compact.Register(new ThingCompactSerializer<Thing>(), schema, false);

            // type name and field name obtained from the registration + schema + serializer = constant
            await AssertCompact(options, ConstantTypeName, ConstantValueFieldName, false);
        }

        [Test]
        public async Task RegisterType()
        {
            var options = GetHazelcastOptions();

            // register a type, but not the schema so the type can be a plain POCO, nor the type name
            // which will derive from the CLR name, nor the serializer which will therefore be the
            // runtime reflection-based serializer, and the schema will be derived from the serializer
            // and pushed to the cluster.
            options.Serialization.Compact.Register<Thing>(false);

            // type name and field name derived from type = computed
            // (uses reflection and has to derive everything)
            await AssertCompact(options, ComputedTypeName, ComputedValueFieldName, true);
        }

        [Test]
        public async Task RegisterTypeWithTypeName()
        {
            var options = GetHazelcastOptions();

            // register a type + type name, but not the schema so the type can be a plain POCO,
            // nor the serializer which will therefore be the runtime reflection-based serializer,
            // and the schema will be derived from the serializer and pushed to the cluster.
            options.Serialization.Compact.Register<Thing>("thing", false);

            // type name derived from registration = constant
            // field name derived from type = computed
            // (uses reflection and can only use property names)
            await AssertCompact(options, ConstantTypeName, ComputedValueFieldName, true);
        }

        [Test]
        public async Task RegisterTypeWithSchema()
        {
            var options = GetHazelcastOptions();

            var schema = SchemaBuilder
                .For("thing")
                .WithField(Thing.FieldNames.Name, FieldKind.StringRef)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build();

            // register a type + schema, but not the serializer which will therefore be the runtime
            // reflection-based serializer.
            options.Serialization.Compact.Register<Thing>(schema, false);

            // type name and field name derived from registration + schema = constant
            // (uses reflection but matching property names to schema field names)
            await AssertCompact(options, ConstantTypeName, ConstantValueFieldName, true);
        }

        private static async Task AssertCompact(HazelcastOptions options, string typeName, string valueFieldName, bool usesReflection)
        {
            var things = new Thing[4];
            for (var i = 0; i < 4; i++) things[i] = new Thing { Name = $"thing{i}", Value = i };

            static void AssertIdentical(Thing t1, Thing t2)
            {
                Assert.That(t1.Name, Is.EqualTo(t2.Name));
                Assert.That(t1.Value, Is.EqualTo(t2.Value));
            }

            if (usesReflection)
            {
                options.Serialization.Compact.ReflectionSerializer = new CountingReflectionSerializer();
                CountingReflectionSerializer.Reset();
            }
            else
            {
                ThingCompactSerializer.Reset();
            }

            await AssertCompact(options, things, t => t.Name, typeName, valueFieldName, AssertIdentical);

            if (usesReflection)
            {
                Assert.That(CountingReflectionSerializer.WriteCount, Is.GreaterThan(0));
                Assert.That(CountingReflectionSerializer.ReadCount, Is.GreaterThan(0));
            }
            else
            {
                Assert.That(ThingCompactSerializer.WriteCount, Is.GreaterThan(0));
                Assert.That(ThingCompactSerializer.ReadCount, Is.GreaterThan(0));
            }
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

            const string valueColumnName = "vvalue"; // 'value' is SQL-reserved  keyword

            // creating our mapping manually
            var mappingCommand =
                $"CREATE MAPPING {map.Name} " +
                "(" + // a column list is required for compact
                "  __key VARCHAR," +
                "  name VARCHAR," +
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
