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

#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.Serialization.Compact;

/// <summary>
/// Implements <see cref="ISchemas"/>
/// </summary>
internal class Schemas : ISchemas
{
    private readonly IClusterMessaging _messaging;
    private readonly int _replicationRetries;
    private readonly TimeSpan _replicationDelay;

    // we are using a ConcurrentDictionary<,> so that this class is multi-threaded
    //
    // we do *not* use an async concurrent dictionary, this means that if a schema is missing,
    // two or more parallel flows of execution could fetch it from the cluster at the same time,
    // instead of having the first flow block the others. should this eventually become an
    // issue, we would need to implement a proper async concurrent dictionary.
    //
    // we use the schema id as a key, i.e. we assume that if two schemas have the same id, they are the same
    // see notes on Compact Serialization design documents about collision risks with ids
    private readonly ConcurrentDictionary<long, SchemaInfo> _schemas = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Schemas"/> class.
    /// </summary>
    /// <param name="messaging">A messaging service.</param>
    /// <param name="options">Compact options.</param>
    public Schemas(IClusterMessaging messaging, CompactOptions options)
    {
        _messaging = messaging;
        _replicationRetries = options.SchemaReplicationRetries;
        _replicationDelay = options.SchemaReplicationDelay;
    }

    /// <inheritdoc />
    public void Dispose()
    { }

    private class SchemaInfo
    {
        public SchemaInfo(Schema schema, bool isPublished = false)
        {
            Schema = schema;
            IsPublished = isPublished;
        }

        public Schema Schema { get; }

        public bool IsPublished { get; set; }
    }

    /// <inheritdoc />
    public void Add(Schema schema, bool isClusterSchema)
    {
        _schemas.AddOrUpdate(schema.Id,
            _ => new SchemaInfo(schema, isClusterSchema),
            (_, schemaInfo) => { schemaInfo.IsPublished = true; return schemaInfo; });
    }

    /// <inheritdoc />
    public bool TryGet(long id, [NotNullWhen(true)] out Schema? schema)
    {
        if (_schemas.TryGetValue(id, out var schemaInfo))
        {
            schema = schemaInfo.Schema;
            return true;
        }

        schema = null;
        return false;
    }

    /// <inheritdoc />
    public ValueTask<Schema?> GetOrFetchAsync(long id)
    {
        return _schemas.TryGetValue(id, out var schemaInfo) 
            ? new ValueTask<Schema?>(schemaInfo.Schema) 
            : FetchAsync(id);
    }

    // internal for tests
    internal async ValueTask<Schema?> FetchAsync(long id)
    {
        var requestMessage = ClientFetchSchemaCodec.EncodeRequest(id);
        var response = await _messaging.SendAsync(requestMessage, CancellationToken.None).CfAwait();
        var schema = ClientFetchSchemaCodec.DecodeResponse(response).Schema;
        if (schema == null) return null;

        // if found, add or mark published
        var schemaInfo = _schemas.GetOrAdd(schema.Id, _ => new SchemaInfo(schema, true));
        schemaInfo.IsPublished = true;
        return schema;
    }

    /// <inheritdoc />
    public bool IsPublished(long schemaId)
    {
        return _schemas.TryGetValue(schemaId, out var schemaInfo) && schemaInfo.IsPublished;
    }

    private async Task PublishAsync(Schema schema)
    {
        var requestMessage = ClientSendSchemaCodec.EncodeRequest(schema);

        for (var i = 0; i < _replicationRetries; i++)
        {
            // NOTE
            // it is important to use the SendAsync overload that accepts a raiseEvents boolean
            // parameter, in order to disable raising events, else we would enter an infinite
            // loop when this method is invoked when handling an event

            var response = await _messaging.SendAsync(requestMessage, false, CancellationToken.None).CfAwait();
            var replicatedMembers = ClientSendSchemaCodec.DecodeResponse(response).ReplicatedMembers;
            var clientMembers = _messaging.GetConnectedMembers();
            var allReplicated = clientMembers.All(x => replicatedMembers.Contains(x));
            if (allReplicated) return;

            if (_replicationDelay > TimeSpan.Zero)
                await Task.Delay(_replicationDelay).CfAwait();
        }

        throw new HazelcastException($"Failed to replicate schema {schema} in the cluster (retried {_replicationRetries} times)."
            + " It might be the case that the client is connected to the two halves of a cluster that is experiencing a"
            + " split-brain. It might be possible to replicate the schema after some time, once the cluster has healed.");
    }

    /// <inheritdoc />
    public async ValueTask PublishAsync(HashSet<long>? ids = null)
    {
        if (ids == null)
        {
            // optimize with ClientSendAllSchemasCodec even though it does not support
            // verifying that each schema has been correctly replicated to all members,
            // this is accepted and Java does the same
            var schemaInfos = _schemas.Values; // capture
            await PublishAsync(schemaInfos.Select(x => x.Schema).ToList()).CfAwait();

            // and mark them all published
            foreach (var schemaInfo in schemaInfos) schemaInfo.IsPublished = true;
        }
        else
        {
            // do NOT optimize with ClientSendAllSchemasCodec all as it does not support
            // verifying that each schema has been correctly replicated to all members
            foreach (var id in ids)
            {
                if (_schemas.TryGetValue(id, out var schemaInfo) && !schemaInfo.IsPublished)
                {
                    await PublishAsync(schemaInfo.Schema).CfAwait();
                    schemaInfo.IsPublished = true;
                }
            }
        }
    }

    /// <inheritdoc />
    public ValueTask OnConnectionOpened(MemberConnection connection, bool isFirstEver, bool isFirst, bool isNewCluster)
    {
        // when the first connection to a cluster is opened, regardless of whether it's a "new"
        // cluster (first time the client connects, or cluster ID change) or not, make sure we
        // republish all schemas (because, in case of split-brain, maybe the cluster ID does not
        // change and yet we are connecting to a "fresh" cluster).

        return isFirst ? PublishAsync() : default;
    }

    private async Task PublishAsync(IList<Schema> schemas)
    {
        // NOTE
        // it is important to use the SendAsync overload that accepts a raiseEvents boolean
        // parameter, in order to disable raising events, else we would enter an infinite
        // loop when this method is invoked when handling an event

        switch (schemas.Count)
        {
            case 0:
            {
                break;
            }

            case 1:
            {
                var schema = schemas[0];
                var requestMessage = ClientSendSchemaCodec.EncodeRequest(schema);
                var response = await _messaging.SendAsync(requestMessage, false, CancellationToken.None).CfAwait();
                _ = ClientSendSchemaCodec.DecodeResponse(response);
                break;
            }

            default:
            {
                var requestMessage = ClientSendAllSchemasCodec.EncodeRequest(schemas);
                var response = await _messaging.SendAsync(requestMessage, false, CancellationToken.None).CfAwait();
                _ = ClientSendAllSchemasCodec.DecodeResponse(response);
                break;
            }
        }
    }
}