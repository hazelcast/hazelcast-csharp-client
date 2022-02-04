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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact
{
    [TestFixture]
    public class SchemasLocalTests
    {
        private class ClusterMessagingStub : IClusterMessaging
        {
            private readonly Func<ClientMessage, ClientMessage> _respond;

            public ClusterMessagingStub(Func<ClientMessage, ClientMessage> respond)
            {
                _respond = respond;
            }

            public Task<ClientMessage> SendAsync(ClientMessage requestMessage, CancellationToken cancellationToken)
                => Task.FromResult(_respond(requestMessage));
        }

        private static Schema GetSchema() => GetSchema("typename", "fieldname");

        private static Schema GetSchema(string typename, string fieldname) => new Schema(typename, new[]
        {
            new SchemaField(fieldname, FieldKind.StringRef)
        });

        [Test]
        public void CanAddAndThenGetSchema()
        {
            var schemas = new Schemas(new ClusterMessagingStub(_ => throw new NotImplementedException()));
            var schema = GetSchema();

            Assert.That(schemas.TryGet(schema.Id, out _), Is.False);
            schemas.Add(schema);
            Assert.That(schemas.TryGet(schema.Id, out var returned), Is.True);
            Assert.That(returned.Id, Is.EqualTo(schema.Id));
        }

        [Test]
        public async Task MissingSchemaIsFetchedAndCached()
        {
            var count = 0;

            var schemas = new Schemas(new ClusterMessagingStub(requestMessage =>
            {
                Assert.That(requestMessage.MessageType, Is.EqualTo(ClientFetchSchemaCodec.RequestMessageType));
                Interlocked.Increment(ref count);
                return ClientFetchSchemaServerCodec.EncodeResponse(GetSchema());
            }));
            var schema = GetSchema();

            Assert.That(schemas.TryGet(schema.Id, out _), Is.False);

            Assert.That(count, Is.EqualTo(0)); // no message has been responded yet

            var fetched = await schemas.GetOrFetchAsync(schema.Id).ConfigureAwait(false);
            Assert.That(count, Is.EqualTo(1)); // a ClientFetchSchemaCodec message was responded
            Assert.That(fetched, Is.Not.Null); // and returned a schema
            Assert.That(fetched.Id, Is.EqualTo(schema.Id)); // which is the right one

            _ = await schemas.GetOrFetchAsync(schema.Id).ConfigureAwait(false);
            Assert.That(count, Is.EqualTo(1)); // no additional message was responded, the schema was cached
        }

        [Test]
        public async Task UnknowSchemaIsFetchedAgain()
        {
            var count = 0;

            var schemas = new Schemas(new ClusterMessagingStub(requestMessage =>
            {
                Assert.That(requestMessage.MessageType, Is.EqualTo(ClientFetchSchemaCodec.RequestMessageType));
                Interlocked.Increment(ref count);
                return ClientFetchSchemaServerCodec.EncodeResponse(null);
            }));

            Assert.That(count, Is.EqualTo(0)); // no message has been responded yet

            var fetched = await schemas.GetOrFetchAsync(42).ConfigureAwait(false);
            Assert.That(count, Is.EqualTo(1)); // a ClientFetchSchemaCodec message was responded
            Assert.That(fetched, Is.Null); // and returned no schema

            fetched = await schemas.GetOrFetchAsync(42).ConfigureAwait(false);
            Assert.That(count, Is.EqualTo(2)); // a ClientFetchSchemaCodec message was responded, again
            Assert.That(fetched, Is.Null); // and returned no schema
        }

        [Test]
        public async Task AddedSingleSchemaWantsToBePublishedOnce()
        {
            var count = 0;

            var schemas = new Schemas(new ClusterMessagingStub(requestMessage =>
            {
                Assert.That(requestMessage.MessageType, Is.EqualTo(ClientSendSchemaCodec.RequestMessageType));
                Interlocked.Increment(ref count);
                return ClientSendSchemaServerCodec.EncodeResponse();
            }));

            var schema = GetSchema();

            Assert.That(schemas.TryGet(schema.Id, out _), Is.False);
            schemas.Add(schema);

            Assert.That(count, Is.EqualTo(0)); // no message has been responded yet

            await schemas.PublishAsync().CfAwait();
            Assert.That(count, Is.EqualTo(1)); // a ClientSendSchemaCodec message was responded

            await schemas.PublishAsync().CfAwait();
            Assert.That(count, Is.EqualTo(1)); // no additional message was responded
        }

        [Test]
        public async Task AddedMultipleSchemasWantToBePublishedOnce()
        {
            var count = 0;

            var schemas = new Schemas(new ClusterMessagingStub(requestMessage =>
            {
                Assert.That(requestMessage.MessageType, Is.EqualTo(ClientSendAllSchemasCodec.RequestMessageType));
                Interlocked.Increment(ref count);
                return ClientSendAllSchemasServerCodec.EncodeResponse();
            }));

            var schema0 = GetSchema("typename", "fieldname0");
            Assert.That(schemas.TryGet(schema0.Id, out _), Is.False);
            schemas.Add(schema0);

            var schema1 = GetSchema("typename", "fieldname1");
            Assert.That(schemas.TryGet(schema1.Id, out _), Is.False);
            schemas.Add(schema1);

            Assert.That(count, Is.EqualTo(0)); // no message has been responded yet

            await schemas.PublishAsync().CfAwait();
            Assert.That(count, Is.EqualTo(1)); // a ClientSendAllSchemasCodec message was responded

            await schemas.PublishAsync().CfAwait();
            Assert.That(count, Is.EqualTo(1)); // no additional message was responded
        }

        [Test]
        public async Task AddedSingleSchemaIsAutoPublished()
        {
            var count = 0;

            var schemas = new Schemas(new ClusterMessagingStub(requestMessage =>
            {
                Assert.That(requestMessage.MessageType, Is.EqualTo(ClientSendSchemaCodec.RequestMessageType));
                Interlocked.Increment(ref count);
                return ClientSendSchemaServerCodec.EncodeResponse();
            }));

            var schema = GetSchema();

            Assert.That(schemas.TryGet(schema.Id, out _), Is.False);
            schemas.Add(schema);

            Assert.That(count, Is.EqualTo(0)); // no message has been responded yet

            // FIXME - do better
            await schemas.IsReadyAsync().CfAwait();
            Assert.That(schemas.IsReadyAsync().IsCompletedSuccessfully, Is.True);
            Assert.That(count, Is.EqualTo(1)); // a ClientSendAllSchemasCodec message was responded

            await schemas.PublishAsync().CfAwait();
            Assert.That(count, Is.EqualTo(1)); // no additional message was responded
        }
    }
}
