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

using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Hazelcast.Testing;
using Hazelcast.Tests.Serialization.Compact.SchemasRemoteTestsLocal;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    namespace SchemasRemoteTestsLocal
    {
        internal class CountingClusterMessaging : IClusterMessaging
        {
            private readonly Hazelcast.Clustering.ClusterMessaging _messaging;
            private int _count;

            public CountingClusterMessaging(Hazelcast.Clustering.ClusterMessaging messaging)
            {
                _messaging = messaging;
            }

            public int SentMessageCount => _count;

            public Task<ClientMessage> SendAsync(ClientMessage requestMessage, CancellationToken cancellationToken)
            {
                Interlocked.Increment(ref _count);
                return _messaging.SendAsync(requestMessage, cancellationToken);
            }
        }

        internal static class Extensions
        {
            public static CountingClusterMessaging GetMessaging(this IHazelcastClient client)
                => new CountingClusterMessaging(((HazelcastClient)client).Cluster.Messaging);
        }
    }

    [TestFixture]
    public class SchemasRemoteTests : SingleMemberClientRemoteTestBase
    {
        // make sure to use distinct schema names since we're running all the tests
        // of this fixture on 1 member which will end up storing & caching schemas
        private readonly ISequence<int> _ids = new Int32Sequence();

        [Test]
        public async Task CanPublishAndFetchSchemas()
        {
            // create a schema cache (don't use the client's one)
            var messaging = Client.GetMessaging();
            var schemas = new Schemas(messaging);

            var schema = new Schema($"sometype{_ids.GetNext()}", new[]
            {
                new SchemaField("somefield", FieldKind.StringRef)
            });

            Assert.That(messaging.SentMessageCount, Is.Zero);
            Assert.That(schemas.TryGet(schema.Id, out _), Is.False);
            Assert.That(messaging.SentMessageCount, Is.Zero); // TryGet is local-only
            var fetched = await schemas.GetOrFetchAsync(schema.Id).CfAwait();
            Assert.That(messaging.SentMessageCount, Is.EqualTo(1)); // tried
            Assert.That(fetched, Is.Null);

            schemas.Add(schema, false);
            await schemas.PublishAsync().ConfigureAwait(false);
            Assert.That(messaging.SentMessageCount, Is.EqualTo(2)); // published

            // create another (empty) schema cache
            var schemas2 = new Schemas(messaging);
            Assert.That(schemas2.TryGet(schema.Id, out _), Is.False);
            Assert.That(messaging.SentMessageCount, Is.EqualTo(2)); // TryGet is local-only

            // that one will actually fetch the schema from the cluster
            var fetched2 = await schemas2.GetOrFetchAsync(schema.Id).CfAwait();
            Assert.That(messaging.SentMessageCount, Is.EqualTo(3)); // fetched
            Assert.That(fetched2, Is.Not.Null);
            Assert.That(fetched2.Id, Is.EqualTo(schema.Id));
        }

        [Test]
        public async Task CanAddSchema()
        {
            // create a schema cache (don't use the client's one)
            var messaging = Client.GetMessaging();
            var schemas = new Schemas(messaging);

            var schema = new Schema($"sometype{_ids.GetNext()}", new[]
            {
                new SchemaField("somefield", FieldKind.StringRef)
            });

            Assert.That(messaging.SentMessageCount, Is.Zero);
            Assert.That(schemas.TryGet(schema.Id, out _), Is.False);
            Assert.That(messaging.SentMessageCount, Is.Zero); // TryGet is local-only
            schemas.Add(schema, false);
            Assert.That(schemas.TryGet(schema.Id, out var returned), Is.True);
            Assert.That(messaging.SentMessageCount, Is.Zero); // TryGet is local-only
            Assert.That(returned.Id, Is.EqualTo(schema.Id));

            returned = await schemas.GetOrFetchAsync(schema.Id).ConfigureAwait(false);
            Assert.That(messaging.SentMessageCount, Is.Zero); // found locally
            Assert.That(returned, Is.Not.Null);
            Assert.That(returned.Id, Is.EqualTo(schema.Id));
        }

        [Test]
        public async Task ClientCanPublishAndFetchSchemas()
        {
            var schemas = (Schemas) ((HazelcastClient)Client).SerializationService.CompactSerializer.Schemas;

            var schema = new Schema($"sometype{_ids.GetNext()}", new[]
            {
                new SchemaField("somefield", FieldKind.StringRef)
            });

            await AssertCanPublishAndFetchSchema(schemas, schema);
        }

        private async Task AssertCanPublishAndFetchSchema(Schemas schemas, Schema schema, bool succeeds = true)
        {
            Assert.That(schemas.TryGet(schema.Id, out _), Is.False);
            Assert.That(await schemas.GetOrFetchAsync(schema.Id), Is.Null);
            Assert.That(await schemas.FetchAsync(schema.Id), Is.Null);

            schemas.Add(schema, false);

            Assert.That(schemas.TryGet(schema.Id, out _), Is.True);
            Assert.That(await schemas.GetOrFetchAsync(schema.Id), Is.Not.Null);
            Assert.That(await schemas.FetchAsync(schema.Id), Is.Null);

            await schemas.PublishAsync();

            Assert.That(schemas.TryGet(schema.Id, out _), Is.True);
            Assert.That(await schemas.GetOrFetchAsync(schema.Id), Is.Not.Null);

            Assert.That(await schemas.FetchAsync(schema.Id), succeeds ? Is.Not.Null : Is.Null);
        }

        [TestCase("meh", true)]
        [TestCase("malue", true)]
        [TestCase("mvalue", true)]
        [TestCase("valuem", true)]
        [TestCase("value", true)]
        public async Task ReservedKeywordCanBreakSchemas(string fieldName, bool succeeds)
        {
            var schemas = (Schemas)((HazelcastClient)Client).SerializationService.CompactSerializer.Schemas;

            var schema = SchemaBuilder
                .For($"thing{_ids.GetNext()}")
                .WithField("name", FieldKind.StringRef)
                .WithField(fieldName, FieldKind.Int32)
                .Build();

            await AssertCanPublishAndFetchSchema(schemas, schema, succeeds);
        }
    }
}
