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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.Serialization.Compact
{
    /// <summary>
    /// Implements <see cref="ISchemas"/>
    /// </summary>
    internal class Schemas : ISchemas
    {
        private class PublishedSchema
        {
            public Schema Schema { get; set; }

            public bool Published { get; set; }

            public PublishedSchema SetPublished()
            {
                Published = true;
                return this;
            }
        }

        private readonly IClusterMessaging _messaging;

        // we are using a ConcurrentDictionary<,> so that this class is multi-threaded
        //
        // we do *not* use an async concurrent dictionary, this means that if a schema is missing,
        // two or more parallel flows of execution could fetch it from the cluster at the same time,
        // instead of having the first flow block the others. should this eventually become an
        // issue, we would need to implement a proper async concurrent dictionary.
        // TODO: or, do it now, as maybe Add() or TryGet() would turn async? NO THEY SHOULD NOT.
        //
        // we use the schema id as a key, i.e. we assume that if two schemas have the same id, they are the same
        // see notes on Compact Serialization design documents about collision risks with ids
        private readonly ConcurrentDictionary<long, PublishedSchema> _schemas = new ConcurrentDictionary<long, PublishedSchema>();

        private readonly HashSet<long> _unpublished = new HashSet<long>(); // protected by _mutex
        private readonly object _mutex = new object();
        private volatile bool _hasUnpublished; // protected by _mutex
        private volatile int _disposed;

        public Schemas(IClusterMessaging messaging)
        {
            _messaging = messaging;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed != 0) throw new ObjectDisposedException("Schemas");
        }

        /// <inheritdoc />
        public void Add(Schema schema, bool published = false)
        {
            ThrowIfDisposed();

            if (_schemas.TryAdd(schema.Id, new PublishedSchema { Schema = schema, Published = published }) && !published)
            {
                lock (_mutex)
                {
                    _unpublished.Add(schema.Id);
                    _hasUnpublished = true;
                }
            }
        }

        /// <inheritdoc />
        public bool TryGet(long id, out Schema schema)
        {
            ThrowIfDisposed();

            if (_schemas.TryGetValue(id, out var publishedSchema))
            {
                schema = publishedSchema.Schema;
                return true;
            }

            schema = null;
            return false;
        }

        /// <inheritdoc />
        public ValueTask<Schema> GetOrFetchAsync(long id)
        {
            ThrowIfDisposed();

            return _schemas.TryGetValue(id, out var publishedSchema)
                ? new ValueTask<Schema>(publishedSchema.Schema)
                : FetchAsync(id);
        }

        // internal for tests
        internal async ValueTask<Schema> FetchAsync(long id)
        {
            ThrowIfDisposed();

            var requestMessage = ClientFetchSchemaCodec.EncodeRequest(id);
            var response = await _messaging.SendAsync(requestMessage, CancellationToken.None).CfAwait();
            var schema = ClientFetchSchemaCodec.DecodeResponse(response).Schema;
            if (schema == null) return null;

            // if found, add or at least update the published state
            _schemas.AddOrUpdate(schema.Id,
                addValueFactory: key => new PublishedSchema { Schema = schema, Published = true },
                updateValueFactory: (key, value) => value.SetPublished());

            lock (_mutex) _unpublished.Remove(schema.Id);

            return schema;
        }

        // FIXME - document
        public ValueTask IsReadyAsync()
        {
            // don't bother about being disposed, go fast
            // we are adding 1 lock + 1 boolean test to *every* message we send
            lock (_mutex) return _hasUnpublished ? PublishWhileNeedsToBePublishedAsync() : default;
        }

        public async ValueTask PublishWhileNeedsToBePublishedAsync()
        {
            while (_disposed == 0)
            {
                // ConcurrentDictionary.Values is creating a snapshot which is safe to enumerate
                var schemas = _schemas.Values.Where(x => !x.Published).ToList();
                await PublishAsync(schemas).CfAwait();

                lock (_mutex)
                {
                    foreach (var schema in schemas) _unpublished.Remove(schema.Schema.Id);
                    if (_unpublished.Count == 0)
                    {
                        _hasUnpublished = false;
                        return;
                    }
                }
            }
        }

        /// <inheritdoc />
        public ValueTask PublishAsync()
        {
            ThrowIfDisposed();

            // ConcurrentDictionary.Values is creating a snapshot which is safe to enumerate
            var schemas = _schemas.Values.Where(x => !x.Published).ToList();
            return schemas.Count == 0 ? default : PublishAsync(schemas);
        }

        private async ValueTask PublishAsync(List<PublishedSchema> schemas)
        {
            switch (schemas.Count)
            {
                case 1:
                {
                    var schema = schemas[0];
                    var requestMessage = ClientSendSchemaCodec.EncodeRequest(schema.Schema);
                    var response = await _messaging.SendAsync(requestMessage, CancellationToken.None).CfAwait();
                    _ = ClientSendSchemaCodec.DecodeResponse(response);
                    schema.Published = true;
                    break;
                }

                default:
                {
                    var requestMessage = ClientSendAllSchemasCodec.EncodeRequest(schemas.Select(x => x.Schema));
                    var response = await _messaging.SendAsync(requestMessage, CancellationToken.None).CfAwait();
                    _ = ClientSendAllSchemasCodec.DecodeResponse(response);

                    foreach (var schema in schemas) schema.Published = true;
                    break;
                }
            }
        }
    }
}
