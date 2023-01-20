// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Compact;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact;

[TestFixture]
public class SchemasLocalTests
{
    private class ClusterMessagingStub : IClusterMessaging
    {
        private readonly Func<ClientMessage, Task<ClientMessage>> _respond;
        private readonly HashSet<Guid>? _memberIds;

        public ClusterMessagingStub(Func<ClientMessage, Task<ClientMessage>> respond, HashSet<Guid>? memberIds = null)
        {
            _respond = respond;
            _memberIds = memberIds;
        }

        public Func<ClientMessage, ValueTask>? SendingMessage { get; set; }

        public async Task<ClientMessage> SendAsync(ClientMessage requestMessage, bool triggerEvents, CancellationToken cancellationToken)
        {
            if (triggerEvents && SendingMessage != null) await SendingMessage.AwaitEach(requestMessage).CfAwait();
            return await _respond(requestMessage);
        }

        public Task<ClientMessage> SendToMemberAsync(ClientMessage message, MemberConnection memberConnection, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<ClientMessage> SendAsync(ClientMessage requestMessage, CancellationToken cancellationToken)
        {
            if (SendingMessage != null) await SendingMessage.AwaitEach(requestMessage).CfAwait();
            return await _respond(requestMessage);
        }

        public IEnumerable<Guid> GetConnectedMembers()
            => _memberIds ?? throw new Exception();
    }

    private static Schema GetSchema() => GetSchema("typename", "fieldname");

    private static Schema GetSchema(string typename, string fieldname) => new Schema(typename, new[]
    {
        new SchemaField(fieldname, FieldKind.String)
    });

    [Test]
    public void CanDispose()
    {
        using var schemas = new Schemas(new ClusterMessagingStub(_ => throw new NotImplementedException()), new CompactOptions());
    }

    [Test]
    public void CanAddAndThenGetSchema()
    {
        var schemas = new Schemas(new ClusterMessagingStub(_ => throw new NotImplementedException()), new CompactOptions());
        var schema = GetSchema();

        Assert.That(schemas.TryGet(schema.Id, out _), Is.False);
        schemas.Add(schema, false);
        Assert.That(schemas.TryGet(schema.Id, out var returned), Is.True);
        Assert.That(returned!.Id, Is.EqualTo(schema.Id));
    }

    [Test]
    public async Task MissingSchemaIsFetchedAndCached()
    {
        var count = 0;

        var schemas = new Schemas(new ClusterMessagingStub(requestMessage =>
        {
            Assert.That(requestMessage.MessageType, Is.EqualTo(ClientFetchSchemaCodec.RequestMessageType));
            Interlocked.Increment(ref count);
            return Task.FromResult(ClientFetchSchemaServerCodec.EncodeResponse(GetSchema()));
        }), new CompactOptions());
        var schema = GetSchema();

        Assert.That(schemas.TryGet(schema.Id, out _), Is.False);

        Assert.That(count, Is.EqualTo(0)); // no message has been responded yet

        var fetched = await schemas.GetOrFetchAsync(schema.Id).ConfigureAwait(false);
        Assert.That(count, Is.EqualTo(1)); // a ClientFetchSchemaCodec message was responded
        Assert.That(fetched, Is.Not.Null); // and returned a schema
        Assert.That(fetched!.Id, Is.EqualTo(schema.Id)); // which is the right one

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
            return Task.FromResult(ClientFetchSchemaServerCodec.EncodeResponse(null));
        }), new CompactOptions());

        Assert.That(count, Is.EqualTo(0)); // no message has been responded yet

        var fetched = await schemas.GetOrFetchAsync(42).ConfigureAwait(false);
        Assert.That(count, Is.EqualTo(1)); // a ClientFetchSchemaCodec message was responded
        Assert.That(fetched, Is.Null); // and returned no schema

        fetched = await schemas.GetOrFetchAsync(42).ConfigureAwait(false);
        Assert.That(count, Is.EqualTo(2)); // a ClientFetchSchemaCodec message was responded, again
        Assert.That(fetched, Is.Null); // and returned no schema
    }

    [Test]
    public async Task PublishAllSchemas()
    {
        var count = 0;

        var memberIds = new HashSet<Guid>
        {
            Guid.NewGuid(),
            Guid.NewGuid()
        };

        var schemas = new Schemas(new ClusterMessagingStub(requestMessage =>
        {
            Assert.That(requestMessage.MessageType, Is.EqualTo(ClientSendAllSchemasCodec.RequestMessageType));
            Interlocked.Increment(ref count);
            return Task.FromResult(ClientSendAllSchemasServerCodec.EncodeResponse());
        }, memberIds), new CompactOptions());

        var schema0 = GetSchema("type-0", "field");
        Assert.That(schemas.TryGet(schema0.Id, out _), Is.False);
        schemas.Add(schema0, true);

        var schema1 = GetSchema("type-1", "field");
        Assert.That(schemas.TryGet(schema1.Id, out _), Is.False);
        schemas.Add(schema1, false);

        Assert.That(count, Is.EqualTo(0)); // no message has been responded yet

        // schema0 is considered published, schema1 is not
        Assert.That(schemas.IsPublished(schema0.Id));
        Assert.That(schemas.IsPublished(schema1.Id), Is.False);

        await schemas.PublishAsync().CfAwait();
        Assert.That(count, Is.EqualTo(1)); // a ClientSendAllSchemasCodec message was responded

        // and now both schemas are considered published
        Assert.That(schemas.IsPublished(schema0.Id));
        Assert.That(schemas.IsPublished(schema1.Id));

        await schemas.PublishAsync().CfAwait();
        Assert.That(count, Is.EqualTo(2)); // a ClientSendAllSchemasCodec message was responded

        // schemas are still considered published
        Assert.That(schemas.IsPublished(schema0.Id));
        Assert.That(schemas.IsPublished(schema1.Id));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task PublishSomeSchemasWithRetries(bool succ)
    {
        var count0 = 0;
        var count1 = 0;
        const int retries = 10; // per schemas
        var succCount = succ ? retries / 2 : retries * 2;

        var memberIds = new HashSet<Guid>
        {
            Guid.NewGuid(),
            Guid.NewGuid()
        };

        var options = new CompactOptions
        {
            SchemaReplicationRetries = retries,
            SchemaReplicationDelay = TimeSpan.FromMilliseconds(10)
        };

        var id0 = 0L;
        var id1 = 0L;
        var schemas = new Schemas(new ClusterMessagingStub(requestMessage =>
        {
            Assert.That(requestMessage.MessageType, Is.EqualTo(ClientSendSchemaCodec.RequestMessageType));
            var request = ClientSendSchemaServerCodec.DecodeRequest(requestMessage);
            var schemaId = request.Schema.Id;

            // schema0 is always successfully propagated
            if (schemaId == id0)
            {
                Interlocked.Increment(ref count0);
                return Task.FromResult(ClientSendSchemaServerCodec.EncodeResponse(memberIds));
            }

            // schemaX is never propagated
            if (schemaId != id1)
                return Task.FromResult(ClientSendSchemaServerCodec.EncodeResponse(new HashSet<Guid>()));

            // schema1 is eventually propagated

            Interlocked.Increment(ref count1);
            var ids = count1 <= succCount ? new HashSet<Guid> { memberIds.First() } : memberIds;
            return Task.FromResult(ClientSendSchemaServerCodec.EncodeResponse(ids));

        }, memberIds), options);

        var schema0 = GetSchema("type-0", "field");
        Assert.That(schemas.TryGet(schema0.Id, out _), Is.False);
        schemas.Add(schema0, false);
        id0 = schema0.Id;

        var schema1 = GetSchema("type-1", "field");
        Assert.That(schemas.TryGet(schema1.Id, out _), Is.False);
        schemas.Add(schema1, false);
        id1 = schema1.Id;

        // no message has been responded yet
        Assert.That(count0, Is.EqualTo(0));
        Assert.That(count1, Is.EqualTo(0));

        var schemaIds = new HashSet<long> { schema0.Id, schema1.Id };

        if (succ)
        {
            // send - with retries
            await schemas.PublishAsync(schemaIds).CfAwait();
            Assert.That(count0, Is.EqualTo(1)); // worked on the 1st try
            Assert.That(count1, Is.EqualTo(succCount + 1)); // had to retry - and succeed
        }
        else
        {
            await AssertEx.ThrowsAsync<HazelcastException>(async () => await schemas.PublishAsync(schemaIds).CfAwait()).CfAwait();
            Assert.That(count0, Is.EqualTo(1)); // worked on the 1st try
            Assert.That(count1, Is.EqualTo(retries)); // had to retry - and fail
        }
    }

    [Test]
    public async Task CanAddSchemaAsClusterSchema()
    {
        var count = 0;

        var schemas = new Schemas(new ClusterMessagingStub(requestMessage =>
        {
            Assert.That(requestMessage.MessageType, Is.EqualTo(ClientSendAllSchemasCodec.RequestMessageType));
            Interlocked.Increment(ref count);
            return Task.FromResult(ClientSendAllSchemasServerCodec.EncodeResponse());
        }), new CompactOptions());

        var schema = GetSchema("typename", "fieldname0");
        Assert.That(schemas.TryGet(schema.Id, out _), Is.False);
        schemas.Add(schema, true);

        Assert.That(count, Is.EqualTo(0)); // no message has been responded yet

        var schemaIds = new HashSet<long> { schema.Id };

        await schemas.PublishAsync(schemaIds).CfAwait();
        Assert.That(count, Is.EqualTo(0)); // still, no message
    }
}