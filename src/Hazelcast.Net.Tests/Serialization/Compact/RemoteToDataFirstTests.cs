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
using Hazelcast.Testing.Conditions;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    // a collection of tests that validate the different ways to configure compact
    // serialization with a schema that is not known by the cluster, and therefore
    // must be automatically published, during the first operation which involves
    // ToData (i.e. we ToData before ToObject and have to publish the schema to the
    // cluster).

    [TestFixture]
    [ServerCondition("[5.2,)")]
    internal class RemoteToDataFirstTests : ClusterRemoteTestBase
    {
        // needed for SQL
        protected override string RcClusterConfiguration => Resources.Cluster_JetEnabled;

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
                })
                .Build();

        // the constant values, ie values that the user specifies
        private const string ConstantTypeName = Thing.TypeName;
        private const string ConstantValueFieldName = Thing.FieldNames.Value;

        // the computed values, when not specified by the user
        // ReSharper disable once InconsistentNaming
        private readonly string ComputedTypeName = CompactOptions.GetDefaultTypeName(typeof (Thing));
        private const string ComputedValueFieldName = nameof(Thing.Value);

        [Test]
        public async Task AddNothing()
        {
            var options = GetHazelcastOptions();

            // writing a totally unregistered type works
            // uses the reflection serializer + derives the schema & type name from the serializer

            // type name and field names derived from type = computed
            await AssertCompact(options, ComputedTypeName, ComputedValueFieldName, true);
        }

        [Test]
        public async Task AddSerializer()
        {
            var options = GetHazelcastOptions();

            // add a serializer - which provides the type name - the schema will derive from the serializer
            // and will be published when first used
            options.Serialization.Compact.AddSerializer(new ThingCompactSerializer<Thing>(ConstantTypeName));

            // type name and field name obtained from the serializer
            await AssertCompact(options, ConstantTypeName, ConstantValueFieldName, false);
        }

        [Test]
        public async Task AddSerializerAndSchema()
        {
            var options = GetHazelcastOptions();

            var schema = SchemaBuilder
                .For("thing")
                .WithField(Thing.FieldNames.Name, FieldKind.String)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build();

            // add a serializer. the type name is provided by the serializer. also add the schema,
            // which will be matched to the serializer via the type-name. the schema will be published
            // to the cluster when first used.
            options.Serialization.Compact.AddSerializer(new ThingCompactSerializer<Thing>());
            options.Serialization.Compact.SetSchema<Thing>(schema, false);

            // type name and field name obtained from the registration + schema + serializer = constant
            await AssertCompact(options, ConstantTypeName, ConstantValueFieldName, false);
        }

        [Test]
        public async Task SetTypeName()
        {
            var options = GetHazelcastOptions();

            // set the type name - will use the reflection serializer, but with that type name
            options.Serialization.Compact.SetTypeName<Thing>("thing");

            // type name derived from registration = constant
            // field name derived from type = computed
            // (uses reflection and can only use property names)
            await AssertCompact(options, ConstantTypeName, ComputedValueFieldName, true);
        }

        [Test]
        public async Task SetSchema()
        {
            var options = GetHazelcastOptions();

            var schema = SchemaBuilder
                .For("thing")
                .WithField(Thing.FieldNames.Name, FieldKind.String)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build();

            // register a type + schema, but not the serializer which will therefore be the runtime
            // reflection-based serializer.
            options.Serialization.Compact.SetSchema<Thing>(schema, false);

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

        // FIXME move somewhere else / try with ToObject first
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public async Task TypeHierarchySupport(int serializersMode)
        {
            var options = GetHazelcastOptions();

            // ThingWrapper has a property which is an IThing
            // the instance is deserialized using a serializer for the actual type
            // so, we support an interface (or inheritance) here

            options.Serialization.Compact.AddSerializer(new ThingWrapper.ThingWrapperSerializer());

            switch (serializersMode)
            {
                case 0:
                    // no serializers, will work with reflection serializer
                    break;
                case 1:
                    // explicit, distinct serializers
                    options.Serialization.Compact.AddSerializer(new ThingCompactSerializer<Thing>("thing-a"));
                    options.Serialization.Compact.AddSerializer(new ThingCompactSerializer<DifferentThing>("thing-b"));
                    break;
                case 2:
                    // explicit, but unique serializer
                    var serializer = new ThingInterfaceCompactSerializer();
                    options.Serialization.Compact.AddSerializer<IThing, Thing>(serializer);
                    options.Serialization.Compact.AddSerializer<IThing, DifferentThing>(serializer);
                    break;
                default:
                    throw new NotSupportedException();
            }

            IHazelcastClient client = null;
            IHMap<string, ThingWrapper> map = null;

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

            map = await client.GetMapAsync<string, ThingWrapper>("map_" + Guid.NewGuid().ToString("N"));

            var w1 = new ThingWrapper { Thing = new Thing { Name = "name", Value = 42 } };
            await map.PutAsync("key", w1);
            var w2 = await map.GetAsync("key");
            Assert.That(w2.Thing, Is.InstanceOf<Thing>());
            Assert.That(w2.Thing.Name, Is.EqualTo("name"));
            Assert.That(w2.Thing.Value, Is.EqualTo(42));

            var w3 = new ThingWrapper { Thing = new DifferentThing { Name = "name2", Value = 66 } };
            await map.PutAsync("key2", w3);
            var w4 = await map.GetAsync("key2");
            Assert.That(w4.Thing, Is.InstanceOf<DifferentThing>());
            Assert.That(w4.Thing.Name, Is.EqualTo("name2"));
            Assert.That(w4.Thing.Value, Is.EqualTo(66));
        }
    }
}
