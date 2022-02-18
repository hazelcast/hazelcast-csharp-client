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

#nullable enable

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
        private class ManagedSchema
        {
            public Schema Schema { get; set; }

            public bool IsClusterSchema { get; set; }

            public ManagedSchema SetIsClusterSchema()
            {
                IsClusterSchema = true;
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
        private readonly ConcurrentDictionary<long, ManagedSchema> _schemas = new ConcurrentDictionary<long, ManagedSchema>();

        private readonly object _mutex = new object();
        private readonly HashSet<long> _unpublished = new HashSet<long>(); // protected by _mutex
        private volatile bool _hasUnpublished; // protected by _mutex

        public Schemas(IClusterMessaging messaging)
        {
            _messaging = messaging;
        }

        /// <inheritdoc />
        public void Add(Schema schema, bool isClusterSchema)
        {
            Console.WriteLine($"Adding {schema.Id} {schema.TypeName} at:"); // FIXME - remove this line
            Console.WriteLine(Environment.StackTrace);
            if (_schemas.TryAdd(schema.Id, new ManagedSchema { Schema = schema, IsClusterSchema = isClusterSchema }) && !isClusterSchema)
            {
                lock (_mutex)
                {
                    _unpublished.Add(schema.Id);
                    _hasUnpublished = true;
                }
            }
            
            // FIXME - dead code
            //
            // idea was to return a boolean that we could propagate up to ToData so that we would
            // wait for the serialization service to be 'ready' only if really needed - but that
            // boolean would then need to also pop in IObjectDataOutput.WriteObject and that changes
            // the public APIs? so, not going to do it in this MPV. - to be discussed in TDD.
            //
            /*
            var publishedSchema = new PublishedSchema { Schema = schema, Published = published };
            var result = _schemas.GetOrAdd(schema.Id, publishedSchema);

            // if the schema was already known,
            // depends on the schema published state
            if (!ReferenceEquals(result, publishedSchema)) return result.Published;

            // if the schema is new but known to be published already,
            // nothing to do
            if (published) return true;

            // if the schema is new and known to be unpublished yet,
            // it must be published, we are not ok
            lock (_mutex)
            {
                _unpublished.Add(schema.Id);
                _hasUnpublished = true;
            }

            return false;
            */
        }

        /// <inheritdoc />
        public bool TryGet(long id, out Schema schema)
        {
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
            return _schemas.TryGetValue(id, out var publishedSchema)
                ? new ValueTask<Schema>(publishedSchema.Schema)
                : FetchAsync(id);
        }

        // internal for tests
        internal async ValueTask<Schema> FetchAsync(long id)
        {
            Console.WriteLine($"Fetching {id}"); // FIXME - remove this line
            var requestMessage = ClientFetchSchemaCodec.EncodeRequest(id);
            var response = await _messaging.SendAsync(requestMessage, CancellationToken.None).CfAwait();
            var schema = ClientFetchSchemaCodec.DecodeResponse(response).Schema;
            if (schema == null) return null;

            // if found, add or at least update the published state
            _schemas.AddOrUpdate(schema.Id,
                addValueFactory: key => new ManagedSchema { Schema = schema, IsClusterSchema = true },
                updateValueFactory: (key, value) => value.SetIsClusterSchema());

            lock (_mutex)
            {
                _unpublished.Remove(schema.Id);
                _hasUnpublished = _unpublished.Count > 0;
            }

            return schema;
        }

        

        /// <inheritdoc />
        public ValueTask PublishAsync()
        {
            IList<ManagedSchema> GetSchemasToPublish() => _schemas.Values.Where(x => !x.IsClusterSchema).ToList();

            // fast path is 1 lock + 1 boolean comparison
            lock (_mutex) return _hasUnpublished ? PublishAsync(GetSchemasToPublish()) : default;
        }

        private async ValueTask PublishAsync(IList<ManagedSchema> schemas)
        {
            switch (schemas.Count)
            {
                case 1:
                {
                    var schema = schemas[0];
                    var requestMessage = ClientSendSchemaCodec.EncodeRequest(schema.Schema);
                    var response = await _messaging.SendAsync(requestMessage, CancellationToken.None).CfAwait();
                    _ = ClientSendSchemaCodec.DecodeResponse(response);
                    schema.IsClusterSchema = true;
                    break;
                }

                default:
                {
                    var requestMessage = ClientSendAllSchemasCodec.EncodeRequest(schemas.Select(x => x.Schema));
                    var response = await _messaging.SendAsync(requestMessage, CancellationToken.None).CfAwait();
                    _ = ClientSendAllSchemasCodec.DecodeResponse(response);

                    foreach (var schema in schemas) schema.IsClusterSchema = true;
                    break;
                }
            }

            lock (_mutex)
            {
                foreach (var schema in schemas) _unpublished.Remove(schema.Schema.Id);
                _hasUnpublished = _unpublished.Count > 0;
            }
        }
    }
}
