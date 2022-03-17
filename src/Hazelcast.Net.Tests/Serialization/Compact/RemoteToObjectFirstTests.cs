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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Configuration;
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
    public class CompactOptionsTests
    {
        [Test]
        public void SetTypeName_CannotThenSetToAnotherValue()
        {
            var options = new CompactOptions();

            // can set it once
            options.SetTypeName<Thing>("thing");

            // can set it again as long as it's the same value
            options.SetTypeName(typeof(Thing), "thing");
            options.SetTypeName<Thing>("thing");

            // setting to a different value throws
            Assert.Throws<ConfigurationException>(() => options.SetTypeName<Thing>("different"));
            Assert.Throws<ConfigurationException>(() => options.SetTypeName(typeof(Thing), "different"));
        }

        [Test]
        public void SetTypeName_CanThenAddSerializerWithSameTypeAndName()
        {
            var options = new CompactOptions();

            options.SetTypeName<Thing>("thing");
            options.AddSerializer(new ThingCompactSerializer<Thing>("thing"));
        }

        [Test]
        public void SetTypeName_CannotThenAddSerializerWithDifferentName()
        {
            var options = new CompactOptions();

            options.SetTypeName<Thing>("thing");
            Assert.Throws<ConfigurationException>(() => options.AddSerializer(new ThingCompactSerializer<Thing>("different")));
        }

        [Test]
        public void SetTypeName_CannotThenAddSerializerWithDifferentType()
        {
            var options = new CompactOptions();

            options.SetTypeName<Thing>("thing");

            // one type Thing has been linked to type name "thing",
            // it is not possible to add a serializer for "thing" that does not handle type Thing

            Assert.Throws<ConfigurationException>(() => options.AddSerializer(new ThingCompactSerializer<DifferentThing>("thing")));
            options.AddSerializer(new ThingCompactSerializer<Thing>("thing"));
        }

        [Test]
        public void SetTypeName_MultipleTypesRequireCustomSerializer()
        {
            var options = new CompactOptions();

            // it is OK to have two types share the same type name
            options.SetTypeName<Thing>("thing");
            options.SetTypeName<DifferentThing>("thing");

            // but then an explicit serializer is required
            var reflectionSerializer = CompactSerializerWrapper.Create(new ReflectionSerializer());
            Assert.Throws<ConfigurationException>(() => options.GetRegistrations(reflectionSerializer).ToList());

            // indeed,
            //
            // serialization might work:
            //
            //   serialize Thing1 = uses reflection serializer, creates a schema for Thing1 with "thing" name
            //   serialize Thing2 = uses reflection serializer, already has schema with "thing" name
            //     and the reflection serializer is going to try its best using the schema
            //
            // but de-serialization cannot work:
            //
            //   deserialize "thing" = we have the schema, and use the reflection serializer
            //     which cannot determine which type to create since "thing" maps to both Thing1 and Thing2
            //     but... we cannot determine which type to create since "thing" -> type-1 and type-2
            //
            // a custom serializer is required, which should have a way to determine which type to create

            // with an explicit serializer, it works

            var serializer = new ThingInterfaceCompactSerializer();

            // ok, this fails because the serializer has a different type name for Thing
            Assert.Throws<ConfigurationException>(() => options.AddSerializer<IThing, Thing>(serializer));

            // try again with the correct name
            options = new CompactOptions();
            options.SetTypeName<Thing>(serializer.TypeName);
            options.SetTypeName<DifferentThing>(serializer.TypeName);
            options.AddSerializer<IThing, Thing>(serializer);
            
            var registrations = options.GetRegistrations(reflectionSerializer).ToList();
            Assert.That(registrations.Count, Is.EqualTo(2));
            Assert.That(registrations.Any(x => x.SerializedType == typeof(Thing)));
            Assert.That(registrations.Any(x => x.SerializedType == typeof(DifferentThing)));
            Assert.That(registrations[0].Serializer.Wrapped == serializer);
            Assert.That(registrations[1].Serializer.Wrapped == serializer);
        }

        [Test]
        public void SetTypeName_CanThenSetSchemaWithSameTypeAndName()
        {
            var options = new CompactOptions();

            options.SetTypeName<Thing>("thing");
            options.SetSchema<Thing>(SchemaBuilder.For("thing").Build(), false);
        }

        [Test]
        public void SetTypeName_CanThenSetSchemaWithSameName()
        {
            var options = new CompactOptions();

            options.SetTypeName<Thing>("thing");
            options.SetSchema(SchemaBuilder.For("thing").Build(), false);
        }

        [Test]
        public void SetTypeName_SetSchemaWithSameName_RequiresExplicitSerializer()
        {
            var options = new CompactOptions();

            // it is OK to have two types share the same type name
            options.SetTypeName<Thing>("thing");
            options.SetSchema<DifferentThing>(SchemaBuilder.For("thing").Build(), false);

            // but then an explicit serializer is required
            var reflectionSerializer = CompactSerializerWrapper.Create(new ReflectionSerializer());
            Assert.Throws<ConfigurationException>(() => options.GetRegistrations(reflectionSerializer).ToList());

            // with an explicit serializer, it works
            var serializer = new ThingInterfaceCompactSerializer();

            options = new CompactOptions();
            options.SetTypeName<Thing>(serializer.TypeName);
            options.SetTypeName<DifferentThing>(serializer.TypeName);
            options.AddSerializer<IThing, Thing>(serializer);

            var registrations = options.GetRegistrations(reflectionSerializer).ToList();
            Assert.That(registrations.Count, Is.EqualTo(2));
            Assert.That(registrations.Any(x => x.SerializedType == typeof(Thing)));
            Assert.That(registrations.Any(x => x.SerializedType == typeof(DifferentThing)));
            Assert.That(registrations[0].Serializer.Wrapped == serializer);
            Assert.That(registrations[1].Serializer.Wrapped == serializer);
        }

        [Test]
        public void SetTypeName_CannotThenSetSchemaWithDifferentName()
        {
            var options = new CompactOptions();

            options.SetTypeName<Thing>("thing");
            Assert.Throws<ConfigurationException>(() => options.SetSchema<Thing>(SchemaBuilder.For("different").Build(), false));
        }

        [Test]
        public void SetSchemaOfT_CannotThenSetToAnotherValue()
        {
            var options = new CompactOptions();

            // can set it once
            options.SetSchema<Thing>(SchemaBuilder.For("thing").Build(), false);

            // can set it again as long as it's the same value
            options.SetSchema(typeof(Thing), SchemaBuilder.For("thing").Build(), false);
            options.SetSchema<Thing>(SchemaBuilder.For("thing").Build(), false);

            // setting different schema to same type throws
            Assert.Throws<ConfigurationException>(() => options.SetSchema<Thing>(SchemaBuilder.For("different").Build(), false));
            Assert.Throws<ConfigurationException>(() => options.SetSchema(typeof(Thing), SchemaBuilder.For("different").Build(), false));

            // setting different schema to same type name throws
            Assert.Throws<ConfigurationException>(() => options.SetSchema<Thing>(SchemaBuilder.For("thing").WithField("name", FieldKind.Boolean).Build(), false));

            // it is ok to set same schema for a different type
            // but, we end up with 2 types pointing to same schema, and that is going to require an explicit serializer
            // the exception is thrown when getting registrations (see relevant tests)
            options.SetSchema<DifferentThing>(SchemaBuilder.For("thing").Build(), false);

            // also for type-less
            options.SetSchema(SchemaBuilder.For("thing").Build(), false);
            Assert.Throws<ConfigurationException>(() => options.SetSchema(SchemaBuilder.For("thing").WithField("name", FieldKind.Boolean).Build(), false));
        }

        [Test]
        public void SetSchema_CannotThenSetToAnotherValue()
        {
            var options = new CompactOptions();

            // can set it once
            options.SetSchema(SchemaBuilder.For("thing").Build(), false);

            // can set it again as long as it's the same schema
            options.SetSchema(SchemaBuilder.For("thing").Build(), false);

            // setting a different schema for the same typename throws
            Assert.Throws<ConfigurationException>(() => options.SetSchema(SchemaBuilder.For("thing").WithField("name", FieldKind.Boolean).Build(), false));

            // also for typed
            Assert.Throws<ConfigurationException>(() => options.SetSchema<Thing>(SchemaBuilder.For("thing").WithField("name", FieldKind.Boolean).Build(), false));
            options.SetSchema<Thing>(SchemaBuilder.For("thing").Build(), false);
        }
    }

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

        private static string GetRandomName(string prefix) => $"{prefix}-{Guid.NewGuid().ToString("N")[..7]}";

        private async Task<string> SetUpCluster(Schema schema)
        {
            var options = GetHazelcastOptions();

            // note: do *not* provide the ThingCompactSerializer<Thing> so that we force-use the
            // reflection serializer and use the correct property names as per the schema (the
            // ThingCompactSerializer<Thing> having its own opinion on property names).
            options.Serialization.Compact.SetSchema<Thing>(schema, false);

            // ensure that the cluster has a map with 1 value of type Thing
            // putting to the map ensures that the schema is published to the cluster

            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
            await using var map = await client.GetMapAsync<string, Thing>(GetRandomName("map"));
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
        public async Task AddNothing_FetchSchema_ValidTypeName()
        {
            var mapName = await SetUpCluster(SchemaBuilder
                .For(CompactSerializer.GetTypeName<Thing>())
                .WithField("name", FieldKind.NullableString)
                .WithField("value", FieldKind.Int32)
                .Build());

            var options = GetHazelcastOptions();

            // schema is fetched from the cluster
            // only thing we have is its type name, which matches the CLR type
            // and so it works

            await AssertCompact(options, mapName, true);
        }

        [Test]
        public async Task AddNothing_FetchSchema_InvalidTypeName()
        {
            var mapName = await SetUpCluster(SchemaBuilder
                .For("thing")
                .WithField("name", FieldKind.NullableString)
                .WithField("value", FieldKind.Int32)
                .Build());

            var options = GetHazelcastOptions();

            // schema is fetched from the cluster
            // only thing we have is its type name, which does not match a valid CLR type
            // and so it fails

            var ex = await AssertEx.ThrowsAsync<SerializationException>(async () => await AssertCompact(options, mapName, true));
            Assert.That(ex.Message, Does.StartWith("Could not find a compact serializer for schema"));
        }

        [Test]
        public async Task AddTypeName_FetchSchema_MatchingName()
        {
            var mapName = await SetUpCluster(SchemaBuilder
                .For("thing")
                .WithField("name", FieldKind.NullableString)
                .WithField("value", FieldKind.Int32)
                .Build());

            var options = GetHazelcastOptions();
            options.Serialization.Compact.SetTypeName<Thing>("thing");

            // schema is fetched from the cluster
            // only thing we have is its type name, which we have declared
            // and so it works

            await AssertCompact(options, mapName, true);
        }

        [Test]
        public async Task SetSchema_InvalidTypeName()
        {
            var mapName = await SetUpCluster(SchemaBuilder
                .For("thing")
                .WithField(Thing.FieldNames.Name, FieldKind.NullableString)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build());

            var options = GetHazelcastOptions();
            options.Serialization.Compact.SetSchema<Thing>(SchemaBuilder
                .For("different")
                .WithField(Thing.FieldNames.Name, FieldKind.NullableString)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build(), true);

            // schema is fetched from the cluster
            // only thing we have is its type name, which does not match anything
            // and so it fails

            var ex = await AssertEx.ThrowsAsync<SerializationException>(async () => await AssertCompact(options, mapName, true));
            Assert.That(ex.Message, Does.StartWith("Could not find a compact serializer for schema"));
        }

        [Test]
        public async Task SetSchema_ValidTypeName()
        {
            var mapName = await SetUpCluster(SchemaBuilder
                .For("thing")
                .WithField(Thing.FieldNames.Name, FieldKind.NullableString)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build());

            var options = GetHazelcastOptions();
            options.Serialization.Compact.SetSchema<Thing>(SchemaBuilder
                .For("thing")
                .WithField(Thing.FieldNames.Name, FieldKind.NullableString)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build(), true);

            // no need to fetch schema from cluster
            // matches existing schema and type
            // and so it works

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
            options.Serialization.Compact.SetTypeName<Thing>("thing");
            options.Serialization.Compact.SetSchema<Thing>(true);

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
            options.Serialization.Compact.SetSchema<Thing>(schema, true);

            await AssertCompact(options, mapName, true);
        }

        [Test]
        public async Task AddSerializer_InvalidTypeName()
        {
            var mapName = await SetUpCluster(SchemaBuilder
                .For("thing")
                .WithField("name", FieldKind.NullableString)
                .WithField("value", FieldKind.Int32)
                .Build());

            var options = GetHazelcastOptions();
            options.Serialization.Compact.AddSerializer(new ThingCompactSerializer<Thing>("different"));

            // schema is fetched from the cluster
            // only thing we have is its type name, which does not match the serializer
            // and so it fails

            var ex = await AssertEx.ThrowsAsync<SerializationException>(async () => await AssertCompact(options, mapName, false));
            Assert.That(ex.Message, Does.StartWith("Could not find a compact serializer for schema"));
        }

        [Test]
        public async Task AddSerializer_ValidTypeName()
        {
            // anything can work here since we are providing both the schema and the serializer
            var typeName = GetRandomName("type");

            var mapName = await SetUpCluster(SchemaBuilder
                .For(typeName)
                .WithField("name", FieldKind.NullableString)
                .WithField("value", FieldKind.Int32)
                .Build());

            var options = GetHazelcastOptions();
            options.Serialization.Compact.AddSerializer(new ThingCompactSerializer<Thing>(typeName));

            // schema is fetched from the cluster
            // only thing we have is its type name, which matches the serializer
            // and so it works

            await AssertCompact(options, mapName, false);
        }

        // FIXME - dead code
        // that test does not make sense anymore
        /*
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
        */

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
            options.Serialization.Compact.AddSerializer(new ThingCompactSerializer<Thing>());
            options.Serialization.Compact.SetSchema<Thing>(schema, true);

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

        [Test]
        public async Task AsyncLazyDeserialization()
        {
            var schema = SchemaBuilder
                .For(Thing.TypeName)
                .WithField("name", FieldKind.NullableString)
                .WithField("value", FieldKind.Int32)
                .Build();

            // get options for the client
            // can provide the ThingCompactSerializer<Thing>, names matches the schema above
            var options = GetHazelcastOptions();
            options.Serialization.Compact.AddSerializer(new ThingCompactSerializer<Thing>());
            options.Serialization.Compact.SetSchema<Thing>(schema, false);

            // ensure that the cluster knows the 'thing' schema which will be pushed to it
            // ensure that the cluster has a map with 1 value of type 'thing'

            var client = await HazelcastClientFactory.StartNewClientAsync(options);
            var map = await client.GetMapAsync<string, Thing>("map_" + Guid.NewGuid().ToString("N"));
            var mapName = map.Name;

            for (var i = 1; i < 10; i++)
            {
                await map.PutAsync($"thing{i}", new Thing { Name = $"thing{i}", Value = i });
            }

            await map.DisposeAsync();
            await client.DisposeAsync();

            // get options for the client
            // register without a schema, the schema will have to be fetched from the cluster
            options = GetHazelcastOptions();
            options.Serialization.Compact.AddSerializer(new ThingCompactSerializer<Thing>());
            options.Serialization.Compact.SetSchema<Thing>(true);

            // we need a new, fresh client, which does not know about the schema already
            client = await HazelcastClientFactory.StartNewClientAsync(options);
            map = await client.GetMapAsync<string, Thing>(mapName);

            await using var disposer = new AsyncDisposable(async () =>
            {
                await map.DisposeAsync();
                await client.DisposeAsync();
            });

            var values = await map.GetValuesAsync();

            // this is entirely synchronous and throws, because it cannot retrieve the schema
            Assert.Throws<MissingCompactSchemaException>(() => _ = values.First());

            // this is asynchronous and will successfully fetch the schema
            _ = await values.AsAsyncEnumerable().FirstAsync();

            // and then of course this succeeds
            _ = values.First();

            // and this too, synchronously
            foreach (var value in values)
                Console.WriteLine(value);
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
