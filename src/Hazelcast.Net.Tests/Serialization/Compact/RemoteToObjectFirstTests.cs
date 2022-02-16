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
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    // a collection of tests that validate the different ways to configure compact
    // serialization with a schema that is not known by the client, and therefore
    // must be automatically fetched, during the first operation which involves
    // ToObject (i.e. we ToObject before ToData and have to fetch the schema from
    // the cluster).

    [TestFixture]
    public class RemoteToObjectFirstTests : SingleMemberRemoteTestBase
    {
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

        private async Task<string> SetUpCluster(Schema schema)
        {
            var options = GetHazelcastOptions();

            // note: do *not* provide the ThingCompactSerializer<Thing> so that we force-use the
            // reflection serializer and use the correct property names as per the schema (the
            // ThingCompactSerializer<Thing> having its own opinion on property names).
            options.Serialization.Compact.Register<Thing>(schema, false);

            // ensure that the cluster knows the 'thing' schema which will be pushed to it
            // ensure that the cluster has a map with 1 value of type 'thing'

            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
            await using var map = await client.GetMapAsync<string, Thing>("map_" + Guid.NewGuid().ToString("N"));
            await map.PutAsync("thing1", new Thing { Name = "thing1", Value = 1 });

            return map.Name;
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

        [Test]
        public async Task RegisterNothing()
        {
            var mapName = await SetUpCluster(SchemaBuilder
                .For(CompactSerializer.GetTypeName<Thing>())
                .WithField(nameof(Thing.Name), FieldKind.NullableString)
                .WithField(nameof(Thing.Value), FieldKind.Int32)
                .Build());

            var options = GetHazelcastOptions();

            // reading a totally unregistered type works, if the schema type name
            // is the full assembly qualified name of the serialized object type

            await AssertCompact(options, mapName, true);
        }

        [Test]
        public async Task RegisterTypeForNokTypeName()
        {
            var mapName = await SetUpCluster(SchemaBuilder
                .For("thing")
                .WithField(Thing.FieldNames.Name, FieldKind.NullableString)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build());

            var options = GetHazelcastOptions();

            // register without a schema, the schema will be fetched from the cluster,
            // and the type name will be provided by the schema
            options.Serialization.Compact.Register<Thing>(true);

            // this is bound to fail. we pushed a schema with type name 'thing' but did not specify
            // any type name on the client, so on the client the only thing that is available is the
            // computed type name, i.e. typeof(Thing).AssemblyQualifiedName -> cannot match.
            //
            // see RegisterTypeWithSerializerForOkTypeName for how this can work if the schema which
            // is pushed uses that assembly qualified name instead. see RegisterTypeWithTypeName for
            // how this can work by providing the client-side correct type name.

            var ex = await AssertEx.ThrowsAsync<SerializationException>(async () => await AssertCompact(options, mapName, true));
            Assert.That(ex.Message, Does.StartWith("Could not find a compact serializer for schema"));
        }

        [Test]
        public async Task RegisterTypeForOkTypeName()
        {
            var mapName = await SetUpCluster(SchemaBuilder
                .For(CompactSerializer.GetTypeName<Thing>())
                .WithField(Thing.FieldNames.Name, FieldKind.NullableString)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build());

            var options = GetHazelcastOptions();

            // register without a schema, the schema will be fetched from the cluster,
            // and the type name will be provided by the schema
            options.Serialization.Compact.Register<Thing>(true);

            await AssertCompact(options, mapName, true);
        }

        [Test]
        public async Task RegisterTypeWithTypeName()
        {
            var mapName = await SetUpCluster(SchemaBuilder
                .For("thing")
                .WithField(Thing.FieldNames.Name, FieldKind.NullableString)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build());

            var options = GetHazelcastOptions();

            // register without a schema, the schema will be fetched from the cluster
            // see the ...NokTypeName tests to understand what happens when the type name does not match
            options.Serialization.Compact.Register<Thing>("thing", true);

            await AssertCompact(options, mapName, true);
        }

        [Test]
        public async Task RegisterTypeWithSchema()
        {
            var schema = SchemaBuilder
                .For("thing")
                .WithField(Thing.FieldNames.Name, FieldKind.NullableString)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build();

            var mapName = await SetUpCluster(schema);

            var options = GetHazelcastOptions();

            // register without a schema, the schema will be fetched from the cluster
            // FIXME - what-if we provided a wrong schema?
            options.Serialization.Compact.Register<Thing>(schema, true);

            await AssertCompact(options, mapName, true);
        }

        [Test]
        public async Task RegisterTypeWithSerializerForNokTypeName()
        {
            var mapName = await SetUpCluster(SchemaBuilder
                .For("thing")
                .WithField("name", FieldKind.NullableString)
                .WithField("value", FieldKind.Int32)
                .Build());

            var options = GetHazelcastOptions();

            // register without a schema, the schema will be fetched from the cluster,
            // and the type name will be provided by the schema
            options.Serialization.Compact.Register(new ThingCompactSerializer<Thing>(), true);

            // FIXME FAILS?? EXPLAIN??
            //
            // SerializationException: Could not find a compact serializer for schema 5883859746603219204.
            // on the first get
            // initialize schemas:
            //   new 'thing' schema during setup, which will be pushed to the cluster
            //   new 'thing' schema from ??? FIXME why two schemas ?!
            //   new 'thing' schema from CompactSerializer.FetchSchema because the schema id is unknown

            // this is bound to fail. we pushed a schema with type name 'thing' but did not specify
            // any type name on the client, so on the client the only thing that is available is the
            // computed type name, i.e. typeof(Thing).AssemblyQualifiedName -> cannot match.
            //
            // see RegisterTypeWithSerializerForOkTypeName for how this can work if the schema which
            // is pushed uses that assembly qualified name instead. see RegisterTypeWithTypeName for
            // how this can work by providing the client-side correct type name.

            var ex = await AssertEx.ThrowsAsync<SerializationException>(async () => await AssertCompact(options, mapName, false));
            Assert.That(ex.Message, Does.StartWith("Could not find a compact serializer for schema"));
        }

        [Test]
        public async Task RegisterTypeWithSerializerForOkTypeName()
        {
            var mapName = await SetUpCluster(SchemaBuilder
                .For(CompactSerializer.GetTypeName<Thing>())
                .WithField("name", FieldKind.NullableString)
                .WithField("value", FieldKind.Int32)
                .Build());

            var options = GetHazelcastOptions();

            // register without a schema, the schema will be fetched from the cluster,
            // and the type name will be provided by the schema
            options.Serialization.Compact.Register(new ThingCompactSerializer<Thing>(), true);

            await AssertCompact(options, mapName, false);
        }

        [Test]
        public async Task RegisterTypeWithSerializerAndTypeName()
        {
            var mapName = await SetUpCluster(SchemaBuilder
                .For("thing")
                .WithField(Thing.FieldNames.Name, FieldKind.NullableString)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build());

            var options = GetHazelcastOptions();

            // register without a schema, the schema will be fetched from the cluster
            // see the ...NokTypeName tests to understand what happens when the type name does not match
            options.Serialization.Compact.Register(new ThingCompactSerializer<Thing>(), Thing.TypeName, true);

            await AssertCompact(options, mapName, false);
        }

        [Test]
        public async Task RegisterTypeWithSerializerAndSchema()
        {
            var schema = SchemaBuilder
                .For("thing")
                .WithField(Thing.FieldNames.Name, FieldKind.NullableString)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build();

            var mapName = await SetUpCluster(schema);

            var options = GetHazelcastOptions();

            // register without a schema, the schema will be fetched from the cluster
            // FIXME - what-if we provided a wrong schema?
            options.Serialization.Compact.Register(new ThingCompactSerializer<Thing>(), schema, true);

            await AssertCompact(options, mapName, false);
        }

        private static async Task AssertCompact(HazelcastOptions options, string mapName, bool usesReflection)
        {
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

            if (usesReflection)
            {
                options.Serialization.Compact.ReflectionSerializer = new CountingReflectionSerializer();
                CountingReflectionSerializer.Reset();
            }
            else
            {
                ThingCompactSerializer.Reset();
            }

            client = await HazelcastClientFactory.StartNewClientAsync(options);
            map = await client.GetMapAsync<string, Thing>(mapName);

            var thing = await map.GetAsync("thing1");

            Assert.That(thing.Name, Is.EqualTo("thing1"));
            Assert.That(thing.Value, Is.EqualTo(1));

            // can write back
            var thing2 = new Thing { Name = "thing2", Value = 2 };
            await map.SetAsync(thing2.Name, thing2);

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


        // FIXME - dead code
        /*
        [Test]
        public async Task CanFetchSchemaFromClusterAndToObjectCompactable()
        {
            var options = new HazelcastOptionsBuilder().Build();

            options.ClusterName = RcCluster?.Id ?? options.ClusterName;
            options.Networking.Addresses.Add("127.0.0.1:5701");

            options.Serialization.Compact.Enabled = true;

            IHazelcastClient client = null;
            IHMap<string, ThingCompactableAttributeWithTypeName> map = null;

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
            map = await client.GetMapAsync<string, ThingCompactableAttributeWithTypeName>("map_" + Guid.NewGuid().ToString("N"));

            // setting a value ensures that the schema is pushed to the cluster
            // because it's Compactable we have nothing to declare, it will just work
            var thing = new ThingCompactableAttributeWithTypeName { Name = "thing1", Value = 1 };
            await map.SetAsync(thing.Name, thing);

            var options2 = new HazelcastOptionsBuilder().Build();

            options2.ClusterName = RcCluster?.Id ?? options.ClusterName;
            options2.Networking.Addresses.Add("127.0.0.1:5701");

            options2.Serialization.Compact.Enabled = true;

            IHazelcastClient client2 = null;
            IHMap<string, ThingCompactableAttributeWithTypeName> map2 = null;

            await using var dispose2 = new AsyncDisposable(async () =>
            {
                // ReSharper disable AccessToModifiedClosure
                if (client2 != null)
                {
                    if (map2 != null)
                    {
                        await client2.DestroyAsync(map);
                        await map2.DisposeAsync();
                    }
                    await client2.DisposeAsync();
                }
                // ReSharper restore AccessToModifiedClosure
            });

            client2 = await HazelcastClientFactory.StartNewClientAsync(options2);
            map2 = await client2.GetMapAsync<string, ThingCompactableAttributeWithTypeName>(map.Name);

            // because it's compactable it should all work?
            var thing2 = await map2.GetAsync(thing.Name);

            Assert.That(thing2.Name, Is.EqualTo(thing.Name));
            Assert.That(thing2.Value, Is.EqualTo(thing.Value));

            // should work naturally
            var thing3 = new ThingCompactableAttributeWithTypeName { Name = "thing3", Value = 3 };
            await map.SetAsync(thing3.Name, thing3);
        }
        */
    }
}
