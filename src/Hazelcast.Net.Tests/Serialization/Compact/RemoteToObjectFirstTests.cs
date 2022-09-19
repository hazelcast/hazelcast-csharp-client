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
                })
                .Build();

        [Test]
        public async Task AddNothing_FetchSchema_ValidTypeName()
        {
            var mapName = await SetUpCluster(SchemaBuilder
                .For(CompactOptions.GetDefaultTypeName<Thing>())
                .WithField("name", FieldKind.String)
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
                .WithField("name", FieldKind.String)
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
                .WithField("name", FieldKind.String)
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
                .WithField(Thing.FieldNames.Name, FieldKind.String)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build());

            var options = GetHazelcastOptions();
            options.Serialization.Compact.SetSchema<Thing>(SchemaBuilder
                .For("different")
                .WithField(Thing.FieldNames.Name, FieldKind.String)
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
                .WithField(Thing.FieldNames.Name, FieldKind.String)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build());

            var options = GetHazelcastOptions();
            options.Serialization.Compact.SetSchema<Thing>(SchemaBuilder
                .For("thing")
                .WithField(Thing.FieldNames.Name, FieldKind.String)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build(), true);

            // no need to fetch schema from cluster
            // matches existing schema and type
            // and so it works

            await AssertCompact(options, mapName, true);
        }

        [Test]
        public async Task SetSchemaAndTypeName()
        {
            var mapName = await SetUpCluster(SchemaBuilder
                .For("thing")
                .WithField(Thing.FieldNames.Name, FieldKind.String)
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
        public async Task AddSerializer_InvalidTypeName()
        {
            var mapName = await SetUpCluster(SchemaBuilder
                .For("thing")
                .WithField("name", FieldKind.String)
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
                .WithField("name", FieldKind.String)
                .WithField("value", FieldKind.Int32)
                .Build());

            var options = GetHazelcastOptions();
            options.Serialization.Compact.AddSerializer(new ThingCompactSerializer<Thing>(typeName));

            // schema is fetched from the cluster
            // only thing we have is its type name, which matches the serializer
            // and so it works

            await AssertCompact(options, mapName, false);
        }

        [Test]
        public async Task AddSerializerAndSchema()
        {
            var schema = SchemaBuilder
                .For("thing")
                .WithField(Thing.FieldNames.Name, FieldKind.String)
                .WithField(Thing.FieldNames.Value, FieldKind.Int32)
                .Build();

            var mapName = await SetUpCluster(schema);

            var options = GetHazelcastOptions();

            // register without a schema, the schema will be fetched from the cluster
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
                .WithField("name", FieldKind.String)
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

            // this works, it's all synchronous *but* the schema is there already
            foreach (var value in values)
                Console.WriteLine(value);
        }
    }
}
