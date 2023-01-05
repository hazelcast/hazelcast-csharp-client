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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using Hazelcast.Tests.Serialization.Compact.SchemasRemoteTestsLocal;
using Moq;
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

            public Func<ClientMessage, ValueTask> SendingMessage
            {
                get => _messaging.SendingMessage;
                set => _messaging.SendingMessage = value;
            }

            public Task<ClientMessage> SendAsync(ClientMessage requestMessage, bool triggerEvents, CancellationToken cancellationToken)
            {

                Interlocked.Increment(ref _count);
                return _messaging.SendAsync(requestMessage, triggerEvents, cancellationToken);
            }

            public Task<ClientMessage> SendToMemberAsync(ClientMessage message, MemberConnection memberConnection, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<Guid> GetConnectedMembers() => _messaging.GetConnectedMembers();

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
    [ServerCondition("[5.2,)")]
    public class SchemasRemoteTests : SingleMemberClientRemoteTestBase
    {
        // make sure to use distinct schema names since we're running all the tests
        // of this fixture on 1 member which will end up storing & caching schemas
        private readonly ISequence<int> _ids = new Int32Sequence();

        [Test]
        public void NewSchemasGoToSerializationOutput()
        {
            var obj = new SomeClass { Value = 12 };

            var options = new CompactOptions();
            var messaging = Mock.Of<IClusterMessaging>();
            var schemas = new Schemas(messaging, options);
            const Endianness endianness = Endianness.LittleEndian;
            var ss = new CompactSerializationSerializer(options, schemas, endianness);
            var output = new ObjectDataOutput(1024, ss, endianness);

            ss.Write(output, obj);

            // serialize = output references the schema
            Assert.That(output.HasSchemas);
            Assert.That(output.SchemaIds.Count, Is.EqualTo(1));

            // and schemas know the schema, as unpublished
            var schemaId = output.SchemaIds.First();
            Assert.That(schemas.TryGet(schemaId, out var schema));
            Assert.That(schemas.IsPublished(schemaId), Is.False);

            // serialize again = output still references the schema
            output = new ObjectDataOutput(1024, ss, endianness);
            ss.Write(output, obj);
            Assert.That(output.HasSchemas);
            Assert.That(output.SchemaIds.Count, Is.EqualTo(1));
            Assert.That(output.SchemaIds, Does.Contain(schemaId));

            // add the schema, as published
            schemas.Add(schema, true);
            Assert.That(schemas.IsPublished(schemaId));

            // serialize again = output does not reference the schema anymore
            output = new ObjectDataOutput(1024, ss, endianness);
            ss.Write(output, obj);
            Assert.That(output.HasSchemas, Is.False);
        }

        [Test]
        public void SerializationOutputSchemasGoToClientMessage()
        {
            var obj = new SomeClass { Value = 12 };

            var options = new CompactOptions();
            var messaging = Mock.Of<IClusterMessaging>();
            var schemas = new Schemas(messaging, options);
            const Endianness endianness = Endianness.LittleEndian;
            var ss = new CompactSerializationSerializer(options, schemas, endianness);
            var output = new ObjectDataOutput(1024, ss, endianness);

            ss.Write(output, obj);

            // serialize = output references the schema
            Assert.That(output.HasSchemas);
            var schemaId = output.SchemaIds.First();

            // and schema bubbles up in the message
            var data = new HeapData(output.ToByteArray(), output.HasSchemas ? output.SchemaIds : null);
            var message = ListAddCodec.EncodeRequest("list-name", data);
            Assert.That(message.HasSchemas);
            Assert.That(message.SchemaIds.Count, Is.EqualTo(1));
            Assert.That(message.SchemaIds, Does.Contain(schemaId));
        }

        [Test]
        public async Task CanPublishAndFetchSchemas()
        {
            // create a schema cache (don't use the client's one)
            var messaging = Client.GetMessaging();
            var schemas = new Schemas(messaging, new CompactOptions());

            var schema = new Schema($"sometype{_ids.GetNext()}", new[]
            {
                new SchemaField("somefield", FieldKind.String)
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
            var schemas2 = new Schemas(messaging, new CompactOptions());
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
            var schemas = new Schemas(messaging, new CompactOptions());

            var schema = new Schema($"sometype{_ids.GetNext()}", new[]
            {
                new SchemaField("somefield", FieldKind.String)
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
                new SchemaField("somefield", FieldKind.String)
            });

            await AssertCanPublishAndFetchSchema(schemas, schema);
        }

        private async Task AssertCanPublishAndFetchSchema(Schemas schemas, Schema schema)
        {
            Assert.That(schemas.TryGet(schema.Id, out _), Is.False); // it's not there
            Assert.That(await schemas.GetOrFetchAsync(schema.Id), Is.Null); // and not on the cluster either
            Assert.That(await schemas.FetchAsync(schema.Id), Is.Null); // still not on the cluster

            schemas.Add(schema, false);

            Assert.That(schemas.TryGet(schema.Id, out _), Is.True); // it's there
            Assert.That(await schemas.GetOrFetchAsync(schema.Id), Is.Not.Null); // so returned immediately

            // but it's not yet on the cluster and cannot be fetched
            Assert.That(await schemas.FetchAsync(schema.Id), Is.Null);

            // publish that one schema
            await schemas.PublishAsync(new HashSet<long> { schema.Id });

            // now it can be fetched
            Assert.That(await schemas.FetchAsync(schema.Id), Is.Not.Null);
        }

        [Test]
        public async Task NoReservedKeyword()
        {
            var schemas = (Schemas)((HazelcastClient)Client).SerializationService.CompactSerializer.Schemas;

            var schema = SchemaBuilder
                .For($"thing{_ids.GetNext()}")
                .WithField("name", FieldKind.String)
                .WithField("value", FieldKind.Int32)
                .Build();

            await AssertCanPublishAndFetchSchema(schemas, schema);
        }

        public class SomeClass
        {
            public int Value { get; set; }
        }
    }
}
